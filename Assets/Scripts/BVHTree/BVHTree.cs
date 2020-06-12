
using UnityEngine;

namespace Nullspace
{
    public class BVHAABB2
    {
        public Vector2 mMin;
        public Vector2 mMax;
        public Vector2 mExtent;

        public BVHAABB2(Vector2 min, Vector2 max)
        {
            mMin = min;
            mMax = max;
            mExtent = mMax - mMin;
        }
        public BVHAABB2(Vector2 point)
        {
            mMin = point;
            mMax = point;
            mExtent = mMax - mMin;
        }

        public void ExpandToInclude(Vector2 p)
        {
            mMin = Vector2.Min(mMin, p);
            mMax = Vector2.Max(mMax, p);
            mExtent = mMax - mMin;
        }

        public void ExpandToInclude(GeoAABB2 b)
        {
            mMin = Vector2.Min(mMin, b.mMin);
            mMax = Vector2.Max(mMax, b.mMax);
            mExtent = mMax - mMin;
        }

        public int MaxDimension()
        {
            int result = 0;
            if (mExtent.y > mExtent.x)
            {
                result = 1;
            }
            return result;
        }
    }
    public class BVHAABB3
    {
        public Vector3 mMin;
        public Vector3 mMax;
        public Vector3 mExtent;
        public BVHAABB3(Vector3 min, Vector3 max)
        {
            mMin = min;
            mMax = max;
            mExtent = mMax - mMin;
        }
        public BVHAABB3(Vector3 point)
        {
            mMin = point;
            mMax = point;
            mExtent = mMax - mMin;
        }

        public void ExpandToInclude(Vector3 p)
        {
            mMin = Vector3.Min(mMin, p);
            mMax = Vector3.Max(mMax, p);
            mExtent = mMax - mMin;
        }

        public void ExpandToInclude(GeoAABB3 b)
        {
            mMin = Vector3.Min(mMin, b.mMin);
            mMax = Vector3.Max(mMax, b.mMax);
            mExtent = mMax - mMin;
        }

        public int MaxDimension()
        {
            int result = 0;
            if (mExtent.y > mExtent.x)
            {
                result = 1;
                if (mExtent.z > mExtent.y)
                {
                    result = 2;
                }
            }
            else
            {
                if (mExtent.z > mExtent.x)
                {
                    result = 2;
                }
            }
            return result;
        }
    }
    public class BVHFlatNode3
    {
        public BVHAABB3 mBox;
        public uint mStartIndex;
        public uint mLeafCount;
        public uint mRightOffset;
    }
    public class BVHFlatNode2
    {
        public BVHAABB2 mBox;
        public uint mStartIndex;
        public uint mLeafCount;
        public uint mRightOffset;
    }
    class BVHTraversal
    {
        public int mIndex;
        public float mLength;
        public BVHTraversal() { }
        public BVHTraversal(int idx, float len)
        {
            mIndex = idx;
            mLength = len;
        }
    }
    class BVHBuildEntry
    {
        public uint mParent;
        public uint mStart;
        public uint mEnd;
    }

    

}
