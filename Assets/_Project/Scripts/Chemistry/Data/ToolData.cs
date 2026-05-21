using UnityEngine;

namespace ChemistryGame.Chemistry
{
    [CreateAssetMenu(fileName = "Tool_", menuName = "ChemistryGame/Tool", order = 2)]
    public class ToolData : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public ToolFunctionType FunctionType;
        public Sprite IconSprite;

        [Header("Effects (tuỳ tool)")]
        [Tooltip("Khi tool là Bubbler: chất khí sục vào")]
        public SubstanceData AssociatedSubstance;

        [TextArea] public string ShortDescription;

        public override string ToString() => string.IsNullOrEmpty(Id) ? name : Id;
    }
}
