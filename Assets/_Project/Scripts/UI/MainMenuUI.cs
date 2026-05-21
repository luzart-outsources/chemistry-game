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
    public class MainMenuUI : UIBase
    {
        [Header("Refs")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TMP_Text totalStarsText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text versionText;
        [SerializeField] private GameObject diplomaBadge;
        [SerializeField] private CanvasGroup canvasGroup;

        public override UniTask OnCreateAsync(UIContext ctx, CancellationToken ct)
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlay);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
            if (versionText != null) versionText.text = "v0.1";
            return UniTask.CompletedTask;
        }

        public override UniTask OnBeforeShowAsync(UIContext ctx, CancellationToken ct)
        {
            if (totalStarsText != null)
                totalStarsText.text = $"{SaveSystem.Current.TotalStars()} / 15 sao";
            if (diplomaBadge != null) diplomaBadge.SetActive(SaveSystem.Current.DiplomaUnlocked);
            AudioManager.Instance?.PlayMusic("music_menu");
            return UniTask.CompletedTask;
        }

        public override async UniTask AnimateShowAsync(bool instant, CancellationToken ct)
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 0f;
            if (instant) { canvasGroup.alpha = 1f; return; }
            await canvasGroup.DOFade(1f, 0.35f).SetEase(Ease.OutQuad).AwaitAsync(ct);
        }

        public override async UniTask AnimateHideAsync(bool instant, CancellationToken ct)
        {
            if (canvasGroup == null) return;
            if (instant) { canvasGroup.alpha = 0f; return; }
            await canvasGroup.DOFade(0f, 0.25f).AwaitAsync(ct);
        }

        private async void OnPlay()
        {
            AudioManager.Instance?.PlaySfx("sfx_button");
            await UIManager.Instance.HideAsync(Id);
            await UIManager.Instance.ShowAsync(UIId.CG_LevelSelect);
        }

        private async void OnSettings()
        {
            AudioManager.Instance?.PlaySfx("sfx_button");
            await UIManager.Instance.ShowAsync(UIId.CG_SettingsPopup);
        }

        private void OnQuit()
        {
            AudioManager.Instance?.PlaySfx("sfx_button");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
