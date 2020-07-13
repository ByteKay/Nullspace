using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MeshFile
{
    public class HexSkeletonNodeAnimation : IStream
    {
        protected uint m_parent;
        protected ushort m_frameCount;    // non streamed
        protected List<Vector3> m_posArray;
        protected List<Quaternion> m_quatArray;
        protected string m_boneName;
        protected ushort mCurrentVersion;

        public HexSkeletonNodeAnimation(ushort version, ushort frameCount)
        {
            mCurrentVersion = version;
            m_frameCount = frameCount;
        }

        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            m_posArray = new List<Vector3>();
            m_quatArray = new List<Quaternion>();
            bool res = stream.ReadUInt(ref m_parent);
            res &= stream.ReadQuaternionPositionLst(m_quatArray, m_posArray);
            res &= stream.ReadString(ref m_boneName);
            Debug.Assert(m_frameCount == m_quatArray.Count);
            return res;
        }

        public void Clear()
        {
            m_posArray = null;
            m_quatArray = null;
            m_frameCount = 0;
        }

        public uint GetParent()
        {
            return m_parent;
        }

        public void SetBoneName(string v)
        {
            m_boneName = v;
        }
    }

    public class HexSkeletonAnimation : IStream
    {
        protected string m_animationName;
        protected List<float> m_frameArray;
        protected ushort m_frameCount;
        protected ushort m_frameRate;
        protected List<HexSkeletonNodeAnimation> m_nodeAnimationArray;
        protected ushort m_animationNodeCount;
        protected ushort mCurrentVersion;

        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public HexSkeletonNodeAnimation this[int idx]
        {
            get
            {
                return idx < m_nodeAnimationArray.Count ? m_nodeAnimationArray[idx] : null;
            }
        }


        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadString(ref m_animationName);
            res &= stream.ReadUShort(ref mCurrentVersion);
            ushort frameCount = 0;
            res &= stream.ReadUShort(ref frameCount);
            ushort nodeCount = 0;
            res &= stream.ReadUShort(ref nodeCount);
            SetAnimationNodeAndFrameCount(frameCount, nodeCount);
            res &= stream.ReadUShort(ref m_frameRate);
            res &= stream.ReadFloatLst(ref m_frameArray);
            m_frameCount = (ushort)m_frameArray.Count;
            if (m_animationNodeCount > 0)
            {
                for (int i = 0; i < m_animationNodeCount; i++)
                {
                    res &= m_nodeAnimationArray[i].LoadFromStream(stream);
                }
            }
            return res;
        }

        public bool SetAnimationNodeAndFrameCount(ushort frameCount, ushort nodeCount)
        {
            Clear();
            m_animationNodeCount = nodeCount;
            m_frameCount = frameCount;
            if (m_frameCount > 0)
            {
                if (m_animationNodeCount > 0)
                {
                    m_nodeAnimationArray = new List<HexSkeletonNodeAnimation>(m_animationNodeCount);
                    for (int i = 0; i < m_animationNodeCount; i++)
                    {
                        m_nodeAnimationArray[i] = new HexSkeletonNodeAnimation(mCurrentVersion, m_frameCount);
                    }
                }
                m_frameArray = new List<float>(m_frameCount);
                return true;
            }
            else
            {
                m_animationNodeCount = m_frameCount = 0;
                return false;
            }
        }

        public void Clear()
        {
            m_frameArray = null;
            m_frameCount = 0;
            if (m_nodeAnimationArray != null)
            {
                for (int i = 0; i < m_animationNodeCount; i++)
                {
                    m_nodeAnimationArray[i].Clear();
                }
            }
            m_nodeAnimationArray = null;
            m_animationNodeCount = 0;
        }

        public ushort GetNodeCount()
        {
            return m_animationNodeCount;
        }
    }

    public class HexSkeletonAnimations : IStream
    {
        protected byte m_animationCount;
        protected List<HexSkeletonAnimation> m_animationArray;
        protected ushort mCurrentVersion;
        public HexSkeletonAnimations(ushort version)
        {
            m_animationArray = null;
            m_animationCount = 0;
            mCurrentVersion = version;
        }

        public HexSkeletonAnimation this[int idx]
        {
            get
            {
                return idx < m_animationArray.Count ? m_animationArray[idx] : null;
            }
        }


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
                HexSkeletonAnimation animation = AppendAnimation();
                res &= animation.LoadFromStream(stream);
            }
            return res;
        }

        public HexSkeletonAnimation AppendAnimation()
        {
            if (m_animationArray == null)
            {
                m_animationArray = new List<HexSkeletonAnimation>();
            }
            HexSkeletonAnimation ani = new HexSkeletonAnimation();
            m_animationArray.Add(ani);
            m_animationCount++;
            return ani;
        }

        public void Clear()
        {
            if (m_animationArray == null)
            {
                return;
            }
            for (int i = 0; i < m_animationCount; i++)
            {
                m_animationArray[i].Clear();
            }
            m_animationArray = null;
            m_animationCount = 0;
        }
        public byte GetAnimationCount()
        {
            return m_animationCount;
        }
    }
}
