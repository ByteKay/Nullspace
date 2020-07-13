using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MeshFile
{
    public class HexUVGroup : IStream
    {
        public enum UVType
        {
            UVT_DEFAULT = 0,        //default, equal uv for base-texture
            UVT_NORMAL_MAP = 1,     //uv for normal/gloss map
            UVT_LIGHT_MAP = 2,      //uv for light-map
            UVT_RESERVE = 3,        //maxium texture layer 
        };

        protected byte m_UVDataType;
        protected byte m_UVType;
        protected double m_UVScale;
        protected List<Vector2> m_UVArray;
        protected ushort mCurrentVersion;
        private ushort m_UVCount;
        private uint m_UVGroupSize;
        private byte uvType;

        public HexUVGroup(ushort version, ushort size, byte type)
        {
            mCurrentVersion = version;
            m_UVScale = 1.0;
            m_UVDataType = (byte)DataStructType.DST_FLOAT;
            m_UVCount = size;
            m_UVType = type;
            m_UVArray = null;
        }

        public void Clear()
        {
            m_UVArray = null;
        }

        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadUShort(ref m_UVCount);
            if (m_UVCount == 0)
            {
                return res;
            }
            res &= stream.ReadByte(ref m_UVType);
            res &= stream.ReadByte(ref m_UVDataType);
            m_UVArray = new List<Vector2>();
            res &= stream.ReadVector2Lst(ref m_UVArray);
            return res;
        }
    }

    public class HexUVGroups : IStream
    {
        protected ushort mCurrentVersion;
        protected Dictionary<byte, HexUVGroup> m_UVGroupList;
        private ushort m_UVGroupSize;
        private byte m_internalSize;
        private ushort m_vertexCount;

        public HexUVGroups(ushort currentVersion, ushort vertexCount)
        {
            mCurrentVersion = currentVersion;
            m_vertexCount = vertexCount;
        }

        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            Clear();
            byte size = 0;
            bool res = stream.ReadByte(ref size);
            for (int i = 0; i < size; i++)
            {
                //add a uv whitch type id == 0xff, will always success
                HexUVGroup group = AppendUV(0xff);
                if (group != null)
                {
                    res &= group.LoadFromStream(stream);
                }
            }
            return res;
        }

        public HexUVGroup AppendUV(byte uvType)
        {
            if (m_UVGroupList == null)
            {
                m_UVGroupList = new Dictionary<byte, HexUVGroup>();
            }
            if (m_UVGroupList.ContainsKey(uvType))
            {
                return null;
            }
            HexUVGroup group = new HexUVGroup(mCurrentVersion, m_UVGroupSize, uvType);
            m_UVGroupList.Add(uvType, group);
            m_internalSize++;
            return group;
        }

        public void Clear()
        {
            m_UVGroupList = null;
            m_internalSize = 0;
        }
    }

}
