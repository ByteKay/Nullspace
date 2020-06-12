
using UnityEngine;

namespace Nullspace
{
    public class BVHTriangle3Object : BVHObject3
    {
        public Vector3 mP1;
        public Vector3 mP2;
        public Vector3 mP3;
        public Vector3 mCenter;
        public GeoAABB3 mAABB;
        public int mMeshIndex;
        public int mFaceIndex;

        public BVHTriangle3Object(Vector3 p1, Vector3 p2, Vector3 p3, int meshIndex = 0, int faceIndex = 0)
            : base(GeoShape.GeoTriangle3)
        {
            mP1 = p1;
            mP2 = p2;
            mP3 = p3;
            mCenter = (mP1 + mP2 + mP3) * 0.333333f;
            mAABB = new GeoAABB3(Vector3.Min(Vector3.Min(mP1, mP2), mP3), Vector3.Max(Vector3.Max(mP1, mP2), mP3));
            mMeshIndex = meshIndex;
            mFaceIndex = faceIndex;
        }
        override
        public Vector3 GetCenter()
        {
            return mCenter;
        }

        override
        public GeoAABB3 GetAABB()
        {
            return mAABB;
        }

        override
        public bool IsIntersect(ref GeoRay3 dist, ref GeoInsectPointArrayInfo insect)
        {
            GeoInsectPointInfo info = new GeoInsectPointInfo();
            bool isInsect = GeoRayUtils.IsRayInsectTriangle3(dist.mOrigin, dist.mDirection, mP1, mP2, mP3, ref info);
            if (isInsect)
            {
                insect.mHitObject2 = this;
                insect.mHitGlobalPoint.mPointArray.Add(info.mHitGlobalPoint);
                insect.mLength = (info.mHitGlobalPoint - dist.mOrigin).magnitude;
            }
            return isInsect;
        }
    }
}
