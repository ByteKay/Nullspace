using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

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

        public NullSkeletonOffset(int version, int frameCount) : this()
        {
            CurrentVersion = version;
        }

        public void SetFrameCount(int frameCount)
        {
            Clear();
            for (int i = 0; i < frameCount; ++i)
            {
                mPosArray.Add(Vector3.zero);
                mFrameTimes.Add(0);
            }
        }

        public void SetFrameOffset(int index, Vector3 position)
        {
            if (index >= mPosArray.Count)
            {
                return;
            }
            mPosArray[index] = position;
        }

        public List<Vector3> GetPosition()
        {
            return mPosArray;
        }

        public List<float> GetFrameTimes()
        {
            return mFrameTimes;
        }

        public void SetBone(NullNodeTree nodeTree)
        {
            if (nodeTree == null)
            {
                return;
            }
            SetBoneHandle(nodeTree.GetNodeHandle());
            SetBoneName(nodeTree.GetNodeName());
        }

        public void SetBoneHandle(int handle)
        {
            mBoneHandle = handle;
        }

        public int GetBoneHandle()
        {
            return mBoneHandle;
        }

        public string GetBoneName()
        {
            return mBoneName;
        }

        public void SetBoneName(string name)
        {
            mBoneName = name;
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

        public NullSkeletonOffsets(int version) : this()
        {
            CurrentVersion = version;
        }

        public void SetAnimationName(string name)
        {
            mAnimationName = name;
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

        public NullSkeletonOffset AppendSkeletonOffset(int frameCount)
        {
            NullSkeletonOffset offset = new NullSkeletonOffset(CurrentVersion, frameCount);
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
        public int GetAnimationOffsetsCount()
        {
            return mAnimationOffsetsArray.Count;
        }

        public NullSkeletonOffsets this[int index]
        {
            get
            {
                Assert.IsTrue(index < GetAnimationOffsetsCount(), "");
                return mAnimationOffsetsArray[index];
            }
        }

        public void SetAnimationOffsetsCount(int count)
        {
            Clear();
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    mAnimationOffsetsArray.Add(new NullSkeletonOffsets(CurrentVersion));
                }
            }
        }

        public int OffsetsCount { get { return mAnimationOffsetsArray.Count; } }
        public void BuildOffsetsFromSkeletonAnimations(NullSkeletonAnimations animations, string nodeName, Vector3 skeletonPos)
        {
            if (animations == null)
            {
                return;
            }

            for (int i = 0; i < animations.GetAnimationCount(); i++)
            {
                NullSkeletonAnimation animation = animations[i];
                NullSkeletonOffsets offsets = BuildOffsetsFromSkeletonAnimation(animation, nodeName, skeletonPos);
                if (offsets != null)
                {
                    mAnimationOffsetsArray.Add(offsets);
                }
            }
        }

        protected NullSkeletonOffsets BuildOffsetsFromSkeletonAnimation(NullSkeletonAnimation animation, string nodeName, Vector3 skeletonPos)
        {
            if (animation == null || animation.GetFrameCount() == 0 || animation.GetNodeCount() == 0)
            {
                return null;
            }

            //the first node of animation always is the root node
            NullSkeletonNodeAnimation rootNode = null;
            if (nodeName != null)
            {
                rootNode = animation[nodeName];
            }
            else
            {
                rootNode = animation[0];
            }

            NullSkeletonOffsets offsets = new NullSkeletonOffsets(CurrentVersion);
            offsets.SetAnimationName(animation.GetAnimationName());

            NullSkeletonOffset offset = offsets.AppendSkeletonOffset(rootNode.GetFrameCount());
            offset.SetBoneHandle(0);

            var srcPosOffsets = rootNode.GetPosition();
            var srcQuatOffsets = rootNode.GetQuat();
            var dstFrameOffsets = offset.GetPosition();
            var dstFrameTimes = offset.GetFrameTimes();
            var srcFrameTimes = animation.GetFrameTimes();
            for (int i = 0; i < rootNode.GetFrameCount(); i++)
            {
                dstFrameTimes[i] = srcFrameTimes[i];
                dstFrameOffsets[i] = srcPosOffsets[i] + skeletonPos;
                srcPosOffsets[i] = Vector3.zero;
            }
            return offsets;
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
