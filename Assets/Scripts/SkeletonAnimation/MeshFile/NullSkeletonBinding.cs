using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullSkeletonBindingNode : INullStream
    {
        public struct HexNodeWeight : INullStream
        {
            public uint m_target;
            public float m_weight;

            public bool LoadFromStream(NullMemoryStream stream)
            {
                bool res = stream.ReadUInt(out m_target);
                res &= stream.ReadFloat(out m_weight);
                return res;
            }

            public int SaveToStream(NullMemoryStream stream)
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

        public NullSkeletonBindingNode(ushort version)
        {
            mCurrentVersion = version;
            m_weightCount = 0;
            m_nodeWeightArray = null;
        }


        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            byte count = 0;
            bool res = stream.ReadByte(out count);
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

    public class NullSkeletonPiece : INullStream
    {
        protected ushort mCurrentVersion;
		protected uint m_pieceHandle;
        protected ushort m_bindingNodeCount;
        protected List<NullSkeletonBindingNode> m_bindingNodeArray;

        public NullSkeletonPiece(ushort version)
        {
            mCurrentVersion = version;
            m_pieceHandle = 0;
            m_bindingNodeCount = 0;
            m_bindingNodeArray = null;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(out count);
            res &= stream.ReadUInt(out m_pieceHandle);
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
            m_bindingNodeArray = new List<NullSkeletonBindingNode>(m_bindingNodeCount);
            for (int i = 0; i < m_bindingNodeCount; i++)
            {
                m_bindingNodeArray[i] = new NullSkeletonBindingNode(mCurrentVersion);
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
    
    public class NullSkeletonBinding : INullStream
    {
        protected ushort mCurrentVersion;
        protected string m_skeletonName;
        protected ushort m_bindingPieceCount;
        protected List<NullSkeletonPiece> m_bindingPieceNodeArray;

        public NullSkeletonBinding(ushort version)
        {
            m_bindingPieceCount = 0;
            m_bindingPieceNodeArray = null;
            m_skeletonName = null;
            mCurrentVersion = version;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(out count);
            res &= stream.ReadString(out m_skeletonName);
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
            m_bindingPieceNodeArray = new List<NullSkeletonPiece>(m_bindingPieceCount);
            for (int i = 0; i < m_bindingPieceCount; i++)
            {
                m_bindingPieceNodeArray[i] = new NullSkeletonPiece(mCurrentVersion);
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
