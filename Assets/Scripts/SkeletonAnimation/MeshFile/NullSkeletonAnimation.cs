using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullSkeletonNodeAnimation : INullStream
    {
        public int CurrentVersion;
        protected string mBoneName;
        protected int mParent;
        protected List<Vector3> mPosArray;
        protected List<Quaternion> mQuatArray;

        public NullSkeletonNodeAnimation()
        {
            mPosArray = new List<Vector3>();
            mQuatArray = new List<Quaternion>();
        }

        public NullSkeletonNodeAnimation(int version) : this()
        {
            CurrentVersion = version;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteInt(mParent);
            size += stream.WriteList(mPosArray, false);
            size += stream.WriteList(mQuatArray, false);
            size += stream.WriteString(mBoneName);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadInt(out mParent);
            res &= stream.ReadList(out mPosArray);
            res &= stream.ReadList(out mQuatArray);
            res &= stream.ReadString(out mBoneName);
            return res;
        }

        public void Clear()
        {
            mPosArray.Clear();
            mQuatArray.Clear();
        }

        public int GetParent()
        {
            return mParent;
        }

        public void SetBoneName(string v)
        {
            mBoneName = v;
        }
    }

    public class NullSkeletonAnimation : INullStream
    {
        public int CurrentVersion;
        protected string mAnimationName;
        protected int mFrameRate;
        protected List<float> mFrameArray;
        protected List<NullSkeletonNodeAnimation> mNodeAnimationArray;

        public NullSkeletonAnimation()
        {
            mFrameArray = new List<float>();
            mNodeAnimationArray = new List<NullSkeletonNodeAnimation>();
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteString(mAnimationName);
            size += stream.WriteInt(CurrentVersion);
            size += stream.WriteInt(mFrameRate);
            size += stream.WriteList(mFrameArray, false);
            size += stream.WriteList(mNodeAnimationArray, false);
            return size;
        }

        public NullSkeletonNodeAnimation this[int idx]
        {
            get
            {
                return idx < mNodeAnimationArray.Count ? mNodeAnimationArray[idx] : null;
            }
        }


        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadString(out mAnimationName);
            res &= stream.ReadInt(out CurrentVersion);
            res &= stream.ReadInt(out mFrameRate);
            res &= stream.ReadList(out mFrameArray);
            res &= stream.ReadList(out mNodeAnimationArray);

            return res;
        }

        public void Clear()
        {
            mFrameArray.Clear();
            mNodeAnimationArray.Clear();
        }

        public int GetNodeCount()
        {
            return mNodeAnimationArray.Count;
        }
    }

    public class NullSkeletonAnimations : INullStream
    {
        public int CurrentVersion;
        protected List<NullSkeletonAnimation> mAnimationArray;

        public NullSkeletonAnimations()
        {
            mAnimationArray = new List<NullSkeletonAnimation>();
        }

        public NullSkeletonAnimations(int version) : this()
        {
            CurrentVersion = version;
        }

        public NullSkeletonAnimation this[int idx]
        {
            get
            {
                return idx < mAnimationArray.Count ? mAnimationArray[idx] : null;
            }
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            return stream.WriteList(mAnimationArray, false);
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            return stream.ReadList(out mAnimationArray);
        }

        public NullSkeletonAnimation AppendAnimation()
        {
            NullSkeletonAnimation ani = new NullSkeletonAnimation();
            mAnimationArray.Add(ani);
            return ani;
        }

        public void Clear()
        {
            mAnimationArray.Clear();
        }
        public int GetAnimationCount()
        {
            return mAnimationArray.Count;
        }
    }
}
