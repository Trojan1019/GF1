using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NewSideGame.Editor
{
    public class SkinConfigBatchTool : EditorWindow
    {
        private const string RootFolder = "Assets/Game/CubeCrush/Skins";
        private const string ConfigFolder = RootFolder + "/Configs";
        private const string DatabasePath = RootFolder + "/SkinDatabase.asset";

        [MenuItem("Tools/CubeCrush/Skin/Create Test Skin Configs")]
        public static void CreateTestConfigs()
        {
            EnsureFolder("Assets/Game");
            EnsureFolder("Assets/Game/CubeCrush");
            EnsureFolder(RootFolder);
            EnsureFolder(ConfigFolder);

            var database = AssetDatabase.LoadAssetAtPath<SkinDatabase>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<SkinDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            database.skins = new List<SkinConfig>();
            for (int i = 0; i < 3; i++)
            {
                string assetPath = $"{ConfigFolder}/Skin_{i + 1}.asset";
                var skin = AssetDatabase.LoadAssetAtPath<SkinConfig>(assetPath);
                if (skin == null)
                {
                    skin = ScriptableObject.CreateInstance<SkinConfig>();
                    AssetDatabase.CreateAsset(skin, assetPath);
                }

                skin.skinId = i + 1;
                skin.skinName = $"Skin {i + 1}";
                skin.skinNameKey = (20010 + i).ToString();
                skin.adWatchRequiredCount = i == 0 ? 0 : (i + 1);
                skin.unlocked = i == 0;
                skin.relatedPrefabResourcePaths = skin.relatedPrefabResourcePaths ?? new List<string>();
                EditorUtility.SetDirty(skin);
                database.skins.Add(skin);
            }

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SkinConfigBatchTool] Created SkinDatabase + 3 test SkinConfig assets.");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int index = path.LastIndexOf('/');
            if (index <= 0) return;
            string parent = path.Substring(0, index);
            string name = path.Substring(index + 1);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}

