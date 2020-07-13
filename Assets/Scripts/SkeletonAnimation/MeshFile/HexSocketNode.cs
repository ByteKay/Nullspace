using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MeshFile
{
    public class HexSocketNode : IStream
    {
		protected uint m_handle;
        protected uint m_parent;
        protected Vector3 m_position;
        protected Quaternion m_quat;

        public HexSocketNode(uint handle)
        {
            m_handle = handle;
            m_parent = 0;
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            bool res = stream.ReadUInt(ref m_handle);
            res &= stream.ReadUInt(ref m_parent);
            res &= stream.ReadVector3(ref m_position);
            res &= stream.ReadQuaternion(ref m_quat);
            return res;
        }

        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }
    }

    public class HexSocketNodes : IStream
    {
		protected ushort m_socketNodeCount;
        protected List<HexSocketNode> m_socketNodeArray;

        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(ref count);
            for (int i = 0; i < m_socketNodeCount; i++)
            {
                HexSocketNode node = AppendSocketNode((uint)i);
                res &= node.LoadFromStream(stream);
            }
            return res;
        }

        public HexSocketNode AppendSocketNode(uint handle)
        {
            if (m_socketNodeArray == null)
            {
                m_socketNodeArray = new List<HexSocketNode>();
            }
            HexSocketNode node = new HexSocketNode(handle);
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
