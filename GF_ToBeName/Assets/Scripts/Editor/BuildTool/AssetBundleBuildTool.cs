using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

public static class AssetBundleBuildTool
{
    public static bool BuildAssetBundle(Platform platform, string applicableGameVersion, int internalResourceVersion, bool forceRebuildAssetBundle)
    {
        ResourceBuilderController resourceBuilderController = new ResourceBuilderController();

        resourceBuilderController.AssetBundleCompression = AssetBundleCompressionType.LZ4;
        resourceBuilderController.RefreshCompressionHelper();
        resourceBuilderController.Platforms = platform;
        resourceBuilderController.ApplicableGameVersion = applicableGameVersion;
        resourceBuilderController.InternalResourceVersion = internalResourceVersion;
        resourceBuilderController.ForceRebuildAssetBundleSelected = forceRebuildAssetBundle;

        string outputDirectory = $"{Application.dataPath}/../../Build/AssetBundle/";
        if (!System.IO.Directory.Exists(outputDirectory))
        {
            System.IO.Directory.CreateDirectory(outputDirectory);
        }

        resourceBuilderController.OutputDirectory = outputDirectory;
        resourceBuilderController.BuildEventHandlerTypeName = "NewSideGame.Editor.StarForceBuildEventHandler";
        resourceBuilderController.RefreshBuildEventHandler();

        resourceBuilderController.OutputPackedSelected = false;
        resourceBuilderController.OutputFullSelected = false;
        resourceBuilderController.OutputPackageSelected = true;

        return resourceBuilderController.BuildResources();
    }
}