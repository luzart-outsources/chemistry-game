using ChemistryGame.Chemistry;
using UnityEditor;
using UnityEngine;

namespace ChemistryGame.EditorTools.ContentEditor
{
    /// <summary>Vẽ giả lập 1 ống nghiệm theo Phase + VisualColor.</summary>
    public static class BottlePreviewDrawer
    {
        public static void Draw(Rect r, SubstanceData s)
        {
            if (s == null) return;

            // Background
            EditorGUI.DrawRect(r, new Color(0.18f, 0.18f, 0.18f, 1f));

            // Bottle outline (rect with thin border).
            var border = new Color(0.6f, 0.6f, 0.6f, 1f);
            DrawBorder(r, border, 1f);

            // Formula label at top.
            var labelRect = new Rect(r.x, r.y + 2, r.width, 18);
            GUI.Label(labelRect, $"<b>{s.Formula}</b>", EditorStyles_Chemistry.BoldRichLabel);

            // Inner liquid area (below label).
            var inner = new Rect(r.x + 4, r.y + 22, r.width - 8, r.height - 26);
            DrawByPhase(inner, s);
        }

        private static void DrawByPhase(Rect inner, SubstanceData s)
        {
            var color = s.VisualColor;
            switch (s.Phase)
            {
                case SubstancePhase.Liquid:
                case SubstancePhase.Aqueous:
                {
                    var fillH = inner.height * 0.7f;
                    var fillRect = new Rect(inner.x, inner.yMax - fillH, inner.width, fillH);
                    EditorGUI.DrawRect(fillRect, color);
                    break;
                }
                case SubstancePhase.Solid:
                case SubstancePhase.Crystal:
                {
                    var bottom = new Rect(inner.x, inner.yMax - inner.height * 0.3f, inner.width, inner.height * 0.3f);
                    int blocks = 6;
                    float bw = bottom.width / blocks;
                    for (int i = 0; i < blocks; i++)
                    {
                        var alt = (i % 2 == 0) ? color : new Color(color.r * 0.85f, color.g * 0.85f, color.b * 0.85f, color.a);
                        EditorGUI.DrawRect(new Rect(bottom.x + i * bw, bottom.y, bw - 1, bottom.height), alt);
                    }
                    break;
                }
                case SubstancePhase.Precipitate:
                {
                    // Translucent top, increasingly opaque toward bottom.
                    float fillH = inner.height * 0.7f;
                    int bands = 4;
                    for (int i = 0; i < bands; i++)
                    {
                        float t = (i + 1) / (float)bands;
                        var c = new Color(color.r, color.g, color.b, Mathf.Lerp(0.15f, 0.95f, t));
                        var bandRect = new Rect(inner.x, inner.yMax - fillH * (1 - i / (float)bands), inner.width, fillH / bands);
                        EditorGUI.DrawRect(bandRect, c);
                    }
                    break;
                }
                case SubstancePhase.Gas:
                {
                    var faint = new Color(color.r, color.g, color.b, 0.18f);
                    EditorGUI.DrawRect(inner, faint);
                    // Bubbles.
                    for (int i = 0; i < 5; i++)
                    {
                        float bx = inner.x + (i + 0.5f) * (inner.width / 6f);
                        float by = inner.y + (i * 7f % inner.height);
                        EditorGUI.DrawRect(new Rect(bx - 3, by, 6, 6), new Color(color.r, color.g, color.b, 0.55f));
                    }
                    break;
                }
            }
        }

        private static void DrawBorder(Rect r, Color c, float thickness)
        {
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, thickness), c);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - thickness, r.width, thickness), c);
            EditorGUI.DrawRect(new Rect(r.x, r.y, thickness, r.height), c);
            EditorGUI.DrawRect(new Rect(r.xMax - thickness, r.y, thickness, r.height), c);
        }
    }
}
