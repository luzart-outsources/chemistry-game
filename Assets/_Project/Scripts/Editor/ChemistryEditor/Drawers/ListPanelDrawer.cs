using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChemistryGame.EditorTools.ContentEditor
{
    public static class ListPanelDrawer
    {
        /// <summary>Returns true and sets selectedOut when the user clicks an item.</summary>
        public static bool Draw<T>(
            IReadOnlyList<T> items,
            T selected,
            string searchText,
            Func<T, string> labelOf,
            out T selectedOut) where T : Object
        {
            selectedOut = selected;
            bool changed = false;
            var search = (searchText ?? "").Trim().ToLowerInvariant();

            foreach (var item in items)
            {
                if (item == null) continue;
                var label = labelOf(item) ?? item.name ?? "<unnamed>";
                if (search.Length > 0 && label.ToLowerInvariant().IndexOf(search, StringComparison.Ordinal) < 0)
                    continue;

                var isSel = ReferenceEquals(item, selected);
                var style = isSel ? EditorStyles_Chemistry.SelectedRow : EditorStyles_Chemistry.NormalRow;
                if (GUILayout.Button(label, style, GUILayout.ExpandWidth(true)))
                {
                    selectedOut = item;
                    changed = true;
                }
            }
            return changed;
        }
    }
}
