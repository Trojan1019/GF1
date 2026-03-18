using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace NewSideGame
{
    public class AndroidPrebuild : IPreprocessBuildWithReport
    {
        // template
        private static readonly string SourceManifestPath = "Editor/Platform/Android/AndroidManifest.xml";
        private static readonly string SourceManifestPathAAB = "Editor/Platform/Android/AndroidManifestAAB.xml";
        private static readonly string SourceLauncherGradlePath = "Editor/Platform/Android/launcherTemplate.gradle";
        private static readonly string SourceBaseGradlePath = "Editor/Platform/Android/baseProjectTemplate.gradle";
        private static readonly string SourceServerConfigPath = "Editor/Platform/Android/BaseApplication.java";
        // final
        private static readonly string DestManifestPath = "Plugins/Android/AndroidManifest.xml";
        private static readonly string DestLauncherGradlePath = "Plugins/Android/launcherTemplate.gradle";
        private static readonly string DestBaseGradlePath = "Plugins/Android/baseProjectTemplate.gradle";
        private static readonly string DestServerConfigPath = "Plugins/Android/BaseApplication.java";

        public int callbackOrder { get => 1; }

        public void OnPreprocessBuild(BuildReport report)
        {
#if UNITY_ANDROID
            _OnPreprocessBuild();
#endif
        }

        public static void _OnPreprocessBuild()
        {
            // bool isAAB = EditorPrefs.GetString("EXPORT_GRADLE_PROJECT", "APK").Equals("AAB");
            // KBSettingsEditor.ImportSettings();
            PlayerSettings.SplashScreen.showUnityLogo = false;

            // KBSDK.KBSettings settings = KBSDK.KBSettings.Load();
            // WriteMainfest(settings, isAAB);
            // WriteBaseGradle(settings, isAAB);
            // WriteServerJava(settings, isAAB);
            //
            // KBSDK.KBSignSettings signSettings = KBSDK.KBSignSettings.Load();
            // WriteLauncherGradle(signSettings, isAAB);
        }

        // private static void WriteMainfest(KBSDK.KBSettings settings, bool isAAB)
        // {
        //     string sourcePath = Path.Combine(Application.dataPath, isAAB ? SourceManifestPathAAB : SourceManifestPath);
        //     string content = File.ReadAllText(sourcePath);
        //     content = content.Replace("**ADMOBAPPID**", settings.adMobAndroidAppid)
        //         .Replace("**FACEBOOKAPPID**", settings.facebookAndroidAppId)
        //         .Replace("**FACEBOOKCLIENTTOKEN**", settings.facebookClientToken)
        //         .Replace("**MAXSDKKEY**", settings.appLovinMaxSdkKey);
        //
        //     string destPath = Path.Combine(Application.dataPath, DestManifestPath);
        //     File.Delete(destPath);
        //     File.WriteAllText(destPath, content);
        // }
        //
        // private static void WriteLauncherGradle(KBSDK.KBSignSettings settings, bool isAAB)
        // {
        //     string sourcePath = Path.Combine(Application.dataPath, SourceLauncherGradlePath);
        //     string content = File.ReadAllText(sourcePath);
        //     content = content
        //         .Replace("**SIGNKEYALIAS**", settings.keyaliasName)
        //         .Replace("**SIGNKEYPW**", settings.keyaliasPass)
        //         .Replace("**SIGNSTORENAME**", settings.keystoreName)
        //         .Replace("**SIGNSTOREPW**", settings.keystorePass)
        //         .Replace("**ASSETPACKBUNDLE**", isAAB ? "assetPacks = [\":bundle\"]" : "");
        //
        //     string destPath = Path.Combine(Application.dataPath, DestLauncherGradlePath);
        //     File.Delete(destPath);
        //     File.WriteAllText(destPath, content);
        // }
        //
        // private static void WriteBaseGradle(KBSDK.KBSettings settings, bool isAAB)
        // {
        //     string sourcePath = Path.Combine(Application.dataPath, SourceBaseGradlePath);
        //     string content = File.ReadAllText(sourcePath);
        //     string destPath = Path.Combine(Application.dataPath, DestBaseGradlePath);
        //     File.Delete(destPath);
        //     File.WriteAllText(destPath, content);
        // }
        //
        // private static void WriteServerJava(KBSDK.KBSettings settings, bool isAAB)
        // {
        //     string sourcePath = Path.Combine(Application.dataPath, SourceServerConfigPath);
        //     string content = File.ReadAllText(sourcePath);
        //     content = content.Replace("**SERVERADDRESS**", settings.serverAddress)
        //         .Replace("**CDNSERVERADDRESS**", settings.cdnServerAddress);
        //
        //     string destPath = Path.Combine(Application.dataPath, DestServerConfigPath);
        //     File.Delete(destPath);
        //     File.WriteAllText(destPath, content);
        // }
    }
}