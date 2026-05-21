using System;
using ChemistryGame.Chemistry;
using ChemistryGame.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame.Gameplay
{
    /// <summary>
    /// In-scene popup (không qua Luzart UIManager — gọn nhẹ + sống cùng Gameplay prefab).
    /// Click 1 chai → popup hiện slider chọn lượng → Confirm fires pour event.
    ///
    /// Tránh modal-block: backdrop click outside = Cancel. Popup không pause game,
    /// cho phép user click bottle khác hoặc tool khác mà không cần đóng popup tay.
    /// </summary>
    public class PourAmountPopupUI : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private RectTransform card;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private Slider slider;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Image previewFill;     // hiện màu chất sẽ rót

        [Header("Behavior")]
        [SerializeField] private float defaultPourAmount = 25f;
        [SerializeField] private float minPour = 1f;

        private DraggableBottle _activeBottle;
        private Action<float> _onConfirm;

        private void Awake()
        {
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
            if (cancelButton != null)  cancelButton.onClick.AddListener(Close);
            if (slider != null)        slider.onValueChanged.AddListener(OnSliderChanged);
            // Note: KHÔNG SetActive(false) ở đây. Prefab đã save inactive — Awake không tự ẩn lần đầu Open().
        }

        public void Open(DraggableBottle bottle, Action<float> onConfirm)
        {
            if (bottle == null || bottle.Substance == null) return;
            _activeBottle = bottle;
            _onConfirm = onConfirm;

            // Bind slider range to bottle's current amount.
            if (slider != null)
            {
                slider.minValue = minPour;
                slider.maxValue = Mathf.Max(minPour, bottle.CurrentAmount);
                slider.value = Mathf.Min(defaultPourAmount, bottle.CurrentAmount);
            }
            if (titleText != null) titleText.text = $"Đổ {bottle.Substance.Formula}";
            if (previewFill != null) previewFill.color = bottle.Substance.VisualColor;
            UpdateAmountText();

            gameObject.SetActive(true);
            // Reset & play animate-in
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (card != null) card.localScale = Vector3.one * 0.85f;
            if (canvasGroup != null) canvasGroup.DOFade(1f, 0.18f);
            if (card != null) card.DOScale(1f, 0.22f).SetEase(Ease.OutBack);

            AudioManager.Instance?.PlaySfx("sfx_button");
        }

        public void Close()
        {
            AudioManager.Instance?.PlaySfx("sfx_back");
            _activeBottle = null;
            _onConfirm = null;
            if (canvasGroup != null) canvasGroup.DOFade(0f, 0.12f);
            if (card != null) card.DOScale(0.85f, 0.12f).OnComplete(() => gameObject.SetActive(false));
            else gameObject.SetActive(false);
        }

        private void OnSliderChanged(float v) => UpdateAmountText();

        private void UpdateAmountText()
        {
            if (amountText == null || slider == null) return;
            amountText.text = $"{slider.value:F0} mL  /  {(_activeBottle != null ? _activeBottle.CurrentAmount.ToString("F0") : "0")} mL";
        }

        private void OnConfirm()
        {
            if (_activeBottle == null || slider == null || _onConfirm == null) { Close(); return; }
            var amt = Mathf.Min(slider.value, _activeBottle.CurrentAmount);
            AudioManager.Instance?.PlaySfx("sfx_pour");
            _onConfirm.Invoke(amt);
            Close();
        }
    }
}
