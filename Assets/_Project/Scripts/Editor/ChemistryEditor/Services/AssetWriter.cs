using System.IO;
using ChemistryGame.Chemistry;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChemistryGame.EditorTools.ContentEditor
{
    /// <summary>
    /// Centralised asset creation / modification with Undo registration.
    /// All CRUD on Substance/Reaction/Level should go through here.
    /// </summary>
    public static class AssetWriter
    {
        public const string SubstanceDir = "Assets/_Project/ScriptableObjects/Substances";
        public const string ReactionDir  = "Assets/_Project/ScriptableObjects/Reactions";
        public const string LevelDir     = "Assets/_Project/ScriptableObjects/Levels";

        public static SubstanceData CreateSubstance(string id, System.Action<SubstanceData> configure = null)
        {
            EnsureFolder(SubstanceDir);
            var so = ScriptableObject.CreateInstance<SubstanceData>();
            so.Id = id ?? string.Empty;
            configure?.Invoke(so);
            var path = AssetDatabase.GenerateUniqueAssetPath($"{SubstanceDir}/Sub_{SafeName(id)}.asset");
            AssetDatabase.CreateAsset(so, path);
            Undo.RegisterCreatedObjectUndo(so, $"Create Substance {id}");
            AssetDatabase.SaveAssets();
            return so;
        }

        public static ReactionRule CreateReaction(string id, System.Action<ReactionRule> configure = null)
        {
            EnsureFolder(ReactionDir);
            var so = ScriptableObject.CreateInstance<ReactionRule>();
            so.Id = id ?? string.Empty;
            configure?.Invoke(so);
            var path = AssetDatabase.GenerateUniqueAssetPath($"{ReactionDir}/Rx_{SafeName(id)}.asset");
            AssetDatabase.CreateAsset(so, path);
            Undo.RegisterCreatedObjectUndo(so, $"Create Reaction {id}");
            AssetDatabase.SaveAssets();
            return so;
        }

        public static LevelConfig CreateLevel(int index, string displayName)
        {
            EnsureFolder(LevelDir);
            var so = ScriptableObject.CreateInstance<LevelConfig>();
            so.LevelIndex = index;
            so.DisplayName = displayName ?? $"Level {index}";
            var path = AssetDatabase.GenerateUniqueAssetPath($"{LevelDir}/Level_{index:D2}_{SafeName(displayName)}.asset");
            AssetDatabase.CreateAsset(so, path);
            Undo.RegisterCreatedObjectUndo(so, $"Create Level {index}");
            AssetDatabase.SaveAssets();
            return so;
        }

        public static T Duplicate<T>(T original, string newSuffix = "_Copy") where T : ScriptableObject
        {
            if (original == null) return null;
            var path = AssetDatabase.GetAssetPath(original);
            if (string.IsNullOrEmpty(path)) return null;
            var dir = Path.GetDirectoryName(path).Replace('\\', '/');
            var name = Path.GetFileNameWithoutExtension(path);
            var newPath = AssetDatabase.GenerateUniqueAssetPath($"{dir}/{name}{newSuffix}.asset");
            if (!AssetDatabase.CopyAsset(path, newPath)) return null;
            var dup = AssetDatabase.LoadAssetAtPath<T>(newPath);
            if (dup != null) Undo.RegisterCreatedObjectUndo(dup, $"Duplicate {name}");
            return dup;
        }

        public static void DeleteAsset(Object asset)
        {
            if (asset == null) return;
            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path)) return;
            AssetDatabase.MoveAssetToTrash(path);
        }

        public static void MarkDirty(Object o)
        {
            if (o == null) return;
            Undo.RegisterCompleteObjectUndo(o, $"Modify {o.name}");
            EditorUtility.SetDirty(o);
        }

        public static void SaveAll() => AssetDatabase.SaveAssets();

        private static string SafeName(string s)
        {
            if (string.IsNullOrEmpty(s)) return "New";
            var safe = s.Replace(" ", "_");
            foreach (var c in Path.GetInvalidFileNameChars()) safe = safe.Replace(c, '_');
            // Strip rich text tags so filename stays clean.
            safe = System.Text.RegularExpressions.Regex.Replace(safe, "<.*?>", "");
            return string.IsNullOrEmpty(safe) ? "New" : safe;
        }

        private static void EnsureFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder)) return;
            var parts = assetFolder.Split('/');
            var cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }
}
