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

        public NullUVGroup(int version, UVType type, int uvSize) : this()
        {
            CurrentVersion = version;
            mUVType = type;
            mUVArray = NullMeshFile.Make<Vector2>(uvSize);
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

        public bool ExtractToTrianglesFromIndexedPrimitives(List<Vector3Int> faceIndices)
        {
            List<Vector2> uvArray = new List<Vector2>();
            int triangleCount = faceIndices.Count;
            for (int i = 0; i < triangleCount; ++i)
            {
                Vector3Int face = faceIndices[i];
                uvArray.Add(mUVArray[face.x]);
                uvArray.Add(mUVArray[face.y]);
                uvArray.Add(mUVArray[face.z]);
            }
            mUVArray = uvArray;
            return true;
        }

        public bool BuildIndexedPrimitives(List<NullMergeIndex> indexMapping)
        {
            if (mUVArray.Count == 0)
            {
                return false;
            }
            List<Vector2> newData = NullMeshObject.ReCreateCompactData(mUVArray, indexMapping);
            mUVArray = newData;
            return true;
        }

        private float RoundUpValue(float value, float errorLimit)
        {
            if (value > 0)
            {
                float roundValue = (float)((int)(value + 0.5f));
                float absError = Mathf.Abs(value - roundValue);
                if (absError <= errorLimit)
                {
                    return roundValue;
                }
                else
                {
                    return value;
                }
            }
            else
            {
                float roundValue = (float)((int)(value - 0.5f));
                float absError = Mathf.Abs(value - roundValue);
                if (absError <= errorLimit)
                {
                    return roundValue;
                }
                else
                {
                    return value;
                }
            }
        }
        public bool StandarizeUVs(float errorLimit)
        {
            if (mUVArray == null || mUVArray.Count == 0)
            {
                return false;
            }
            //assume the owner mesh-object had been extracted to triangles and in float format
            for (int i = 0; i < mUVArray.Count / 3; i++)
            {
                //get uvs for triangle
                Vector2 uv1 = mUVArray[i * 3];
                Vector2 uv2 = mUVArray[i * 3 + 1];
                Vector2 uv3 = mUVArray[i * 3 + 2];

                //roundup values
                uv1.x = RoundUpValue(uv1.x, errorLimit);
                uv1.y = RoundUpValue(uv1.y, errorLimit);
                uv2.x = RoundUpValue(uv2.x, errorLimit);
                uv2.y = RoundUpValue(uv2.y, errorLimit);
                uv3.x = RoundUpValue(uv3.x, errorLimit);
                uv3.y = RoundUpValue(uv3.y, errorLimit);

                Vector2 min = Vector2.Min(uv3, Vector2.Min(uv1, uv2));
                Vector2 max = Vector2.Max(uv3, Vector2.Max(uv1, uv2));
                //standarize uv
                float du = 0.0f;
                float dv = 0.0f;
                while (min.x < 0.0f)
                {
                    min.x += 1.0f;
                    max.x += 1.0f;
                    du += 1.0f;
                }
                while (min.y < 0.0f)
                {
                    min.y += 1.0f;
                    max.y += 1.0f;
                    dv += 1.0f;
                }
                while (min.x > 1.0f)
                {
                    min.x -= 1.0f;
                    max.x -= 1.0f;
                    du -= 1.0f;
                }
                while (min.y > 1.0f)
                {
                    min.y -= 1.0f;
                    max.y -= 1.0f;
                    dv -= 1.0f;
                }
                Vector2 duv = new Vector2(du, dv);
                //adjust triangle-uvs center
                uv1 += duv;
                uv2 += duv;
                uv3 += duv;
                //calculating and pushing back new values
                mUVArray[i * 3] = uv1;
                mUVArray[i * 3 + 1] = uv2;
                mUVArray[i * 3 + 2] = uv3;
            }
            return true;
        }

        public List<Vector2> GetUVData()
        {
            return mUVArray;
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

        public bool ExtractToTrianglesFromIndexedPrimitives(List<Vector3Int> faceIndices)
        {
            bool res = false;
            for (int i = 0; i < mUVGroupList.Count; i++)
            {
                res |= mUVGroupList[i].ExtractToTrianglesFromIndexedPrimitives(faceIndices);
            }
            return res;
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

        public int Count { get { return mUVGroupList.Count; } }

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

        public void BuildIndexedPrimitives(List<NullMergeIndex> indexMapping)
        {
            for (int i = 0; i < mUVGroupList.Count; ++i)
            {
                mUVGroupList[i].BuildIndexedPrimitives(indexMapping);
            }
            mVertexCount = indexMapping.Count;
        }
    }

}
