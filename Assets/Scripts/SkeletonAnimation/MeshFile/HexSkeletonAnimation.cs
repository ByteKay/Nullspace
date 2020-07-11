using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class HexSkeletonNodeAnimation
    {
        protected uint m_parent;
        protected ushort m_frameCount;                 //non streamed
        protected float[] m_posQuatArray;				//7 * m_frameCount
        protected string m_boneName;
        protected ushort mCurrentVersion;
    }

    public class HexSkeletonAnimation
    {
        protected string m_animationName;
        protected float[] m_frameArray;
        protected ushort m_frameCount;
        protected ushort m_frameRate;
        protected HexSkeletonNodeAnimation[] m_nodeAnimationArray;
        protected ushort m_animationNodeCount;
        protected ushort m_version;
        protected ushort mCurrentVersion;
    }

    public class HexSkeletonAnimations
    {
        protected byte m_animationCount;
        protected HexSkeletonAnimation[] m_animationArray;
        protected ushort mCurrentVersion;

    }
}
