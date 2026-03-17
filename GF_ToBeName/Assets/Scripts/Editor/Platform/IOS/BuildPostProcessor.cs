//
// Copyright (c) 2017 eppz! mobile, Gergely Borbás (SP)
//
// http://www.twitter.com/_eppz
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if UNITY_IOS

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using KBSDK.Editor;
using KBSDK;
using Unity.Notifications;


public class BuildPostProcessor
{
    private static string[] LANGUAGES = new string[] { "en" };
    private static string[] APP_NAMES = { "Travel Tile - Puzzle Game" };

    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            // 开启推送
            NotificationSettings.iOSSettings.AddRemoteNotificationCapability = true;

            // Read.
            string projectPath = PBXProject.GetPBXProjectPath(path);
            PBXProject project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));
            //string targetName = PBXProject.GetUnityTargetName();
            //string targetGUID = project.TargetGuidByName(targetName);
            string frameworkGuid = project.GetUnityFrameworkTargetGuid() ??
                                   throw new ArgumentNullException("project.GetUnityFrameworkTargetGuid()");
            string mainGuid = project.GetUnityMainTargetGuid();

            string entitlement = Application.dataPath + "/Editor/Platform/IOS/Base.entitlements";
            File.Copy(entitlement, path + "/Unity-iPhone/Unity-iPhone.entitlements");
            project.AddCapability(mainGuid, PBXCapabilityType.InAppPurchase,
                "Unity-iPhone/Unity-iPhone.entitlements", true);
            project.AddCapability(mainGuid, PBXCapabilityType.SignInWithApple,
                "Unity-iPhone/Unity-iPhone.entitlements", true);
            project.AddCapability(mainGuid, PBXCapabilityType.PushNotifications,
                "Unity-iPhone/Unity-iPhone.entitlements", true);
            project.AddCapability(mainGuid, PBXCapabilityType.BackgroundModes,
                "Unity-iPhone/Unity-iPhone.entitlements", true);

            AddFrameworks(project, frameworkGuid);
            //project.AddFrameworkToProject(frameworkGUID, "GameKit.framework", false);

            //add files
            // AddFile(project, frameworkGUID, path, "b_base_config.plist");
            AddFile(project, frameworkGuid, path, "Podfile");
            //AddFile(project, frameworkGUID, path, "File.swift");
            //AddFile(project, frameworkGUID, path, "Unity-iPhone-Bridging-Header.h");
            AddConfigFile(project, frameworkGuid, path, "GoogleService-Info.plist");
            //AddConfigFile(project, frameworkGUID, path, "FullScreenVideo_preConfig.json");
            //AddConfigFile(project, frameworkGUID,path, "Rewarded_preConfig.json");
            //AddConfigFile(project, frameworkGUID,path, "Banner_preConfig.json");
            project.SetBuildProperty(mainGuid, "ENABLE_BITCODE", "NO");
            project.AddBuildProperty(frameworkGuid, "SWIFT_VERSION", "5.0");
            project.SetBuildProperty(frameworkGuid, "ENABLE_BITCODE", "NO");
            project.AddBuildProperty(frameworkGuid, "CLANG_ALLOW_NON_MODULAR_INCLUDES_IN_FRAMEWORK_MODULES", "YES");
            project.SetBuildProperty(frameworkGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            project.AddBuildProperty(frameworkGuid, "OTHER_CPLUSPLUSFLAGS", "$(OTHER_CFLAGS)");
            project.AddBuildProperty(frameworkGuid, "OTHER_CPLUSPLUSFLAGS", "-fmodules");
            project.AddBuildProperty(frameworkGuid, "OTHER_CPLUSPLUSFLAGS", "-fcxx-modules");

            project.AddBuildProperty(mainGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
            project.AddBuildProperty(mainGuid, "OTHER_LDFLAGS", "-ObjC");

            //AddAbTestFile(project, mainGuid, path);
            // Write.
            File.WriteAllText(projectPath, project.WriteToString());

            // 推送
            ProjectCapabilityManager projCapability = new ProjectCapabilityManager(
                    projectPath, "Unity-iPhone.entitlements", "Unity-iPhone");
            projCapability.AddSignInWithApple();
            projCapability.AddBackgroundModes(
                BackgroundModesOptions.BackgroundFetch | BackgroundModesOptions.RemoteNotifications);
            projCapability.WriteToFile();

            FixInfoPlist(project, path, frameworkGuid);

            // 应用名多语言
            AddLanguages(path);
            AddLocalization(path);
        }
    }

    //修改文件Info.plist
    static void FixInfoPlist(PBXProject project, string path, string targetGuid)
    {
        KBSettings settings = KBSettings.Load();
        // Get plist
        string plistPath = path + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        // Get root
        //PlistElementDict rootDict = plist.root;
        //         // Set encryption usage boolean
        //         string encryptKey = "ITSAppUsesNonExemptEncryption";
        //         rootDict.SetBoolean(encryptKey, false);
        //rootDict.SetString("NSPhotoLibraryUsageDescription", "This app requires access to the photo library.");
        //         plist.root.CreateDict("NSAppTransportSecurity").SetBoolean("NSAllowsArbitraryLoads", true);

        //         // remove exit on suspend if it exists.
        //         string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
        //         if (rootDict.values.ContainsKey(exitsOnSuspendKey))
        //         {
        //          rootDict.values.Remove(exitsOnSuspendKey);
        //         }

        //         // support http
        //         PlistElementDict allowsDict = plist.root.CreateDict("NSAppTransportSecurity");
        //         allowsDict.SetBoolean("NSAllowsArbitraryLoads", true);

        string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
        if (plist.root.values.ContainsKey(exitsOnSuspendKey))
        {
            plist.root.values.Remove(exitsOnSuspendKey);
        }

        plist.root.SetString("NSPhotoLibraryUsageDescription",
            "The APP would like to access your Photo Library for your profile photo upload");
        plist.root.SetString("NSCameraUsageDescription",
            "The APP would like to access your Camera for your profile photo upload");
        plist.root.SetString("NSMicrophoneUsageDescription",
            "The APP would like to access your Microphone for your profile video upload");
        plist.root.SetString("NSUserTrackingUsageDescription",
            "This identifier will be used to deliver personalized ads to you.");
        plist.root.SetString("NSLocationWhenInUseUsageDescription",
            "The APP needs to decide on different privacy policies based on your location information");
        plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
        //plist.root.SetString("AppLovinSdkKey", "pvF10l1xoDbXDPB9J9Stg4D8RCplShKXloSaGa7zJxC816Vp1SOxa-A_n7yFuzMPTZ6-fwfp96exHAfRNqsThN");
        plist.root.SetString("NSAdvertisingAttributionReportEndpoint",
            "https://appsflyer-skadnetwork.com/");

        var atsDic = plist.root.CreateDict("NSAppTransportSecurity");
        atsDic.SetBoolean("NSAllowsArbitraryLoads", true);
        PlistElementDict exceptionsDict = atsDic.CreateDict("NSExceptionDomains");
        PlistElementDict domainDict = exceptionsDict.CreateDict("superlight-engine.com");
        domainDict.SetBoolean("NSExceptionAllowsInsecureHTTPLoads", true);
        domainDict.SetBoolean("NSIncludesSubdomains", true);

        plist.root.SetBoolean("FacebookAdvertiserIDCollectionEnabled", true);
        plist.root.SetString("FacebookAppID", settings.facebookIosAppId);
        plist.root.SetString("FacebookClientToken", settings.facebookClientToken);
        plist.root.SetBoolean("FacebookAutoLogAppEventsEnabled", true);
        var cfBundleURLTypes = plist.root.CreateArray("CFBundleURLTypes").AddDict();
        cfBundleURLTypes.SetString("CFBundleURLName", "facebook-unity-sdk");
        cfBundleURLTypes.CreateArray("CFBundleURLSchemes").AddString("fb" + settings.facebookIosAppId);
        var cfBundleURLSchemes = plist.root.CreateArray("LSApplicationQueriesSchemes");
        cfBundleURLSchemes.AddString("fbapi");
        cfBundleURLSchemes.AddString("fb-messenger-api");
        cfBundleURLSchemes.AddString("fbauth2");
        cfBundleURLSchemes.AddString("fbshareextension");
        string key = "UIRequiredDeviceCapabilities";
        if (plist.root.values.ContainsKey(key))
        {
            plist.root.values.Remove(key);
            plist.root.CreateArray(key).AddString("arm64");
        }

        //plist.root.SetString("AppflyerAppkey", settings.appflyerDevKey);
        //plist.root.SetString("AppflyerAppId", settings.appflyerIosAppId);
        plist.root.SetString("ServerHost", settings.serverHost);
        plist.root.SetString("DpType", settings.assetsDpType);
        plist.root.SetString("DpPkg", settings.assetsIosPkg);
        // Write to plist
        File.WriteAllText(plistPath, plist.WriteToString());
    }

    static void AddFrameworks(PBXProject project, string targetGuid)
    {
        // Frameworks
        project.AddFrameworkToProject(targetGuid, "libz.dylib", false);
        project.AddFrameworkToProject(targetGuid, "libsqlite3.tbd", false);
        project.AddFrameworkToProject(targetGuid, "CoreTelephony.framework", false);
        project.AddFrameworkToProject(targetGuid, "UserNotifications.framework", false);
        project.AddFrameworkToProject(targetGuid, "AdServices.framework", true);
        project.AddFrameworkToProject(targetGuid, "AdSupport.framework", false);
        project.AddFrameworkToProject(targetGuid, "AppTrackingTransparency.framework", false);
        // project.AddFrameworkToProject(targetGuid, "iAd.framework", false);
        project.AddFrameworkToProject(targetGuid, "AuthenticationServices.framework", true);

        //string fileGuid = project.AddFile("Pods/OpenSSL-Universal/Frameworks/OpenSSL.xcframework",
        //    "Pods/OpenSSL-Universal/Frameworks/OpenSSL.xcframework", PBXSourceTree.Source);
        //string mainGuid = project.GetUnityMainTargetGuid() ??
        //                  throw new ArgumentNullException("project.GetUnityMainTargetGuid()");
        //PBXProjectExtensions.AddFileToEmbedFrameworks(project, mainGuid, fileGuid);
        //project.AddFrameworkToProject(targetGuid, "OpenSSL.xcframework", false);
    }

    static void AddFile(PBXProject project, string targetGuid, string path, string fileName)
    {
        var filePath = Path.Combine("Assets/Plugins/iOS", fileName);
        File.Copy(filePath, Path.Combine(path, fileName));
        project.AddFileToBuild(targetGuid, project.AddFile(fileName, fileName, PBXSourceTree.Source));
    }

    static void AddConfigFile(PBXProject project, string targetGuid, string path, string fileName)
    {
        var filePath = Path.Combine("Assets/Resources/KB", fileName);
        var dstPath = Path.Combine(path, fileName);
        if (File.Exists(dstPath))
            File.Delete(dstPath);
        File.Copy(filePath, dstPath);
        project.AddFileToBuild(targetGuid, project.AddFile(fileName, fileName, PBXSourceTree.Source));
    }

    static void AddAbTestFile(PBXProject project, string targetGuid, string path)
    {
        if (Directory.Exists("Assets/KBConfig/AbTest/iOS"))
        {
            List<string> files = new List<string>(Directory.GetFiles("Assets/KBConfig/AbTest/iOS"));
            files.ForEach(f =>
            {
                string fileName = Path.GetFileName(f);
                if (!fileName.EndsWith(".meta"))
                {
                    var filePath = Path.Combine("Assets/KBConfig/AbTest/iOS", fileName);
                    File.Copy(filePath, Path.Combine(path, fileName));
                    project.AddFileToBuild(targetGuid, project.AddFile(fileName, fileName, PBXSourceTree.Source));
                }
            });
        }
    }

    static void AddLanguages(string path)
    {
#if UNITY_EDITOR_WIN
#else
        string projectPath = PBXProject.GetPBXProjectPath(path);
        PBXProject project = new PBXProject();
        project.ReadFromFile(projectPath);
        project.ClearKnownRegions();
        PlistDocument plist = new PlistDocument();
        string plistPath = Path.Combine(path, "Info.plist");
        plist.ReadFromFile(plistPath);
        plist.root.SetBoolean("LSHasLocalizedDisplayName", true);
        PlistElementArray languages = plist.root.CreateArray("CFBundleLocalizations");
        foreach (string code in LANGUAGES)
        {
            project.AddKnownRegion(code);
            languages.AddString(code);
            Debug.LogFormat("Language \"{0}\" added to project", code);
        }
        plist.WriteToFile(plistPath);
        project.WriteToFile(projectPath);
#endif
    }

    static void AddLocalization(string path, string infoFile = "InfoPlist.strings")
    {
#if UNITY_EDITOR_WIN
#else
        string projectPath = PBXProject.GetPBXProjectPath(path);
        PBXProject project = new PBXProject();
        project.ReadFromFile(projectPath);
        for (int i = 0; i < LANGUAGES.Length; i++)
        {
            string code = LANGUAGES[i];
            string name = APP_NAMES[i];

            string langDir = string.Format("{0}.lproj", code);
            string directory = Path.Combine(path, langDir);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string filePath = Path.Combine(directory, infoFile);
            string relativePath = Path.Combine(langDir, infoFile);
            string content = "\"CFBundleDisplayName\" = \"" + name + "\";\n";
            File.WriteAllText(filePath, content);
            project.AddLocaleVariantFile(infoFile, code, relativePath);
            Debug.LogFormat("Localization \"{0}\" added to project", langDir);
        }
        project.WriteToFile(projectPath);
#endif
    }
}

#endif