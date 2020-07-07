
using UnityEngine;

namespace Nullspace
{
    public class GeoLine2
    {
        public Vector2 mP1;
        public Vector2 mP2;
        public Vector2 mDirection;

        public GeoLine2(Vector2 p1, Vector2 p2)
        {
            mP1 = p1;
            mP2 = p2;
            mDirection = mP2 - mP1;
            mDirection.Normalize();
        }
    }

    public class GeoLine3
    {
        public Vector3 mP1;
        public Vector3 mP2;
        public Vector3 mDirection;
        public GeoLine3(Vector3 p1, Vector3 p2)
        {
            mP1 = p1;
            mP2 = p2;
            mDirection = mP2 - mP1;
            mDirection.Normalize();
        }
    }
}
