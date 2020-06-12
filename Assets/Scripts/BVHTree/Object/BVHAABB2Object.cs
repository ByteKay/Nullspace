
using UnityEngine;

namespace Nullspace
{
    public class BVHAABB2Object : BVHObject2
    {
        public GeoAABB2 mAABB2;
        public Vector2 mExtent;
        public Vector2 mCenter;

        public BVHAABB2Object()
            : base(GeoShape.GeoAABB2)
        {

        }

        public BVHAABB2Object(Vector2 min, Vector2 max) 
            : base(GeoShape.GeoAABB2)
        {
            mAABB2 = new GeoAABB2(min, max);
            mCenter = (min + max) * 0.5f;
            mExtent = max - min;
        }

        override
        public Vector2 GetCenter()
        {
            return mCenter;
        }

        override
        public GeoAABB2 GetAABB()
        {
            return mAABB2;
        }

        override
        public bool TestAABBIntersect(GeoAABB2 aabb)
        {
            return GeoAABBUtils.IsAABBInsectAABB2(aabb.mMin, aabb.mMax, mAABB2.mMin, mAABB2.mMax);
        }

        override
        public bool IsIntersect(ref GeoRay2 dist, ref GeoInsectPointArrayInfo insect)
        {
            bool isInsect = GeoRayUtils.IsRayInsectAABB2(dist.mOrigin, dist.mDirection, mAABB2.mMin, mAABB2.mMax, ref insect);
            if (isInsect)
            {
                insect.mHitObject2 = this;
                insect.mLength = (GeoUtils.ToVector2(insect.mHitGlobalPoint.mPointArray[0]) - dist.mOrigin).magnitude;
            }
            return isInsect;
        }
    }
}
