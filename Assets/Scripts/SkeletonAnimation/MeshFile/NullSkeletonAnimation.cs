using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullSkeletonNodeAnimation : INullStream
    {
        protected uint Parent;
        protected ushort FrameCount;    // non streamed
        protected List<Vector3> PosArray;
        protected List<Quaternion> QuatArray;
        protected string BoneName;
        protected ushort CurrentVersion;

        public NullSkeletonNodeAnimation(ushort version, ushort frameCount)
        {
            CurrentVersion = version;
            FrameCount = frameCount;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteUInt(Parent);
            size += stream.WriteUShort(FrameCount);
            size += stream.WriteList(PosArray, true);
            size += stream.WriteList(QuatArray, true);
            size += stream.WriteString(BoneName);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadUInt(out Parent);
            res &= stream.ReadUShort(out FrameCount);
            res &= stream.ReadList(out PosArray, FrameCount);
            res &= stream.ReadList(out QuatArray, FrameCount);
            res &= stream.ReadString(out BoneName);
            return res;
        }

        public void Clear()
        {
            PosArray = null;
            QuatArray = null;
            FrameCount = 0;
        }

        public uint GetParent()
        {
            return Parent;
        }

        public void SetBoneName(string v)
        {
            BoneName = v;
        }
    }

    public class NullSkeletonAnimation : INullStream
    {
        protected string AnimationName;
        protected List<float> FrameArray;
        protected ushort FrameCount;
        protected ushort FrameRate;
        protected List<NullSkeletonNodeAnimation> NodeAnimationArray;
        protected ushort AnimationNodeCount;
        protected ushort CurrentVersion;

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion =NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteString(AnimationName);
            size += stream.WriteUShort(CurrentVersion);
            size += stream.WriteUShort(FrameCount);
            size += stream.WriteUShort(AnimationNodeCount);
            size += stream.WriteUShort(FrameRate);
            if (FrameCount > 0)
            {
                size += stream.WriteList(FrameArray, true);
            }
            if (AnimationNodeCount > 0)
            {
                for (int i = 0; i < AnimationNodeCount; i++)
                {
                    size += NodeAnimationArray[i].SaveToStream(stream);
                }
            }
            return size;
        }

        public NullSkeletonNodeAnimation this[int idx]
        {
            get
            {
                return idx < NodeAnimationArray.Count ? NodeAnimationArray[idx] : null;
            }
        }


        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadString(out AnimationName);
            res &= stream.ReadUShort(out CurrentVersion);
            res &= stream.ReadUShort(out FrameCount);
            res &= stream.ReadUShort(out AnimationNodeCount);
            res &= stream.ReadUShort(out FrameRate);
            SetAnimationNodeAndFrameCount(FrameCount, AnimationNodeCount);
            res &= stream.ReadList(out FrameArray, FrameCount);
            if (AnimationNodeCount > 0)
            {
                for (int i = 0; i < AnimationNodeCount; i++)
                {
                    res &= NodeAnimationArray[i].LoadFromStream(stream);
                }
            }
            return res;
        }

        public bool SetAnimationNodeAndFrameCount(ushort frameCount, ushort nodeCount)
        {
            Clear();
            AnimationNodeCount = nodeCount;
            FrameCount = frameCount;
            if (FrameCount > 0)
            {
                if (AnimationNodeCount > 0)
                {
                    NodeAnimationArray = new List<NullSkeletonNodeAnimation>(AnimationNodeCount);
                    for (int i = 0; i < AnimationNodeCount; i++)
                    {
                        NodeAnimationArray[i] = new NullSkeletonNodeAnimation(CurrentVersion, FrameCount);
                    }
                }
                FrameArray = new List<float>(FrameCount);
                return true;
            }
            else
            {
                AnimationNodeCount = FrameCount = 0;
                return false;
            }
        }

        public void Clear()
        {
            FrameArray = null;
            FrameCount = 0;
            if (NodeAnimationArray != null)
            {
                for (int i = 0; i < AnimationNodeCount; i++)
                {
                    NodeAnimationArray[i].Clear();
                }
            }
            NodeAnimationArray = null;
            AnimationNodeCount = 0;
        }

        public ushort GetNodeCount()
        {
            return AnimationNodeCount;
        }
    }

    public class NullSkeletonAnimations : INullStream
    {
        protected byte AnimationCount;
        protected List<NullSkeletonAnimation> AnimationArray;
        protected ushort CurrentVersion;
        public NullSkeletonAnimations(ushort version)
        {
            AnimationArray = null;
            AnimationCount = 0;
            CurrentVersion = version;
        }

        public NullSkeletonAnimation this[int idx]
        {
            get
            {
                return idx < AnimationArray.Count ? AnimationArray[idx] : null;
            }
        }


        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
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
                NullSkeletonAnimation animation = AppendAnimation();
                res &= animation.LoadFromStream(stream);
            }
            return res;
        }

        public NullSkeletonAnimation AppendAnimation()
        {
            if (AnimationArray == null)
            {
                AnimationArray = new List<NullSkeletonAnimation>();
            }
            NullSkeletonAnimation ani = new NullSkeletonAnimation();
            AnimationArray.Add(ani);
            AnimationCount++;
            return ani;
        }

        public void Clear()
        {
            if (AnimationArray == null)
            {
                return;
            }
            for (int i = 0; i < AnimationCount; i++)
            {
                AnimationArray[i].Clear();
            }
            AnimationArray = null;
            AnimationCount = 0;
        }
        public byte GetAnimationCount()
        {
            return AnimationCount;
        }
    }
}
