using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MeshFile
{
    public class HexNodeDummyObject : IStream
    {
        protected string m_nodeName;
        protected uint m_nodeHandle;
        protected Vector3 m_pos;
        protected Quaternion m_q;

        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            bool res = stream.ReadString(ref m_nodeName);
            res &= stream.ReadVector3(ref m_pos);
            res &= stream.ReadQuaternion(ref m_q);
            res &= stream.ReadUInt(ref m_nodeHandle);
            return res;
        }
    }

    public class HexNodeDummy : IStream
    {
        protected ushort m_dummyCount;
        protected List<HexNodeDummyObject> m_dummyArray;

        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(ref count);
            SetDummyCount(count);
            for (int i = 0; i < m_dummyCount; i++)
            {
                res &= m_dummyArray[i].LoadFromStream(stream);
            }
            return res;
        }

        public void SetDummyCount(ushort count)
        {
            m_dummyCount = count;
            if (m_dummyArray == null)
            {
                m_dummyArray = new List<HexNodeDummyObject>();
            }
            m_dummyArray.Clear();
            for (int i = 0; i < count; ++i)
            {
                m_dummyArray.Add(new HexNodeDummyObject());
            }
        }

        public void Clear()
        {
            m_dummyArray = null;
            m_dummyCount = 0;
        }
    }
}
