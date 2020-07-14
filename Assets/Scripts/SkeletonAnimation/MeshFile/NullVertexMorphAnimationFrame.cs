using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullVertexMorphObject : INullStream
    {
        protected byte VertexDataType;
        protected ushort VertexCount;
        protected List<Vector3> VertexPosArray;
        protected List<Vector3> NormalArray;
        protected uint MeshObjectIndex;

        public NullVertexMorphObject()
        {
            VertexDataType = 0;
            VertexCount = 0;
            VertexPosArray = new List<Vector3>();
            NormalArray = new List<Vector3>();
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteByte(VertexDataType);
            size += stream.WriteUShort(VertexCount);
            size += stream.WriteUInt(MeshObjectIndex);
            size += stream.WriteList(VertexPosArray, true);
            size += stream.WriteList(NormalArray, true);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadByte(out VertexDataType);
            res &= stream.ReadUShort(out VertexCount);
            res &= stream.ReadUInt(out MeshObjectIndex);
            res &= stream.ReadList(out VertexPosArray, VertexCount);
            res &= stream.ReadList(out NormalArray, VertexCount);
            return res;
        }
    }

    public class NullVertexMorphAnimationFrame : INullStream
    {
        protected List<NullVertexMorphObject> VertexMorphObjectList;
        protected ushort VertexMorphObjectCount;


        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteUShort(VertexMorphObjectCount);
            for (int i = 0; i < VertexMorphObjectCount; i++)
            {
                size += VertexMorphObjectList[i].SaveToStream(stream);
            }
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            ushort count;
            bool res = stream.ReadUShort(out count);
            for (int i = 0; i < count; i++)
            {
                NullVertexMorphObject av = AppendVertexMorphObject();
                res &= av.LoadFromStream(stream);
            }
            return res;
        }

        public NullVertexMorphObject AppendVertexMorphObject()
        {
            if (VertexMorphObjectList == null)
            {
                VertexMorphObjectList = new List<NullVertexMorphObject>();
            }
            NullVertexMorphObject av = new NullVertexMorphObject();
            VertexMorphObjectList.Add(av);
            VertexMorphObjectCount++;
            return av;
        }

        public void Clear()
        {
            VertexMorphObjectList = null;
            VertexMorphObjectCount = 0;
        }
    }
}
