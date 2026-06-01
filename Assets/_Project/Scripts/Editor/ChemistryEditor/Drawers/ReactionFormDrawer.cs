using System;
using System.Text;
using ChemistryGame.Chemistry;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChemistryGame.EditorTools.ContentEditor
{
    public static class ReactionFormDrawer
    {
        /// <summary>
        /// openInlineCreate(prefilledId, onCreated): triggers the inline Substance creation overlay
        /// owned by ContentEditorWindow. onCreated receives the newly created substance.
        /// </summary>
        public static void Draw(
            SerializedObject so,
            ReactionRule r,
            IAssetIndexReadOnly idx,
            Action<string, Action<SubstanceData>> openInlineCreate,
            Action<Object> onNavigate)
        {
            if (so == null || r == null) return;
            so.Update();

            EditorGUILayout.LabelField($"Reaction — {r.name}", EditorStyles_Chemistry.BoldRichLabel);
            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(so.FindProperty("Id"));
            EditorGUILayout.PropertyField(so.FindProperty("Description"));
            EditorGUILayout.PropertyField(so.FindProperty("ReactionEquation"));
            if (GUILayout.Button("Auto-fill equation từ Inputs/Outputs", GUILayout.Height(20)))
            {
                so.FindProperty("ReactionEquation").stringValue = BuildEquation(r);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Inputs", EditorStyles.boldLabel);
            DrawStoichList(r.Inputs, idx, openInlineCreate, allowRemove: true);

            if (GUILayout.Button("+ Add Input", GUILayout.Width(120)))
            {
                Undo.RegisterCompleteObjectUndo(r, "Add Input");
                r.Inputs.Add(new ReactionStoich { Ratio = 1f });
                EditorUtility.SetDirty(r);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Outputs", EditorStyles.boldLabel);
            DrawStoichList(r.Outputs, idx, openInlineCreate, allowRemove: true);

            if (GUILayout.Button("+ Add Output", GUILayout.Width(120)))
            {
                Undo.RegisterCompleteObjectUndo(r, "Add Output");
                r.Outputs.Add(new ReactionStoich { Ratio = 1f });
                EditorUtility.SetDirty(r);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("Conditions"), includeChildren: true);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("FX / Behavior", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("PrimarySideEffect"));
            EditorGUILayout.PropertyField(so.FindProperty("FlashColor"));
            EditorGUILayout.PropertyField(so.FindProperty("LimitedByLowestInput"));
            EditorGUILayout.PropertyField(so.FindProperty("SlowReaction"));

            so.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Dùng ở Level", EditorStyles.boldLabel);
            var levels = idx.GetLevelsUsingReaction(r);
            if (levels.Count == 0)
                EditorGUILayout.LabelField("  (chưa được bật ở Level nào)", EditorStyles.miniLabel);
            else
                foreach (var l in levels)
                    if (GUILayout.Button($"  ▸ L{l.LevelIndex:D2} — {l.DisplayName}", EditorStyles_Chemistry.LinkLabelRich))
                        onNavigate?.Invoke(l);
        }

        private static void DrawStoichList(
            System.Collections.Generic.List<ReactionStoich> list,
            IAssetIndexReadOnly idx,
            Action<string, Action<SubstanceData>> openInlineCreate,
            bool allowRemove)
        {
            if (list == null) return;
            int removeAt = -1;
            for (int i = 0; i < list.Count; i++)
            {
                var st = list[i];
                if (st == null) { list[i] = new ReactionStoich { Ratio = 1f }; st = list[i]; }

                EditorGUILayout.BeginHorizontal();
                {
                    var label = st.Substance == null ? "<chưa chọn>" : (st.Substance.Id ?? st.Substance.name);
                    if (GUILayout.Button(label, EditorStyles.popup, GUILayout.MinWidth(120)))
                    {
                        int captureI = i;
                        var menu = new GenericMenu();
                        foreach (var sub in idx.GetAllSubstances())
                        {
                            if (sub == null) continue;
                            var capSub = sub;
                            menu.AddItem(new GUIContent(sub.Id ?? sub.name), st.Substance == sub, () => { list[captureI].Substance = capSub; });
                        }
                        if (openInlineCreate != null)
                        {
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("+ Tạo chất mới…"), false, () =>
                            {
                                openInlineCreate("", picked => { if (picked != null) list[captureI].Substance = picked; });
                            });
                        }
                        menu.ShowAsContext();
                    }

                    EditorGUILayout.LabelField("×", GUILayout.Width(10));
                    st.Ratio = EditorGUILayout.FloatField(st.Ratio, GUILayout.Width(50));

                    if (allowRemove && GUILayout.Button("X", GUILayout.Width(22)))
                        removeAt = i;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (removeAt >= 0) list.RemoveAt(removeAt);
        }

        private static string BuildEquation(ReactionRule r)
        {
            var lhs = JoinSide(r.Inputs);
            var rhs = JoinSide(r.Outputs);
            return $"{lhs} → {rhs}";
        }

        private static string JoinSide(System.Collections.Generic.List<ReactionStoich> list)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var st in list)
            {
                if (st == null || st.Substance == null) continue;
                if (!first) sb.Append(" + ");
                first = false;
                if (Mathf.Abs(st.Ratio - 1f) > 0.001f) sb.Append(st.Ratio % 1 == 0 ? ((int)st.Ratio).ToString() : st.Ratio.ToString("0.##"));
                sb.Append(string.IsNullOrEmpty(st.Substance.Formula) ? st.Substance.Id : st.Substance.Formula);
                if (st.Substance.Phase == SubstancePhase.Gas) sb.Append("↑");
                else if (st.Substance.Phase == SubstancePhase.Precipitate) sb.Append("↓");
            }
            return first ? "?" : sb.ToString();
        }
    }
}
