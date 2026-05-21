using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChemistryGame.Chemistry
{
    [Serializable]
    public class ReactionStoich
    {
        public SubstanceData Substance;
        [Min(0.01f)] public float Ratio = 1f; // tỉ lệ mol (đơn giản hoá)
    }

    [Serializable]
    public class ReactionCondition
    {
        public ReactionConditionType Type = ReactionConditionType.None;
        [Tooltip("Số lượng hoặc cường độ điều kiện (vd: nhiệt độ tối thiểu, lượng khí cần sục)")]
        public float Value = 0f;
    }

    /// <summary>Luật phản ứng: A + B (+ condition) → C + D + sideFx.</summary>
    [CreateAssetMenu(fileName = "Rx_", menuName = "ChemistryGame/Reaction Rule", order = 1)]
    public class ReactionRule : ScriptableObject
    {
        [Header("Identity")]
        public string Id;
        [TextArea] public string Description;

        [Header("Inputs (đặt 1-3 reactants)")]
        public List<ReactionStoich> Inputs = new List<ReactionStoich>();

        [Header("Conditions")]
        public List<ReactionCondition> Conditions = new List<ReactionCondition>();

        [Header("Outputs")]
        public List<ReactionStoich> Outputs = new List<ReactionStoich>();

        [Header("FX / Feedback")]
        public SideEffectType PrimarySideEffect = SideEffectType.None;
        public Color FlashColor = Color.white;
        [TextArea] public string ReactionEquation;

        [Header("Behavior")]
        [Tooltip("Phản ứng tiêu thụ HẾT chất ít hơn (limiting reagent) rồi dừng.")]
        public bool LimitedByLowestInput = true;

        [Tooltip("Phản ứng chậm — tạo ra cảnh báo và tốc độ phản ứng giảm")]
        public bool SlowReaction = false;

        public override string ToString() => string.IsNullOrEmpty(Id) ? name : Id;
    }
}
