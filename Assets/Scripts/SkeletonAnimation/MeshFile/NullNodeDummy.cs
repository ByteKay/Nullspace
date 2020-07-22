using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullNodeDummyObject : INullStream
    {
        protected string mNodeName;
        protected int mNodeHandle;
        protected Vector3 mPos;
        protected Quaternion mQuat;

        public NullNodeDummyObject()
        {
            mPos = Vector3.zero;
            mQuat = Quaternion.identity;
        }

        public void SetTransform(float v1, float v2, float v3, float v4, float v5, float v6, float v7)
        {
            mPos.Set(v1, v2, v3);
            mQuat.Set(v4, v5, v6, v7);
        }
        
        public Vector3 GetPosition()
        {
            return mPos;
        }

        public Quaternion GetQuaternion()
        {
            return mQuat;
        }

        public void SetNodeName(string name)
        {
            mNodeName = name;
        }

        public string GetNodeName()
        {
            return mNodeName;
        }

        public void SetNodeHandle(int handle)
        {
            mNodeHandle = handle;
        }

        public int GetNodeHandle()
        {
            return mNodeHandle;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteString(mNodeName);
            size += stream.WriteVector3(mPos);
            size += stream.WriteQuaternion(mQuat);
            size += stream.WriteInt(mNodeHandle);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadString(out mNodeName);
            res &= stream.ReadVector3(out mPos);
            res &= stream.ReadQuaternion(out mQuat);
            res &= stream.ReadInt(out mNodeHandle);
            return res;
        }
    }

    public class NullNodeDummy : INullStream
    {
        protected List<NullNodeDummyObject> mDummyArray;

        public NullNodeDummy()
        {
            mDummyArray = new List<NullNodeDummyObject>();
        }

        public bool SetDummyCount(int dummyCount)
        {
            Clear();
            if (dummyCount == 0)
            {
                return false;
            }
            for (int i = 0; i < dummyCount; i++)
            {
                mDummyArray.Add(new NullNodeDummyObject());
            }
            return true;
        }

        public NullNodeDummyObject this[int index]
        {
            get
            {
                return index < mDummyArray.Count ? mDummyArray[index] : null;
            }
            
        }

        public int GetNodeCount()
        {
            return mDummyArray.Count;
        }

        public NullNodeDummyObject FindNodeObject(int nodeId)
        {
            for (int i = 0; i < mDummyArray.Count; i++)
            {
                NullNodeDummyObject nodeObject = mDummyArray[i];
                if (nodeId == nodeObject.GetNodeHandle())
                {
                    return nodeObject;
                }
            }
            return null;
        }

        public NullNodeDummyObject FindNodeObject(string nodeName)
        {
            for (int i = 0; i < mDummyArray.Count; i++)
            {
                NullNodeDummyObject nodeObject = mDummyArray[i];
                if (nodeName.Equals(nodeObject.GetNodeName()))
                {
                    return nodeObject;
                }
            }
            return null;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            return stream.WriteList(mDummyArray, false);
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            return stream.ReadList(out mDummyArray);
        }

        public void Clear()
        {
            mDummyArray.Clear();
        }
    }
}
