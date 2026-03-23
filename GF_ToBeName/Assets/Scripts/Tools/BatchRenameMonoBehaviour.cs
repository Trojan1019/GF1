// #if UNITY_EDITOR
//
// using UnityEditor;
// using UnityEngine;
// using System;
// using System.IO;
// using System.Text.RegularExpressions;
// using System.Linq;
// using System.Collections.Generic;
//
// public class BatchRenameAllMonoBehaviours : EditorWindow
// {
//     private string targetFolder = "Assets"; // 默认目标文件夹
//     private HashSet<string> generatedClassNames = new HashSet<string>(); // 存储已生成的类名
//     private static List<string> skipFolderName = new List<string>() // 跳过的文件夹
//     {
//         "Assets/Scripts/Tools/BatchRenameMonoBehaviour",
//         "Assets/AmplifyShaderEditor",
//         "Assets/GameFramework",
//         "Assets/KB",
//         "Assets/OPS",
//         "Assets/Plugins",
//         "Assets/Scripts/GamEntry",
//         "Assets/Scripts/DataTables",
//         "Assets/Scripts/Definition",
//     };
//     private static List<string> speicalFileName = new List<string>() // 跳过的文件夹排除的文件
//     {
//         "Assets/KB/Scripts",
//         "Assets/Scripts/Definition/UIFormType",
//     };
//     private static List<string> nameSpaces = new List<string>() // 替换的命名空间
//     {
//         "NewSideGame",
//         "KBSDK",
//     };
//
//     private Dictionary<string, string> classNameMap = new Dictionary<string, string>(); // 老类名到新类名的映射
//     private Dictionary<string, string> inheritanceMap = new Dictionary<string, string>(); // 类名到父类名的映射
//     private Dictionary<string, string> inheritanceFilePathMap = new Dictionary<string, string>(); // 类名到文件的映射
//     private Dictionary<string, List<string>> classChangedFilePath = new Dictionary<string, List<string>>(); // 老类名到新类名更改的文件路径
//     private Dictionary<string, List<string>> classScriptChangedFilePath = new Dictionary<string, List<string>>(); // 老类名到新类名更改的文件路径
//     private Dictionary<string, string> nameSpaceNameMap = new Dictionary<string, string>(); // 老命名空间到新命名空间的映射
//     private Dictionary<string, List<string>> nameSpaceChangedFilePath = new Dictionary<string, List<string>>(); // 老命名空间到新命名空间更改的文件路径
//     private HashSet<string> processedClasses = new HashSet<string>(); // 已处理的类名
//     private string[] scriptFiles;
//
//     [MenuItem("Tools/Batch Rename All MonoBehaviours")]
//     public static void ShowWindow()
//     {
//         var window = GetWindow<BatchRenameAllMonoBehaviours>("Batch Rename All MonoBehaviours");
//     }
//
//     public static bool BuildPreConfoundProcessor()
//     {
//         try
//         {
//             string targetFolder = "Assets";
//
//             var renamer = new BatchRenameAllMonoBehaviours();
//             renamer.RenameAllScriptsInFolder(targetFolder);
//
//             return true;
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"BuildPreConfoundProcessor failed: {e.Message}");
//             return false;
//         }
//     }
//
//     private Vector2 scrollPosition;
//
//     private void OnGUI()
//     {
//         GUILayout.Label("Batch Rename MonoBehaviours", EditorStyles.boldLabel);
//
//         targetFolder = EditorGUILayout.TextField("Target Folder", targetFolder);
//
//         if (GUILayout.Button("Start Renaming"))
//         {
//             RenameAllScriptsInFolder(targetFolder);
//             Debug.Log("Renaming completed.");
//         }
//
//         scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
//
//         if (classNameMap.Count > 0)
//         {
//             GUILayout.Space(10);
//             GUILayout.Label("Renamed Classes:", EditorStyles.boldLabel);
//
//             foreach (var entry in classNameMap)
//             {
//                 GUILayout.Label($"{entry.Key} -> {entry.Value}");
//             }
//         }
//
//         EditorGUILayout.EndScrollView();
//     }
//
//     public void RenameAllScriptsInFolder(string folderPath)
//     {
//         if (!Directory.Exists(folderPath))
//         {
//             Debug.LogError("Target folder does not exist!");
//             return;
//         }
//
//         generatedClassNames.Clear();
//         classNameMap.Clear();
//         inheritanceMap.Clear();
//         inheritanceFilePathMap.Clear();
//         classChangedFilePath.Clear();
//         classScriptChangedFilePath.Clear();
//         nameSpaceNameMap.Clear();
//         nameSpaceChangedFilePath.Clear();
//         processedClasses.Clear();
//         scriptFiles = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);
//         try
//         {
//             // EditorUtility.DisplayProgressBar("Rename Progress", "NameSpaceTask Start.", 0f);
//             // NameSpaceTask(folderPath);
//             EditorUtility.DisplayProgressBar("Rename Progress", "ClassTask Start.", 0.25f);
//             ClassTask();
//             EditorUtility.DisplayProgressBar("Rename Progress", "ScriptTask Start.", 0.5f);
//             ScriptTask(folderPath);
//             EditorUtility.DisplayProgressBar("Rename Progress", "ResourcesTask Start.", 0.75f);
//             ResourcesTask(folderPath);
//             EditorUtility.DisplayProgressBar("Rename Progress", "WriteFile Start.", 1f);
//             WriteFile();
//             Debug.Log("All MonoBehaviour scripts renamed and references updated.");
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"Failed to rename MonoBehaviour scripts: {e.Message}");
//             return;
//         }
//         finally
//         {
//             EditorUtility.ClearProgressBar();
//             // AssetDatabase.Refresh();
//         }
//     }
//
//     private void NameSpaceTask(string folderPath)
//     {
//         foreach (var item in nameSpaces)
//         {
//             var newNamespace = GenerateUniqueClassName();
//             nameSpaceNameMap.Add(item, newNamespace);
//         }
//         ReplaceNamespaceInScripts(folderPath);
//     }
//
//     private void ClassTask()
//     {
//         // 第一步：建立继承关系映射
//         foreach (var filePath in scriptFiles)
//         {
//             if (speicalFileName.Any(speicalName => filePath.Contains(speicalName)))
//             {
//                 Debug.Log($"Special file: {filePath}");
//             }
//             else
//             {
//                 if (skipFolderName.Any(skipName => filePath.Contains(skipName)))
//                     continue;
//             }
//
//             string content;
//             try
//             {
//                 content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError($"Failed to read file: {filePath}. Error: {e.Message}");
//                 continue;
//             }
//
//             Match match = Regex.Match(content, @"\b(?:public|private|protected|internal|sealed|static|abstract|partial)?\s*class\s+(\w+)\s*:\s*(\w+)");
//             if (match.Success)
//             {
//                 string className = match.Groups[1].Value;
//                 string baseClassName = match.Groups[2].Value;
//
//                 if (!inheritanceMap.ContainsKey(className))
//                 {
//                     inheritanceMap[className] = baseClassName;
//                     inheritanceFilePathMap[className] = filePath;
//                 }
//             }
//         }
//
//         int i = 0;
//         foreach (var filePath in scriptFiles)
//         {
//             if (speicalFileName.Any(speicalName => filePath.Contains(speicalName)))
//             {
//                 Debug.Log($"Special file: {filePath}");
//             }
//             else
//             {
//                 if (skipFolderName.Any(skipName => filePath.Contains(skipName)))
//                 {
//                     continue;
//                 }
//             }
//
//             RenameClassInFile(filePath);
//             EditorUtility.DisplayProgressBar("Task Progress", $"Processing File {i++ + 1}/{scriptFiles.Length}...", (float)(i + 1) / scriptFiles.Length);
//         }
//
//         var classNameMapSnapshot = classNameMap.ToList();
//         foreach (var entry in classNameMapSnapshot)
//         {
//             UpdateDerivedClasses(entry.Key);
//         }
//     }
//
//     private void ScriptTask(string folderPath)
//     {
//         UpdateScriptReferences(folderPath);
//     }
//
//     private void ResourcesTask(string folderPath)
//     {
//         // UpdatePrefabAndSceneReferences(folderPath);
//     }
//
//     private void RenameClassInFile(string filePath)
//     {
//         string content;
//         try
//         {
//             content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"Failed to read file: {filePath}. Error: {e.Message}");
//             return;
//         }
//
//         Match match = Regex.Match(content, @"\b(?:public|private|protected|internal|sealed|static|abstract|partial)?\s*class\s+(\w+)\s*(?:<[^>]+>)?\s*:\s*(MonoBehaviour|UGuiForm)");
//         if (match.Success)
//         {
//             string oldClassName = match.Groups[1].Value;
//             RenameFile(content, oldClassName, filePath);
//         }
//     }
//
//     private void RenameFile(string content, string oldClassName, string filePath)
//     {
//         string newClassName = "";
//         if (classNameMap.TryGetValue(oldClassName, out string existingNewClassName))
//         {
//             newClassName = existingNewClassName;
//         }
//         else
//         {
//             newClassName = GenerateUniqueClassName();
//         }
//         Debug.Log($"Renaming class: {oldClassName} -> {newClassName}");
//
//         content = Regex.Replace(content, $@"\bclass\s+{oldClassName}\b", $"class {newClassName}");
//         try
//         {
//             File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"Failed to write file: {filePath}. Error: {e.Message}");
//             return;
//         }
//
//         try
//         {
//             string fileName = Path.GetFileNameWithoutExtension(filePath);
//             string[] nameParts = fileName.Split('.');
//             string suffix = nameParts.Length > 1 ? string.Join(".", nameParts.Skip(1)) : "";
//
//             string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), newClassName + (string.IsNullOrEmpty(suffix) ? "" : "." + suffix) + ".cs");
//             File.Move(filePath, newFilePath);
//             int index = Array.IndexOf(scriptFiles, filePath);
//             if (index != -1)
//             {
//                 scriptFiles[index] = newFilePath;
//                 if (classChangedFilePath.TryGetValue(oldClassName, out List<string> list))
//                 {
//                     list.Add($"Class {oldClassName} ReNameClass to {newClassName} --> In File Change {filePath} to {newFilePath}");
//                 }
//                 else
//                 {
//                     classChangedFilePath[oldClassName] = new List<string>() { $"Class {oldClassName} ReNameClass to {newClassName} --> In File Change {filePath} to {newFilePath}" };
//                 }
//             }
//
//             string oldMetaPath = filePath + ".meta";
//             string newMetaPath = newFilePath + ".meta";
//             if (File.Exists(oldMetaPath))
//             {
//                 File.Move(oldMetaPath, newMetaPath);
//             }
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"Failed to rename file: {filePath}. Error: {e.Message}");
//             return;
//         }
//
//         classNameMap[oldClassName] = newClassName;
//     }
//
//     private void UpdateDerivedClasses(string oldClassName)
//     {
//         foreach (var entry in inheritanceMap.Where(e => e.Value == oldClassName))
//         {
//             string derivedClassName = entry.Key;
//
//             if (processedClasses.Contains(derivedClassName))
//                 continue;
//
//             processedClasses.Add(derivedClassName);
//
//             // 第一步：建立继承关系映射
//             foreach (var filePath in scriptFiles)
//             {
//                 if (speicalFileName.Any(speicalName => filePath.Contains(speicalName)))
//                 {
//                     Debug.Log($"Special file: {filePath}");
//                 }
//                 else
//                 {
//                     if (skipFolderName.Any(skipName => filePath.Contains(skipName)))
//                         continue;
//                 }
//
//                 string content;
//                 try
//                 {
//                     content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
//                 }
//                 catch (Exception e)
//                 {
//                     Debug.LogError($"Failed to read file: {filePath}. Error: {e.Message}");
//                     continue;
//                 }
//
//                 Match match = Regex.Match(content, @"\b(?:public|private|protected|internal|sealed|static|abstract|partial)?\s*class\s+(\w+)\s*:\s*(\w+)");
//                 if (match.Success && match.Groups[1].Value == derivedClassName)
//                 {
//                     RenameFile(content, derivedClassName, filePath);
//                 }
//             }
//             UpdateDerivedClasses(derivedClassName);
//         }
//     }
//
//     private string GenerateUniqueClassName()
//     {
//         const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
//         const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
//         var random = new System.Random();
//         string newClassName;
//
//         do
//         {
//             newClassName = letters[random.Next(letters.Length)] +
//                            new string(Enumerable.Range(0, 11).Select(_ => chars[random.Next(chars.Length)]).ToArray());
//         } while (generatedClassNames.Contains(newClassName));
//
//         generatedClassNames.Add(newClassName);
//         return newClassName;
//     }
//
//     private void UpdateScriptReferences(string folderPath)
//     {
//         int i = 0;
//         foreach (var filePath in scriptFiles)
//         {
//             if (speicalFileName.Any(speicalName => filePath.Contains(speicalName)))
//             {
//                 Debug.Log($"Special file: {filePath}");
//             }
//             else
//             {
//                 if (skipFolderName.Any(skipName => filePath.Contains(skipName)))
//                     continue;
//             }
//
//             string content;
//             try
//             {
//                 content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError($"Failed to read file: {filePath}. Error: {e.Message}");
//                 continue;
//             }
//
//             bool modified = false;
//
//             foreach (var entry in classNameMap)
//             {
//                 string oldClassName = entry.Key;
//                 string newClassName = entry.Value;
//
//                 string pattern = $@"(?<![""'\/\w]){oldClassName}(?![""'\/\w])";
//                 string newContent = Regex.Replace(content, pattern, newClassName);
//                 if (!modified && !newContent.Equals(content))
//                 {
//                     modified = true;
//                     if (classScriptChangedFilePath.TryGetValue(oldClassName, out List<string> list))
//                     {
//                         list.Add($"UpdateScriptReferences {oldClassName} --> {filePath} has References, Change Complete!");
//                     }
//                     else
//                     {
//                         classScriptChangedFilePath[oldClassName] = new List<string>() { $"UpdateScriptReferences {oldClassName} --> {filePath} has References, Change Complete!" };
//                     }
//                 }
//                 content = newContent;
//             }
//
//             if (modified)
//             {
//                 try
//                 {
//                     File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
//                     EditorUtility.DisplayProgressBar("Task Progress", $"Processing File {i++ + 1}/{scriptFiles.Length}...", (float)(i + 1) / scriptFiles.Length);
//                     Debug.Log($"Updated script references in file: {filePath}");
//                 }
//                 catch (Exception e)
//                 {
//                     Debug.LogError($"Failed to write file: {filePath}. Error: {e.Message}");
//                 }
//             }
//         }
//     }
//
//     private void ReplaceNamespaceInScripts(string folderPath)
//     {
//         var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
//         foreach (var file in files)
//         {
//             if (file.Contains(skipFolderName[0]))
//             {
//                 continue;
//             }
//
//             if (file.EndsWith(".cs") || file.EndsWith(".prefab") || file.EndsWith(".unity") || file.EndsWith(".json"))
//             {
//                 try
//                 {
//                     foreach (var item in nameSpaceNameMap)
//                     {
//                         var content = File.ReadAllText(file);
//                         var pattern = $@"\b{Regex.Escape(item.Key)}\b";
//                         var updatedContent = Regex.Replace(content, pattern, item.Value);
//
//                         if (content != updatedContent)
//                         {
//                             File.WriteAllText(file, updatedContent);
//                             Debug.Log($"Updated namespace in file: {file}");
//                             if (nameSpaceChangedFilePath.TryGetValue(item.Key, out List<string> list))
//                             {
//                                 list.Add($"NameSpace Replace {item.Key} to {item.Value} --> {file} Change Complete!");
//                             }
//                             else
//                             {
//                                 nameSpaceChangedFilePath[item.Key] = new List<string>() { $"NameSpace Replace {item.Key} to {item.Value} --> {file} Change Complete!" };
//                             }
//                         }
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     Debug.LogError($"Failed to update namespace in file: {file}{ex.Message}");
//                 }
//             }
//         }
//     }
//
//     private void WriteFile()
//     {
//         System.IO.DirectoryInfo dataPath = new DirectoryInfo(Application.dataPath);
//         System.IO.DirectoryInfo docPath = dataPath.Parent;
//         string filePath = Path.Combine(docPath.FullName + "/exportClass.txt");
//         StreamWriter strmWriterObj;
//         if (File.Exists(filePath))
//         {
//             File.Delete(filePath);
//         }
//
//         File.Create(filePath).Close();
//         strmWriterObj = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
//
//         strmWriterObj.WriteLine(string.Format("NameSpace Changed: "));
//         foreach (var np in nameSpaceChangedFilePath)
//         {
//             List<string> list = np.Value;
//             for (int i = 0; i < list.Count; i++)
//             {
//                 strmWriterObj.WriteLine($"{list[i]}");
//             }
//         }
//
//         strmWriterObj.WriteLine(string.Format("\nClass Changed: "));
//         foreach (var cp in classChangedFilePath)
//         {
//             List<string> list = cp.Value;
//             for (int i = 0; i < list.Count; i++)
//             {
//                 strmWriterObj.WriteLine($"{list[i]}");
//             }
//         }
//
//         strmWriterObj.WriteLine(string.Format("\nReferences Changed: "));
//         foreach (var cp in classScriptChangedFilePath)
//         {
//             List<string> list = cp.Value;
//             for (int i = 0; i < list.Count; i++)
//             {
//                 strmWriterObj.WriteLine($"{list[i]}");
//             }
//         }
//         strmWriterObj.Close();
//     }
// }
//
// #endif
