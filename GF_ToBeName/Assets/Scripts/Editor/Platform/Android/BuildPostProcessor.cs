// //
// // Copyright (c) 2017 eppz! mobile, Gergely Borbás (SP)
// //
// // http://www.twitter.com/_eppz
// //
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// // INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// // PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// // HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// // CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
// // OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// //
//
// #if UNITY_ANDROID
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
// using UnityEditor;
// using UnityEditor.Callbacks;
// using UnityEditor.Android;
//
// public class BuildPostProcessor : IPostGenerateGradleAndroidProject
// {
//     private static readonly string AndroidRootPath = "Editor/Platform/Android";
//
//     //private static readonly string SourceActivityPath = "Resources/KB/google-services.json";
//     //private static readonly string DestActivityPath = "../launcher/google-services.json";
//
//     private static readonly string SourceGradleDirPath = $"{AndroidRootPath}/gradle";
//     private static readonly string DestGradleDirPath = "/gradle";
//
//     private static readonly string SourceGradlewPath = $"{AndroidRootPath}/gradlew";
//     private static readonly string SourceGradlewBatPath = $"{AndroidRootPath}/gradlew.bat";
//
//     private static readonly string SourceLibDirPath = "/unityLibrary/libs";
//     private static readonly string DestLibDirPath = "/launcher/libs";
//
//     // settings.gradle
//     private static readonly string SourceSettingsGradlePathAPK = "Editor/Platform/Android/settingsAPK.gradle";
//     private static readonly string SourceSettingsGradlePathAAB = "Editor/Platform/Android/settingsAAB.gradle";
//
//     public int callbackOrder
//     {
//         get { return 0; }
//     }
//
//     // public void OnPostGenerateGradleAndroidProject(string path)
//     // {
//     //     string sourcePath = Path.Combine(Application.dataPath, SourceActivityPath);
//     //     string destPath = Path.Combine(path, DestActivityPath);
//     //     File.Copy(sourcePath, destPath, true);
//     // }
//
//     [PostProcessBuildAttribute(1)]
//     public static void OnPostProcessBuild(BuildTarget target, string path)
//     {
//         // 拷贝相关资源
//         CopyAndroidResources(path);
//     }
//
//     public static void CopyAndroidResources(string pathToBuiltProject)
//     {
//         // 拷贝工程基础文件
//         CopyGradleFolder(pathToBuiltProject);
//         CopyLibFolder(pathToBuiltProject);
//
//         File.Copy(Path.Combine(Application.dataPath, SourceGradlewPath), pathToBuiltProject + "/gradlew", true);
//         File.Copy(Path.Combine(Application.dataPath, SourceGradlewBatPath), pathToBuiltProject + "/gradlew.bat", true);
//
//         if (EditorPrefs.GetString("EXPORT_GRADLE_PROJECT", "APK").Equals("AAB"))
//         {
//             File.Copy(Path.Combine(Application.dataPath, SourceSettingsGradlePathAAB), pathToBuiltProject + "/settings.gradle", true);
//         }
//         else
//         {
//             File.Copy(Path.Combine(Application.dataPath, SourceSettingsGradlePathAPK), pathToBuiltProject + "/settings.gradle", true);
//         }
//     }
//
//     private static void CopyGradleFolder(string pathToBuiltProject)
//     {
//         string srcDir = Path.Combine(Application.dataPath, SourceGradleDirPath);
//         string dstDir = pathToBuiltProject + DestGradleDirPath;
//         CopyFolder(srcDir, dstDir);
//     }
//
//     private static void CopyLibFolder(string pathToBuiltProject)
//     {
//         string srcDir = pathToBuiltProject + SourceLibDirPath;
//         string dstDir = pathToBuiltProject + DestLibDirPath;
//         CopyFolder(srcDir, dstDir);
//     }
//
//     private static void CopyFolder(string sourceDir, string destDir, bool includeMeta = true)
//     {
//         if (!Directory.Exists(destDir))
//         {
//             Directory.CreateDirectory(destDir);
//         }
//
//         try
//         {
//             string[] fileList = Directory.GetFiles(sourceDir, "*");
//             foreach (string f in fileList)
//             {
//                 string fName = f.Substring(sourceDir.Length + 1);
//
//                 if (!includeMeta && f.EndsWith(".meta"))
//                     continue;
//
//                 File.Copy(Path.Combine(sourceDir, fName), Path.Combine(destDir, fName), true);
//             }
//
//             string[] dirList = Directory.GetDirectories(sourceDir, "*");
//             foreach (string d in dirList)
//             {
//                 string currentdir = destDir + "/" + d.Substring(d.LastIndexOf("/") + 1);
//                 if (!Directory.Exists(currentdir))
//                 {
//                     Directory.CreateDirectory(currentdir);
//                 }
//                 CopyFolder(d, currentdir, includeMeta);
//             }
//         }
//         catch (DirectoryNotFoundException dirNotFound)
//         {
//             throw new DirectoryNotFoundException(dirNotFound.Message);
//         }
//     }
// }
// #endif