using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullNodeTree : INullStream
    {
        public string NodeName;    // node name
        public uint NodeHandle;         // unique handle to the node
        public Vector3 Pos;
        public Quaternion Quat;
        public ushort NumChildren;            // number of children
        public List<NullNodeTree> Children;               // sub trees
        public ushort GroupId;
		protected ushort CurrentVersion;
        public uint TypeName;           // non-streamed data


        public NullNodeTree(ushort version)
        {
            NodeName = null;
            NodeHandle = 0;
            Pos = Vector3.zero;
            Quat = Quaternion.identity;
            NumChildren = 0;
            Children = null;
            TypeName = 0;
            CurrentVersion = version;
            GroupId = 0;
        }

        public NullNodeTree this[int idx]
        {
            get
            {
                return idx < Children.Count ? Children[idx] : null;
            }
        }


        public int SaveToStream(NullMemoryStream stream)
        {
            // SaveToStream(NullMemoryStream stream, bool namehandleOnly)
            throw new Exception();
        }

        public int SaveToStream(NullMemoryStream stream, bool namehandleOnly)
        {
            return SaveNodeTreeToStreamRecursive(this, stream, namehandleOnly);
        }

        public int SaveNodeTreeToStreamRecursive(NullNodeTree nodeTree, NullMemoryStream stream, bool namehandleOnly)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteString(nodeTree.NodeName);
            size += stream.WriteUInt(nodeTree.NodeHandle);
            size += stream.WriteUShort(nodeTree.GroupId);
            size += stream.WriteBool(namehandleOnly);
            if (!namehandleOnly)
            {
                size += stream.WriteVector3(Pos);
                size += stream.WriteQuaternion(Quat);
            }
            size += stream.WriteUShort(nodeTree.NumChildren);
            for (int i = 0; i < nodeTree.NumChildren; i++)
            {
                NullNodeTree node = nodeTree.Children[i];
                if (node != null)
                {
                    size += SaveNodeTreeToStreamRecursive(node, stream, namehandleOnly);
                }
            }
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            return LoadNodeTreeFromStreamRecursive(this, stream);
        }

        public void Clear()
        {
            for (int i = 0; i < NumChildren; i++)
            {
                DeleteNodeRecursive(Children[i]);
            }
            NumChildren = 0;
            Children = null;
        }

        public void DeleteNodeRecursive(NullNodeTree nodeTree)
        {
            if (nodeTree == null)
            {
                return;
            }
            for (int i = 0; i < nodeTree.NumChildren; i++)
            {
                NullNodeTree node = nodeTree.Children[i];
                DeleteNodeRecursive(node);
            }
            nodeTree.NumChildren = 0;
            nodeTree.Children = null;
        }

        public bool LoadNodeTreeFromStreamRecursive(NullNodeTree nodeTree, NullMemoryStream stream)
        {
            nodeTree.Clear();
            bool res = stream.ReadString(out nodeTree.NodeName);
            res &= stream.ReadUInt(out nodeTree.NodeHandle);
            res &= stream.ReadUShort(out nodeTree.GroupId);
            bool nameHandleOnly = false;
            res &= stream.ReadBool(out nameHandleOnly);
            if (!nameHandleOnly)
            {
                res &= stream.ReadVector3(out Pos);
                res &= stream.ReadQuaternion(out Quat);
            }
            ushort count;
            res &= stream.ReadUShort(out count);
            nodeTree.SetNumChildren(count);
            for (int i = 0; i < nodeTree.NumChildren; i++)
            {
                NullNodeTree node = nodeTree.Children[i];
                res &= LoadNodeTreeFromStreamRecursive(node, stream);
            }
            return res;
        }

        public void SetNumChildren(ushort count)
        {
            if (count > 0)
            {
                NumChildren = count;
                if (Children == null)
                {
                    Children = new List<NullNodeTree>(NumChildren);
                }
                for (int i = 0; i < NumChildren; ++i)
                {
                    Children.Add(new NullNodeTree(CurrentVersion));
                }
            }
        }

        public ushort GetNodeCount()
        {
            return GetNodeCountRecursive(this);
        }


        public ushort GetNodeCountRecursive(NullNodeTree nodeTree)
        {
            if (nodeTree == null)
            {
                return 0;
            }
            ushort count = 1;
            for (int i = 0; i < nodeTree.NumChildren; i++)
            {
                count += GetNodeCountRecursive(nodeTree.Children[i]);
            }
            return count;
        }

        public NullNodeTree FindNode(uint boneId)
        {
            NullNodeTree result = null;
            FindNodeRecursive(this, boneId, ref result);
            return result;
        }

        internal string GetNodeName()
        {
            return NodeName;
        }

        public void FindNodeRecursive(NullNodeTree node, uint boneId, ref NullNodeTree result)
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
            return NumChildren;
        }

        private uint GetNodeHandle()
        {
            return NodeHandle;
        }
    }
}
