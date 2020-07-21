using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace NullMesh
{
    public class NullNodeTree : INullStream
    {
        public int CurrentVersion;
        public string NodeName;    // node name
        public int NodeHandle;         // unique handle to the node
        public Vector3 Pos;
        public Quaternion Quat;
        public int GroupId;
        public int TypeName;           // non-streamed data
        public List<NullNodeTree> Children;               // sub trees

        public NullNodeTree()
        {
            NodeName = null;
            NodeHandle = 0;
            Pos = Vector3.zero;
            Quat = Quaternion.identity;
            Children = new List<NullNodeTree>();
            TypeName = 0;
            GroupId = 0;
        }

        public NullNodeTree(int version) : this()
        {
            CurrentVersion = version;
        }
        public Vector3 GetPosition()
        {
            return Pos;
        }

        public Quaternion GetQuaternion()
        {
            return Quat;
        }

        public int NumChildren { get { return Children.Count; } }

        public NullNodeTree this[int idx]
        {
            get
            {
                Assert.IsTrue(idx < Children.Count, "");
                return Children[idx];
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
            size += stream.WriteInt(nodeTree.NodeHandle);
            size += stream.WriteInt(nodeTree.GroupId);
            size += stream.WriteBool(namehandleOnly);
            if (!namehandleOnly)
            {
                size += stream.WriteVector3(Pos);
                size += stream.WriteQuaternion(Quat);
            }
            size += stream.WriteInt(nodeTree.NumChildren);
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
            Children.Clear();
        }

        public void SetNumChildren(int count)
        {
            if (count > 0)
            {
                Children.Clear();
                for (int i = 0; i < NumChildren; ++i)
                {
                    Children.Add(new NullNodeTree(CurrentVersion));
                }
            }
        }

        public int GetNodeCount()
        {
            return GetNodeCountRecursive(this);
        }

        public void SetNodeName(string name)
        {
            NodeName = name;
        }
        public string GetNodeName()
        {
            return NodeName;
        }

        public void SetNodeHandle(int handle)
        {
            NodeHandle = handle;
        }
        public int GetNodeHandle()
        {
            return NodeHandle;
        }

        public int GetChildrenCount()
        {
            return NumChildren;
        }


        public NullNodeTree FindNode(int boneId)
        {
            NullNodeTree result = null;
            FindNodeRecursive(this, boneId, ref result);
            return result;
        }

        public NullNodeTree FindNode(string nodeName)
        {
            NullNodeTree result = null;
            FindNodeRecursive(this, nodeName, ref result);
            return result;
        }
        public void FindNodeRecursive(NullNodeTree node, string nodeName, ref NullNodeTree result)
        {
            if (result != null)
            {
                return;
            }
	        if (node.GetNodeName().Equals(nodeName))
	        {
		        result = node;
		        return;
	        }
	        for (int i = 0; i < node.GetChildrenCount(); i++)
	        {

                FindNodeRecursive(node[i], nodeName, ref result);
                if (result != null)
                {
                    break;
                }
	        }
        }

        public void FindNodeRecursive(NullNodeTree node, int boneId, ref NullNodeTree result)
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

        public int GetNodeCountRecursive(NullNodeTree nodeTree)
        {
            if (nodeTree == null)
            {
                return 0;
            }
            int count = 1;
            for (int i = 0; i < nodeTree.NumChildren; i++)
            {
                count += GetNodeCountRecursive(nodeTree.Children[i]);
            }
            return count;
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
            nodeTree.Children.Clear();
        }

        public bool LoadNodeTreeFromStreamRecursive(NullNodeTree nodeTree, NullMemoryStream stream)
        {
            nodeTree.Clear();
            bool res = stream.ReadString(out nodeTree.NodeName);
            res &= stream.ReadInt(out nodeTree.NodeHandle);
            res &= stream.ReadInt(out nodeTree.GroupId);
            bool nameHandleOnly = false;
            res &= stream.ReadBool(out nameHandleOnly);
            if (!nameHandleOnly)
            {
                res &= stream.ReadVector3(out Pos);
                res &= stream.ReadQuaternion(out Quat);
            }
            int count;
            res &= stream.ReadInt(out count);
            nodeTree.SetNumChildren(count);
            for (int i = 0; i < nodeTree.NumChildren; i++)
            {
                NullNodeTree node = nodeTree.Children[i];
                res &= LoadNodeTreeFromStreamRecursive(node, stream);
            }
            return res;
        }

    }
}
