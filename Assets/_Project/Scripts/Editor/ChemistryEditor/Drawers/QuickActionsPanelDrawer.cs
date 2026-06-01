using System;
using ChemistryGame.Chemistry;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChemistryGame.EditorTools.ContentEditor
{
    public static class QuickActionsPanelDrawer
    {
        public static void Draw(Object selected, IAssetIndexReadOnly idx, Action<Object> onSelectionChanged, Action onIndexInvalidated)
        {
            if (selected == null) return;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Duplicate"))
            {
                Object dup = null;
                switch (selected)
                {
                    case SubstanceData s: dup = QuickActions.DuplicateSubstance(s); break;
                    case ReactionRule  r: dup = QuickActions.DuplicateReaction(r); break;
                    case LevelConfig   l: dup = QuickActions.DuplicateLevel(l); break;
                }
                onIndexInvalidated?.Invoke();
                if (dup != null) onSelectionChanged?.Invoke(dup);
            }

            if (GUILayout.Button("Find references"))
            {
                var refs = QuickActions.FindReferences(selected, idx);
                if (refs.Count == 0) Debug.Log($"[ContentEditor] No references for {selected.name}.");
                else
                {
                    var lines = new System.Text.StringBuilder($"[ContentEditor] References for {selected.name}:\n");
                    foreach (var r in refs) lines.AppendLine($"  • {r.GetType().Name} — {r.name}");
                    Debug.Log(lines.ToString());
                }
            }

            if (selected is LevelConfig level)
            {
                if (GUILayout.Button("Add to GameManager"))
                {
                    var ok = QuickActions.AddLevelToGameManager(level);
                    EditorUtility.DisplayDialog(
                        "Content Editor",
                        ok ? "Đã thêm level vào GameManager." : "Không cần thêm — level đã có trong GameManager (hoặc lỗi).",
                        "OK");
                }
            }

            if (GUILayout.Button("Reveal in Project"))
                EditorGUIUtility.PingObject(selected);

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Delete asset"))
            {
                if (EditorUtility.DisplayDialog("Xoá asset", $"Chắc chắn xoá '{selected.name}'?", "Xoá", "Huỷ"))
                {
                    AssetWriter.DeleteAsset(selected);
                    onIndexInvalidated?.Invoke();
                    onSelectionChanged?.Invoke(null);
                }
            }
        }
    }
}
