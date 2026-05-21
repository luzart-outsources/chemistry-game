using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChemistryGame.Chemistry
{
    [Serializable]
    public class BottleSpawn
    {
        public SubstanceData Substance;
        [Min(0f)] public float InitialAmount = 50f;
        [Tooltip("Hiển thị label hay là 'Lọ X' (mất nhãn)")]
        public bool MaskLabel = false;
        public string MaskedLabel = "?";
    }

    [Serializable]
    public class ToolSpawn
    {
        public ToolData Tool;
    }

    [CreateAssetMenu(fileName = "Level_", menuName = "ChemistryGame/Level Config", order = 4)]
    public class LevelConfig : ScriptableObject
    {
        [Header("Identity")]
        public int LevelIndex = 1;
        public string DisplayName;
        [TextArea(2, 3)] public string ObjectiveText;

        [Header("Available substances")]
        public List<BottleSpawn> Bottles = new List<BottleSpawn>();

        [Header("Available tools")]
        public List<ToolSpawn> Tools = new List<ToolSpawn>();

        [Header("Reactions enabled in this level")]
        [Tooltip("Subset của ReactionRule mà ChemistryEngine sẽ match.")]
        public List<ReactionRule> AvailableReactions = new List<ReactionRule>();

        [Header("Win / Purity")]
        public PurityRule PurityRule = new PurityRule();

        [Header("Traps (để giải thích khi sai)")]
        public List<TrapDefinition> Traps = new List<TrapDefinition>();

        [Header("Hints")]
        public HintBundle Hints;

        [Header("3⭐ extra conditions")]
        [Tooltip("Khoá star=3: phải KHÔNG kích hoạt trap nào trong list này.")]
        public List<TrapDefinition> ThreeStarBlockingTraps = new List<TrapDefinition>();
    }
}
