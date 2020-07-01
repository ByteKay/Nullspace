using UnityEngine;

namespace Nullspace
{
    public class bbox
    {
        public Vector3 min;
        public Vector3 mid;
        public Vector3 max;
        public Vector3 size;
        public float zmin;
        public float zmax;

        public bbox()
        {

        }

        public void ToMidSize()
        {
            mid = (min + max) * 0.5f;
            size = (max - mid);
        }

        public void ToMinMax()
        {
            min = mid - size;
            max = mid + size;
        }

        public void Begin()
        {
            min = Vector3.one * float.MaxValue;
            max = Vector3.one * float.MinValue;
        }

        public void Add(ref Vector3 v)
        {
            min = Vector3.Min(min, v);
            max = Vector3.Max(max, v);
        }
    }
}

