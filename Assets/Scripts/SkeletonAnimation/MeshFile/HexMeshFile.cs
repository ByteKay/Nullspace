using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class HexMeshFile
    {
        protected byte m_workingMode;
        protected ushort m_version;
        protected uint m_blockSize;
        //base mesh
        protected HexMeshObjects m_meshObjectList;
        protected HexMeshObjects m_skinObjectList;
        protected HexSocketNodes m_socketNodeList;
        protected HexNodeDummy m_nodeDummy;
        //skeleton animation
        protected HexNodeTree m_nodeTree;
        protected HexSkeletonBinding m_skeletonBinding;
        protected HexSkeletonAnimations m_skeletonAnimations;
        //ertex morph animation
        protected HexVertexMorphAnimations m_vertexMorphAnimations;
    }
}
