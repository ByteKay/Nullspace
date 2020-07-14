using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullSkeletonOffset : INullStream
    {
        protected uint BoneHandle;
        protected string BoneName;
        protected ushort FrameCount;
        protected List<Vector3> PosArray;
        protected List<float> FrameTimes;                
		protected ushort CurrentVersion;

        public NullSkeletonOffset(ushort version, ushort frameCount)
        {
            CurrentVersion = version;
            FrameCount = frameCount;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            ushort count = 0;
            bool res = stream.ReadUShort(out count);
            SetFrameCount(count);
            res &= stream.ReadUInt(out BoneHandle);
            res &= stream.ReadString(out BoneName);
            res &= stream.ReadList(out PosArray);
            res &= stream.ReadList(out FrameTimes);
            return res;
        }

        public bool SetFrameCount(ushort frameCount)
        {
            Clear();
            FrameCount = frameCount;
            if (FrameCount > 0)
            {
                PosArray = new List<Vector3>(FrameCount);
                FrameTimes = new List<float>(FrameCount);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Clear()
        {
            FrameCount = 0;
            PosArray = null;
            FrameTimes = null;
        }
    }

    public class NullSkeletonOffsets : INullStream
    {
		protected string AnimationName;
        protected byte AnimationOffsetCount;
        protected List<NullSkeletonOffset> AnimationOffsetArray;
		protected ushort CurrentVersion;

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            byte count = 0;
            bool res = stream.ReadByte(out count);
            if (count > 0)
            {
                res &= stream.ReadString(out AnimationName);
                for (int i = 0; i < count; i++)
                {
                    NullSkeletonOffset offset = AppendSkeletonOffset(0);
                    res &= offset.LoadFromStream(stream);
                }
            }
            return res;
        }

        public NullSkeletonOffset AppendSkeletonOffset(ushort frameCount)
        {
            if (AnimationOffsetArray == null)
            {
                AnimationOffsetArray = new List<NullSkeletonOffset>();
            }
            NullSkeletonOffset offset = new NullSkeletonOffset(CurrentVersion, frameCount);
            AnimationOffsetArray.Add(offset);
            AnimationOffsetCount++;
            return offset;
        }

        public void Clear()
        {
            for (int i = 0; i < AnimationOffsetCount; i++)
            {
                AnimationOffsetArray[i].Clear();
            }
            AnimationOffsetArray = null;
            AnimationOffsetCount = 0;
        }
    }
    
    public class NullSkeletonOffsetFile : INullStream
    {
        protected ushort OffsetsCount;
        protected List<NullSkeletonOffsets> AnimationOffsetsArray;
        protected ushort BlockSize;
        protected ushort Version;

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            uint fourCC = 0;
            stream.ReadUInt(out fourCC);
            if (NullMeshFile.MakeFourCC("HXBO") != fourCC)
            {
                return false;
            }
            bool res = stream.ReadUShort(out Version);
            res &= stream.ReadUShort(out BlockSize);
            ushort count = 0;
            res &= stream.ReadUShort(out count);
            SetAnimationOffsetsCount(count);
            for (int i = 0; i < OffsetsCount; i++)
            {
                res &= AnimationOffsetsArray[i].LoadFromStream(stream);
            }
            return res;
        }

        public void Clear()
        {
            for (int i = 0; i < OffsetsCount; i++)
            {
                AnimationOffsetsArray[i].Clear();
            }
            AnimationOffsetsArray = null;
            OffsetsCount = 0;
        }

        public void SetAnimationOffsetsCount(ushort count)
        {
            Clear();
            OffsetsCount = count;
            if (OffsetsCount > 0)
            {
                AnimationOffsetsArray = new List<NullSkeletonOffsets>(OffsetsCount);
                for (int i = 0; i < OffsetsCount; ++i)
                {
                    AnimationOffsetsArray.Add(new NullSkeletonOffsets());
                }
            }
        }
    }

}
