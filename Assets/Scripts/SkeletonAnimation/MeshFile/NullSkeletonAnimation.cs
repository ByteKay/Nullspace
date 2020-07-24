using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

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

        public NullSkeletonNodeAnimation(int version, int frameCount) : this()
        {
            CurrentVersion = version;
            mParent = 0;
            mBoneName = "";
            SetFrameCount(frameCount);
        }

        public void SetFrameCount(int frameCount)
        {
            Clear();
            if (frameCount > 0)
            {
                for (int i = 0; i < frameCount; i++)
                {
                    mPosArray.Add(Vector3.zero);
                    mQuatArray.Add(Quaternion.identity);
                }
            }
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteInt(mParent);
            size += stream.WriteList(mPosArray, false);
            size += stream.WriteList(mQuatArray, true);
            size += stream.WriteString(mBoneName);
            return size;
        }

        public void RemoveFrames(HashSet<int> indexSet)
        {
            if (indexSet.Count > 0)
            {
                List<int> idxes = indexSet.ToList();
                idxes.Sort();
                for (int i = idxes.Count - 1; i >= 0; --i)
                {
                    mPosArray.RemoveAt(idxes[i]);
                    mQuatArray.RemoveAt(idxes[i]);
                }
            }
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadInt(out mParent);
            res &= stream.ReadList(out mPosArray);
            res &= stream.ReadList(out mQuatArray, GetFrameCount());
            res &= stream.ReadString(out mBoneName);
            return res;
        }

        public void Clear()
        {
            mPosArray.Clear();
            mQuatArray.Clear();
        }

        public List<Vector3>  GetPosition()
        {
            return mPosArray;
        }

        public List<Quaternion> GetQuat()
        {
            return mQuatArray;
        }

        public void SetPosition(int index, Vector3 pos)
        {
            Assert.IsTrue(index < GetFrameCount(), "");
            mPosArray[index] = pos;
        }

        public void SetQuaternion(int index, Quaternion q)
        {
            Assert.IsTrue(index < GetFrameCount(), "");
            mQuatArray[index] = q;
        }

        public void SetParent(int parent)
        {
            mParent = parent;
        }

        public int GetParent()
        {
            return mParent;
        }

        public int GetFrameCount()
        {
            return mPosArray.Count;
        }

        public void SetBoneName(string v)
        {
            mBoneName = v;
        }

        public string GetBoneName()
        {
            return mBoneName;
        }
    }

    public class NullSkeletonAnimation : INullStream
    {
        public int CurrentVersion;
        protected string mAnimationName;
        protected int mFrameRate;
        protected List<float> mFrameTimes;
        protected List<NullSkeletonNodeAnimation> mNodeAnimationArray;

        public NullSkeletonAnimation()
        {
            mFrameTimes = new List<float>();
            mNodeAnimationArray = new List<NullSkeletonNodeAnimation>();
        }

        public bool SetAnimationNodeAndFrameCount(int frameCount, int nodeCount = 0)
        {
            Clear();
            if (frameCount > 0)
            {
                if (nodeCount > 0)
                {
                    for (int i = 0; i < nodeCount; i++)
                    {
                        mNodeAnimationArray.Add(new NullSkeletonNodeAnimation(CurrentVersion, frameCount));
                    }
                }
                for (int i = 0; i < frameCount; ++i)
                {
                    mFrameTimes.Add(0);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public NullSkeletonNodeAnimation this[int idx]
        {
            get
            {
                Assert.IsTrue(idx < mNodeAnimationArray.Count, "");
                return mNodeAnimationArray[idx];
            }
        }

        public NullSkeletonNodeAnimation this[string name]
        {
            get
            {
                foreach (var anim in mNodeAnimationArray)
                {
                    if (anim.GetBoneName() == name)
                    {
                        return anim;
                    }
                }
                return null;
            }
        }
        public void SetAnimationName(string name)
        {
            mAnimationName = name;
        }

        public NullSkeletonNodeAnimation AppendAnimationNode()
        {
            NullSkeletonNodeAnimation animation = new NullSkeletonNodeAnimation(CurrentVersion, mFrameTimes.Count);
            mNodeAnimationArray.Add(animation);
            return animation;
        }

        public int GetNodeCount()
        {
            return mNodeAnimationArray.Count;
        }

        public int GetFrameCount()
        {
            return mFrameTimes.Count;
        }

        public string GetAnimationName()
        {
            return mAnimationName;
        }

        public void SetFrameTime(int index, float time)
        {
            Assert.IsTrue(index < mFrameTimes.Count, "");
            mFrameTimes[index] = time;
        }

        public void SetFrameRate(int frameRate)
        {
            mFrameRate = frameRate;
        }

        public List<float> GetFrameTimes()
        {
            return mFrameTimes;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteString(mAnimationName);
            size += stream.WriteInt(CurrentVersion);
            size += stream.WriteInt(mFrameRate);
            size += stream.WriteList(mFrameTimes, false);
            size += stream.WriteList(mNodeAnimationArray, false);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadString(out mAnimationName);
            res &= stream.ReadInt(out CurrentVersion);
            res &= stream.ReadInt(out mFrameRate);
            res &= stream.ReadList(out mFrameTimes);
            res &= stream.ReadList(out mNodeAnimationArray);

            return res;
        }

        public void Clear()
        {
            mFrameTimes.Clear();
            mNodeAnimationArray.Clear();
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
        public int GetAnimationCount()
        {
            return mAnimationArray.Count;
        }

        public NullSkeletonAnimation this[int idx]
        {
            get
            {
                Assert.IsTrue(idx < mAnimationArray.Count, "");
                return mAnimationArray[idx];
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

    }
}
