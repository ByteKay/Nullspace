using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MeshFile
{
    public class HexVertexMorphObject : IStream
    {
        protected byte m_vertexDataType;
        protected ushort m_vertexCount;
        protected List<Vector3> m_vertexPosArray;
        protected List<Vector3> m_normalArray;
        protected uint m_meshObjectIndex;

        //protected float[] m_offset; // 3
        //protected double m_scale;

        public HexVertexMorphObject(byte dst, ushort vc)
        {
            m_vertexDataType = dst;
            m_vertexCount = vc;
            m_vertexPosArray = new List<Vector3>();
            m_normalArray = new List<Vector3>();
        }

        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            bool res = stream.ReadUInt(ref m_meshObjectIndex);
            res &= stream.ReadVector3Lst(ref m_vertexPosArray);
            res &= stream.ReadVector3Lst(ref m_normalArray);
            return res;
        }
    }

    public class HexVertexMorphAnimationFrame : IStream
    {
        protected List<HexVertexMorphObject> m_vertexMorphObjectList;
        protected ushort m_vertexMorphObjectCount;


        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(ref count);
            byte dst = 0;
            ushort vc = 0;
            for (int i = 0; i < count; i++)
            {
                res = stream.ReadByte(ref dst);
                res &= stream.ReadUShort(ref vc);
                HexVertexMorphObject av = AppendVertexMorphObject(dst, vc);
                res &= av.LoadFromStream(stream);
            }
            return res;
        }

        public HexVertexMorphObject AppendVertexMorphObject(byte dst, ushort vc)
        {
            if (m_vertexMorphObjectList == null)
            {
                m_vertexMorphObjectList = new List<HexVertexMorphObject>();
            }
            HexVertexMorphObject av = new HexVertexMorphObject(dst, vc);
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
