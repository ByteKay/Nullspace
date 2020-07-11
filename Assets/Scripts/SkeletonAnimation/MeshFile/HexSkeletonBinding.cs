using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class HexSkeletonBindingNode
    {
        public struct HexNodeWeight
        {
            uint m_target;
            float m_weight;
        }
        protected ushort mCurrentVersion;
        // number of bones
        protected byte m_weightCount;       
        protected HexNodeWeight[] m_nodeWeightArray;
        
    }

    public class HexSkeletonPiece
    {
        protected ushort mCurrentVersion;
		protected uint m_pieceHandle;
        protected ushort m_bindingNodeCount;
        protected HexSkeletonBindingNode[] m_bindingNodeArray;
    }
    
    public class HexSkeletonBinding
    {
        protected ushort mCurrentVersion;
        protected string m_skeletonName;
        protected ushort m_bindingPieceCount;
        protected HexSkeletonPiece[] m_bindingPieceNodeArray;
        
    }
}
