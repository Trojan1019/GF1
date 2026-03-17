using System.IO;
using UnityEditor;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR

namespace NewSideGame
{
    public static class PictureNameTool
    {
        private const string rawAssetPath = "Assets/RawAsset/TravelSprites";

        [UnityEditor.MenuItem("Tools/ResourceTool/RandomNames", false, 1)]
        public static void RandomNames()
        {
            string[] directories = Directory.GetDirectories(rawAssetPath);

            foreach (string directory in directories)
            {
                string[] files = Directory.GetFiles(directory);
                bool isGoogleBg = directory.Contains("TravelSprites/GoogleBg");
                if (isGoogleBg)
                {
                    List<string> pngFiles = new List<string>();
                    int _index = 0;
                    foreach (string file in files)
                    {
                        if (file.ToLower().EndsWith(".jpg") || file.ToLower().EndsWith(".jpeg"))
                        {
                            continue;
                        }

                        if (file.ToLower().EndsWith(".png"))
                        {
                            string newFileName = $"{++_index}{Path.GetExtension(file)}";
                            string newFilePath = Path.Combine(directory, newFileName);
                            File.Move(file, newFilePath);
                            pngFiles.Add(newFilePath);
                        }
                    }

                    if (pngFiles.Count > 0)
                    {
                        System.Random rng = new System.Random();
                        pngFiles = pngFiles.OrderBy(x => rng.Next()).ToList();

                        int index = 0;
                        for (int i = 0; i < pngFiles.Count; i++)
                        {
                            string oldFilePath = pngFiles[i];
                            string newFileName = $"picture_{++index}{Path.GetExtension(oldFilePath)}";
                            string newFilePath = Path.Combine(directory, newFileName);

                            File.Move(oldFilePath, newFilePath);
                        }
                    }
                }
            }

            Debug.Log("Files renamed successfully.");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        [UnityEditor.MenuItem("Tools/ResourceTool/全命名TravelSprites", false, 1)]
        public static void UpdateNames()
        {
            string[] directories = Directory.GetDirectories(rawAssetPath);

            foreach (string directory in directories)
            {
                string[] files = Directory.GetFiles(directory);
                bool isSpecial = directory.Contains("TravelSprites/Special");
                Array.Sort(files, (x, y) =>
                {
                    int xNumber = ExtractNumberFromFileName(x);
                    int yNumber = ExtractNumberFromFileName(y);
                    return xNumber.CompareTo(yNumber);
                });

                int index = 0;
                for (int i = 0; i < files.Length; i++)
                {
                    string oldFilePath = files[i];
                    if (oldFilePath.ToLower().EndsWith(".jpg") || oldFilePath.ToLower().EndsWith(".jpeg"))
                    {
                        Debug.LogError($"File {oldFilePath} is not a PNG file. Skipping renaming.");
                        continue;
                    }

                    if (!oldFilePath.ToLower().EndsWith(".png"))
                    {
                        continue;
                    }

                    string newFileName = "";
                    if (isSpecial)
                    {
                        newFileName = $"picture_{++index * 5}{Path.GetExtension(oldFilePath)}";
                    }
                    else
                    {
                        newFileName = $"picture_{++index}{Path.GetExtension(oldFilePath)}";
                    }

                    string newFilePath = Path.Combine(directory, newFileName);

                    bool skip = File.Exists(newFilePath);
                    if (File.Exists(newFilePath))
                    {
                        Debug.LogWarning($"File {newFilePath} already exists. Skipping renaming of {oldFilePath}.");
                        continue;
                    }

                    File.Move(oldFilePath, newFilePath);
                }
            }

            Debug.Log("Files renamed successfully.");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        // [MenuItem("Tools/ResourceTool/选中文件夹背景图规则命名", false, 2)]
        // public static void RenamePicturesInSelectedFolder()
        // {
        //     string selectedFolderPath = GetSelectedFolderPath();

        //     if (string.IsNullOrEmpty(selectedFolderPath))
        //     {
        //         Debug.LogError("No folder selected. Please select a folder in the Project window.");
        //         return;
        //     }

        //     string[] files = Directory.GetFiles(selectedFolderPath);

        //     int index = 0;
        //     for (int i = 0; i < files.Length; i++)
        //     {
        //         string oldFilePath = files[i];
        //         if (oldFilePath.EndsWith(".meta")) continue;

        //         string newFileName = $"picture_{++index}{Path.GetExtension(oldFilePath)}";
        //         string newFilePath = Path.Combine(selectedFolderPath, newFileName);

        //         if (File.Exists(newFilePath))
        //         {
        //             Debug.LogWarning($"File {newFilePath} already exists. Skipping renaming of {oldFilePath}.");
        //             continue;
        //         }

        //         File.Move(oldFilePath, newFilePath);
        //     }

        //     Debug.Log("Files renamed successfully in the selected folder.");
        //     UnityEditor.AssetDatabase.SaveAssets();
        //     UnityEditor.AssetDatabase.Refresh();
        // }

        private static string GetSelectedFolderPath()
        {
            UnityEngine.Object selectedObject = Selection.activeObject;

            if (selectedObject == null)
            {
                return null;
            }

            string path = AssetDatabase.GetAssetPath(selectedObject);

            if (Directory.Exists(path))
            {
                return path;
            }

            return null;
        }

        private static int ExtractNumberFromFileName(string fileName)
        {
            var match = Regex.Match(Path.GetFileNameWithoutExtension(fileName), @"\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        }
    }
}
#endif