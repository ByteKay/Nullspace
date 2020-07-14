using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullSocketNode : INullStream
    {
		protected uint Handle;
        protected uint Parent;
        protected Vector3 Position;
        protected Quaternion Quat;

        public NullSocketNode(uint handle)
        {
            Handle = handle;
            Parent = 0;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadUInt(out Handle);
            res &= stream.ReadUInt(out Parent);
            res &= stream.ReadVector3(out Position);
            res &= stream.ReadQuaternion(out Quat);
            return res;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteUInt(Handle);
            size += stream.WriteUInt(Parent);
            size += stream.WriteVector3(Position);
            size += stream.WriteQuaternion(Quat);
            return size;
        }
    }

    public class NullSocketNodes : INullStream
    {
		protected ushort SocketNodeCount;
        protected List<NullSocketNode> SocketNodeArray;

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteUShort(SocketNodeCount);
            for (int i = 0; i < SocketNodeCount; i++)
            {
                size += SocketNodeArray[i].SaveToStream(stream);
            }
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(out count);
            for (int i = 0; i < SocketNodeCount; i++)
            {
                NullSocketNode node = AppendSocketNode((uint)i);
                res &= node.LoadFromStream(stream);
            }
            return res;
        }

        public NullSocketNode AppendSocketNode(uint handle)
        {
            if (SocketNodeArray == null)
            {
                SocketNodeArray = new List<NullSocketNode>();
            }
            NullSocketNode node = new NullSocketNode(handle);
            SocketNodeArray.Add(node);
            SocketNodeCount++;
            return node;
        }

        public void Clear()
        {
            SocketNodeArray = null;
            SocketNodeCount = 0;
        }
    }
}
