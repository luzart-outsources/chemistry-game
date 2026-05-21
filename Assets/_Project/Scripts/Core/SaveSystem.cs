using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChemistryGame.Core
{
    public static class SaveSystem
    {
        private const string KEY = "OngNghiem_PlayerData";

        public static PlayerData Current { get; private set; } = new PlayerData();

        [Serializable]
        private class SerializableEnvelope
        {
            public int Version;
            public List<int> LevelIds = new List<int>();
            public List<LevelRecord> LevelRecords = new List<LevelRecord>();
            public GameSettings Settings = new GameSettings();
            public bool DiplomaUnlocked;
        }

        public static void Load()
        {
            if (!PlayerPrefs.HasKey(KEY))
            {
                Current = new PlayerData();
                return;
            }
            try
            {
                var json = PlayerPrefs.GetString(KEY);
                var env = JsonUtility.FromJson<SerializableEnvelope>(json);
                if (env == null) { Current = new PlayerData(); return; }
                var pd = new PlayerData
                {
                    Version = env.Version,
                    Settings = env.Settings ?? new GameSettings(),
                    DiplomaUnlocked = env.DiplomaUnlocked
                };
                for (int i = 0; i < env.LevelIds.Count && i < env.LevelRecords.Count; i++)
                    pd.Levels[env.LevelIds[i]] = env.LevelRecords[i];
                Current = pd;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Load failed: {e.Message}. Reset.");
                Current = new PlayerData();
            }
        }

        public static void Save()
        {
            var env = new SerializableEnvelope
            {
                Version = Current.Version,
                Settings = Current.Settings,
                DiplomaUnlocked = Current.DiplomaUnlocked
            };
            foreach (var kv in Current.Levels)
            {
                env.LevelIds.Add(kv.Key);
                env.LevelRecords.Add(kv.Value);
            }
            var json = JsonUtility.ToJson(env);
            PlayerPrefs.SetString(KEY, json);
            PlayerPrefs.Save();
        }

        public static void ReportLevelResult(int levelIndex, int stars, bool usedHint)
        {
            var rec = Current.GetOrCreate(levelIndex);
            rec.Attempts++;
            if (usedHint) rec.HintsUsedEver++;
            if (stars > rec.Stars) rec.Stars = stars;
            if (stars >= 1) rec.Completed = true;

            // Check diploma
            if (!Current.DiplomaUnlocked && Current.TotalStars() >= 15)
                Current.DiplomaUnlocked = true;

            Save();
        }
    }
}
