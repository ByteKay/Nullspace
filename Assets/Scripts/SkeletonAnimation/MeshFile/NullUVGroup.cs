using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullUVGroup : INullStream
    {
        public enum UVType
        {
            UVT_DEFAULT = 0,        //default, equal uv for base-texture
            UVT_NORMAL_MAP = 1,     //uv for normal/gloss map
            UVT_LIGHT_MAP = 2,      //uv for light-map
            UVT_RESERVE = 3,        //maxium texture layer 
        };

        protected byte UVDataType;
        protected byte mUVType;
        protected double UVScale;
        protected List<Vector2> UVArray;
        protected ushort CurrentVersion;
        private ushort UVCount;
        private uint UVGroupSize;

        public NullUVGroup(ushort version, ushort size, byte type)
        {
            CurrentVersion = version;
            UVScale = 1.0;
            UVDataType = (byte)NullDataStructType.DST_FLOAT;
            UVCount = size;
            mUVType = type;
            UVArray = null;
        }

        public void Clear()
        {
            UVArray = null;
            UVCount = 0;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            uint dataSize = UVCount;
            if (UVArray == null)
            {
                dataSize = 0;
            }
            int size = stream.WriteUInt(dataSize);
            if (dataSize == 0)
            {
                return size;
            }
            size += stream.WriteByte(mUVType);
            size += stream.WriteByte(UVDataType);
            size += stream.WriteList(UVArray, true);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadUShort(out UVCount);
            if (UVCount == 0)
            {
                return res;
            }
            res &= stream.ReadByte(out mUVType);
            res &= stream.ReadByte(out UVDataType);
            res &= stream.ReadList(out UVArray, UVCount);
            return res;
        }
    }

    public class NullUVGroups : INullStream
    {
        protected ushort CurrentVersion;
        protected Dictionary<byte, NullUVGroup> UVGroupList;
        private ushort UVGroupSize;
        private byte InternalSize;
        private ushort VertexCount;

        public NullUVGroups(ushort currentVersion, ushort vertexCount)
        {
            CurrentVersion = currentVersion;
            VertexCount = vertexCount;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteByte(InternalSize);
            for (byte i = 0; i < InternalSize; i++)
            {
                size += UVGroupList[i].SaveToStream(stream);
            }
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            byte size = 0;
            bool res = stream.ReadByte(out size);
            for (int i = 0; i < size; i++)
            {
                //add a uv whitch type id == 0xff, will always success
                NullUVGroup group = AppendUV(0xff);
                if (group != null)
                {
                    res &= group.LoadFromStream(stream);
                }
            }
            return res;
        }

        public NullUVGroup AppendUV(byte uvType)
        {
            if (UVGroupList == null)
            {
                UVGroupList = new Dictionary<byte, NullUVGroup>();
            }
            if (UVGroupList.ContainsKey(uvType))
            {
                return null;
            }
            NullUVGroup group = new NullUVGroup(CurrentVersion, UVGroupSize, uvType);
            UVGroupList.Add(uvType, group);
            InternalSize++;
            return group;
        }

        public void Clear()
        {
            UVGroupList = null;
            InternalSize = 0;
        }

        public int GetUVGroupCount()
        {
            return UVGroupList.Count;
        }
    }

}
