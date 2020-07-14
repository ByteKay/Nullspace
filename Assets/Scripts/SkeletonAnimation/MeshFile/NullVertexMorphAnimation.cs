using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullVertexMorphAnimation : INullStream
    {
        protected string AnimationName;
        protected List<NullVertexMorphAnimationFrame> VertexMorphFrameList;
        protected ushort VertexMorphFrameCount;
        protected ushort FrameRate;
        protected List<float> FrameArray;

        public NullVertexMorphAnimation(ushort frameRate)
        {
            FrameRate = frameRate;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteUShort(VertexMorphFrameCount);
            if (VertexMorphFrameCount == 0)
            {
                return size;
            }
            size += stream.WriteString(AnimationName);
            size += stream.WriteUShort(FrameRate);
            for (int i = 0; i < VertexMorphFrameCount; i++)
            {
                NullVertexMorphAnimationFrame av = VertexMorphFrameList[i];
                size += av.SaveToStream(stream);
            }
            size += stream.WriteList(FrameArray, true);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadUShort(out VertexMorphFrameCount);
            if (VertexMorphFrameCount == 0)
            {
                return res;
            }
            res &= stream.ReadString(out AnimationName);
            res &= stream.ReadUShort(out FrameRate);
            for (int i = 0; i < VertexMorphFrameCount; i++)
            {
                NullVertexMorphAnimationFrame av = AppendVertexMorphAnimationFrame();
                res &= av.LoadFromStream(stream);
            }
            res &= stream.ReadList(out FrameArray, VertexMorphFrameCount);
            return res;
        }

        public NullVertexMorphAnimationFrame AppendVertexMorphAnimationFrame()
        {
            NullVertexMorphAnimationFrame av = new NullVertexMorphAnimationFrame();
            if (VertexMorphFrameList == null)
            {
                VertexMorphFrameList = new List<NullVertexMorphAnimationFrame>();
            }
            VertexMorphFrameList.Add(av);
            VertexMorphFrameCount++;
            return av;
        }

        public void Clear()
        {
            VertexMorphFrameList = null;
            VertexMorphFrameCount = 0;
            FrameArray = null;
        }
    }

    public class NullVertexMorphAnimations : INullStream
    {
        protected byte AnimationCount;
        protected List<NullVertexMorphAnimation> AnimationArray;

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteByte(AnimationCount);
            for (int i = 0; i < AnimationCount; i++)
            {
                size += AnimationArray[i].SaveToStream(stream);
            }
            return size;
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
            if (AnimationArray == null)
            {
                AnimationArray = new List<NullVertexMorphAnimation>();
            }
            AnimationArray.Add(animation);
            AnimationCount++;
            return animation;
        }

        public void Clear()
        {
            AnimationArray = null;
            AnimationCount = 0;
        }
    }
}
