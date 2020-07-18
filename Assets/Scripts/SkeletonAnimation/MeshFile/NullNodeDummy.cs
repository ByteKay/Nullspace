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
