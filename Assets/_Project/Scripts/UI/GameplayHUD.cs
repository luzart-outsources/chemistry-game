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
    public class GameplayHUD : UIBase
    {
        [Header("HUD elements")]
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button undoButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button hintButton;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Gameplay area")]
        [SerializeField] private GameplayController gameplayController;

        public override UniTask OnCreateAsync(UIContext ctx, CancellationToken ct)
        {
            if (pauseButton != null) pauseButton.onClick.AddListener(OnPause);
            if (submitButton != null) submitButton.onClick.AddListener(OnSubmit);
            if (undoButton != null) undoButton.onClick.AddListener(OnUndo);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
            if (hintButton != null) hintButton.onClick.AddListener(OnHint);
            return UniTask.CompletedTask;
        }

        public override UniTask OnBeforeShowAsync(UIContext ctx, CancellationToken ct)
        {
            var lvl = GameManager.Instance?.CurrentLevel;
            if (lvl == null && GameManager.Instance != null)
            {
                GameManager.Instance.SetCurrentLevel(1);
                lvl = GameManager.Instance.CurrentLevel;
            }
            if (lvl != null)
            {
                if (levelLabel != null) levelLabel.text = $"Màn {lvl.LevelIndex}";
                if (objectiveText != null) objectiveText.text = lvl.ObjectiveText;
                if (gameplayController != null) gameplayController.LoadLevel(lvl);
            }
            AudioManager.Instance?.PlayMusic("music_gameplay");
            MaybeShowTutorial();
            return UniTask.CompletedTask;
        }

        private void MaybeShowTutorial()
        {
            // Show tutorial on Level 1 first attempt
            var lvl = GameManager.Instance?.CurrentLevel;
            if (lvl == null || lvl.LevelIndex != 1) return;
            if (SaveSystem.Current.Levels.TryGetValue(1, out var rec) && rec.Attempts > 0) return;
            if (UIManager.Instance != null)
                UIManager.Instance.ShowAsync(UIId.CG_TutorialOverlay).Forget();
        }

        public override async UniTask AnimateShowAsync(bool instant, CancellationToken ct)
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 0f;
            if (instant) { canvasGroup.alpha = 1f; return; }
            await canvasGroup.DOFade(1f, 0.3f).AwaitAsync(ct);
        }

        public override async UniTask AnimateHideAsync(bool instant, CancellationToken ct)
        {
            if (canvasGroup == null) return;
            if (instant) { canvasGroup.alpha = 0f; return; }
            await canvasGroup.DOFade(0f, 0.2f).AwaitAsync(ct);
        }

        private async void OnPause()
        {
            AudioManager.Instance?.PlaySfx("sfx_button");
            await UIManager.Instance.ShowAsync(UIId.CG_PausePopup);
        }

        private void OnSubmit()
        {
            AudioManager.Instance?.PlaySfx("sfx_button");
            GameplayEvents.RaiseSubmit();
        }

        private void OnUndo()
        {
            AudioManager.Instance?.PlaySfx("sfx_back");
            GameplayEvents.RaiseUndo();
        }

        private void OnRestart()
        {
            AudioManager.Instance?.PlaySfx("sfx_back");
            GameplayEvents.RaiseRestart();
        }

        private async void OnHint()
        {
            AudioManager.Instance?.PlaySfx("sfx_button");
            await UIManager.Instance.ShowAsync(UIId.CG_HintPopup);
        }
    }
}
