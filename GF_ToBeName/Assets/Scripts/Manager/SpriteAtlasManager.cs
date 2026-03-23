using System;
using System.Collections.Generic;
using GameFramework;
using UnityEngine;

namespace NewSideGame
{
    public class SpriteAtlasId
    {
        public static readonly string Item = "Item";
        public static readonly string Home = "Home";
        public static readonly string Launch = "Launch";
        public static readonly string Puzzle = "Puzzle";
        public static readonly string Vip = "Vip";
        public static readonly string Treasure = "Treasure";
        public static readonly string LuckySpin = "LuckySpin"; 
        public static readonly string SpecialLv = "SpecialLv";
        
        public static readonly string Block = "Block";
    }

    public class SpriteAtlasManager : Singleton<SpriteAtlasManager>
    {
        public static string SpritePath = "Assets/RawAsset/Sprites/";
        public static string TravelSpritePath = "Assets/RawAsset/TravelSprites/";

        /// <summary>
        /// 获取精灵
        /// </summary>
        /// <param name="atlasId"></param>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public Sprite GetSprite(string atlasName, string spriteName)
        {
            Sprite sprite = null;
            string assetPath = SpritePath + atlasName + "/" + spriteName + ".png";
            GameEntry.Resource.LoadAssetSync<Sprite>(assetPath, (asset) => { sprite = asset; });
            return sprite;
        }

        public void SetSprite(UnityEngine.UI.Image img, string atlasName, string spriteName)
        {
            img.sprite = GetSprite(atlasName, spriteName);
        }

        public Sprite GetTravelSprite(string atlasName, string spriteName)
        {
            Sprite sprite = null;
            string assetPath = TravelSpritePath + atlasName + "/" + spriteName + ".png";
            GameEntry.Resource.LoadAssetSync<Sprite>(assetPath, (asset) => { sprite = asset; });
            return sprite;
        }
    }
}