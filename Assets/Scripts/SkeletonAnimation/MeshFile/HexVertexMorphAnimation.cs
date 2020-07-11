using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class HexVertexMorphAnimation
    {
        protected string m_animationName;
        protected HexVertexMorphAnimationFrame[] m_vertexMorphFrameList;
        protected ushort m_vertexMorphFrameCount;
        protected ushort m_frameRate;
        protected float[] m_frameArray;
    }

    public class HexVertexMorphAnimations
    {
        protected byte m_animationCount;
        protected HexVertexMorphAnimation[] m_animationArray;
    }
}
