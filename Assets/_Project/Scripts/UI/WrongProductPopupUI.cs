using System.Threading;
using ChemistryGame.Chemistry;
using ChemistryGame.Core;
using ChemistryGame.Gameplay;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Luzart;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame.UI
{
    public class WrongProductPopupData
    {
        public LevelConfig Level;
        public string Reason;
        public SubstanceData WrongProduct;
        public SubstanceData Impurity;
    }

    public class WrongProductPopupUI : UIBase<WrongProductPopupData>
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text reasonText;
        [SerializeField] private TMP_Text wrongProductText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private RectTransform card;
        [SerializeField] private CanvasGroup canvasGroup;

        public static async UniTask ShowAsync(LevelConfig lvl, string reason, SubstanceData wrong, SubstanceData impurity)
        {
            var data = new WrongProductPopupData { Level = lvl, Reason = reason, WrongProduct = wrong, Impurity = impurity };
            await UIManager.Instance.ShowAsync(UIId.CG_WrongProductPopup, new UIContext(data));
        }

        public override UniTask OnCreateAsync(UIContext ctx, CancellationToken ct)
        {
            if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
            if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OnLevelSelect);
            return UniTask.CompletedTask;
        }

        protected override UniTask OnBeforeShowAsync(WrongProductPopupData data, CancellationToken ct)
        {
            if (titleText != null) titleText.text = "Sản phẩm chưa đạt";
            if (reasonText != null) reasonText.text = data?.Reason ?? "Hãy thử lại";
            if (wrongProductText != null)
            {
                if (data?.WrongProduct != null)
                    wrongProductText.text = $"Bạn tạo: {data.WrongProduct.Formula}";
                else if (data?.Impurity != null)
                    wrongProductText.text = $"Tạp: {data.Impurity.Formula}";
                else wrongProductText.text = string.Empty;
            }
            return UniTask.CompletedTask;
        }

        public override async UniTask AnimateShowAsync(bool instant, CancellationToken ct)
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (card != null) card.localScale = Vector3.one * 0.85f;
            if (instant) { if (canvasGroup != null) canvasGroup.alpha = 1f; if (card != null) card.localScale = Vector3.one; return; }
            if (canvasGroup != null) canvasGroup.DOFade(1f, 0.2f);
            if (card != null)
            {
                await card.DOScale(1.05f, 0.18f).AwaitAsync(ct);
                await card.DOScale(1f, 0.12f).AwaitAsync(ct);
                // shake to indicate fail
                card.DOShakePosition(0.4f, 8f, 14, 90f);
            }
        }

        public override async UniTask AnimateHideAsync(bool instant, CancellationToken ct)
        {
            if (instant) return;
            if (canvasGroup != null) canvasGroup.DOFade(0f, 0.12f);
            if (card != null) await card.DOScale(0.85f, 0.12f).AwaitAsync(ct);
        }

        private async void OnRetry()
        {
            AudioManager.Instance?.PlaySfx("sfx_back");
            await UIManager.Instance.HideAsync(Id);
            GameplayEvents.RaiseRestart();
        }

        private async void OnLevelSelect()
        {
            AudioManager.Instance?.PlaySfx("sfx_back");
            await UIManager.Instance.HideAsync(Id);
            await UIManager.Instance.HideAsync(UIId.CG_Gameplay);
            await UIManager.Instance.ShowAsync(UIId.CG_LevelSelect);
        }
    }
}
