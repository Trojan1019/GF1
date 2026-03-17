using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;
using Object = UnityEngine.Object;

namespace NewSideGame
{
    public static class ResourceComponentExtension
    {
        public static void LoadAsset<T>(this ResourceComponent resourceComponent, int assetID,
            Action<T> loadAssetSuccessCallback, Action loadAssetFailureCallback)
        {
            string assetPath =
                ResourceIdentificationTool.Instance.GetAssetPathById(assetID);

            resourceComponent.LoadAsset(assetPath, typeof(T), Constant.AssetPriority.PrefabAsset,
                new LoadAssetCallbacks(
                    (assetName, asset, duration, userData) =>
                    {
                        if (asset is T tObject)
                        {
                            loadAssetSuccessCallback.Invoke(tObject);
                        }
                        else if (asset is GameObject gameObject)
                        {
                            T component = gameObject.GetComponent<T>();
                            loadAssetSuccessCallback.Invoke(component);
                        }
                        else
                        {
                            Debug.LogError($"资源{assetPath}:加载失败");
                        }
                    },
                    (assetName, status, errorMessage, userData) =>
                    {
                        loadAssetFailureCallback?.Invoke();
                    }), null);
        }

        public static void LoadSpriteSync(this ResourceComponent resourceComponent,
            string atlasName, string spriteLocalPath,
            Action<Sprite> loadAssetSuccessCallback, Action loadAssetFailureCallback = null)
        {

        }

        public static void LoadAssetSync<T>(this ResourceComponent resourceComponent,
            string assetPath,
            Action<T> loadAssetSuccessCallback, Action loadAssetFailureCallback = null)
        {
            Type assetType = typeof(T).IsSubclassOf(typeof(MonoBehaviour)) ? typeof(Object) : typeof(T);

            resourceComponent.LoadAssetSync(assetPath, assetType,
                new LoadAssetCallbacks(
                    (assetName, asset, duration, userData) =>
                    {
                        if (asset is T tObject)
                        {
                            loadAssetSuccessCallback.Invoke(tObject);
                        }
                        else if (asset is GameObject gameObject)
                        {
                            T component = gameObject.GetComponent<T>();
                            loadAssetSuccessCallback.Invoke(component);
                        }
                        else
                        {
                            Debug.LogError($"资源{assetPath}:加载失败");
                        }
                    },
                    (assetName, status, errorMessage, userData) =>
                    {
                        Debug.LogError($"资源{assetPath}:加载失败");
                        loadAssetFailureCallback?.Invoke();
                    }), null);
        }

        public static void LoadAssetSync<T>(this ResourceComponent resourceComponent,
            int assetID,
            Action<T> loadAssetSuccessCallback, Action loadAssetFailureCallback = null)
        {
            string assetPath =
                ResourceIdentificationTool.Instance.GetAssetPathById(assetID);

            if (string.IsNullOrEmpty(assetPath)) return;

            LoadAssetSync<T>(resourceComponent, assetPath, loadAssetSuccessCallback, loadAssetFailureCallback);
        }
    }
}