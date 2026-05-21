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
    public class SettingsPopupUI : UIBase
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text musicLabel;
        [SerializeField] private TMP_Text sfxLabel;
        [SerializeField] private RectTransform card;
        [SerializeField] private CanvasGroup canvasGroup;

        public override UniTask OnCreateAsync(UIContext ctx, CancellationToken ct)
        {
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusic);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfx);
            if (closeButton != null) closeButton.onClick.AddListener(OnClose);
            return UniTask.CompletedTask;
        }

        public override UniTask OnBeforeShowAsync(UIContext ctx, CancellationToken ct)
        {
            var s = SaveSystem.Current.Settings;
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(s.MusicVol);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(s.SfxVol);
            RefreshLabels();
            return UniTask.CompletedTask;
        }

        public override async UniTask AnimateShowAsync(bool instant, CancellationToken ct)
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (card != null) card.localScale = Vector3.one * 0.85f;
            if (instant) { if (canvasGroup != null) canvasGroup.alpha = 1f; if (card != null) card.localScale = Vector3.one; return; }
            if (canvasGroup != null) canvasGroup.DOFade(1f, 0.2f);
            if (card != null) await card.DOScale(1f, 0.25f).SetEase(Ease.OutBack).AwaitAsync(ct);
        }

        public override async UniTask AnimateHideAsync(bool instant, CancellationToken ct)
        {
            if (instant) return;
            if (canvasGroup != null) canvasGroup.DOFade(0f, 0.12f);
            if (card != null) await card.DOScale(0.85f, 0.12f).AwaitAsync(ct);
        }

        private void OnMusic(float v)
        {
            AudioManager.Instance?.SetMusicVolume(v);
            SaveSystem.Save();
            RefreshLabels();
        }

        private void OnSfx(float v)
        {
            AudioManager.Instance?.SetSfxVolume(v);
            SaveSystem.Save();
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            if (musicLabel != null) musicLabel.text = $"Nhạc: {SaveSystem.Current.Settings.MusicVol * 100:F0}%";
            if (sfxLabel != null) sfxLabel.text = $"Âm: {SaveSystem.Current.Settings.SfxVol * 100:F0}%";
        }

        private async void OnClose()
        {
            AudioManager.Instance?.PlaySfx("sfx_button");
            await UIManager.Instance.HideAsync(Id);
        }
    }
}
