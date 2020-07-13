using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class HexSkeletonBindingNode : IStream
    {
        public struct HexNodeWeight : IStream
        {
            public uint m_target;
            public float m_weight;

            public bool LoadFromStream(SimpleMemoryStream stream)
            {
                bool res = stream.ReadUInt(ref m_target);
                res &= stream.ReadFloat(ref m_weight);
                return res;
            }

            public int SaveToStream(SimpleMemoryStream stream)
            {
                int size = stream.WriteUInt(m_target);
                size += stream.WriteFloat(m_weight);
                return size;
            }
        }
        protected ushort mCurrentVersion;
        // number of bones
        protected byte m_weightCount;       
        protected List<HexNodeWeight> m_nodeWeightArray;

        public HexSkeletonBindingNode(ushort version)
        {
            mCurrentVersion = version;
            m_weightCount = 0;
            m_nodeWeightArray = null;
        }


        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            Clear();
            byte count = 0;
            bool res = stream.ReadByte(ref count);
            SetWeightCount(count);
            for (int i = 0; i < m_weightCount; i++)
            {
                res &= m_nodeWeightArray[i].LoadFromStream(stream);
            }
            return res;
        }

        public bool SetWeightCount(byte cnt)
        {
            m_weightCount = cnt;
            if (m_weightCount == 0)
            {
                return false;
            }
            for (int i = 0; i < m_weightCount; ++i)
            {
                m_nodeWeightArray.Add(new HexNodeWeight());
            }
            return true;
        }

        public void Clear()
        {
            m_nodeWeightArray = null;
            m_weightCount = 0;
        }
    }

    public class HexSkeletonPiece : IStream
    {
        protected ushort mCurrentVersion;
		protected uint m_pieceHandle;
        protected ushort m_bindingNodeCount;
        protected List<HexSkeletonBindingNode> m_bindingNodeArray;

        public HexSkeletonPiece(ushort version)
        {
            mCurrentVersion = version;
            m_pieceHandle = 0;
            m_bindingNodeCount = 0;
            m_bindingNodeArray = null;
        }

        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(ref count);
            res &= stream.ReadUInt(ref m_pieceHandle);
            SetSkeletonBindingNodeCount(count);
            for (int i = 0; i < m_bindingNodeCount; i++)
            {
                res &= m_bindingNodeArray[i].LoadFromStream(stream);
            }
            return res;
        }

        public bool SetSkeletonBindingNodeCount(ushort count)
        {
            Clear();
            m_bindingNodeCount = count;
            if (m_bindingNodeCount == 0)
            {
                return false;
            }
            m_bindingNodeArray = new List<HexSkeletonBindingNode>(m_bindingNodeCount);
            for (int i = 0; i < m_bindingNodeCount; i++)
            {
                m_bindingNodeArray[i] = new HexSkeletonBindingNode(mCurrentVersion);
            }
            return true;

        }

        public void Clear()
        {
            for (int i = 0; i < m_bindingNodeCount; i++)
            {
                m_bindingNodeArray[i].Clear();
            }

            m_bindingNodeArray = null;
            m_bindingNodeCount = 0;
            m_pieceHandle = 0;
        }
    }
    
    public class HexSkeletonBinding : IStream
    {
        protected ushort mCurrentVersion;
        protected string m_skeletonName;
        protected ushort m_bindingPieceCount;
        protected List<HexSkeletonPiece> m_bindingPieceNodeArray;

        public HexSkeletonBinding(ushort version)
        {
            m_bindingPieceCount = 0;
            m_bindingPieceNodeArray = null;
            m_skeletonName = null;
            mCurrentVersion = version;
        }

        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(ref count);
            res &= stream.ReadString(ref m_skeletonName);
            SetSkeletonBindingCount(count);
            for (int i = 0; i < m_bindingPieceCount; i++)
            {
                res &= m_bindingPieceNodeArray[i].LoadFromStream(stream);
            }
            return res;
        }

        public bool SetSkeletonBindingCount(ushort count)
        {
            Clear();
            m_bindingPieceCount = count;
            if (m_bindingPieceCount == 0)
            {
                return false;
            }
            m_bindingPieceNodeArray = new List<HexSkeletonPiece>(m_bindingPieceCount);
            for (int i = 0; i < m_bindingPieceCount; i++)
            {
                m_bindingPieceNodeArray[i] = new HexSkeletonPiece(mCurrentVersion);
            }
            return true;
        }

        public void Clear()
        {
            m_bindingPieceNodeArray = null;
            m_bindingPieceCount = 0;
        }
    }
}
