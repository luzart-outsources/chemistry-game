using System.Collections.Generic;
using ChemistryGame.Chemistry;
using UnityEngine;

namespace ChemistryGame.Core
{
    /// <summary>Singleton lưu state cross-scene: level đang chơi, các LevelConfig.</summary>
    [DefaultExecutionOrder(-90)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Levels (theo thứ tự)")]
        [SerializeField] private List<LevelConfig> levels = new List<LevelConfig>();

        public IReadOnlyList<LevelConfig> Levels => levels;
        public LevelConfig CurrentLevel { get; private set; }
        public int CurrentLevelIndex { get; private set; } = 1;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public LevelConfig GetLevel(int index)
        {
            foreach (var l in levels)
                if (l != null && l.LevelIndex == index) return l;
            return null;
        }

        public void SetCurrentLevel(int index)
        {
            CurrentLevelIndex = index;
            CurrentLevel = GetLevel(index);
        }

        public bool HasNextLevel() => GetLevel(CurrentLevelIndex + 1) != null;

        public void AdvanceLevel()
        {
            if (HasNextLevel()) SetCurrentLevel(CurrentLevelIndex + 1);
        }
    }
}
