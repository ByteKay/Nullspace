using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullVertexMorphAnimation : INullStream
    {
        protected string mAnimationName;
        protected int mFrameRate;
        protected List<float> mFrameArray;
        protected List<NullVertexMorphAnimationFrame> mVertexMorphFrameList;

        public NullVertexMorphAnimation()
        {
            mVertexMorphFrameList = new List<NullVertexMorphAnimationFrame>();
            mFrameArray = new List<float>();
            mFrameRate = 0;
            mAnimationName = "";
        }

        public NullVertexMorphAnimation(int frameRate) : this()
        {
            mFrameRate = frameRate;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteList(mFrameArray, false);
            if (mFrameArray.Count == 0)
            {
                return size;
            }
            size += stream.WriteList(mVertexMorphFrameList, true);
            size += stream.WriteString(mAnimationName);
            size += stream.WriteInt(mFrameRate);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadList(out mFrameArray);
            if (mFrameArray.Count == 0)
            {
                return res;
            }
            res &= stream.ReadList(out mVertexMorphFrameList, mFrameArray.Count);
            res &= stream.ReadString(out mAnimationName);
            res &= stream.ReadInt(out mFrameRate);
            return res;
        }

        public NullVertexMorphAnimationFrame AppendVertexMorphAnimationFrame()
        {
            NullVertexMorphAnimationFrame av = new NullVertexMorphAnimationFrame();
            mVertexMorphFrameList.Add(av);
            return av;
        }

        public void Clear()
        {
            mVertexMorphFrameList.Clear();
            mFrameArray.Clear();
        }
    }

    public class NullVertexMorphAnimations : INullStream
    {
        protected List<NullVertexMorphAnimation> mAnimationArray;

        public NullVertexMorphAnimations()
        {
            mAnimationArray = new List<NullVertexMorphAnimation>();
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteList(mAnimationArray, false);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadList(out mAnimationArray);
            return res;
        }

        public NullVertexMorphAnimation AppendAnimation(int frameRate = 30)
        {
            NullVertexMorphAnimation animation = new NullVertexMorphAnimation(frameRate);
            mAnimationArray.Add(animation);
            return animation;
        }

        public void Clear()
        {
            mAnimationArray.Clear();
        }
    }
}
