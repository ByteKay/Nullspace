using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullSocketNode : INullStream
    {
		protected int mHandle;
        protected int mParent;
        protected Vector3 mPos;
        protected Quaternion mQuat;

        public NullSocketNode()
        {
            mHandle = 0;
            mParent = 0;
            mPos = Vector3.zero;
            mQuat = Quaternion.identity;
        }


        public NullSocketNode(int handle) : this()
        {
            mHandle = handle;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadInt(out mHandle);
            res &= stream.ReadInt(out mParent);
            res &= stream.ReadVector3(out mPos);
            res &= stream.ReadQuaternion(out mQuat);
            return res;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteInt(mHandle);
            size += stream.WriteInt(mParent);
            size += stream.WriteVector3(mPos);
            size += stream.WriteQuaternion(mQuat);
            return size;
        }
    }

    public class NullSocketNodes : INullStream
    {
        protected List<NullSocketNode> mSocketNodeArray;

        public NullSocketNodes()
        {
            mSocketNodeArray = new List<NullSocketNode>();
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteList(mSocketNodeArray, false);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadList(out mSocketNodeArray);
            return res;
        }

        public NullSocketNode AppendSocketNode(int handle)
        {
            NullSocketNode node = new NullSocketNode(handle);
            mSocketNodeArray.Add(node);
            return node;
        }

        public void Clear()
        {
            mSocketNodeArray.Clear();
        }
    }
}
