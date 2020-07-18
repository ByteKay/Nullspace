using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public enum UVType
    {
        UVT_DEFAULT = 0,        //default, equal uv for base-texture
        UVT_NORMAL_MAP = 1,     //uv for normal/gloss map
        UVT_LIGHT_MAP = 2,      //uv for light-map
        UVT_RESERVE = 3,        //maxium texture layer 
    };

    public class NullUVGroup : INullStream
    {
        public int CurrentVersion;
        protected UVType mUVType;
        protected double mUVScale;
        protected NullDataStructType mUVDataType;
        protected List<Vector2> mUVArray;

        public NullUVGroup()
        {
            mUVScale = 1.0;
            mUVType = UVType.UVT_DEFAULT;
            mUVDataType = NullDataStructType.DST_FLOAT;
            mUVArray = new List<Vector2>();
        }

        public NullUVGroup(int version, UVType type) : this()
        {
            CurrentVersion = version;
            mUVType = type;
        }

        public void Clear()
        {
            mUVArray.Clear();
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteList(mUVArray, false);
            if (mUVArray.Count == 0)
            {
                return size;
            }
            size += stream.WriteByte((byte)mUVType);
            size += stream.WriteByte((byte)mUVDataType);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadList(out mUVArray);
            if (mUVArray.Count == 0)
            {
                return res;
            }
            byte b;
            res &= stream.ReadByte(out b);
            mUVType = (UVType)b;
            res &= stream.ReadByte(out b);
            mUVDataType = (NullDataStructType)b;
            return res;
        }
    }

    public class NullUVGroups : INullStream
    {
        public int CurrentVersion;
        protected int mVertexCount;
        protected List<NullUVGroup> mUVGroupList;

        public NullUVGroups()
        {
            mUVGroupList = new List<NullUVGroup>();
            mVertexCount = 0;
        }

        public NullUVGroups(int currentVersion, int vertexCount) : this()
        {
            CurrentVersion = currentVersion;
            mVertexCount = vertexCount;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            return stream.WriteList(mUVGroupList, false);
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            return stream.ReadList(out mUVGroupList);
        }

        public NullUVGroup AppendUV(byte uvType)
        {
            //if (mUVGroupList.ContainsKey(uvType))
            //{
            //    return null;
            //}
            //NullUVGroup group = new NullUVGroup(CurrentVersion, uvType);
            //mUVGroupList.Add(uvType, group);
            //mInternalSize++;
            return null;
        }

        public void Clear()
        {
            mUVGroupList.Clear();
        }

        public int GetUVGroupCount()
        {
            return mUVGroupList.Count;
        }
    }

}
