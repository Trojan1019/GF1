using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;
using UnityGameFramework.Editor;

public static class ProjectBuildTool
{
    private const string EnableMaterialDefineSymbol = "ENABLE_MATERIAL";

    public enum DevelopEnvironment
    {
        None = 0,
        Production = 1, // 正式服
        staging = 2, // 测试服
    }

    public enum AndroidBuildState
    {
        None = 0,
        APK = 2,
        AppBundle = 3,
    };

    // 通过 Name 获取参数
    public static string GetArgument(int index)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        return args[index];
    }

    public static string ProjectName
    {
        get
        {
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("project", System.StringComparison.Ordinal))
                {
                    return arg.Split("-"[0])[1];
                }
            }
            return "AIWaterSort";
        }
    }

    public static T GetEnum<T>(string text)
    {
        return (T)Enum.Parse(typeof(T), text);
    }

    public static bool ExportBatchMonoBehavior()
    {
        return BatchRenameAllMonoBehaviours.BuildPreConfoundProcessor();
    }

    // 导出安卓工程项目
    public static void ExportProjectForAndroid()
    {
        string Version = GetArgument(8);
        string InternalResourceVersion = GetArgument(9);
        string IsAAB = GetArgument(10);
        string IsBuildBundle = GetArgument(11);
        string ForceRebuildAssetBundle = GetArgument(12);
        string IsMaterial = GetArgument(13);

        Debug.Log($"Version: {Version}");
        Debug.Log($"InternalResourceVersion: {InternalResourceVersion}");
        Debug.Log($"IsAAB: {IsAAB}");
        Debug.Log($"IsBuildBundle: {IsBuildBundle}");
        Debug.Log($"ForceRebuildAssetBundle: {ForceRebuildAssetBundle}");
        Debug.Log($"IsMaterial: {IsMaterial}");

        if (bool.TryParse(IsBuildBundle, out bool isBuildAssetBundle) && isBuildAssetBundle)
        {
            AssetBundleBuildTool.BuildAssetBundle(Platform.Android, Version, int.Parse(InternalResourceVersion), bool.Parse(ForceRebuildAssetBundle));
        }

        if (bool.TryParse(IsAAB, out bool isAAB) && isAAB)
        {
            UnityEditor.EditorUserBuildSettings.buildAppBundle = true;
            UnityEditor.PlayerSettings.Android.useAPKExpansionFiles = true;
            UnityEditor.EditorPrefs.SetString("EXPORT_GRADLE_PROJECT", "AAB");
        }
        else
        {
            UnityEditor.EditorUserBuildSettings.buildAppBundle = false;
            UnityEditor.PlayerSettings.Android.useAPKExpansionFiles = false;
            UnityEditor.EditorPrefs.SetString("EXPORT_GRADLE_PROJECT", "APK");
        }

        UnityEditor.PlayerSettings.bundleVersion = Version;
        UnityEditor.EditorUserBuildSettings.androidBuildSystem = UnityEditor.AndroidBuildSystem.Gradle;
        UnityEditor.EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        UnityEditor.BuildOptions buildOption = UnityEditor.BuildOptions.None;
        buildOption |= UnityEditor.BuildOptions.CompressWithLz4HC;

        if (bool.TryParse(IsMaterial, out bool isMaterialPackage) && isMaterialPackage)
        {
            AddEnableMaterialDefineSymbol();
        }
        else
        {
            RemoveEnableMaterialDefineSymbol();
        }

        // 开始构建工程
        UnityEditor.BuildPipeline.BuildPlayer(GetScenes(), ProjectName, UnityEditor.BuildTarget.Android,
            buildOption);
    }

    // 导出 XCode 工程
    public static void ExportProjectForiOS()
    {
        string Version = GetArgument(8);
        string InternalResourceVersion = GetArgument(9);
        string IsBuildBundle = GetArgument(10);
        string ForceRebuildAssetBundle = GetArgument(11);
        string BuildNumber = GetArgument(12);

        if (bool.TryParse(IsBuildBundle, out bool isBuildAssetBundle) && isBuildAssetBundle)
        {
            AssetBundleBuildTool.BuildAssetBundle(Platform.IOS, Version, int.Parse(InternalResourceVersion), bool.Parse(ForceRebuildAssetBundle));
        }

        // 导出iOS工程
        UnityEditor.PlayerSettings.SplashScreen.show = false;
        UnityEditor.PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        UnityEditor.PlayerSettings.iOS.sdkVersion = UnityEditor.iOSSdkVersion.DeviceSDK;
        UnityEditor.PlayerSettings.iOS.allowHTTPDownload = true;
        UnityEditor.PlayerSettings.iOS.requiresFullScreen = true;
        UnityEditor.PlayerSettings.statusBarHidden = true;
        UnityEditor.PlayerSettings.iOS.showActivityIndicatorOnLoading = UnityEditor.iOSShowActivityIndicatorOnLoading.DontShow;
        UnityEditor.PlayerSettings.SetArchitecture(UnityEditor.BuildTargetGroup.iOS, 1);//arm64
        UnityEditor.PlayerSettings.iOS.appleDeveloperTeamID = "7ZYQ76X7HX";//变更开发者账号时，需要修改此处
        UnityEditor.PlayerSettings.defaultInterfaceOrientation = UnityEditor.UIOrientation.Portrait;//如果需要支持横屏，修改此处
        UnityEditor.PlayerSettings.iOS.targetDevice = UnityEditor.iOSTargetDevice.iPhoneAndiPad;//如果需要支持ipad，修改此处
        UnityEditor.PlayerSettings.iOS.buildNumber = BuildNumber;

        UnityEditor.BuildOptions buildOption = UnityEditor.BuildOptions.None;
        buildOption |= UnityEditor.BuildOptions.CompressWithLz4HC;

        // 开始构建工程
        UnityEditor.BuildPipeline.BuildPlayer(GetScenes(), ProjectName, UnityEditor.BuildTarget.iOS,
            buildOption);
    }

    private static UnityEditor.EditorBuildSettingsScene[] GetScenes()
    {
        UnityEditor.EditorBuildSettingsScene[] scenes = new UnityEditor.EditorBuildSettingsScene[1];
        scenes[0] = new UnityEditor.EditorBuildSettingsScene("Assets/Resources/Scenes/MainScene.unity", true);
        UnityEditor.EditorBuildSettings.scenes = scenes;

        return UnityEditor.EditorBuildSettings.scenes;
    }

    public static void BuildApk()
    {
        // 获取当前系统时间
        DateTime currentTime = DateTime.Now;

        // 将时间格式化为 "yyyyMMdd" 格式
        string formattedTime = currentTime.ToString("yyyyMMddHHmm");

        string PROJECT_NAME = $"../Build/Project/Android/{formattedTime}.apk";

        UnityEditor.EditorUserBuildSettings.buildAppBundle = false;
        UnityEditor.PlayerSettings.Android.useAPKExpansionFiles = false;
        UnityEditor.EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        UnityEditor.BuildOptions buildOption = UnityEditor.BuildOptions.None;
        buildOption |= UnityEditor.BuildOptions.CompressWithLz4HC;

        // 开始构建工程
        UnityEditor.BuildPipeline.BuildPlayer(GetScenes(), PROJECT_NAME, UnityEditor.BuildTarget.Android,
            buildOption);
    }

    [UnityEditor.MenuItem("Tools/打包/直接打APK(包含AB)")]
    public static void BuildAPKAndAssetBundle()
    {
        if (AssetBundleBuildTool.BuildAssetBundle(Platform.Android, Application.version, 1, false))
        {
            UnityEditor.EditorBuildSettingsScene[] scenes = new UnityEditor.EditorBuildSettingsScene[1];
            scenes[0] = new UnityEditor.EditorBuildSettingsScene("Assets/Resources/Scenes/MainScene.unity", true);
            UnityEditor.EditorBuildSettings.scenes = scenes;

            BuildApk();
        }
    }

    [UnityEditor.MenuItem("Tools/打包/直接打Xcode(包含AB)", false, 10)]
    public static void BuildXcode()
    {
        if (AssetBundleBuildTool.BuildAssetBundle(Platform.IOS, Application.version, 1, true))
        {
            UnityEditor.EditorBuildSettingsScene[] scenes = new UnityEditor.EditorBuildSettingsScene[1];
            scenes[0] = new UnityEditor.EditorBuildSettingsScene("Assets/Resources/Scenes/MainScene.unity", true);
            UnityEditor.EditorBuildSettings.scenes = scenes;

            UnityEditor.PlayerSettings.SplashScreen.show = false;
            UnityEditor.PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            UnityEditor.PlayerSettings.iOS.sdkVersion = UnityEditor.iOSSdkVersion.DeviceSDK;
            UnityEditor.PlayerSettings.iOS.allowHTTPDownload = true;
            UnityEditor.PlayerSettings.iOS.requiresFullScreen = true;
            UnityEditor.PlayerSettings.statusBarHidden = true;
            UnityEditor.PlayerSettings.iOS.showActivityIndicatorOnLoading = UnityEditor.iOSShowActivityIndicatorOnLoading.DontShow;
            UnityEditor.PlayerSettings.SetArchitecture(UnityEditor.BuildTargetGroup.iOS, 1);//arm64
            UnityEditor.PlayerSettings.iOS.appleDeveloperTeamID = "7ZYQ76X7HX";//变更开发者账号时，需要修改此处
            UnityEditor.PlayerSettings.defaultInterfaceOrientation = UnityEditor.UIOrientation.Portrait;//如果需要支持横屏，修改此处
            UnityEditor.PlayerSettings.iOS.targetDevice = UnityEditor.iOSTargetDevice.iPhoneAndiPad;//如果需要支持ipad，修改此处
            UnityEditor.PlayerSettings.iOS.buildNumber = "1";

            UnityEditor.BuildOptions buildOption = UnityEditor.BuildOptions.None;
            buildOption |= UnityEditor.BuildOptions.CompressWithLz4HC;

            // 开始构建工程
            UnityEditor.BuildPipeline.BuildPlayer(GetScenes(), ProjectName, UnityEditor.BuildTarget.iOS,
                buildOption);
        }
    }

    public static void AddEnableMaterialDefineSymbol()
    {
        var targetGroup = UnityEditor.BuildTargetGroup.Android;
        ScriptingDefineSymbols.AddScriptingDefineSymbol(targetGroup, EnableMaterialDefineSymbol);
    }

    public static void RemoveEnableMaterialDefineSymbol()
    {
        var targetGroup = UnityEditor.BuildTargetGroup.Android;
        ScriptingDefineSymbols.RemoveScriptingDefineSymbol(targetGroup, EnableMaterialDefineSymbol);
    }

    [UnityEditor.MenuItem("Tools/打包/Build Project")]
    public static void ttAndroid()
    {
        string Version = "0.0.2";
        string InternalResourceVersion = "1";
        string IsAAB = "false";
        string IsBuildBundle = "true";
        string ForceRebuildAssetBundle = "true";
        string IsMaterial = "false";
        string buildNumber = "1";

        Debug.Log($"Version: {Version}");
        Debug.Log($"InternalResourceVersion: {InternalResourceVersion}");
        Debug.Log($"IsAAB: {IsAAB}");
        Debug.Log($"IsBuildBundle: {IsBuildBundle}");
        Debug.Log($"ForceRebuildAssetBundle: {ForceRebuildAssetBundle}");
        Debug.Log($"IsMaterial: {IsMaterial}");

        if (Directory.Exists(ProjectName))
            Directory.Delete(ProjectName, true);

#if UNITY_ANDROID
        if (ExportBatchMonoBehavior())
        {
            CompilationListener.RequestCompilationAndListen(() =>
            {
                if (bool.TryParse(IsBuildBundle, out bool isBuildAssetBundle) && isBuildAssetBundle)
                {
                    if (AssetBundleBuildTool.BuildAssetBundle(Platform.Android, Version, int.Parse(InternalResourceVersion), bool.Parse(ForceRebuildAssetBundle)))
                    {
                        if (bool.TryParse(IsAAB, out bool isAAB) && isAAB)
                        {
                            UnityEditor.EditorUserBuildSettings.buildAppBundle = true;
                            UnityEditor.PlayerSettings.Android.useAPKExpansionFiles = true;
                            UnityEditor.EditorPrefs.SetString("EXPORT_GRADLE_PROJECT", "AAB");
                        }
                        else
                        {
                            UnityEditor.EditorUserBuildSettings.buildAppBundle = false;
                            UnityEditor.PlayerSettings.Android.useAPKExpansionFiles = false;
                            UnityEditor.EditorPrefs.SetString("EXPORT_GRADLE_PROJECT", "APK");
                        }

                        UnityEditor.PlayerSettings.bundleVersion = Version;
                        UnityEditor.EditorUserBuildSettings.androidBuildSystem = UnityEditor.AndroidBuildSystem.Gradle;
                        UnityEditor.EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

                        UnityEditor.BuildOptions buildOption = UnityEditor.BuildOptions.None;
                        buildOption |= UnityEditor.BuildOptions.CompressWithLz4HC;

                        if (bool.TryParse(IsMaterial, out bool isMaterialPackage) && isMaterialPackage)
                        {
                            AddEnableMaterialDefineSymbol();
                        }
                        else
                        {
                            RemoveEnableMaterialDefineSymbol();
                        }

                        // 开始构建工程
                        UnityEditor.BuildPipeline.BuildPlayer(GetScenes(), ProjectName, UnityEditor.BuildTarget.Android,
                            buildOption);
                    }
                }
            });
        }
#elif UNITY_IOS
        if (ExportBatchMonoBehavior())
        {
            CompilationListener.RequestCompilationAndListen(() =>
            {
                if (bool.TryParse(IsBuildBundle, out bool isBuildAssetBundle) && isBuildAssetBundle)
                {
                    if (AssetBundleBuildTool.BuildAssetBundle(Platform.IOS, Version, int.Parse(InternalResourceVersion), bool.Parse(ForceRebuildAssetBundle)))
                    {
                        // 导出iOS工程
                        UnityEditor.PlayerSettings.SplashScreen.show = false;
                        UnityEditor.PlayerSettings.iOS.appleEnableAutomaticSigning = true;
                        UnityEditor.PlayerSettings.iOS.sdkVersion = UnityEditor.iOSSdkVersion.DeviceSDK;
                        UnityEditor.PlayerSettings.iOS.allowHTTPDownload = true;
                        UnityEditor.PlayerSettings.iOS.requiresFullScreen = true;
                        UnityEditor.PlayerSettings.statusBarHidden = true;
                        UnityEditor.PlayerSettings.iOS.showActivityIndicatorOnLoading = UnityEditor.iOSShowActivityIndicatorOnLoading.DontShow;
                        UnityEditor.PlayerSettings.SetArchitecture(UnityEditor.BuildTargetGroup.iOS, 1);//arm64
                        UnityEditor.PlayerSettings.iOS.appleDeveloperTeamID = "7ZYQ76X7HX";//变更开发者账号时，需要修改此处
                        UnityEditor.PlayerSettings.defaultInterfaceOrientation = UnityEditor.UIOrientation.Portrait;//如果需要支持横屏，修改此处
                        UnityEditor.PlayerSettings.iOS.targetDevice = UnityEditor.iOSTargetDevice.iPhoneAndiPad;//如果需要支持ipad，修改此处
                        UnityEditor.PlayerSettings.iOS.buildNumber = buildNumber;

                        UnityEditor.PlayerSettings.iOS.applicationDisplayName = "Travel Tile - Puzzle Game";
                        UnityEditor.PlayerSettings.SetApplicationIdentifier(UnityEditor.BuildTargetGroup.iOS, "ios.sort.block.color");
                        UnityEditor.BuildOptions buildOption = UnityEditor.BuildOptions.None;
                        buildOption |= UnityEditor.BuildOptions.CompressWithLz4HC;

                        // 开始构建工程
                        UnityEditor.BuildPipeline.BuildPlayer(GetScenes(), ProjectName, UnityEditor.BuildTarget.iOS,
                            buildOption);
                    }
                }
            });
        }
#endif
    }
}