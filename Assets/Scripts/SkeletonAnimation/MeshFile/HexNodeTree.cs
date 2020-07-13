using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MeshFile
{
    public class HexNodeTree : IStream
    {
        public string m_nodeName;    // node name
        public uint m_nodeHandle;         // unique handle to the node
        public Vector3 m_pos;
        public Quaternion m_q;
        public ushort m_numChildren;            // number of children
        public List<HexNodeTree> m_children;               // sub trees
        public ushort m_groupId;
		protected ushort mCurrentVersion;
        public uint m_typeName;           // non-streamed data


        public HexNodeTree(ushort version)
        {
            m_nodeName = null;
            m_nodeHandle = 0;
            m_pos = Vector3.zero;
            m_q = Quaternion.identity;
            m_numChildren = 0;
            m_children = null;
            m_typeName = 0;
            mCurrentVersion = version;
            m_groupId = 0;
        }

        public HexNodeTree this[int idx]
        {
            get
            {
                return idx < m_children.Count ? m_children[idx] : null;
            }
        }


        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            return LoadNodeTreeFromStreamRecursive(this, stream);
        }

        public void Clear()
        {
            for (int i = 0; i < m_numChildren; i++)
            {
                DeleteNodeRecursive(m_children[i]);
            }
            m_numChildren = 0;
            m_children = null;
        }

        public void DeleteNodeRecursive(HexNodeTree nodeTree)
        {
            if (nodeTree == null)
            {
                return;
            }
            for (int i = 0; i < nodeTree.m_numChildren; i++)
            {
                HexNodeTree node = nodeTree.m_children[i];
                DeleteNodeRecursive(node);
            }
            nodeTree.m_numChildren = 0;
            nodeTree.m_children = null;
        }

        public bool LoadNodeTreeFromStreamRecursive(HexNodeTree nodeTree, SimpleMemoryStream stream)
        {
            nodeTree.Clear();
            bool res = stream.ReadString(ref nodeTree.m_nodeName);
            res &= stream.ReadUInt(ref nodeTree.m_nodeHandle);
            res &= stream.ReadUShort(ref nodeTree.m_groupId);
            bool nameHandleOnly = false;
            res &= stream.ReadBool(ref nameHandleOnly);
            if (!nameHandleOnly)
            {
                res &= stream.ReadVector3(ref m_pos);
                res &= stream.ReadQuaternion(ref m_q);
            }
            ushort count = 0;
            res &= stream.ReadUShort(ref count);
            nodeTree.SetNumChildren(count);
            for (int i = 0; i < nodeTree.m_numChildren; i++)
            {
                HexNodeTree node = nodeTree.m_children[i];
                res &= LoadNodeTreeFromStreamRecursive(node, stream);
            }
            return res;
        }

        public void SetNumChildren(ushort count)
        {
            if (count > 0)
            {
                m_numChildren = count;
                if (m_children == null)
                {
                    m_children = new List<HexNodeTree>(m_numChildren);
                }
                for (int i = 0; i < m_numChildren; ++i)
                {
                    m_children.Add(new HexNodeTree(mCurrentVersion));
                }
            }
        }

        public ushort GetNodeCount()
        {
            return GetNodeCountRecursive(this);
        }


        public ushort GetNodeCountRecursive(HexNodeTree nodeTree)
        {
            if (nodeTree == null)
            {
                return 0;
            }
            ushort count = 1;
            for (int i = 0; i < nodeTree.m_numChildren; i++)
            {
                count += GetNodeCountRecursive(nodeTree.m_children[i]);
            }
            return count;
        }

        public HexNodeTree FindNode(uint boneId)
        {
            HexNodeTree result = null;
            FindNodeRecursive(this, boneId, ref result);
            return result;
        }

        internal string GetNodeName()
        {
            return m_nodeName;
        }

        public void FindNodeRecursive(HexNodeTree node, uint boneId, ref HexNodeTree result)
        {
            if (result != null)
            {
                return;
            }
            if (node.GetNodeHandle() == boneId)
            {
                result = node;
                return;
            }
            for (int i = 0; i < node.GetChildrenCount(); i++)
            {
                FindNodeRecursive(node[i], boneId, ref result);
                if (result != null)
                {
                    break;
                }
            }
        }

        private int GetChildrenCount()
        {
            return m_numChildren;
        }

        private uint GetNodeHandle()
        {
            return m_nodeHandle;
        }
    }
}
