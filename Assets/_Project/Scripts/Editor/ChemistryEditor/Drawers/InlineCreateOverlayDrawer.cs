using ChemistryGame.Chemistry;
using UnityEditor;
using UnityEngine;

namespace ChemistryGame.EditorTools.ContentEditor
{
    public static class InlineCreateOverlayDrawer
    {
        /// <summary>
        /// Draws a modal overlay over the window. Returns Save / Cancel actions via out flags.
        /// `draft` is an in-memory SubstanceData (not yet an asset) that we edit directly.
        /// </summary>
        public static void Draw(Rect windowRect, SubstanceData draft, out bool save, out bool cancel)
        {
            save = false; cancel = false;
            if (draft == null) return;

            // Dim background.
            EditorGUI.DrawRect(windowRect, new Color(0f, 0f, 0f, 0.55f));

            // Centered panel.
            const float w = 420, h = 380;
            var panel = new Rect(windowRect.x + (windowRect.width - w) / 2f, windowRect.y + (windowRect.height - h) / 2f, w, h);
            EditorGUI.DrawRect(panel, new Color(0.22f, 0.22f, 0.22f, 1f));

            GUILayout.BeginArea(panel);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("  Tạo chất mới (inline)", EditorStyles_Chemistry.BoldRichLabel);
            EditorGUILayout.Space(6);

            EditorGUILayout.BeginVertical(GUILayout.Width(w - 20));
            EditorGUI.indentLevel++;

            draft.Id          = EditorGUILayout.TextField("Id",          draft.Id);
            draft.Formula     = EditorGUILayout.TextField("Formula",     draft.Formula);
            draft.DisplayName = EditorGUILayout.TextField("Display Name", draft.DisplayName);
            draft.Category    = (SubstanceCategoryType)EditorGUILayout.EnumPopup("Category", draft.Category);
            draft.Phase       = (SubstancePhase)EditorGUILayout.EnumPopup("Phase", draft.Phase);
            draft.PH          = EditorGUILayout.Slider("pH", draft.PH, 0f, 14f);
            draft.VisualColor = EditorGUILayout.ColorField("Color", draft.VisualColor);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Preview", EditorStyles.miniBoldLabel);
            var prevRect = GUILayoutUtility.GetRect(100, 110, GUILayout.Width(100));
            BottlePreviewDrawer.Draw(prevRect, draft);

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("Huỷ", GUILayout.Height(28)))   cancel = true;
            if (GUILayout.Button("Tạo & dùng", GUILayout.Height(28)))
            {
                if (string.IsNullOrWhiteSpace(draft.Id))
                    EditorUtility.DisplayDialog("Thiếu Id", "Hãy nhập Id cho chất.", "OK");
                else
                    save = true;
            }
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);
            GUILayout.EndArea();
        }
    }
}
