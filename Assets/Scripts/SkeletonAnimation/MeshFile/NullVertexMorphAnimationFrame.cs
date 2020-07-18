using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullVertexMorphObject : INullStream
    {
        protected NullDataStructType VertexDataType;
        protected int MeshObjectIndex;
        protected List<Vector3> VertexPosArray;
        protected List<Vector3> NormalArray;


        public NullVertexMorphObject()
        {
            VertexDataType = 0;
            VertexPosArray = new List<Vector3>();
            NormalArray = new List<Vector3>();
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteByte((byte)VertexDataType);
            size += stream.WriteInt(MeshObjectIndex);
            size += stream.WriteList(VertexPosArray, false);
            size += stream.WriteList(NormalArray, true);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            byte b;
            bool res = stream.ReadByte(out b);
            res &= stream.ReadInt(out MeshObjectIndex);
            res &= stream.ReadList(out VertexPosArray);
            res &= stream.ReadList(out NormalArray, Count);
            return res;
        }

        public int Count { get { return VertexPosArray.Count; } }
    }

    public class NullVertexMorphAnimationFrame : INullStream
    {
        protected List<NullVertexMorphObject> mVertexMorphObjectList;

        public NullVertexMorphAnimationFrame()
        {
            mVertexMorphObjectList = new List<NullVertexMorphObject>();
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

        public NullVertexMorphObject AppendVertexMorphObject()
        {
            NullVertexMorphObject av = new NullVertexMorphObject();
            mVertexMorphObjectList.Add(av);
            return av;
        }

        public void Clear()
        {
            mVertexMorphObjectList.Clear();
        }
    }
}
