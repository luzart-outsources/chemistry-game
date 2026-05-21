using System.Threading;
using ChemistryGame.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Luzart;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame.UI
{
    /// <summary>
    /// Simple tutorial overlay shown khi vào Level 1 lần đầu (HintsUsedEver==0 && Attempts==0).
    /// </summary>
    public class TutorialOverlay : UIBase
    {
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button gotItButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform card;

        public override UniTask OnCreateAsync(UIContext ctx, CancellationToken ct)
        {
            if (gotItButton != null) gotItButton.onClick.AddListener(OnGotIt);
            return UniTask.CompletedTask;
        }

        public override UniTask OnBeforeShowAsync(UIContext ctx, CancellationToken ct)
        {
            if (bodyText != null)
            {
                bodyText.text =
                    "<b>Cách chơi nhanh:</b>\n\n" +
                    "  1. <b>Kéo</b> các lọ ở bên trái thả vào <b>ống nghiệm chính</b>.\n" +
                    "  2. Dùng <b>dụng cụ</b> (quỳ tím, đèn khò...) ở giữa-dưới.\n" +
                    "  3. Theo dõi <b>màu dung dịch</b> và pH ở panel phải.\n" +
                    "  4. Bấm <b>Nộp bài</b> khi đã có sản phẩm đúng.\n\n" +
                    "Tip: sai cũng học được — bấm <b>Bắt đầu lại</b> để thử cách khác.";
            }
            return UniTask.CompletedTask;
        }

        public override async UniTask AnimateShowAsync(bool instant, CancellationToken ct)
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (card != null) card.localScale = Vector3.one * 0.85f;
            if (instant) { if (canvasGroup != null) canvasGroup.alpha = 1f; if (card != null) card.localScale = Vector3.one; return; }
            if (canvasGroup != null) canvasGroup.DOFade(1f, 0.3f);
            if (card != null) await card.DOScale(1f, 0.35f).SetEase(Ease.OutBack).AwaitAsync(ct);
        }

        public override async UniTask AnimateHideAsync(bool instant, CancellationToken ct)
        {
            if (instant) return;
            if (canvasGroup != null) canvasGroup.DOFade(0f, 0.18f);
            if (card != null) await card.DOScale(0.85f, 0.18f).AwaitAsync(ct);
        }

        private async void OnGotIt()
        {
            AudioManager.Instance?.PlaySfx("sfx_button");
            await UIManager.Instance.HideAsync(Id);
        }
    }
}
