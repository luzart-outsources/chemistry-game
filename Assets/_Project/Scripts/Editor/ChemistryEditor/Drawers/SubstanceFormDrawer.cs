using System;
using ChemistryGame.Chemistry;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChemistryGame.EditorTools.ContentEditor
{
    public static class SubstanceFormDrawer
    {
        public static void Draw(SerializedObject so, SubstanceData s, IAssetIndexReadOnly idx, Action<Object> onNavigate)
        {
            if (so == null || s == null) return;
            so.Update();

            EditorGUILayout.LabelField($"Substance — {s.name}", EditorStyles_Chemistry.BoldRichLabel);
            EditorGUILayout.Space(4);

            // ---- Identity ----
            EditorGUILayout.PropertyField(so.FindProperty("Id"));
            EditorGUILayout.PropertyField(so.FindProperty("Formula"));
            EditorGUILayout.PropertyField(so.FindProperty("DisplayName"));

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Classification", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("Category"));
            EditorGUILayout.PropertyField(so.FindProperty("Phase"));

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Properties", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("PH"));
            EditorGUILayout.PropertyField(so.FindProperty("VisualColor"));

            // ---- Live preview ----
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Preview (như trong game)", EditorStyles.miniBoldLabel);
            var previewRect = GUILayoutUtility.GetRect(120, 140, GUILayout.ExpandWidth(false), GUILayout.Width(120));
            BottlePreviewDrawer.Draw(previewRect, s);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Visual Assets", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("IconSprite"));
            EditorGUILayout.PropertyField(so.FindProperty("BottleSprite"));

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Crystallization", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("CrystalForm"));

            EditorGUILayout.Space(6);
            EditorGUILayout.PropertyField(so.FindProperty("ShortDescription"));

            so.ApplyModifiedProperties();

            // ---- Cross references ----
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Phản ứng liên quan", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("  AS INPUT (chất phản ứng):", EditorStyles.miniBoldLabel);
            DrawReactionLinks(idx.GetReactionsUsingInput(s), onNavigate);

            EditorGUILayout.LabelField("  AS OUTPUT (sản phẩm tạo ra chất này):", EditorStyles.miniBoldLabel);
            DrawReactionLinks(idx.GetReactionsProducing(s), onNavigate);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Dùng trong Level", EditorStyles.boldLabel);
            var levels = idx.GetLevelsUsingSubstance(s);
            if (levels.Count == 0)
                EditorGUILayout.LabelField("  (chưa dùng trong level nào)", EditorStyles.miniLabel);
            else
                foreach (var l in levels)
                    if (GUILayout.Button($"  ▸ L{l.LevelIndex:D2} — {l.DisplayName}", EditorStyles_Chemistry.LinkLabelRich))
                        onNavigate?.Invoke(l);
        }

        private static void DrawReactionLinks(System.Collections.Generic.IReadOnlyList<ReactionRule> list, Action<Object> onNavigate)
        {
            if (list == null || list.Count == 0)
            {
                EditorGUILayout.LabelField("    (không có)", EditorStyles.miniLabel);
                return;
            }
            foreach (var r in list)
            {
                if (r == null) continue;
                var label = string.IsNullOrEmpty(r.ReactionEquation) ? (r.Id ?? r.name) : r.ReactionEquation;
                if (GUILayout.Button($"  ▸ {label}", EditorStyles_Chemistry.LinkLabelRich))
                    onNavigate?.Invoke(r);
            }
        }
    }
}
