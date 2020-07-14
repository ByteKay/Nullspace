using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullSkeletonNodeAnimation : INullStream
    {
        protected uint m_parent;
        protected ushort m_frameCount;    // non streamed
        protected List<Vector3> m_posArray;
        protected List<Quaternion> m_quatArray;
        protected string m_boneName;
        protected ushort mCurrentVersion;

        public NullSkeletonNodeAnimation(ushort version, ushort frameCount)
        {
            mCurrentVersion = version;
            m_frameCount = frameCount;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadUInt(out m_parent);
            res &= stream.ReadList(out m_posArray);
            res &= stream.ReadList(out m_quatArray);
            res &= stream.ReadString(out m_boneName);
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

    public class NullSkeletonAnimation : INullStream
    {
        protected string m_animationName;
        protected List<float> m_frameArray;
        protected ushort m_frameCount;
        protected ushort m_frameRate;
        protected List<NullSkeletonNodeAnimation> m_nodeAnimationArray;
        protected ushort m_animationNodeCount;
        protected ushort mCurrentVersion;

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public NullSkeletonNodeAnimation this[int idx]
        {
            get
            {
                return idx < m_nodeAnimationArray.Count ? m_nodeAnimationArray[idx] : null;
            }
        }


        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadString(out m_animationName);
            res &= stream.ReadUShort(out mCurrentVersion);
            ushort frameCount = 0;
            res &= stream.ReadUShort(out frameCount);
            ushort nodeCount = 0;
            res &= stream.ReadUShort(out nodeCount);
            SetAnimationNodeAndFrameCount(frameCount, nodeCount);
            res &= stream.ReadUShort(out m_frameRate);
            res &= stream.ReadList(out m_frameArray);
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
                    m_nodeAnimationArray = new List<NullSkeletonNodeAnimation>(m_animationNodeCount);
                    for (int i = 0; i < m_animationNodeCount; i++)
                    {
                        m_nodeAnimationArray[i] = new NullSkeletonNodeAnimation(mCurrentVersion, m_frameCount);
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

    public class NullSkeletonAnimations : INullStream
    {
        protected byte m_animationCount;
        protected List<NullSkeletonAnimation> m_animationArray;
        protected ushort mCurrentVersion;
        public NullSkeletonAnimations(ushort version)
        {
            m_animationArray = null;
            m_animationCount = 0;
            mCurrentVersion = version;
        }

        public NullSkeletonAnimation this[int idx]
        {
            get
            {
                return idx < m_animationArray.Count ? m_animationArray[idx] : null;
            }
        }


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
                NullSkeletonAnimation animation = AppendAnimation();
                res &= animation.LoadFromStream(stream);
            }
            return res;
        }

        public NullSkeletonAnimation AppendAnimation()
        {
            if (m_animationArray == null)
            {
                m_animationArray = new List<NullSkeletonAnimation>();
            }
            NullSkeletonAnimation ani = new NullSkeletonAnimation();
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
