using GameFramework;
// using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public partial class MyGameTools
{
    public const string LinkFile = "Assets/link.xml";
    public const string STRIP_GENERATE_TAG = "<!--GENERATE_TAG-->";
    private const string MatchPattern = "<assembly[\\s]+fullname[\\s]*=[\\s]*\"([^\"]+)\"";
    [MenuItem("Tools/Strip Config Window", false, 1)]
    public static void ShowStripConfigEditor()
    {
        EditorWindow.GetWindow<StripLinkConfigWindow>("Strip LinkConfig Editor").Show();
    }

    public static string GetStripAssembliesDir2021(BuildTarget target)
    {
        string projectDir = Directory.GetParent(Application.dataPath).FullName;
#if UNITY_STANDALONE_WIN
            return $"{projectDir}/Library/Bee/artifacts/WinPlayerBuildProgram/ManagedStripped";
#elif UNITY_ANDROID
        return $"{projectDir}/Library/Bee/artifacts/Android/ManagedStripped";
#elif UNITY_IOS
            return $"{projectDir}/Temp/StagingArea/Data/Managed/tempStrip";
#elif UNITY_WEBGL
            return $"{projectDir}/Library/Bee/artifacts/WebGL/ManagedStripped";
#elif UNITY_EDITOR_OSX
            return $"{projectDir}/Library/Bee/artifacts/MacStandalonePlayerBuildProgram/ManagedStripped";
#else
            throw new NotSupportedException("GetOriginBuildStripAssembliesDir");
#endif
    }

    /// <summary>
    /// 获取项目全部dll
    /// </summary>
    /// <returns></returns>
    public static string[] GetProjectAssemblyDlls()
    {
        List<string> dlls = new List<string>();
        var dllDir = GetStripAssembliesDir2021(EditorUserBuildSettings.activeBuildTarget);
        if (!Directory.Exists(dllDir))
        {
            return dlls.ToArray();
        }
        var files = Directory.GetFiles(dllDir, "*.dll", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            var fileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
            if (!dlls.Contains(fileName)) dlls.Add(fileName);
        }
        return dlls.ToArray();
    }
    /// <summary>
    /// 获取已经配置到link.xml里的dll
    /// </summary>
    /// <returns></returns>
    public static string[] GetSelectedAssemblyDlls()
    {
        List<string> dlls = new List<string>();
        if (!File.Exists(LinkFile))
        {
            return dlls.ToArray();
        }
        var lines = File.ReadAllLines(LinkFile);
        int generateBeginLine = lines.Length, generateEndLine = lines.Length;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (generateBeginLine >= lines.Length && line.Trim().CompareTo(STRIP_GENERATE_TAG) == 0)
            {
                generateBeginLine = i;
            }
            else if (generateEndLine >= lines.Length && line.Trim().CompareTo(STRIP_GENERATE_TAG) == 0)
            {
                generateEndLine = i;
            }
            if (((i > generateBeginLine && generateEndLine >= lines.Length) || (i > generateBeginLine && i < generateEndLine)) && !string.IsNullOrWhiteSpace(line))
            {
                var match = Regex.Match(line, MatchPattern);
                if (match.Success)
                {
                    var assemblyName = match.Result("$1");
                    if (!dlls.Contains(assemblyName)) dlls.Add(assemblyName);
                }
            }

        }
        return dlls.ToArray();
    }
    public static bool Save2LinkFile(string[] stripList)
    {
        if (!File.Exists(LinkFile))
        {
            File.WriteAllText(LinkFile, $"<linker>{Environment.NewLine}{STRIP_GENERATE_TAG}{Environment.NewLine}{STRIP_GENERATE_TAG}</linker>");
        }
        var lines = File.ReadAllLines(LinkFile);
        FindGenerateLine(lines, out int beginLineIdx, out int endLineIdx);
        int headIdx = ArrayUtility.FindIndex(lines, line => line.Trim().CompareTo("<linker>") == 0);
        if (beginLineIdx >= lines.Length)
        {
            ArrayUtility.Insert(ref lines, headIdx + 1, STRIP_GENERATE_TAG);
        }
        if (endLineIdx >= lines.Length)
        {
            ArrayUtility.Insert(ref lines, headIdx + 1, STRIP_GENERATE_TAG);
        }
        FindGenerateLine(lines, out beginLineIdx, out endLineIdx);
        int insertIdx = beginLineIdx;
        for (int i = 0; i < stripList.Length; i++)
        {
            insertIdx = beginLineIdx + i + 1;
            if (insertIdx >= endLineIdx)
            {
                ArrayUtility.Insert(ref lines, endLineIdx, FormatStripLine(stripList[i]));
            }
            else
            {
                lines[insertIdx] = FormatStripLine(stripList[i]);
            }
        }
        while ((insertIdx + 1) < lines.Length && lines[insertIdx + 1].Trim().CompareTo(STRIP_GENERATE_TAG) != 0)
        {
            ArrayUtility.RemoveAt(ref lines, insertIdx + 1);
        }
        try
        {
            File.WriteAllLines(LinkFile, lines, System.Text.Encoding.UTF8);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("Save2LinkFile Failed:{0}", e.Message);
            return false;
        }
    }
    private static string FormatStripLine(string assemblyName)
    {
        return $"\t<assembly fullname=\"{assemblyName}\" preserve=\"all\" />";
    }
    private static void FindGenerateLine(string[] lines, out int beginLineIdx, out int endLineIdx)
    {
        beginLineIdx = endLineIdx = lines.Length;
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (beginLineIdx >= lines.Length && line.Trim().CompareTo(STRIP_GENERATE_TAG) == 0)
            {
                beginLineIdx = i;
            }
            else if (endLineIdx >= lines.Length && line.Trim().CompareTo(STRIP_GENERATE_TAG) == 0)
            {
                endLineIdx = i;
            }
        }
    }
}