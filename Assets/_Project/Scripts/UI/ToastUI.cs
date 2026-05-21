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
    /// <summary>Simple toast popup. Auto-hides after data.DurationSeconds.</summary>
    public class ToastUI : UIBase<ToastData>
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Image background;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform card;

        protected override UniTask OnBeforeShowAsync(ToastData data, CancellationToken ct)
        {
            if (data == null) return UniTask.CompletedTask;
            if (messageText != null) messageText.text = data.Message;
            if (background != null)
            {
                switch (data.Style)
                {
                    case ToastStyle.Success: background.color = new Color(0.32f, 0.7f, 0.4f, 0.95f); break;
                    case ToastStyle.Warning: background.color = new Color(0.95f, 0.7f, 0.3f, 0.95f); break;
                    case ToastStyle.Error:   background.color = new Color(0.85f, 0.35f, 0.35f, 0.95f); break;
                    default:                 background.color = new Color(0.2f, 0.3f, 0.45f, 0.95f); break;
                }
            }
            return UniTask.CompletedTask;
        }

        protected override UniTask OnShownAsync(ToastData data, CancellationToken ct)
        {
            float dur = data?.DurationSeconds ?? 2f;
            // Fire-and-forget delay → hide. Don't block OnShown returning,
            // otherwise UIManager.ShowInternalAsync stays in "Showing" state forever.
            AutoHideAsync(dur, ct).Forget();
            return UniTask.CompletedTask;
        }

        private async UniTask AutoHideAsync(float dur, CancellationToken ct)
        {
            try
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(dur), cancellationToken: ct);
                if (UIManager.Instance != null && IsVisible)
                    await UIManager.Instance.HideAsync(Id);
            }
            catch (System.OperationCanceledException) { }
        }

        public override async UniTask AnimateShowAsync(bool instant, CancellationToken ct)
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (card != null) card.localScale = Vector3.one * 0.9f;
            if (instant) { if (canvasGroup != null) canvasGroup.alpha = 1f; if (card != null) card.localScale = Vector3.one; return; }
            if (canvasGroup != null) canvasGroup.DOFade(1f, 0.18f);
            if (card != null) await card.DOScale(1f, 0.18f).SetEase(Ease.OutBack).AwaitAsync(ct);
        }

        public override async UniTask AnimateHideAsync(bool instant, CancellationToken ct)
        {
            if (instant) return;
            if (canvasGroup != null) canvasGroup.DOFade(0f, 0.18f);
            if (card != null) await card.DOScale(0.9f, 0.18f).AwaitAsync(ct);
        }
    }
}
