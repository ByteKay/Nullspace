using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class NullSkeletonOffset : INullStream
    {
        protected uint m_boneHandle;
        protected string m_boneName;
        //non streamed
        protected ushort m_frameCount;
        protected List<Vector3> m_posArray;
        protected List<float> m_frameTimes;                
		protected ushort mCurrentVersion;

        public NullSkeletonOffset(ushort version, ushort frameCount)
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
            ushort count = 0;
            bool res = stream.ReadUShort(out count);
            SetFrameCount(count);
            res &= stream.ReadUInt(out m_boneHandle);
            res &= stream.ReadString(out m_boneName);
            res &= stream.ReadList(out m_posArray);
            res &= stream.ReadList(out m_frameTimes);
            return res;
        }

        public bool SetFrameCount(ushort frameCount)
        {
            Clear();
            m_frameCount = frameCount;
            if (m_frameCount > 0)
            {
                m_posArray = new List<Vector3>(m_frameCount);
                m_frameTimes = new List<float>(m_frameCount);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Clear()
        {
            m_frameCount = 0;
            m_posArray = null;
            m_frameTimes = null;
        }
    }

    public class NullSkeletonOffsets : INullStream
    {
		protected string m_animationName;
        protected byte m_animationOffsetCount;
        protected List<NullSkeletonOffset> m_animationOffsetArray;
		protected ushort mCurrentVersion;

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
                res &= stream.ReadString(out m_animationName);
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
            if (m_animationOffsetArray == null)
            {
                m_animationOffsetArray = new List<NullSkeletonOffset>();
            }
            NullSkeletonOffset offset = new NullSkeletonOffset(mCurrentVersion, frameCount);
            m_animationOffsetArray.Add(offset);
            m_animationOffsetCount++;
            return offset;
        }

        public void Clear()
        {
            for (int i = 0; i < m_animationOffsetCount; i++)
            {
                m_animationOffsetArray[i].Clear();
            }
            m_animationOffsetArray = null;
            m_animationOffsetCount = 0;
        }
    }
    
    public class NullSkeletonOffsetFile : INullStream
    {
        protected ushort m_offsetsCount;
        protected List<NullSkeletonOffsets> m_animationOffsetsArray;
        protected ushort m_blockSize;
        protected ushort m_version;

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
            bool res = stream.ReadUShort(out m_version);
            res &= stream.ReadUShort(out m_blockSize);
            ushort count = 0;
            res &= stream.ReadUShort(out count);
            SetAnimationOffsetsCount(count);
            for (int i = 0; i < m_offsetsCount; i++)
            {
                res &= m_animationOffsetsArray[i].LoadFromStream(stream);
            }
            return res;
        }

        public void Clear()
        {
            for (int i = 0; i < m_offsetsCount; i++)
            {
                m_animationOffsetsArray[i].Clear();
            }
            m_animationOffsetsArray = null;
            m_offsetsCount = 0;
        }

        public void SetAnimationOffsetsCount(ushort count)
        {
            Clear();
            m_offsetsCount = count;
            if (m_offsetsCount > 0)
            {
                m_animationOffsetsArray = new List<NullSkeletonOffsets>(m_offsetsCount);
                for (int i = 0; i < m_offsetsCount; ++i)
                {
                    m_animationOffsetsArray.Add(new NullSkeletonOffsets());
                }
            }
        }
    }

}
