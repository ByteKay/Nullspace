using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullVertexMorphAnimation : INullStream
    {
        protected string m_animationName;
        protected List<NullVertexMorphAnimationFrame> m_vertexMorphFrameList;
        protected ushort m_vertexMorphFrameCount;
        protected ushort m_frameRate;
        protected List<float> m_frameArray;

        public NullVertexMorphAnimation(ushort frameRate)
        {
            m_frameRate = frameRate;
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
            if (count == 0)
            {
                return res;
            }
            res &= stream.ReadString(out m_animationName);
            res &= stream.ReadUShort(out m_frameRate);
            for (int i = 0; i < count; i++)
            {
                NullVertexMorphAnimationFrame av = AppendVertexMorphAnimationFrame();
                res &= av.LoadFromStream(stream);
            }
            res &= stream.ReadList(out m_frameArray);
            m_vertexMorphFrameCount = (ushort)m_frameArray.Count;
            return res;
        }

        public NullVertexMorphAnimationFrame AppendVertexMorphAnimationFrame()
        {
            NullVertexMorphAnimationFrame av = new NullVertexMorphAnimationFrame();
            if (m_vertexMorphFrameList == null)
            {
                m_vertexMorphFrameList = new List<NullVertexMorphAnimationFrame>();
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

    public class NullVertexMorphAnimations : INullStream
    {
        protected byte m_animationCount;
        protected List<NullVertexMorphAnimation> m_animationArray;

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            byte count = 0;
            bool res = stream.ReadByte(out count);
            for (int i = 0; i < count; i++)
            {
                NullVertexMorphAnimation animation = AppendAnimation();
                res &= animation.LoadFromStream(stream);
            }
            return res;
        }

        public NullVertexMorphAnimation AppendAnimation(ushort frameRate = 30)
        {
            NullVertexMorphAnimation animation = new NullVertexMorphAnimation(frameRate);
            if (m_animationArray == null)
            {
                m_animationArray = new List<NullVertexMorphAnimation>();
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
