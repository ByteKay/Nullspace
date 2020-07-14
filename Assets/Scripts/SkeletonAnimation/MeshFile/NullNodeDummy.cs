using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullNodeDummyObject : INullStream
    {
        protected string NodeName;
        protected uint NodeHandle;
        protected Vector3 Pos;
        protected Quaternion Quat;

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteString(NodeName);
            size += stream.WriteVector3(Pos);
            size += stream.WriteQuaternion(Quat);
            size += stream.WriteUInt(NodeHandle);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadString(out NodeName);
            res &= stream.ReadVector3(out Pos);
            res &= stream.ReadQuaternion(out Quat);
            res &= stream.ReadUInt(out NodeHandle);
            return res;
        }
    }

    public class NullNodeDummy : INullStream
    {
        protected ushort DummyCount;
        protected List<NullNodeDummyObject> DummyArray;

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteUShort(DummyCount);
            for (int i = 0; i < DummyCount; i++)
            {
                size += DummyArray[i].SaveToStream(stream);
            }
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(out count);
            SetDummyCount(count);
            for (int i = 0; i < DummyCount; i++)
            {
                res &= DummyArray[i].LoadFromStream(stream);
            }
            return res;
        }

        public void SetDummyCount(ushort count)
        {
            DummyCount = count;
            if (DummyArray == null)
            {
                DummyArray = new List<NullNodeDummyObject>();
            }
            DummyArray.Clear();
            for (int i = 0; i < count; ++i)
            {
                DummyArray.Add(new NullNodeDummyObject());
            }
        }

        public void Clear()
        {
            DummyArray = null;
            DummyCount = 0;
        }
    }
}
