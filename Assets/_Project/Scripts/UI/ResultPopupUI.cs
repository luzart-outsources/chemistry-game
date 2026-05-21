using System.Threading;
using ChemistryGame.Chemistry;
using ChemistryGame.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Luzart;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame.UI
{
    public class ResultPopupData
    {
        public LevelConfig Level;
        public int Stars;
        public string Reason;
    }

    public class ResultPopupUI : UIBase<ResultPopupData>
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text reasonText;
        [SerializeField] private Image[] starImages; // 3
        [SerializeField] private Sprite starFilled;
        [SerializeField] private Sprite starEmpty;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private GameObject confettiPrefab;
        [SerializeField] private RectTransform card;
        [SerializeField] private CanvasGroup canvasGroup;

        public static async UniTask ShowAsync(LevelConfig lvl, int stars, string reason)
        {
            var data = new ResultPopupData { Level = lvl, Stars = stars, Reason = reason };
            await UIManager.Instance.ShowAsync(UIId.CG_ResultPopup, new UIContext(data));
        }

        public override UniTask OnCreateAsync(UIContext ctx, CancellationToken ct)
        {
            if (replayButton != null) replayButton.onClick.AddListener(OnReplay);
            if (nextButton != null) nextButton.onClick.AddListener(OnNext);
            if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OnLevelSelect);
            return UniTask.CompletedTask;
        }

        protected override UniTask OnBeforeShowAsync(ResultPopupData data, CancellationToken ct)
        {
            if (data == null) return UniTask.CompletedTask;
            if (titleText != null) titleText.text = $"Hoàn thành! Màn {data.Level?.LevelIndex}";
            if (reasonText != null) reasonText.text = data.Reason;
            if (starImages != null)
            {
                for (int i = 0; i < starImages.Length; i++)
                {
                    if (starImages[i] == null) continue;
                    starImages[i].sprite = i < data.Stars ? starFilled : starEmpty;
                }
            }
            // Disable next nếu là màn cuối
            if (nextButton != null) nextButton.gameObject.SetActive(GameManager.Instance != null && GameManager.Instance.HasNextLevel());
            return UniTask.CompletedTask;
        }

        public override async UniTask AnimateShowAsync(bool instant, CancellationToken ct)
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (card != null) card.localScale = Vector3.one * 0.7f;
            if (instant) { if (canvasGroup != null) canvasGroup.alpha = 1f; if (card != null) card.localScale = Vector3.one; return; }
            if (canvasGroup != null) canvasGroup.DOFade(1f, 0.25f);
            if (card != null) await card.DOScale(1f, 0.45f).SetEase(Ease.OutBack).AwaitAsync(ct);

            if (confettiPrefab != null)
            {
                var go = Instantiate(confettiPrefab, transform);
                go.transform.localPosition = Vector3.zero;
                Destroy(go, 4f);
            }
            AudioManager.Instance?.PlaySfx("sfx_star");
        }

        public override async UniTask AnimateHideAsync(bool instant, CancellationToken ct)
        {
            if (instant) return;
            if (canvasGroup != null) canvasGroup.DOFade(0f, 0.15f);
            if (card != null) await card.DOScale(0.7f, 0.15f).AwaitAsync(ct);
        }

        private async void OnReplay()
        {
            AudioManager.Instance?.PlaySfx("sfx_button");
            await UIManager.Instance.HideAsync(Id);
            ChemistryGame.Gameplay.GameplayEvents.RaiseRestart();
        }

        private async void OnNext()
        {
            AudioManager.Instance?.PlaySfx("sfx_button");
            GameManager.Instance.AdvanceLevel();
            await UIManager.Instance.HideAsync(Id);
            // Trigger HUD reload
            await UIManager.Instance.HideAsync(UIId.CG_Gameplay);
            await UIManager.Instance.ShowAsync(UIId.CG_Gameplay);
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
