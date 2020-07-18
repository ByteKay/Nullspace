using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullSkeletonOffset : INullStream
    {
        public int CurrentVersion;
        protected string mBoneName;
        protected int mBoneHandle;
        protected List<Vector3> mPosArray;
        protected List<float> mFrameTimes;

        public NullSkeletonOffset()
        {
            mPosArray = new List<Vector3>();
            mFrameTimes = new List<float>();
            mBoneName = "";
            mBoneHandle = 0;
        }

        public NullSkeletonOffset(int version) : this()
        {
            CurrentVersion = version;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteInt(mBoneHandle);
            size += stream.WriteString(mBoneName);
            size += stream.WriteList(mPosArray, false);
            size += stream.WriteList(mFrameTimes, true);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadInt(out mBoneHandle);
            res &= stream.ReadString(out mBoneName);
            res &= stream.ReadList(out mPosArray);
            res &= stream.ReadList(out mFrameTimes, mPosArray.Count);
            return res;
        }

        public void Clear()
        {
            mPosArray.Clear();
            mFrameTimes.Clear();
        }
    }

    public class NullSkeletonOffsets : INullStream
    {
        public int CurrentVersion;
        protected string mAnimationName;
        protected List<NullSkeletonOffset> mAnimationOffsetArray;

        public NullSkeletonOffsets()
        {
            mAnimationOffsetArray = new List<NullSkeletonOffset>();
            mAnimationName = "";
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteList(mAnimationOffsetArray, false);
            if (mAnimationOffsetArray.Count > 0)
            {
                size += stream.WriteString(mAnimationName);
            }
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadList(out mAnimationOffsetArray);
            if (mAnimationOffsetArray.Count > 0)
            {
                res &= stream.ReadString(out mAnimationName);
            }
            return res;
        }

        public NullSkeletonOffset AppendSkeletonOffset()
        {
            NullSkeletonOffset offset = new NullSkeletonOffset(CurrentVersion);
            mAnimationOffsetArray.Add(offset);
            return offset;
        }

        public void Clear()
        {
            for (int i = 0; i < mAnimationOffsetArray.Count(); i++)
            {
                mAnimationOffsetArray[i].Clear();
            }
            mAnimationOffsetArray.Clear();
        }
    }
    
    public class NullSkeletonOffsetFile : INullStream
    {
        public int CurrentVersion;
        protected List<NullSkeletonOffsets> mAnimationOffsetsArray;
        protected int mBlockSize;
        
        public NullSkeletonOffsetFile()
        {
            mAnimationOffsetsArray = new List<NullSkeletonOffsets>();
            mBlockSize = 0;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            uint fourCC = NullMeshFile.MakeFourCC("HXBO");
            int size = stream.WriteUInt(fourCC);
            size += stream.WriteInt(CurrentVersion);
            size += stream.WriteInt(mBlockSize);
            size += stream.WriteList(mAnimationOffsetsArray, false);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            uint fourCC;
            stream.ReadUInt(out fourCC);
            if (NullMeshFile.MakeFourCC("HXBO") != fourCC)
            {
                return false;
            }
            bool res = stream.ReadInt(out CurrentVersion);
            res &= stream.ReadInt(out mBlockSize);
            res &= stream.ReadList(out mAnimationOffsetsArray);
            return res;
        }

        public void Clear()
        {
            for (int i = 0; i < mAnimationOffsetsArray.Count; i++)
            {
                mAnimationOffsetsArray[i].Clear();
            }
            mAnimationOffsetsArray.Clear();
        }
    }

}
