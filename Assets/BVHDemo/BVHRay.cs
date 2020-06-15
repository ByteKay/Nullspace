using UnityEngine;

namespace Nullspace
{
    public class BVHRay
    {
        public Vector3 mOrigin; 
        public Vector3 mDirection;
        public Vector3 mInvDirection;

        public BVHRay(Vector3 o, Vector3 d)
        {
            mOrigin = o;
            mDirection = d;
            mInvDirection = new Vector3(1 / d[0], 1 / d[1], 1 / d[2]);
        }
    }
}
