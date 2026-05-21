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
            if (llv != null && s != null) llv.BindSingle(s.VisualColor, initialAmount);
        }

        public void SetPourPerDrop(float v) => PourPerDrop = Mathf.Max(1f, v);

        public void Consume(float amt)
        {
            CurrentAmount = Mathf.Max(0f, CurrentAmount - amt);
            Refresh();
            var llv = GetComponentInChildren<LayeredLiquidView>();
            if (llv != null) llv.TweenFillFromAmount(CurrentAmount);
        }

        private void Refresh()
        {
            if (Substance == null) return;
            if (liquidImage != null) liquidImage.color = Substance.VisualColor;
            if (labelText != null)
            {
                labelText.text = _masked ? _maskedLabel : Substance.Formula;
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
