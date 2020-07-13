using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class HexVertexMorphAnimation : IStream
    {
        protected string m_animationName;
        protected List<HexVertexMorphAnimationFrame> m_vertexMorphFrameList;
        protected ushort m_vertexMorphFrameCount;
        protected ushort m_frameRate;
        protected List<float> m_frameArray;

        public HexVertexMorphAnimation(ushort frameRate)
        {
            m_frameRate = frameRate;
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
            if (count == 0)
            {
                return res;
            }
            res &= stream.ReadString(ref m_animationName);
            res &= stream.ReadUShort(ref m_frameRate);
            for (int i = 0; i < count; i++)
            {
                HexVertexMorphAnimationFrame av = AppendVertexMorphAnimationFrame();
                res &= av.LoadFromStream(stream);
            }
            m_frameArray = new List<float>();
            res &= stream.ReadFloatLst(ref m_frameArray);
            m_vertexMorphFrameCount = (ushort)m_frameArray.Count;
            return res;
        }

        public HexVertexMorphAnimationFrame AppendVertexMorphAnimationFrame()
        {
            HexVertexMorphAnimationFrame av = new HexVertexMorphAnimationFrame();
            if (m_vertexMorphFrameList == null)
            {
                m_vertexMorphFrameList = new List<HexVertexMorphAnimationFrame>();
            }
            m_vertexMorphFrameList.Add(av);
            m_vertexMorphFrameCount++;
            return av;
        }

        public void Clear()
        {
            m_vertexMorphFrameList = null;
            m_vertexMorphFrameCount = 0;
            m_frameArray = null;
        }
    }

    public class HexVertexMorphAnimations : IStream
    {
        protected byte m_animationCount;
        protected List<HexVertexMorphAnimation> m_animationArray;

        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            Clear();
            byte count = 0;
            bool res = stream.ReadByte(ref count);
            for (int i = 0; i < count; i++)
            {
                HexVertexMorphAnimation animation = AppendAnimation();
                res &= animation.LoadFromStream(stream);
            }
            return res;
        }

        public HexVertexMorphAnimation AppendAnimation(ushort frameRate = 30)
        {
            HexVertexMorphAnimation animation = new HexVertexMorphAnimation(frameRate);
            if (m_animationArray == null)
            {
                m_animationArray = new List<HexVertexMorphAnimation>();
            }
            m_animationArray.Add(animation);
            m_animationCount++;
            return animation;
        }

        public void Clear()
        {
            m_animationArray = null;
            m_animationCount = 0;
        }
    }
}
