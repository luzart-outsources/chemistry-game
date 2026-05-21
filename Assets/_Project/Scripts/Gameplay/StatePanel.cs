using ChemistryGame.Chemistry;
using TMPro;
using UnityEngine;

namespace ChemistryGame.Gameplay
{
    /// <summary>Bên phải HUD: hiển thị pH, nhiệt độ, lượng, log actions.</summary>
    public class StatePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text phText;
        [SerializeField] private TMP_Text tempText;
        [SerializeField] private TMP_Text contentsText;
        [SerializeField] private TMP_Text logText;
        [SerializeField] private int maxLogLines = 6;

        public void Render(WorkspaceState s)
        {
            if (s == null) return;
            if (phText != null) phText.text = $"pH: {s.CalculatePH():F1}";
            if (tempText != null) tempText.text = $"T°: {s.Temperature:F0}°C";

            if (contentsText != null)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var kv in s.Contents)
                {
                    if (kv.Value.IsZero) continue;
                    sb.AppendLine($"{kv.Key.Formula}: {kv.Value.Value:F1}");
                }
                contentsText.text = sb.Length == 0 ? "(rỗng)" : sb.ToString();
            }

            if (logText != null)
            {
                var sb = new System.Text.StringBuilder();
                int total = s.History.Count;
                int start = Mathf.Max(0, total - maxLogLines);
                for (int i = start; i < total; i++)
                    sb.AppendLine($"• {s.History[i].Action} {s.History[i].Subject?.Formula}");
                logText.text = sb.ToString();
            }
        }
    }
}
