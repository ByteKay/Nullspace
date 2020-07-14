using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullSocketNode : INullStream
    {
		protected uint m_handle;
        protected uint m_parent;
        protected Vector3 m_position;
        protected Quaternion m_quat;

        public NullSocketNode(uint handle)
        {
            m_handle = handle;
            m_parent = 0;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadUInt(out m_handle);
            res &= stream.ReadUInt(out m_parent);
            res &= stream.ReadVector3(out m_position);
            res &= stream.ReadQuaternion(out m_quat);
            return res;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }
    }

    public class NullSocketNodes : INullStream
    {
		protected ushort m_socketNodeCount;
        protected List<NullSocketNode> m_socketNodeArray;

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(out count);
            for (int i = 0; i < m_socketNodeCount; i++)
            {
                NullSocketNode node = AppendSocketNode((uint)i);
                res &= node.LoadFromStream(stream);
            }
            return res;
        }

        public NullSocketNode AppendSocketNode(uint handle)
        {
            if (m_socketNodeArray == null)
            {
                m_socketNodeArray = new List<NullSocketNode>();
            }
            NullSocketNode node = new NullSocketNode(handle);
            m_socketNodeArray.Add(node);
            m_socketNodeCount++;
            return node;
        }

        public void Clear()
        {
            m_socketNodeArray = null;
            m_socketNodeCount = 0;
        }
    }
}
