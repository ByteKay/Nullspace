using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullNodeDummyObject : INullStream
    {
        protected string m_nodeName;
        protected uint m_nodeHandle;
        protected Vector3 m_pos;
        protected Quaternion m_q;

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadString(out m_nodeName);
            res &= stream.ReadVector3(out m_pos);
            res &= stream.ReadQuaternion(out m_q);
            res &= stream.ReadUInt(out m_nodeHandle);
            return res;
        }
    }

    public class NullNodeDummy : INullStream
    {
        protected ushort m_dummyCount;
        protected List<NullNodeDummyObject> m_dummyArray;

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(out count);
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
                m_dummyArray = new List<NullNodeDummyObject>();
            }
            m_dummyArray.Clear();
            for (int i = 0; i < count; ++i)
            {
                m_dummyArray.Add(new NullNodeDummyObject());
            }
        }

        public void Clear()
        {
            m_dummyArray = null;
            m_dummyCount = 0;
        }
    }
}
