using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullVertexMorphObject : INullStream
    {
        protected byte m_vertexDataType;
        protected ushort m_vertexCount;
        protected List<Vector3> m_vertexPosArray;
        protected List<Vector3> m_normalArray;
        protected uint m_meshObjectIndex;

        //protected float[] m_offset; // 3
        //protected double m_scale;

        public NullVertexMorphObject(byte dst, ushort vc)
        {
            m_vertexDataType = dst;
            m_vertexCount = vc;
            m_vertexPosArray = new List<Vector3>();
            m_normalArray = new List<Vector3>();
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadUInt(out m_meshObjectIndex);
            res &= stream.ReadList(out m_vertexPosArray);
            res &= stream.ReadList(out m_normalArray);
            return res;
        }
    }

    public class NullVertexMorphAnimationFrame : INullStream
    {
        protected List<NullVertexMorphObject> m_vertexMorphObjectList;
        protected ushort m_vertexMorphObjectCount;


        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(out count);
            byte dst = 0;
            ushort vc = 0;
            for (int i = 0; i < count; i++)
            {
                res = stream.ReadByte(out dst);
                res &= stream.ReadUShort(out vc);
                NullVertexMorphObject av = AppendVertexMorphObject(dst, vc);
                res &= av.LoadFromStream(stream);
            }
            return res;
        }

        public NullVertexMorphObject AppendVertexMorphObject(byte dst, ushort vc)
        {
            if (m_vertexMorphObjectList == null)
            {
                m_vertexMorphObjectList = new List<NullVertexMorphObject>();
            }
            NullVertexMorphObject av = new NullVertexMorphObject(dst, vc);
            m_vertexMorphObjectList.Add(av);
            m_vertexMorphObjectCount++;
            return av;
        }

        public void Clear()
        {
            m_vertexMorphObjectList = null;
            m_vertexMorphObjectCount = 0;
        }
    }
}
