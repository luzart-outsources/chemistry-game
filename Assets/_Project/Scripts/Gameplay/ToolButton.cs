using ChemistryGame.Chemistry;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame.Gameplay
{
    public class ToolButton : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button button;

        public ToolData Tool { get; private set; }

        public void Bind(ToolData t)
        {
            Tool = t;
            if (icon != null && t != null && t.IconSprite != null) icon.sprite = t.IconSprite;
            if (label != null) label.text = t != null ? t.DisplayName : "?";
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClick);
            }
        }

        private void OnClick()
        {
            if (Tool == null) return;
            GameplayEvents.RaiseToolUsed(Tool.FunctionType, Tool.AssociatedSubstance);
        }
    }
}
