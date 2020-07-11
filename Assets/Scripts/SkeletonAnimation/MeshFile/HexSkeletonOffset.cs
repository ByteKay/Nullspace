using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class HexSkeletonOffset
    {
        protected uint m_boneHandle;
        protected string m_boneName;
        //non streamed
        protected ushort m_frameCount;
        //3 * m_frameCount
        protected float[] m_posArray;
        //1 * m_frameCount
        protected float[] m_frameTimes;                
		protected ushort mCurrentVersion;
    }

    public class HexSkeletonOffsets
    {
		protected string m_animationName;
        protected byte m_animationOffsetCount;
        protected HexSkeletonOffset[] m_animationOffsetArray;
		protected ushort mCurrentVersion;
    }
    
    public class HexSkeletonOffsetFile
    {
        protected ushort m_offsetsCount;
        protected HexSkeletonOffsets[] m_animationOffsetsArray;
        protected ushort m_blockSize;
        protected ushort m_version;
    }

}
