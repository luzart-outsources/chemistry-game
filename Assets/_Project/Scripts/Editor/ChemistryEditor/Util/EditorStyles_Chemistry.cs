using UnityEditor;
using UnityEngine;

namespace ChemistryGame.EditorTools.ContentEditor
{
    public static class EditorStyles_Chemistry
    {
        private static GUIStyle _boldRich;
        public static GUIStyle BoldRichLabel
        {
            get
            {
                if (_boldRich == null)
                    _boldRich = new GUIStyle(EditorStyles.boldLabel) { richText = true };
                return _boldRich;
            }
        }

        private static GUIStyle _linkRich;
        public static GUIStyle LinkLabelRich
        {
            get
            {
                if (_linkRich == null)
                {
                    _linkRich = new GUIStyle(EditorStyles.linkLabel) { richText = true, alignment = TextAnchor.MiddleLeft };
                }
                return _linkRich;
            }
        }

        private static GUIStyle _selectedRow;
        public static GUIStyle SelectedRow
        {
            get
            {
                if (_selectedRow == null)
                {
                    _selectedRow = new GUIStyle(EditorStyles.helpBox)
                    {
                        richText = true,
                        alignment = TextAnchor.MiddleLeft,
                        fontStyle = FontStyle.Bold
                    };
                }
                return _selectedRow;
            }
        }

        private static GUIStyle _normalRow;
        public static GUIStyle NormalRow
        {
            get
            {
                if (_normalRow == null)
                    _normalRow = new GUIStyle(EditorStyles.label)
                    {
                        richText = true,
                        alignment = TextAnchor.MiddleLeft
                    };
                return _normalRow;
            }
        }
    }
}
