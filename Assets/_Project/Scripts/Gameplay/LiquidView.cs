using System.Collections.Generic;
using ChemistryGame.Chemistry;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame.Gameplay
{
    /// <summary>
    /// Fake-physics liquid in test tube using DOTween fillAmount + color blend.
    /// 2 child Images: liquidFill (Image.Type=Filled, Vertical bottom→top) + outline.
    /// </summary>
    public class LiquidView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Image liquidFill;
        [SerializeField] private Image surfaceImage; // optional

        [Header("Capacity")]
        [SerializeField] private float maxCapacity = 100f;

        [Header("Tween durations")]
        [SerializeField] private float fillTween = 0.5f;
        [SerializeField] private float colorTween = 0.4f;

        public float MaxCapacity => maxCapacity;

        private float _currentVolume;
        private Color _currentColor = Color.clear;

        private Tween _fillTween;
        private Tween _colorTween;

        public void Reset()
        {
            _fillTween?.Kill(); _colorTween?.Kill();
            _currentVolume = 0f;
            _currentColor = Color.clear;
            if (liquidFill != null) { liquidFill.fillAmount = 0f; liquidFill.color = Color.clear; }
            if (surfaceImage != null) surfaceImage.color = Color.clear;
        }

        public void UpdateFromContents(IReadOnlyDictionary<SubstanceData, Amount> contents)
        {
            float totalVolume = 0f;
            Color blended = Color.clear;
            float totalWeight = 0f;
            int n = 0;

            foreach (var kv in contents)
            {
                if (kv.Key == null || kv.Value.IsZero) continue;
                // Solid/precipitate cũng đóng góp volume nhỏ; gas thì không hiển thị trong tube
                if (kv.Key.IsGas) continue;
                var weight = kv.Value.Value;
                totalVolume += weight;
                blended += kv.Key.VisualColor * weight;
                totalWeight += weight;
                n++;
            }
            if (totalWeight > 0f) blended /= totalWeight;

            var fillTarget = Mathf.Clamp01(totalVolume / maxCapacity);
            if (liquidFill != null)
            {
                _fillTween?.Kill();
                _fillTween = liquidFill.DOFillAmount(fillTarget, fillTween).SetEase(Ease.OutQuad);
            }
            if (n > 0 && liquidFill != null)
            {
                _colorTween?.Kill();
                _colorTween = liquidFill.DOColor(blended, colorTween).SetEase(Ease.OutQuad);
            }
            else if (n == 0 && liquidFill != null)
            {
                _colorTween?.Kill();
                _colorTween = liquidFill.DOColor(Color.clear, colorTween);
            }

            if (surfaceImage != null && n > 0)
            {
                var sc = blended; sc.a *= 0.6f;
                surfaceImage.DOColor(sc, colorTween);
            }

            _currentVolume = totalVolume;
            _currentColor = blended;
        }

        public Vector3 GetTopWorldPosition()
        {
            if (liquidFill == null) return transform.position;
            var rt = liquidFill.rectTransform;
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            // upper-left = 1, upper-right = 2
            return Vector3.Lerp(corners[1], corners[2], 0.5f);
        }
    }
}
