using System.Collections.Generic;
using System.Linq;
using ChemistryGame.Chemistry;
using ChemistryGame.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ChemistryGame.EditorTools.ContentEditor
{
    public static class QuickActions
    {
        public const string MainScenePath = "Assets/_Project/Scenes/Main.unity";

        public static SubstanceData DuplicateSubstance(SubstanceData s) => AssetWriter.Duplicate(s);
        public static ReactionRule  DuplicateReaction(ReactionRule r)   => AssetWriter.Duplicate(r);
        public static LevelConfig   DuplicateLevel(LevelConfig l)       => AssetWriter.Duplicate(l);

        /// <summary>
        /// Adds the level to GameManager.levels (private List). Opens Main scene if needed.
        /// Returns true if added, false if already present or on error.
        /// </summary>
        public static bool AddLevelToGameManager(LevelConfig level)
        {
            if (level == null) return false;

            var openedTemporarily = false;
            var scene = SceneManager.GetSceneByPath(MainScenePath);
            if (!scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Additive);
                openedTemporarily = true;
            }

            try
            {
                var gm = Object.FindObjectsOfType<GameManager>(true).FirstOrDefault(x => x.gameObject.scene == scene);
                if (gm == null)
                {
                    Debug.LogError("[ContentEditor] GameManager not found in Main scene.");
                    return false;
                }

                var so = new SerializedObject(gm);
                var levelsProp = so.FindProperty("levels");
                if (levelsProp == null)
                {
                    Debug.LogError("[ContentEditor] GameManager.levels SerializedProperty not found.");
                    return false;
                }

                for (int i = 0; i < levelsProp.arraySize; i++)
                    if (levelsProp.GetArrayElementAtIndex(i).objectReferenceValue == level)
                        return false;

                levelsProp.arraySize++;
                levelsProp.GetArrayElementAtIndex(levelsProp.arraySize - 1).objectReferenceValue = level;
                so.ApplyModifiedProperties();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                return true;
            }
            finally
            {
                if (openedTemporarily) EditorSceneManager.CloseScene(scene, true);
            }
        }

        public static List<Object> FindReferences(Object asset, IAssetIndexReadOnly idx)
        {
            var result = new List<Object>();
            if (asset == null || idx == null) return result;

            if (asset is SubstanceData s)
            {
                foreach (var r in idx.GetReactionsUsingInput(s))  result.Add(r);
                foreach (var r in idx.GetReactionsProducing(s))   result.Add(r);
                foreach (var l in idx.GetLevelsUsingSubstance(s)) result.Add(l);
            }
            else if (asset is ReactionRule r)
            {
                foreach (var l in idx.GetLevelsUsingReaction(r)) result.Add(l);
            }
            return result;
        }
    }
}
