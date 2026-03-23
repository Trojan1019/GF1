using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ResourcesIdentificationPostprocessor : AssetPostprocessor
{
    private void OnPreprocessAsset()
    {
        if(assetPath.Contains("#"))
        {
            ResourceIdentificationTool.InitResourceIdentificationInfoMapInProject();
        }
    }
} 
