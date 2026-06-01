using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ChemistryGame.EditorTools.ContentEditor
{
    public static class ValidationPanelDrawer
    {
        public static void Draw(List<ValidationIssue> issues)
        {
            if (issues == null || issues.Count == 0)
            {
                EditorGUILayout.HelpBox("Không phát hiện lỗi.", MessageType.None);
                return;
            }

            int err = 0, warn = 0, info = 0;
            foreach (var i in issues)
            {
                switch (i.Severity) { case Severity.Error: err++; break; case Severity.Warning: warn++; break; default: info++; break; }
            }
            EditorGUILayout.LabelField($"❌ {err}    ⚠ {warn}    ℹ {info}", EditorStyles.miniBoldLabel);
            EditorGUILayout.Space(2);

            foreach (var i in issues)
            {
                var msgType = i.Severity switch
                {
                    Severity.Error   => MessageType.Error,
                    Severity.Warning => MessageType.Warning,
                    _                => MessageType.Info,
                };
                EditorGUILayout.HelpBox($"[{i.Code}] {i.Message}", msgType);
                if (i.QuickFix != null && !string.IsNullOrEmpty(i.QuickFixLabel))
                {
                    if (GUILayout.Button(i.QuickFixLabel, GUILayout.Height(18)))
                        i.QuickFix.Invoke();
                }
            }
        }
    }
}
