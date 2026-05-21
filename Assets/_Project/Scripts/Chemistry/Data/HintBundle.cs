using UnityEngine;

namespace ChemistryGame.Chemistry
{
    [CreateAssetMenu(fileName = "Hint_", menuName = "ChemistryGame/Hint Bundle", order = 3)]
    public class HintBundle : ScriptableObject
    {
        [TextArea(2, 4)] public string Tier1_Reagent;  // Gợi ý thuốc thử
        [TextArea(2, 4)] public string Tier2_Order;    // Gợi ý thứ tự
        [TextArea(2, 4)] public string Tier3_Formula;  // Công thức cụ thể

        public string GetHint(int tier)
        {
            switch (tier)
            {
                case 1: return Tier1_Reagent;
                case 2: return Tier2_Order;
                case 3: return Tier3_Formula;
                default: return string.Empty;
            }
        }
    }
}
