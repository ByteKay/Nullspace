
using UnityEngine;

namespace Nullspace
{
    public class BVHAABB3Object : BVHObject3
    {
        public GeoAABB3 mAABB3;
        public Vector3 mExtent;
        public Vector3 mCenter;

        public BVHAABB3Object()
            : base(GeoShape.GeoAABB3)
        {
            
        }

        public BVHAABB3Object(Vector3 min, Vector3 max)
            : base(GeoShape.GeoAABB3)
        {
            mAABB3 = new GeoAABB3(min, max);
            mCenter = (min + max) * 0.5f;
            mExtent = max - min;
        }
        override
        public Vector3 GetCenter()
        {
            return mCenter;
        }

        override
        public GeoAABB3 GetAABB()
        {
            return mAABB3;
        }

        override
        public bool IsIntersect(ref GeoRay3 dist, ref GeoInsectPointArrayInfo insect)
        {
            bool isInsect = GeoRayUtils.IsRayInsectAABB2(dist.mOrigin, dist.mDirection, mAABB3.mMin, mAABB3.mMax, ref insect);
            if (isInsect)
            {
                insect.mHitObject2 = this;
                insect.mLength = (insect.mHitGlobalPoint.mPointArray[0] - dist.mOrigin).magnitude;
            }
            return isInsect;
        }
    }
}
