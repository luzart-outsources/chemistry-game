using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChemistryGame.EditorTools.ContentEditor
{
    /// <summary>
    /// Layout-friendly substance/reaction/tool picker. Renders as a button showing the
    /// current label; on click opens a GenericMenu with filtered items plus a
    /// "+ Tạo mới…" footer (when onCreateNew != null).
    /// </summary>
    public static class SearchableDropdown
    {
        public static void Draw<T>(
            string fieldLabel,
            T current,
            IReadOnlyList<T> items,
            Func<T, string> labelOf,
            Action<T> onPick,
            Action onCreateNew = null,
            string createNewLabel = "+ Tạo mới…") where T : Object
        {
            EditorGUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(fieldLabel))
                EditorGUILayout.LabelField(fieldLabel, GUILayout.Width(80));

            var label = current == null ? "<chưa chọn>" : (labelOf(current) ?? current.name);
            if (GUILayout.Button(label, EditorStyles.popup))
            {
                var menu = new GenericMenu();
                foreach (var item in items)
                {
                    if (item == null) continue;
                    var capture = item;
                    var menuLabel = labelOf(item) ?? item.name;
                    menu.AddItem(new GUIContent(menuLabel), ReferenceEquals(current, item), () => onPick?.Invoke(capture));
                }
                if (onCreateNew != null)
                {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent(createNewLabel), false, () => onCreateNew());
                }
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
