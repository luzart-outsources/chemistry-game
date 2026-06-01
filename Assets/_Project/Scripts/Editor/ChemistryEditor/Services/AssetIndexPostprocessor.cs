using UnityEditor;

namespace ChemistryGame.EditorTools.ContentEditor
{
    /// <summary>Invalidates the currently active AssetIndex whenever any project asset changes.</summary>
    internal class AssetIndexPostprocessor : AssetPostprocessor
    {
        internal static AssetIndex Active;

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            Active?.Invalidate();
        }
    }
}
