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

    public class DRUIForm : DataRowBase
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
        /// 获取AssetName。
        /// </summary>
        public string AssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取UIGroupName。
        /// </summary>
        public string UIGroupName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取UIResolution。
        /// </summary>
        public int UIResolution
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取AllowMultiInstance。
        /// </summary>
        public bool AllowMultiInstance
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取PauseCoveredUIForm。
        /// </summary>
        public bool PauseCoveredUIForm
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Mutex。
        /// </summary>
        public bool Mutex
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
            AssetName = columnStrings[index++];
            UIGroupName = columnStrings[index++];
            UIResolution = int.Parse(columnStrings[index++]);
            AllowMultiInstance = bool.Parse(columnStrings[index++]);
            PauseCoveredUIForm = bool.Parse(columnStrings[index++]);
            Mutex = bool.Parse(columnStrings[index++]);

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
                    AssetName = binaryReader.ReadString();
                    UIGroupName = binaryReader.ReadString();
                    UIResolution = binaryReader.Read7BitEncodedInt32();
                    AllowMultiInstance = binaryReader.ReadBoolean();
                    PauseCoveredUIForm = binaryReader.ReadBoolean();
                    Mutex = binaryReader.ReadBoolean();
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
