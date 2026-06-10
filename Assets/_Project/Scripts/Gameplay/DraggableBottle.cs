using ChemistryGame.Chemistry;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ChemistryGame.Gameplay
{
    /// <summary>Chai chứa chất, có thể drag để rót vào ống nghiệm — HOẶC click để mở popup chọn lượng.</summary>
    public class DraggableBottle : DraggableItem, IPointerClickHandler
    {
        [Header("Refs")]
        [SerializeField] private Image bodyImage;
        [SerializeField] private Image liquidImage;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text amountText;

        [Header("Chất rắn (solid / precipitate render)")]
        [Tooltip("Sprite 'cục rắn'. Khi chất là rắn/kết tủa/tinh thể, CẢ CHAI đổi thành cục này " +
                 "(tint theo VisualColor), ẩn phần chất lỏng bên trong. " +
                 "Để trống = fallback kiểu cũ: tô đặc (opaque) lớp lỏng.")]
        [SerializeField] private Sprite solidChunkSprite;
        [Tooltip("Hậu tố thêm vào nhãn cho chất rắn (vd ' (rắn)'). Để trống nếu không muốn.")]
        [SerializeField] private string solidLabelSuffix = " (rắn)";
        private Sprite _liquidFillSprite;   // cache sprite gốc của liquidImage (dạng chất lỏng)
        private Sprite _bodySprite;         // cache visual gốc của bodyImage (dạng chai thủy tinh)
        private Color _bodyColor;
        private bool _bodyPreserveAspect;
        private bool _spriteCached;

        public SubstanceData Substance { get; private set; }
        public float CurrentAmount { get; private set; }
        public float PourPerDrop { get; private set; } = 25f;
        private bool _masked;
        private string _maskedLabel;

        public void Bind(SubstanceData s, float initialAmount, bool masked = false, string maskedLabel = "?")
        {
            Substance = s;
            CurrentAmount = initialAmount;
            _masked = masked;
            _maskedLabel = maskedLabel;
            Refresh();
            // Also drive LayeredLiquidView if present (new shader-based fill)
            var llv = GetComponentInChildren<LayeredLiquidView>();
            if (llv != null && s != null)
            {
                var fillColor = s.VisualColor;
                if (IsSolidLike) fillColor.a = 1f; // chất rắn: lớp đặc, không trong như dung dịch
                llv.BindSingle(fillColor, initialAmount);
            }
        }

        public void SetPourPerDrop(float v) => PourPerDrop = Mathf.Max(1f, v);

        public void Consume(float amt)
        {
            CurrentAmount = Mathf.Max(0f, CurrentAmount - amt);
            Refresh();
            var llv = GetComponentInChildren<LayeredLiquidView>();
            if (llv != null) llv.TweenFillFromAmount(CurrentAmount);
        }

        /// <summary>Chất rắn / kết tủa / tinh thể — render khác chất lỏng (không nhìn như ống nghiệm nước).</summary>
        public bool IsSolidLike => Substance != null && (Substance.IsSolid || Substance.IsPrecipitate);

        /// <summary>Sprite cục rắn (cho GameplayController truyền vào animation thả cục).</summary>
        public Sprite SolidChunkSprite => solidChunkSprite;

        private void Refresh()
        {
            if (Substance == null) return;

            if (!_spriteCached)
            {
                if (liquidImage != null) _liquidFillSprite = liquidImage.sprite;
                if (bodyImage != null)
                {
                    _bodySprite = bodyImage.sprite;
                    _bodyColor = bodyImage.color;
                    _bodyPreserveAspect = bodyImage.preserveAspect;
                }
                _spriteCached = true;
            }

            // Chunk mode: vật rắn KHÔNG hiển thị như ống nghiệm — cả chai là 1 cục.
            bool chunkMode = IsSolidLike && solidChunkSprite != null && bodyImage != null;

            if (bodyImage != null)
            {
                if (chunkMode)
                {
                    var c = Substance.VisualColor; c.a = 1f;     // cục đặc, đục
                    bodyImage.sprite = solidChunkSprite;
                    bodyImage.preserveAspect = true;
                    bodyImage.color = c;
                }
                else
                {
                    bodyImage.sprite = _bodySprite;
                    bodyImage.preserveAspect = _bodyPreserveAspect;
                    bodyImage.color = _bodyColor;
                }
            }

            if (liquidImage != null)
            {
                // Chunk mode: cục rắn không có phần "nước" — ẩn hẳn child Liquid.
                liquidImage.gameObject.SetActive(!chunkMode);

                if (!chunkMode)
                {
                    if (IsSolidLike)
                    {
                        var c = Substance.VisualColor; c.a = 1f;     // fallback thiếu sprite: tô đặc, đục
                        liquidImage.color = c;
                    }
                    else
                    {
                        liquidImage.color = Substance.VisualColor;   // lỏng/dung dịch: giữ độ trong suốt
                        if (_liquidFillSprite != null) liquidImage.sprite = _liquidFillSprite;
                    }
                }
            }

            if (labelText != null)
            {
                if (_masked) labelText.text = _maskedLabel;
                else labelText.text = IsSolidLike && !string.IsNullOrEmpty(solidLabelSuffix)
                    ? Substance.Formula + solidLabelSuffix
                    : Substance.Formula;
            }

            if (amountText != null) amountText.text = $"{CurrentAmount:F0}";
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            // Check raycast destinations để tìm DroppableTube
            var go = eventData.pointerCurrentRaycast.gameObject;
            DroppableTube target = null;
            if (go != null)
            {
                target = go.GetComponentInParent<DroppableTube>();
            }
            if (target != null && CurrentAmount > 0f)
            {
                var amt = Mathf.Min(PourPerDrop, CurrentAmount);
                // Snap back to original parent/pos FIRST so animator can use clean origin.
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                transform.localScale = Vector3.one;
                rect.SetParent(originalParent, true);
                rect.anchoredPosition = originalPos;

                target.ReceiveSubstance(Substance, amt);
                Consume(amt);
                return;
            }
            base.OnEndDrag(eventData);
        }

        // === Click (no drag) → open PourAmountPopup để chọn lượng chính xác ===

        /// <summary>Set bởi GameplayController khi spawn bottle — cung cấp callback mở popup.</summary>
        public System.Action<DraggableBottle> OnClickRequest;

        public void OnPointerClick(PointerEventData eventData)
        {
            // Bỏ qua nếu vừa drag (eventData.dragging vẫn true tại OnPointerClick nếu drag fired).
            // Unity EventSystem: drag end mới fire OnEndDrag rồi OnPointerClick — eventData.dragging = false
            // tại thời điểm click. Nếu kéo > drag threshold (~5px), OnPointerClick KHÔNG fire (drag thay thế).
            // Vì vậy OnPointerClick chỉ fire khi user click thuần (không kéo).
            OnClickRequest?.Invoke(this);
        }
    }
}
