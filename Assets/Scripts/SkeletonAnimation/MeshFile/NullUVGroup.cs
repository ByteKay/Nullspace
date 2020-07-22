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
        protected int mUVCount;

        public NullUVGroup()
        {
            mUVScale = 1.0;
            mUVType = UVType.UVT_DEFAULT;
            mUVDataType = NullDataStructType.DST_FLOAT;
            mUVArray = new List<Vector2>();
            mUVCount = 0;
        }

        public NullUVGroup(int version, UVType type) : this()
        {
            CurrentVersion = version;
            mUVType = type;
        }

        public NullUVGroup(int version, UVType type, int uvSize) : this()
        {
            CurrentVersion = version;
            mUVType = type;
            mUVCount = uvSize;
        }

        public UVType GetUVType()
        {
            return mUVType;
        }

        public bool GetUV(int index, ref Vector2 uv)
        {
            if (index >= mUVArray.Count)
            {
                return false;
            }
            uv = mUVArray[index];
            return true;
        }

        public bool SetUV(int index, Vector2 uv)
        {
            if (index >= mUVArray.Count)
            {
                return false;
            }
            mUVArray[index] = uv;
            return true;
        }

        public bool GetUVRange(out Vector2 min, out Vector2 max)
        {
            min = Vector2.one * 1e20f;
            max = Vector2.one * -1e20f;
            if (mUVArray.Count == 0)
            {
                return false;
            }
            for (int i = 0; i < mUVArray.Count; i++)
            {
                min = Vector2.Min(min, mUVArray[i]);
                max = Vector2.Max(max, mUVArray[i]);
            }
            return true;
        }

        public void Clear()
        {
            mUVArray.Clear();
            mUVCount = 0;
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

        public int GetUVGroupCount()
        {
            return mUVGroupList.Count;
        }

        public NullUVGroup this[UVType uvType]
        {
            get
            {
                for (int i = 0; i < GetUVGroupCount(); i++)
                {
                    NullUVGroup group = mUVGroupList[i];
                    if (group.GetUVType() == uvType)
                    {
                        return group;
                    }
                }
                return null;
            }
        }

        public NullUVGroup GetUVGroupByIndex(int index)
        {
            return index < GetUVGroupCount() ? mUVGroupList[index] : null;
        }

        public bool HasDefaultUV()
        {
            return this[UVType.UVT_DEFAULT] != null;
        }

        public bool HasNormalmapUV()
        {
            return this[UVType.UVT_NORMAL_MAP] != null;
        }

        public bool HasLightmapUV()
        {
            return this[UVType.UVT_LIGHT_MAP] != null;
        }

        public void RemoveUVGroup(UVType uvType)
        {
            if (uvType == UVType.UVT_DEFAULT)
            {
                return;
            }
            mUVGroupList.RemoveAll((uvGroup) => { return uvGroup.GetUVType() == uvType; });
        }

        public NullUVGroup AppendUV(UVType uvType)
        {
            NullUVGroup group = this[uvType];
            if (group != null)
            {
                return null;
            }
            group = new NullUVGroup(CurrentVersion, uvType);
            mUVGroupList.Add(group);
            return group;
        }

        public NullUVGroup AppendUV(UVType uvType, int uvSize)
        {
            NullUVGroup group = this[uvType];
            if (group != null)
            {
                return null;
            }
            group = new NullUVGroup(CurrentVersion, uvType, uvSize);
            mUVGroupList.Add(group);
            return group;
        }

        public void Clear()
        {
            mUVGroupList.Clear();
        }
    }

}
