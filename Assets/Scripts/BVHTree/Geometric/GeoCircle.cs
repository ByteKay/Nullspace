
using UnityEngine;

namespace Nullspace
{
    public class GeoCircle2
    {
        public Vector2 mCenter;
        public float mRadius;

        public GeoCircle2(Vector2 center, float r)
        {
            mCenter = center;
            mRadius = r;
        }
    }

    public class GeoCircle3
    {
        public Vector3 mCenter;
        public float mRadius;
        // private GeoPlane mPlane;
        public GeoCircle3(Vector3 center, float r, Vector3 normal)
        {
            mCenter = center;
            mRadius = r;
        }

    }
}
