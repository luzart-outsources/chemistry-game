using System.Threading;
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
    public class HintPopupUI : UIBase
    {
        [SerializeField] private Button[] tierButtons;          // 3
        [SerializeField] private TMP_Text[] tierLabels;         // 3
        [SerializeField] private TMP_Text revealedText;
        [SerializeField] private Button closeButton;
        [SerializeField] private RectTransform card;
        [SerializeField] private CanvasGroup canvasGroup;

        public override UniTask OnCreateAsync(UIContext ctx, CancellationToken ct)
        {
            if (closeButton != null) closeButton.onClick.AddListener(OnClose);
            if (tierButtons != null)
            {
                for (int i = 0; i < tierButtons.Length; i++)
                {
                    int tier = i + 1;
                    if (tierButtons[i] == null) continue;
                    tierButtons[i].onClick.RemoveAllListeners();
                    tierButtons[i].onClick.AddListener(() => OnTier(tier));
                }
            }
            return UniTask.CompletedTask;
        }

        public override UniTask OnBeforeShowAsync(UIContext ctx, CancellationToken ct)
        {
            if (revealedText != null) revealedText.text = "Chọn mức gợi ý (càng cao càng tốn sao):";
            return UniTask.CompletedTask;
        }

        public override async UniTask AnimateShowAsync(bool instant, CancellationToken ct)
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (card != null) card.localScale = Vector3.one * 0.85f;
            if (instant) { if (canvasGroup != null) canvasGroup.alpha = 1f; if (card != null) card.localScale = Vector3.one; return; }
            if (canvasGroup != null) canvasGroup.DOFade(1f, 0.2f);
            if (card != null) await card.DOScale(1f, 0.3f).SetEase(Ease.OutBack).AwaitAsync(ct);
        }

        public override async UniTask AnimateHideAsync(bool instant, CancellationToken ct)
        {
            if (instant) return;
            if (canvasGroup != null) canvasGroup.DOFade(0f, 0.12f);
            if (card != null) await card.DOScale(0.85f, 0.12f).AwaitAsync(ct);
        }

        private void OnTier(int tier)
        {
            AudioManager.Instance?.PlaySfx("sfx_button");
            var hint = GameManager.Instance?.CurrentLevel?.Hints?.GetHint(tier);
            if (revealedText != null) revealedText.text = string.IsNullOrEmpty(hint) ? "(Chưa có gợi ý)" : $"→{hint}";
            GameplayEvents.RaiseHint(tier);
        }

        private async void OnClose()
        {
            AudioManager.Instance?.PlaySfx("sfx_back");
            await UIManager.Instance.HideAsync(Id);
        }
    }
}
