using System.Text;
//------------------------------------------------------------
// File : AutoBundleCheck.cs
// Email: mailto:zhiqiang.yang@
// Desc : 
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using UnityEditor;
using GameFramework;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;



namespace UnityGameFramework.Editor.ResourceTools
{


    public class AutoBundleCheck
    {
        private const string SceneExtension = ".unity";
        private static readonly Regex ResourceNameRegex = new Regex(@"^([A-Za-z0-9\._-]+/)*[A-Za-z0-9\._-]+$");
        private static readonly Regex ResourceVariantRegex = new Regex(@"^[a-z0-9_-]+$");
        private static string m_ConfigurationPath;
        private static SortedDictionary<string, Resource> m_Resources;
        private static SortedDictionary<string, Asset> m_Assets;

        private static ResourceEditorController m_Controller = null;
        // private static ResourceBuilderController.BuildVersionInfo buildVersion = null;
        // private static ResourceBuilderController.VersionInfo m_versionInfo = null;

        private static Dictionary<string, GroupAsset> assetDep = new Dictionary<string, GroupAsset>();//资源引用计数器


        [UnityEditor.MenuItem("Tools/ResourceTool/AutoDivBundle")]
        public static void AutoDivBundle()
        {
            m_ConfigurationPath = Type.GetConfigurationPath<ResourceCollectionConfigPathAttribute>() ?? Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "GameFramework/Configs/ResourceCollection.xml"));
            m_Resources = new SortedDictionary<string, Resource>(StringComparer.Ordinal);
            m_Assets = new SortedDictionary<string, Asset>(StringComparer.Ordinal);

            assetDep.Clear();

            //1、找到所有的Group
            Group[] groups = EditorUtilityExtend.FindAssets<Group>();

            for (int i = 0; i < groups.Length; i++)
            {
                Group group = groups[i];

                if (group.bundleMode == BundleMode.PackByFolder)
                {
                    Group.CollectAssets(group, (path, entry) =>
                    {
                        var asset = group.CreateAsset(path, entry);
                        asset.bundle = Group.PackAsset(asset).Replace(Settings.BundleExtension, "");

                        if (!m_Resources.ContainsKey(asset.bundle))
                        {
                            Resource _res = Resource.Create(asset.bundle, null, null, LoadType.LoadFromFile, false, new string[0]);
                            m_Resources.Add(_res.Name, _res);
                        }

                        Asset _asset = Asset.Create(asset.guid, m_Resources[asset.bundle]);
                        m_Assets.Add(_asset.Guid, _asset);

                    });
                }
                else if (group.bundleMode == BundleMode.PackByTopSubFolder)
                {
                    Group.CollectAssets(group, (path, entry) =>
                    {
                        var asset = group.CreateAsset(path, entry);
                        asset.bundle = Group.PackAsset(asset).Replace(Settings.BundleExtension, "");

                        if (!m_Resources.ContainsKey(asset.bundle))
                        {
                            Resource _res = Resource.Create(asset.bundle, null, null, LoadType.LoadFromFile, false, new string[0]);
                            m_Resources.Add(_res.Name, _res);
                        }

                        Asset _asset = Asset.Create(asset.guid, m_Resources[asset.bundle]);
                        if (m_Assets.ContainsKey(_asset.Guid))
                        {
                            Debug.LogError("Key 有重复 " + _asset.Name);
                        }
                        m_Assets.Add(_asset.Guid, _asset);

                        // Debug.Log(asset.bundle + "  " + asset.path);
                        // treeView.assets.Add(asset);
                    });
                }
                else if (group.bundleMode == BundleMode.PackByFile || group.bundleMode == BundleMode.PackByFileDep)
                {
                    foreach (var asset in group.entries)
                    {
                        var path = AssetDatabase.GetAssetPath(asset);
                        var guilds = AssetDatabase.FindAssets(group.filter, new[] { path });
                        foreach (var guild in guilds)
                        {
                            var child = AssetDatabase.GUIDToAssetPath(guild);

                            var gAsset = group.CreateAsset(child, path);
                            gAsset.bundle = Group.PackAsset(gAsset).Replace(Settings.BundleExtension, "");
                            gAsset.bundle = gAsset.bundle.Replace("#", "_");

                            if (!m_Resources.ContainsKey(gAsset.bundle))
                            {
                                Resource _res = Resource.Create(gAsset.bundle, null, null, LoadType.LoadFromFile, false, new string[0]);
                                m_Resources.Add(_res.Name, _res);
                            }
                            Asset _asset = Asset.Create(gAsset.guid, m_Resources[gAsset.bundle]);
                            m_Assets.Add(_asset.Guid, _asset);


                            if (group.bundleMode == BundleMode.PackByFileDep)
                            {
                                //记录依赖添加
                                var deps = Group.GetDep(child);
                                foreach (var dep in deps)
                                {
                                    if (!assetDep.ContainsKey(dep))
                                    {
                                        var depAsset = group.CreateAsset(dep, path);
                                        depAsset.deps = 1;
                                        depAsset.bundleRsName = gAsset.bundle;
                                        assetDep.Add(dep, depAsset);
                                    }
                                    else
                                    {
                                        assetDep[dep].deps++;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (group.bundleMode == BundleMode.PackByRaw)
                {
                    foreach (var asset in group.entries)
                    {
                        var path = AssetDatabase.GetAssetPath(asset);
                        var guilds = AssetDatabase.FindAssets(group.filter, new[] { path });
                        foreach (var guild in guilds)
                        {
                            var child = AssetDatabase.GUIDToAssetPath(guild);

                            var gAsset = group.CreateAsset(child, path);
                            gAsset.bundle = Group.PackAsset(gAsset).Replace(Settings.BundleExtension, "");
                            gAsset.bundle = gAsset.bundle.Replace("#", "_");

                            if (!m_Resources.ContainsKey(gAsset.bundle))
                            {
                                Resource _res = Resource.Create(gAsset.bundle, null, null, LoadType.LoadFromBinaryAndDecrypt, false, new string[0]);
                                m_Resources.Add(_res.Name, _res);
                            }
                            Asset _asset = Asset.Create(gAsset.guid, m_Resources[gAsset.bundle]);
                            m_Assets.Add(_asset.Guid, _asset);
                        }
                    }
                }
                else if (group.bundleMode == BundleMode.PackByNumber)
                {
                    foreach (var asset in group.entries)
                    {
                        var path = AssetDatabase.GetAssetPath(asset);
                        var guilds = AssetDatabase.FindAssets(group.filter, new[] { path });
                        SortedDictionary<int, string> numbers = new SortedDictionary<int, string>();
                        foreach (var guild in guilds)
                        {
                            var child = AssetDatabase.GUIDToAssetPath(guild);
                            if (int.TryParse(Regex.Match(child, @"\d+").Value, out int number))
                            {
                                // Debug.LogError(child + "  " + number);
                                numbers.Add(number, child);
                            }
                        }
                        int index = 0;
                        Resource _res = null;
                        foreach (var item in numbers)
                        {
                            var gAsset = group.CreateAsset(item.Value, path);
                            if (index % 100 == 0)
                            {
                                gAsset.bundle = Group.PackAsset(gAsset).Replace(Settings.BundleExtension, "");
                                int no = index / 100;
                                gAsset.bundle = gAsset.bundle.Replace("#", "_") + "_" + no * 100 + "_" + (no + 1) * 100;
                                if (!m_Resources.ContainsKey(gAsset.bundle))
                                {
                                    _res = Resource.Create(gAsset.bundle, null, null, LoadType.LoadFromFile, false, new string[0]);
                                    m_Resources.Add(_res.Name, _res);
                                }
                            }
                            if (_res == null)
                            {
                                Debug.LogError("Asset is null " + item.Value);
                            }
                            Asset _asset = Asset.Create(gAsset.guid, _res);
                            m_Assets.Add(_asset.Guid, _asset);
                            index++;
                        }
                    }
                }
            }


            //2、 对每个Group种对文件进行按照划分bundle

            //3、重新编辑ResourceCollect文件

            //4、

            foreach (var item in assetDep)
            {
                // if (item.Value.deps == 1)
                // {
                var gAsset = item.Value;
                if (!m_Assets.ContainsKey(gAsset.guid))
                {
                    Asset _asset = Asset.Create(gAsset.guid, m_Resources[gAsset.bundleRsName]);
                    m_Assets.Add(_asset.Guid, _asset);
                }
                // }
            }

            Save();
        }


        private static bool Save()
        {
            // try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null));

                XmlElement xmlRoot = xmlDocument.CreateElement("UnityGameFramework");
                xmlDocument.AppendChild(xmlRoot);

                XmlElement xmlCollection = xmlDocument.CreateElement("ResourceCollection");
                xmlRoot.AppendChild(xmlCollection);

                XmlElement xmlResources = xmlDocument.CreateElement("Resources");
                xmlCollection.AppendChild(xmlResources);

                XmlElement xmlAssets = xmlDocument.CreateElement("Assets");
                xmlCollection.AppendChild(xmlAssets);

                XmlElement xmlElement = null;
                XmlAttribute xmlAttribute = null;

                foreach (Resource resource in m_Resources.Values)
                {
                    xmlElement = xmlDocument.CreateElement("Resource");
                    xmlAttribute = xmlDocument.CreateAttribute("Name");
                    xmlAttribute.Value = resource.Name;
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);

                    if (resource.Variant != null)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("Variant");
                        xmlAttribute.Value = resource.Variant;
                        xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    }

                    if (resource.FileSystem != null)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("FileSystem");
                        xmlAttribute.Value = resource.FileSystem;
                        xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    }

                    xmlAttribute = xmlDocument.CreateAttribute("LoadType");
                    xmlAttribute.Value = ((byte)resource.LoadType).ToString();
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    xmlAttribute = xmlDocument.CreateAttribute("Packed");
                    xmlAttribute.Value = resource.Packed.ToString();
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    string[] resourceGroups = resource.GetResourceGroups();
                    if (resourceGroups.Length > 0)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("ResourceGroups");
                        xmlAttribute.Value = string.Join(",", resourceGroups);
                        xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    }

                    xmlResources.AppendChild(xmlElement);
                }

                foreach (Asset asset in m_Assets.Values)
                {
                    xmlElement = xmlDocument.CreateElement("Asset");
                    xmlAttribute = xmlDocument.CreateAttribute("Guid");
                    xmlAttribute.Value = asset.Guid;
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    xmlAttribute = xmlDocument.CreateAttribute("ResourceName");
                    xmlAttribute.Value = asset.Resource.Name;
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    if (asset.Resource.Variant != null)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("ResourceVariant");
                        xmlAttribute.Value = asset.Resource.Variant;
                        xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    }

                    xmlAssets.AppendChild(xmlElement);
                }

                string configurationDirectoryName = Path.GetDirectoryName(m_ConfigurationPath);
                if (!Directory.Exists(configurationDirectoryName))
                {
                    Directory.CreateDirectory(configurationDirectoryName);
                }

                xmlDocument.Save(m_ConfigurationPath);
                AssetDatabase.Refresh();


                CheckPack();
                return true;
            }
        }

        private static void CheckPack()
        {
            m_Controller = new ResourceEditorController();
            m_Controller.OnLoadCompleted += OnLoadCompleted;

            // buildVersion = new ResourceBuilderController.BuildVersionInfo();
            // m_versionInfo = new ResourceBuilderController.VersionInfo();

            if (m_Controller.Load())
            {
                Debug.Log("Load configuration success.");
            }
            else
            {
                Debug.LogWarning("Load configuration failure.");
            }

        }

        private static void OnLoadCompleted()
        {
            ResourcesDependencies();
            AutoCheckPacked();
            SaveConfiguration();
        }


        private static void ResourcesDependencies()
        {
            // buildVersion.CalulateDependences(m_Controller.Collection, m_versionInfo);
        }

        //TODO 打标记方式待定
        private static void AutoCheckPacked()
        {
            Resource[] allResources = m_Controller.GetResources();//bundle

            Dictionary<string, Resource> allDependences = new Dictionary<string, Resource>();
            for (int i = 0; i < allResources.Length; i++)
            {
                var res = allResources[i];
                res.Packed = false;
                if (!allDependences.ContainsKey(res.Name))
                {
                    allDependences.Add(res.Name, res);
                }
            }


            // for (int i = 0; i < m_versionInfo.PackedResources.Count; i++)
            // {
            //     if (allDependences.ContainsKey(m_versionInfo.PackedResources[i]))
            //     {
            //         Resource rs = allDependences[m_versionInfo.PackedResources[i]];

            //         if (rs.Name.Contains("Change/ChapterBg"))
            //         {
            //             Debug.Log(rs.Name);
            //         }

            //         rs.Packed = true;
            //     }
            // }
        }

        private static void SaveConfiguration()
        {
            if (m_Controller.Save())
            {
                Debug.Log("Save configuration success.");
            }
            else
            {
                Debug.LogWarning("Save configuration failure.");
            }
        }


    }

}