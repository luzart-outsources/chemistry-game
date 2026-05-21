using System.Collections.Generic;
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
    public class LevelSelectUI : UIBase
    {
        [SerializeField] private RectTransform cardsRoot;
        [SerializeField] private LevelCard cardPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text totalStarsText;
        [SerializeField] private CanvasGroup canvasGroup;

        private readonly List<LevelCard> _cards = new List<LevelCard>();

        public override UniTask OnCreateAsync(UIContext ctx, CancellationToken ct)
        {
            if (backButton != null) backButton.onClick.AddListener(OnBack);
            return UniTask.CompletedTask;
        }

        public override UniTask OnBeforeShowAsync(UIContext ctx, CancellationToken ct)
        {
            BuildCards();
            return UniTask.CompletedTask;
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
            await canvasGroup.DOFade(0f, 0.25f).AwaitAsync(ct);
        }

        private void BuildCards()
        {
            foreach (var c in _cards) if (c != null) Destroy(c.gameObject);
            _cards.Clear();
            if (GameManager.Instance == null || cardPrefab == null || cardsRoot == null) return;
            foreach (var lvl in GameManager.Instance.Levels)
            {
                if (lvl == null) continue;
                var card = Instantiate(cardPrefab, cardsRoot);
                var rec = SaveSystem.Current.Levels.TryGetValue(lvl.LevelIndex, out var r) ? r : null;
                bool unlocked = SaveSystem.Current.IsLevelUnlocked(lvl.LevelIndex);
                int stars = rec?.Stars ?? 0;
                card.Bind(lvl, stars, unlocked, OnCardClick);
                _cards.Add(card);
            }
            if (totalStarsText != null)
                totalStarsText.text = $"{SaveSystem.Current.TotalStars()} / 15 sao";
        }

        private async void OnCardClick(LevelConfig lvl)
        {
            if (!SaveSystem.Current.IsLevelUnlocked(lvl.LevelIndex)) return;
            AudioManager.Instance?.PlaySfx("sfx_button");
            GameManager.Instance.SetCurrentLevel(lvl.LevelIndex);
            await UIManager.Instance.HideAsync(Id);
            await UIManager.Instance.ShowAsync(UIId.CG_Gameplay);
        }

        private async void OnBack()
        {
            AudioManager.Instance?.PlaySfx("sfx_back");
            await UIManager.Instance.HideAsync(Id);
            await UIManager.Instance.ShowAsync(UIId.CG_MainMenu);
        }
    }
}
