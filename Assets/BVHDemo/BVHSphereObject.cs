using UnityEngine;

namespace Nullspace
{
    public class BVHSphereObject : BVHObject
    {
        public Vector3 mCenter;
        public float mRadius, mRadius2; // Radius, Radius^2

        public BVHSphereObject(Vector3 center, float r)
        {
            mCenter = center;
            mRadius = r;
            mRadius2 = mRadius * mRadius;
        }

        
        public override bool GetIntersection(ref BVHRay ray, ref BVHIntersectionInfo intersection)
        {
            Vector3 s = mCenter - ray.mOrigin;
            float sd = Vector3.Dot(s, ray.mDirection);
            float ss = s.magnitude * s.magnitude;
            float disc = sd * sd + mRadius2 - ss;
            if (disc < 0.0f)
            {
                return false;
            }
            intersection.mObject = this;
            intersection.mLength = sd - Mathf.Sqrt(disc);
            return true;
        }


        public override Vector3 GetNormal(ref BVHIntersectionInfo i)
        {
            Vector3 nor = i.mHitPoint - mCenter;
            nor.Normalize();
            return nor;
        }

        
        public override BVHBox GetBBox()
        {
            return new BVHBox(mCenter - new Vector3(mRadius, mRadius, mRadius), mCenter + new Vector3(mRadius, mRadius, mRadius)); ;
        }

        
        public override Vector3 GetCentroid()
        {
            return mCenter;
        }
    }
}
