using System;
using System.Collections.Generic;
using ChemistryGame.Chemistry;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChemistryGame.EditorTools.ContentEditor
{
    public static class LevelFormDrawer
    {
        public static void Draw(
            SerializedObject so,
            LevelConfig l,
            IAssetIndexReadOnly idx,
            Action<string, Action<SubstanceData>> openInlineCreate,
            Action<Object> onNavigate)
        {
            if (so == null || l == null) return;
            so.Update();

            EditorGUILayout.LabelField($"Level — {l.name}", EditorStyles_Chemistry.BoldRichLabel);
            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(so.FindProperty("LevelIndex"));
            EditorGUILayout.PropertyField(so.FindProperty("DisplayName"));
            EditorGUILayout.PropertyField(so.FindProperty("ObjectiveText"));

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Bottles (chất có sẵn)", EditorStyles.boldLabel);
            DrawBottlesList(l, idx, openInlineCreate);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Tools (công cụ)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("Tools"), includeChildren: true);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Available Reactions", EditorStyles.boldLabel);
            DrawReactionList(l, idx);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Purity Rule", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("PurityRule"), includeChildren: true);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Traps (giải thích khi sai)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("Traps"), includeChildren: true);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("3⭐ Blocking Traps", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("ThreeStarBlockingTraps"), includeChildren: true);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Hints", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("Hints"));

            so.ApplyModifiedProperties();
        }

        private static void DrawBottlesList(LevelConfig l, IAssetIndexReadOnly idx, Action<string, Action<SubstanceData>> openInlineCreate)
        {
            if (l.Bottles == null) l.Bottles = new List<BottleSpawn>();
            int removeAt = -1;
            for (int i = 0; i < l.Bottles.Count; i++)
            {
                var b = l.Bottles[i];
                if (b == null) { l.Bottles[i] = new BottleSpawn { InitialAmount = 50 }; b = l.Bottles[i]; }

                EditorGUILayout.BeginHorizontal();
                {
                    var name = b.Substance == null ? "<chưa chọn>" : (b.Substance.Id ?? b.Substance.name);
                    if (GUILayout.Button(name, EditorStyles.popup, GUILayout.MinWidth(140)))
                    {
                        int capI = i;
                        var menu = new GenericMenu();
                        foreach (var sub in idx.GetAllSubstances())
                        {
                            if (sub == null) continue;
                            var capSub = sub;
                            menu.AddItem(new GUIContent(sub.Id ?? sub.name), b.Substance == sub, () => l.Bottles[capI].Substance = capSub);
                        }
                        if (openInlineCreate != null)
                        {
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("+ Tạo chất mới…"), false, () =>
                                openInlineCreate("", picked => { if (picked != null) l.Bottles[capI].Substance = picked; }));
                        }
                        menu.ShowAsContext();
                    }
                    EditorGUILayout.LabelField("amt", GUILayout.Width(28));
                    b.InitialAmount = EditorGUILayout.FloatField(b.InitialAmount, GUILayout.Width(50));
                    b.MaskLabel     = EditorGUILayout.ToggleLeft("mask", b.MaskLabel, GUILayout.Width(50));
                    if (b.MaskLabel) b.MaskedLabel = EditorGUILayout.TextField(b.MaskedLabel, GUILayout.Width(60));
                    if (GUILayout.Button("X", GUILayout.Width(22))) removeAt = i;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (removeAt >= 0) l.Bottles.RemoveAt(removeAt);
            if (GUILayout.Button("+ Add Bottle", GUILayout.Width(120)))
            {
                Undo.RegisterCompleteObjectUndo(l, "Add Bottle");
                l.Bottles.Add(new BottleSpawn { InitialAmount = 50 });
                EditorUtility.SetDirty(l);
            }
        }

        private static void DrawReactionList(LevelConfig l, IAssetIndexReadOnly idx)
        {
            if (l.AvailableReactions == null) l.AvailableReactions = new List<ReactionRule>();
            int removeAt = -1;
            for (int i = 0; i < l.AvailableReactions.Count; i++)
            {
                var rx = l.AvailableReactions[i];
                EditorGUILayout.BeginHorizontal();
                {
                    var name = rx == null ? "<chưa chọn>" : (rx.Id ?? rx.name);
                    if (GUILayout.Button(name, EditorStyles.popup, GUILayout.MinWidth(160)))
                    {
                        int capI = i;
                        var menu = new GenericMenu();
                        foreach (var r in idx.GetAllReactions())
                        {
                            if (r == null) continue;
                            var capR = r;
                            menu.AddItem(new GUIContent(r.Id ?? r.name), rx == r, () => l.AvailableReactions[capI] = capR);
                        }
                        menu.ShowAsContext();
                    }
                    if (GUILayout.Button("X", GUILayout.Width(22))) removeAt = i;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (removeAt >= 0) l.AvailableReactions.RemoveAt(removeAt);
            if (GUILayout.Button("+ Add Reaction", GUILayout.Width(140)))
            {
                Undo.RegisterCompleteObjectUndo(l, "Add Reaction");
                l.AvailableReactions.Add(null);
                EditorUtility.SetDirty(l);
            }
        }
    }
}
