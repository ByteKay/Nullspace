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

        public NullSocketNode(int handle, int parent, Vector3 pos, Quaternion quat) : this()
        {
            mHandle = handle;
            mParent = parent;
            mPos = pos;
            mQuat = quat;
        }

        public Vector3 GetPosition()
        {
            return mPos;
        }

        public Quaternion GetQuaternion()
        {
            return mQuat;
        }

        public void SetPosition(Vector3 v)
        {
            mPos = v;
        }

        public void SetQuaternion(Quaternion quat)
        {
            mQuat = quat;
        }

        public void SetPosition(float v1, float v2, float v3)
        {
            mPos.Set(v1, v2, v3);
        }

        public void SetQuaternion(float v1, float v2, float v3, float v4)
        {
            mQuat.Set(v1, v2, v3, v4);
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

        public bool SetSocketNodeCount(int socketCount)
        {
            Clear();
            if (socketCount == 0)
            {
                return false;
            }
            for (int i = 0; i < socketCount; i++)
            {
                mSocketNodeArray.Add(new NullSocketNode(i, 0, Vector3.zero, Quaternion.identity));
            }
            return true;
        }

        public int GetSocketCount()
        {
            return mSocketNodeArray.Count;
        }

        public NullSocketNode this[int index]
        {
            get
            {
                return index < GetSocketCount() ? mSocketNodeArray[index] : null;
            }
        }

        public NullSocketNode AppendSocketNode(int handle, int parent, Vector3 pos, Quaternion quat)
        {
            NullSocketNode node = new NullSocketNode(handle, parent, pos, quat);
            mSocketNodeArray.Add(node);
            return node;
        }

        public void Clear()
        {
            mSocketNodeArray.Clear();
        }
    }
}
