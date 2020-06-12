
using UnityEngine;

namespace Nullspace
{
    public class GeoAABB2
    {
        public Vector2 mMin;
        public Vector2 mMax;
        public Vector2 mSize;
        public GeoAABB2()
        {

        }
        public GeoAABB2(Vector2 min, Vector2 max)
        {
            mMin = min;
            mMax = max;
            mSize = mMax - mMin;
        }

        public float Area()
        {
            Vector2 size = mMax - mMin;
            return size[0] * size[1];
        }
    }

    public class GeoAABB3
    {
        public Vector3 mMin;
        public Vector3 mMax;
        public Vector3 mSize;
        public GeoAABB3()
        {
        }

        public GeoAABB3(Vector3 min, Vector3 max)
        {
            mMin = min;
            mMax = max;
            mSize = mMax - mMin;
        }
    }
}
