using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace NewSideGame.Editor
{
    public class SceneSwitcherWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<string> scenePaths = new List<string>();

        [MenuItem("Tools/Scene Switcher", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneSwitcherWindow>("Scene Switcher");
            window.minSize = new Vector2(300, 400);
            window.RefreshSceneList();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Scene List", GUILayout.Height(30)))
            {
                RefreshSceneList();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (scenePaths.Count == 0)
            {
                EditorGUILayout.HelpBox("No scenes found in the project.", MessageType.Info);
                return;
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (var path in scenePaths)
            {
                string sceneName = Path.GetFileNameWithoutExtension(path);
                
                GUILayout.BeginHorizontal("box");
                
                GUILayout.Label(sceneName, EditorStyles.boldLabel);
                
                if (GUILayout.Button("Open", GUILayout.Width(80)))
                {
                    OpenScene(path);
                }
                
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        private void RefreshSceneList()
        {
            scenePaths.Clear();
            string[] guids = AssetDatabase.FindAssets("t:Scene");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // 可以选择排除某些不需要显示的目录，例如框架自带的测试场景等
                if (!path.Contains("Packages/") && !path.Contains("Plugins/"))
                {
                    scenePaths.Add(path);
                }
            }
            // 按场景名称排序
            scenePaths.Sort((a, b) => Path.GetFileName(a).CompareTo(Path.GetFileName(b)));
        }

        private void OpenScene(string scenePath)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath);
            }
        }
    }
}