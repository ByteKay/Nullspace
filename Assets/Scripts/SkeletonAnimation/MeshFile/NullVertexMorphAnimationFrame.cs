using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullVertexMorphObject : INullStream
    {
        protected NullDataStructType mVertexDataType;
        protected int mMeshObjectIndex;
        protected List<Vector3> mVertexPosArray;
        protected List<Vector3> mNormalArray;


        public NullVertexMorphObject()
        {
            mVertexDataType = NullDataStructType.DST_FLOAT;
            mVertexPosArray = new List<Vector3>();
            mNormalArray = new List<Vector3>();
        }

        public NullVertexMorphObject(NullDataStructType vertexDataType, int vertexCount) : this()
        {
            mVertexDataType = vertexDataType;
            for (int i = 0; i < vertexCount; ++i)
            {
                mVertexPosArray.Add(Vector3.zero);
                mNormalArray.Add(Vector3.zero);
            }
        }
        public bool SetVertexAndNormal(int index, ref Vector3 vertex, ref Vector3 normal)
        {
            return SetVertex(index, vertex) && SetNormal(index, normal);
        }

        public bool SetVertex(int index, Vector3 vertex)
        {
            if (index >= GetVertexCount())
            {
                return false;
            }
            mVertexPosArray[index] = vertex;
            return true;
        }

        public bool SetNormal(int index, Vector3 normal)
        {
            if (index >= GetVertexCount())
            {
                return false;
            }
            mNormalArray[index] = normal;
            return true;
        }

        public bool GetVertex(int index, out Vector3 vertex)
        {
            vertex = Vector3.zero;
            if (index >= GetVertexCount())
            {
                return false;
            }
            vertex = mVertexPosArray[index];
            return true;
        }

        public bool GetNormal(int index, out Vector3 normal)
        {
            normal = Vector3.zero;
            if (index >= GetVertexCount())
            {
                return false;
            }
            normal = mNormalArray[index];
            return true;
        }

        public List<Vector3> GetVertexData()
        {
            return mVertexPosArray;
        }

        public int GetMeshObjectIndex()
        {
            return mMeshObjectIndex;
        }

        public void SetMeshObjectIndex(int index)
        {
            mMeshObjectIndex = index;
        }

        public int GetVertexCount()
        {
            return mVertexPosArray.Count;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteByte((byte)mVertexDataType);
            size += stream.WriteInt(mMeshObjectIndex);
            size += stream.WriteList(mVertexPosArray, false);
            size += stream.WriteList(mNormalArray, true);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            byte b;
            bool res = stream.ReadByte(out b);
            res &= stream.ReadInt(out mMeshObjectIndex);
            res &= stream.ReadList(out mVertexPosArray);
            res &= stream.ReadList(out mNormalArray, GetVertexCount());
            return res;
        }

        public void Clear()
        {
            mVertexPosArray.Clear();
            mNormalArray.Clear();
        }
    }

    public class NullVertexMorphAnimationFrame : INullStream
    {
        protected List<NullVertexMorphObject> mVertexMorphObjectList;

        public NullVertexMorphAnimationFrame()
        {
            mVertexMorphObjectList = new List<NullVertexMorphObject>();
        }

        public NullVertexMorphObject AppendVertexMorphObject(NullDataStructType vertexDataType, int vertexCount)
        {
            NullVertexMorphObject av = new NullVertexMorphObject(vertexDataType, vertexCount);
            mVertexMorphObjectList.Add(av);
            return av;
        }

        public NullVertexMorphObject FindMorphAnimationNodeByIndex(int index)
        {
            for (int i = 0; i < mVertexMorphObjectList.Count; i++)
            {
                NullVertexMorphObject mo = mVertexMorphObjectList[i];
                if (mo.GetMeshObjectIndex() == index)
                {
                    return mo;
                }
            }
            return null;
        }

        public NullVertexMorphObject this[int index]
        {
            get
            {
                return index < mVertexMorphObjectList.Count ? mVertexMorphObjectList[index] : null;
            }
        }

        public int GetMorphObjectCount()
        {
            return mVertexMorphObjectList.Count;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteList(mVertexMorphObjectList, false);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadList(out mVertexMorphObjectList);
            return res;
        }

        public void Clear()
        {
            mVertexMorphObjectList.Clear();
        }
    }
}
