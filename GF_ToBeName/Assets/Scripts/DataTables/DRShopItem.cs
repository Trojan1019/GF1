//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace NewSideGame
{
    /// <summary>
    /// Id。
    /// </summary>

    public class DRShopItem : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取Id。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取Type。
        /// </summary>
        public int Type
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Name。
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取IapId。
        /// </summary>
        public int IapId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Desc。
        /// </summary>
        public string Desc
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Rewards。
        /// </summary>
        public string Rewards
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Icon。
        /// </summary>
        public string Icon
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
            Type = int.Parse(columnStrings[index++]);
            Name = columnStrings[index++];
            IapId = int.Parse(columnStrings[index++]);
            Desc = columnStrings[index++];
            Rewards = columnStrings[index++];
            Icon = columnStrings[index++];

            GeneratePropertyArray();
            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    Type = binaryReader.Read7BitEncodedInt32();
                    Name = binaryReader.ReadString();
                    IapId = binaryReader.Read7BitEncodedInt32();
                    Desc = binaryReader.ReadString();
                    Rewards = binaryReader.ReadString();
                    Icon = binaryReader.ReadString();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private void GeneratePropertyArray()
        {

        }
    }
}
