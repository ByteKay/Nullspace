
using UnityEngine;

namespace Nullspace
{
    public class GeoRay2
    {
        public Vector2 mOrigin;
        public Vector2 mDirection;

        public GeoRay2(Vector2 origin, Vector2 direction)
        {
            mOrigin = origin;
            mDirection = direction;
        }
    }

    public class GeoRay3
    {
        public Vector3 mOrigin;
        public Vector3 mDirection;

        public GeoRay3(Vector3 origin, Vector3 direction)
        {
            mOrigin = origin;
            mDirection = direction;
        }
    }
}
