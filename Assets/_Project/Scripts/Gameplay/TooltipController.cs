using ChemistryGame.Chemistry;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame.Gameplay
{
    /// <summary>
    /// Global tooltip singleton — hover bottle/tool shows substance info.
    /// </summary>
    public class TooltipController : MonoBehaviour
    {
        public static TooltipController Instance { get; private set; }

        [SerializeField] private RectTransform panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Awake()
        {
            Instance = this;
            Hide();
        }

        public void Show(string title, string body, Vector2 screenPos)
        {
            if (panel == null) return;
            if (titleText != null) titleText.text = title;
            if (bodyText != null) bodyText.text = body;
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            panel.position = screenPos + new Vector2(20, 20);
            panel.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (panel != null) panel.gameObject.SetActive(false);
        }
    }
}
