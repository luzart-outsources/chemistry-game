using System;
using System.Collections.Generic;

namespace ChemistryGame.Core
{
    [Serializable]
    public class LevelRecord
    {
        public int Stars;
        public bool Completed;
        public int Attempts;
        public int HintsUsedEver;
    }

    [Serializable]
    public class GameSettings
    {
        public float MusicVol = 0.7f;
        public float SfxVol = 0.85f;
        public string Lang = "vi";
    }

    [Serializable]
    public class PlayerData
    {
        public int Version = 1;
        public Dictionary<int, LevelRecord> Levels = new Dictionary<int, LevelRecord>();
        public GameSettings Settings = new GameSettings();
        public bool DiplomaUnlocked;

        public LevelRecord GetOrCreate(int levelIndex)
        {
            if (!Levels.TryGetValue(levelIndex, out var rec))
            {
                rec = new LevelRecord();
                Levels[levelIndex] = rec;
            }
            return rec;
        }

        public int TotalStars()
        {
            int t = 0;
            foreach (var kv in Levels) t += kv.Value.Stars;
            return t;
        }

        public bool IsLevelUnlocked(int levelIndex)
        {
            if (levelIndex <= 1) return true;
            return Levels.TryGetValue(levelIndex - 1, out var prev) && prev.Stars >= 1;
        }
    }
}
