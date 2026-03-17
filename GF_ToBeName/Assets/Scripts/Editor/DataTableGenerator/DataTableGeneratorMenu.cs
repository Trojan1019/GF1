using System.Collections.Generic;
using System.IO;
using GameFramework;
using UnityEditor;
using UnityEngine;

namespace NewSideGame
{
    public sealed class DataTableGeneratorMenu
    {
        [MenuItem("Tools/Excel/Generate DataTables")]
        private static void GenerateDataTables()
        {
            GenerateDataTables(IsMac());
        }

        [MenuItem("Tools/Excel/Generate Language")]
        private static void GenerateLanguage()
        {
            GenerateLanguage(IsMac());
        }

        private static bool IsMac()
        {
            string os = SystemInfo.operatingSystem;

            if (os.Contains("Windows"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 生成数据表格
        /// </summary>
        /// <param name="isMac"></param>
        private static void GenerateDataTables(bool isMac)
        {
            string workingDirectory = $"{Application.dataPath}/../../Doc/Excel/tool";
            string outputPath = $"{Application.dataPath}/../../Doc/Excel/csv";

            if (isMac)
            {
                ProcessCommandUtility.ProcessCommand("/bin/sh", "ExcelToCsv.sh", workingDirectory);
            }
            else
            {
                ProcessCommandUtility.ProcessCommand("ExcelToCsv.bat", "", workingDirectory);
            }


            string dataTablePath = Utility.Path.GetRegularPath($"{Application.dataPath}/{DataTableGenerator.DataTablePath.Replace("Assets/", "")}");
            if (Directory.Exists(dataTablePath))
            {
                string[] fileNames = Directory.GetFiles(dataTablePath, "*", SearchOption.AllDirectories);
                foreach (string fileName in fileNames)
                {
                    File.Delete(fileName);
                }
            }


            DirectoryInfo directoryInfo = new DirectoryInfo(outputPath);
            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                if (fileInfo.Extension == ".csv")
                {
                    string dataTableName = fileInfo.Name.Replace(fileInfo.Extension, "");

                    if (dataTableName == "GlobalConfig")
                        continue;

                    DataTableProcessor dataTableProcessor = DataTableGenerator.CreateDataTableProcessor(fileInfo.FullName);
                    if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                    {
                        Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                        break;
                    }

                    DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
                    DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
                }
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 生成多语言配置
        /// </summary>
        /// <param name="isMac"></param>
        private static void GenerateLanguage(bool isMac)
        {
            string workingDirectory = $"{Application.dataPath}/../..//Doc/Language/tool";

            if (isMac)
            {
                ProcessCommandUtility.ProcessCommand("/bin/sh", "uiText.sh", workingDirectory);
            }
            else
            {
                // ProcessCommand("uiText.bat", "", Application.dataPath + "/../Doc/TranslationTool/");
            }

            AssetDatabase.Refresh();
        }

    }
}
