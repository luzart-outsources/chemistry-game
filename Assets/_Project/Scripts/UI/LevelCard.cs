using System;
using ChemistryGame.Chemistry;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame.UI
{
    public class LevelCard : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private Image[] stars; // 3 elements
        [SerializeField] private Sprite starFilled;
        [SerializeField] private Sprite starEmpty;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private Button button;

        public void Bind(LevelConfig cfg, int starsEarned, bool unlocked, Action<LevelConfig> onClick)
        {
            if (levelLabel != null) levelLabel.text = $"Màn {cfg.LevelIndex}";
            if (objectiveText != null) objectiveText.text = cfg.DisplayName;
            if (lockedOverlay != null) lockedOverlay.SetActive(!unlocked);
            if (stars != null)
            {
                for (int i = 0; i < stars.Length; i++)
                {
                    if (stars[i] == null) continue;
                    stars[i].sprite = i < starsEarned ? starFilled : starEmpty;
                }
            }
            if (button != null)
            {
                button.interactable = unlocked;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick?.Invoke(cfg));
            }
        }
    }
}
