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

    public class DRItem : DataRowBase
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
        /// 获取Style。
        /// </summary>
        public int Style
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

        /// <summary>
        /// 获取Icon2。
        /// </summary>
        public string Icon2
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
            Style = int.Parse(columnStrings[index++]);
            Icon = columnStrings[index++];
            Icon2 = columnStrings[index++];

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
                    Style = binaryReader.Read7BitEncodedInt32();
                    Icon = binaryReader.ReadString();
                    Icon2 = binaryReader.ReadString();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private KeyValuePair<int, string>[] m_Icon = null;

        public int IconCount
        {
            get
            {
                return m_Icon.Length;
            }
        }

        public string GetIcon(int id)
        {
            foreach (KeyValuePair<int, string> i in m_Icon)
            {
                if (i.Key == id)
                {
                    return i.Value;
                }
            }

            throw new GameFrameworkException(Utility.Text.Format("GetIcon with invalid id '{0}'.", id));
        }

        public string GetIconAt(int index)
        {
            if (index < 0 || index >= m_Icon.Length)
            {
                throw new GameFrameworkException(Utility.Text.Format("GetIconAt with invalid index '{0}'.", index));
            }

            return m_Icon[index].Value;
        }

        private void GeneratePropertyArray()
        {
            m_Icon = new KeyValuePair<int, string>[]
            {
                new KeyValuePair<int, string>(2, Icon2),
            };
        }
    }
}
