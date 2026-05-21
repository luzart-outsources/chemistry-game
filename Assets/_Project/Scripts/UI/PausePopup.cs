using System.Threading;
using ChemistryGame.Core;
using ChemistryGame.Gameplay;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Luzart;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame.UI
{
    public class PausePopup : UIBase
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private RectTransform card;
        [SerializeField] private CanvasGroup canvasGroup;

        public override UniTask OnCreateAsync(UIContext ctx, CancellationToken ct)
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResume);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
            if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OnLevelSelect);
            return UniTask.CompletedTask;
        }

        public override async UniTask AnimateShowAsync(bool instant, CancellationToken ct)
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (card != null) card.localScale = Vector3.one * 0.85f;
            if (instant) { if (canvasGroup != null) canvasGroup.alpha = 1f; if (card != null) card.localScale = Vector3.one; return; }
            if (canvasGroup != null) canvasGroup.DOFade(1f, 0.25f);
            if (card != null) await card.DOScale(1f, 0.3f).SetEase(Ease.OutBack).AwaitAsync(ct);
        }

        public override async UniTask AnimateHideAsync(bool instant, CancellationToken ct)
        {
            if (instant) return;
            if (canvasGroup != null) canvasGroup.DOFade(0f, 0.15f);
            if (card != null) await card.DOScale(0.85f, 0.15f).AwaitAsync(ct);
        }

        private async void OnResume()
        {
            AudioManager.Instance?.PlaySfx("sfx_button");
            await UIManager.Instance.HideAsync(Id);
        }

        private async void OnRestart()
        {
            AudioManager.Instance?.PlaySfx("sfx_back");
            GameplayEvents.RaiseRestart();
            await UIManager.Instance.HideAsync(Id);
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
