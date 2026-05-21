using System.Collections.Generic;
using ChemistryGame.Chemistry;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame.Gameplay
{
    /// <summary>
    /// Liquid view sử dụng shader ChemistryGame/LiquidLayered. Hỗ trợ TỐI ĐA 4 lớp
    /// (mỗi lớp = 1 substance chiếm portion theo amount). Khi cần tilt (bottle đang
    /// rotate khi rót), set <see cref="SurfaceTilt"/>.
    ///
    /// Khác <see cref="LiquidView"/> (chỉ blend màu thành 1 tổng) — class này giữ
    /// được "layer" giống WaterSort, hợp lý cho tube chính của chemistry game khi
    /// nhiều chất chưa phản ứng hoàn toàn.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class LayeredLiquidView : MonoBehaviour
    {
        private static readonly int PropC1 = Shader.PropertyToID("_C1");
        private static readonly int PropC2 = Shader.PropertyToID("_C2");
        private static readonly int PropC3 = Shader.PropertyToID("_C3");
        private static readonly int PropC4 = Shader.PropertyToID("_C4");
        private static readonly int PropFill = Shader.PropertyToID("_FillAmount");
        private static readonly int PropSarm = Shader.PropertyToID("_SARM");
        private static readonly int PropCount = Shader.PropertyToID("_LayerCount");

        // Tune-able qua Inspector. Default 80 = đủ để fill rõ với pour 25/lần.
        [SerializeField] private float maxCapacity = 80f;
        [SerializeField] private float fillTweenDur = 0.45f;
        [SerializeField] private float colorTweenDur = 0.35f;

        private Image _image;
        private Material _matInstance; // instance để không ảnh hưởng material asset
        private float _currentFill;
        private float _surfaceTilt;
        private Color[] _layerColors = new Color[4];
        private Tween _fillTween;

        public float MaxCapacity => maxCapacity;
        public float CurrentFill01 => _currentFill;

        /// <summary>Surface tilt SARM trong [-1, 1]. Set bởi BottlePourAnimator khi rotate.</summary>
        public float SurfaceTilt
        {
            get => _surfaceTilt;
            set
            {
                _surfaceTilt = Mathf.Clamp(value, -1f, 1f);
                if (_matInstance != null) _matInstance.SetFloat(PropSarm, _surfaceTilt);
            }
        }

        private void Awake()
        {
            _image = GetComponent<Image>();
            // clone material để mỗi instance có state riêng
            _matInstance = new Material(_image.material);
            _image.material = _matInstance;
            for (int i = 0; i < 4; i++) _layerColors[i] = new Color(0, 0, 0, 0);
            ApplyLayerColors();
            SetFillImmediate(0f);
        }

        private void OnDestroy()
        {
            if (_matInstance != null) Destroy(_matInstance);
        }

        public void Reset()
        {
            _fillTween?.Kill();
            for (int i = 0; i < 4; i++) _layerColors[i] = new Color(0, 0, 0, 0);
            ApplyLayerColors();
            if (_matInstance != null) _matInstance.SetFloat(PropCount, 1);
            SetFillImmediate(0f);
            SurfaceTilt = 0f;
        }

        public void SetFillImmediate(float fill01)
        {
            _currentFill = Mathf.Clamp01(fill01);
            if (_matInstance != null) _matInstance.SetFloat(PropFill, _currentFill);
        }

        public void TweenFill(float target01, float duration = -1f)
        {
            _fillTween?.Kill();
            float dur = duration < 0 ? fillTweenDur : duration;
            target01 = Mathf.Clamp01(target01);
            _fillTween = DOTween.To(() => _currentFill, x =>
            {
                _currentFill = x;
                if (_matInstance != null) _matInstance.SetFloat(PropFill, x);
            }, target01, dur).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// Update từ WorkspaceState theo logic HOÁ HỌC (không phải WaterSort puzzle):
        ///   • Aqueous + Liquid (Solvent) → blend thành 1 màu "dung dịch đồng nhất" → top layer.
        ///   • Precipitate + Solid + Crystal → blend thành 1 màu "lắng đáy" → bottom layer.
        ///   • Gas → không hiển thị (thoát khỏi tube).
        ///
        /// → Tối đa 2 layer (dissolved + settled), khớp với cách dung dịch hoá học thật
        ///    nhìn trong ống nghiệm.
        ///
        /// Nếu chỉ có 1 nhóm thì hiển thị đơn lớp duy nhất.
        /// </summary>
        public void UpdateFromContents(IReadOnlyDictionary<SubstanceData, Amount> contents)
        {
            float totalVolume = 0f;
            float dissolvedAmt = 0f, settledAmt = 0f;
            Color dissolvedSum = Color.clear, settledSum = Color.clear;

            foreach (var kv in contents)
            {
                if (kv.Key == null || kv.Value.IsZero) continue;
                if (kv.Key.IsGas) continue; // gas thoát ra

                float amt = kv.Value.Value;
                totalVolume += amt;

                if (IsSettled(kv.Key))
                {
                    settledAmt += amt;
                    settledSum += kv.Key.VisualColor * amt;
                }
                else
                {
                    dissolvedAmt += amt;
                    dissolvedSum += kv.Key.VisualColor * amt;
                }
            }

            // Bottom layer (C1) = settled (precipitate/solid) — chìm đáy do tỉ trọng
            // Top layer (C2) = dissolved solution — đồng nhất
            int activeLayers = 0;
            for (int i = 0; i < 4; i++) _layerColors[i] = new Color(0, 0, 0, 0);

            if (settledAmt > 0.01f)
            {
                _layerColors[0] = settledSum / settledAmt;
                activeLayers++;
            }
            if (dissolvedAmt > 0.01f)
            {
                int idx = settledAmt > 0.01f ? 1 : 0; // nếu có settled, dissolved ở layer 2; nếu không, ở layer 1
                _layerColors[idx] = dissolvedSum / dissolvedAmt;
                activeLayers++;
            }

            ApplyLayerColors();
            if (_matInstance != null)
                _matInstance.SetFloat(PropCount, Mathf.Max(1, activeLayers));

            TweenFill(Mathf.Clamp01(totalVolume / maxCapacity));
        }

        /// <summary>
        /// Substance có "lắng đáy" không? Precipitate (kết tủa), Solid (rắn cứng), Crystal (tinh thể).
        /// Liquid solvent (nước) và Aqueous (dd) thì hoà vào solution.
        /// </summary>
        private static bool IsSettled(SubstanceData s)
        {
            switch (s.Phase)
            {
                case SubstancePhase.Precipitate:
                case SubstancePhase.Solid:
                case SubstancePhase.Crystal:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Bottle (single-substance): 1 layer, color = substance.VisualColor.</summary>
        public void BindSingle(Color color, float amount)
        {
            _layerColors[0] = color;
            for (int i = 1; i < 4; i++) _layerColors[i] = new Color(0, 0, 0, 0);
            if (_matInstance != null) _matInstance.SetFloat(PropCount, 1);
            ApplyLayerColors();
            TweenFill(Mathf.Clamp01(amount / maxCapacity));
        }

        public void TweenFillFromAmount(float amount)
        {
            TweenFill(Mathf.Clamp01(amount / maxCapacity));
        }

        public Vector3 GetTopWorldPosition()
        {
            if (_image == null) return transform.position;
            var rt = _image.rectTransform;
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            // upper-left = 1, upper-right = 2
            var topMid = Vector3.Lerp(corners[1], corners[2], 0.5f);
            return topMid;
        }

        private void ApplyLayerColors()
        {
            if (_matInstance == null) return;
            _matInstance.SetColor(PropC1, _layerColors[0]);
            _matInstance.SetColor(PropC2, _layerColors[1]);
            _matInstance.SetColor(PropC3, _layerColors[2]);
            _matInstance.SetColor(PropC4, _layerColors[3]);
        }
    }
}
