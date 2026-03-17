using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NewSideGame.Editor
{
    /// <summary>
    /// 确保在进入播放模式时始终从 Build Settings 中的第 0 个场景启动。
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeSceneBootstrapper
    {
        private const string MenuPath = "Tools/Scene Bootstrapper/始终从场景 0 启动";
        private const string PrefKey = "PlayModeSceneBootstrapper_Enabled";

        static PlayModeSceneBootstrapper()
        {
            // 延迟初始化以确保编辑器资源已加载完毕
            EditorApplication.delayCall += Initialize;
            // 监听 Build Settings 列表变化，确保路径更新
            EditorBuildSettings.sceneListChanged += UpdateBootstrapper;
        }

        private static void Initialize()
        {
            UpdateBootstrapper();
        }

        [MenuItem(MenuPath)]
        private static void ToggleBootstrapper()
        {
            bool isEnabled = EditorPrefs.GetBool(PrefKey, true);
            EditorPrefs.SetBool(PrefKey, !isEnabled);
            UpdateBootstrapper();
            
            Debug.Log($"[SceneBootstrapper] 已{(!isEnabled ? "启用" : "禁用")}自动引导至场景 0。");
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleBootstrapperValidate()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(PrefKey, true));
            return true;
        }

        private static void UpdateBootstrapper()
        {
            bool isEnabled = EditorPrefs.GetBool(PrefKey, true);
            if (isEnabled)
            {
                if (EditorBuildSettings.scenes.Length > 0)
                {
                    string path = EditorBuildSettings.scenes[0].path;
                    SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                    if (sceneAsset != null)
                    {
                        // 设置播放模式的起始场景
                        EditorSceneManager.playModeStartScene = sceneAsset;
                    }
                }
                else
                {
                    Debug.LogWarning("[SceneBootstrapper] Build Settings 中没有场景，请先添加场景。");
                }
            }
            else
            {
                // 清除设置，恢复 Unity 默认行为（运行当前打开的场景）
                EditorSceneManager.playModeStartScene = null;
            }
        }
    }
}