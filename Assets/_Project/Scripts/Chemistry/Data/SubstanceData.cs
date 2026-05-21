using UnityEngine;

namespace ChemistryGame.Chemistry
{
    /// <summary>1 chất hoá học. Data-only, không logic.</summary>
    [CreateAssetMenu(fileName = "Sub_", menuName = "ChemistryGame/Substance", order = 0)]
    public class SubstanceData : ScriptableObject
    {
        [Header("Identity")]
        public string Id;          // "HCl", "NaOH", "NaCl_aq" ...
        public string Formula;     // "HCl", "NaOH" ...
        public string DisplayName; // "Axit clohidric"

        [Header("Classification")]
        public SubstanceCategoryType Category = SubstanceCategoryType.Other;
        public SubstancePhase Phase = SubstancePhase.Liquid;

        [Header("Properties")]
        [Tooltip("pH gần đúng. <7 acid, 7 trung tính, >7 bazơ. Dùng để feedback quỳ tím.")]
        [Range(0f, 14f)] public float PH = 7f;
        public Color VisualColor = new Color(0.6f, 0.85f, 1f, 0.8f);

        [Header("Visual")]
        public Sprite IconSprite;
        public Sprite BottleSprite;

        [Header("Crystallization (optional)")]
        [Tooltip("Khi chất này là Aqueous và bị mất hết nước, kết tinh thành SubstanceData nào " +
                 "(thường là phiên bản Crystal/Solid). Để null nếu không thể kết tinh — " +
                 "burner sẽ không crystallize substance này.")]
        public SubstanceData CrystalForm;

        [Header("Description")]
        [TextArea(2, 4)] public string ShortDescription;

        // Convenience flags
        public bool IsAcid => Category == SubstanceCategoryType.Acid;
        public bool IsBase => Category == SubstanceCategoryType.Base;
        public bool IsGas => Phase == SubstancePhase.Gas;
        public bool IsSolid => Phase == SubstancePhase.Solid || Phase == SubstancePhase.Crystal;
        public bool IsPrecipitate => Phase == SubstancePhase.Precipitate;

        public override string ToString() => string.IsNullOrEmpty(Id) ? name : Id;
    }
}
