using UnityEngine;

namespace Nullspace
{
    public class OOBox
    {
        public Vector3 Min;
        public Vector3 Mid;
        public Vector3 Max;
        public Vector3 Size;
        public float Zmin;
        public float Zmax;

        public OOBox()
        {
            Begin();
            Mid = (Min + Max) * 0.5f;
            Size = (Max - Mid);
            Zmin = Zmax = 0;
        }

        public OOBox(Vector3 minV, Vector3 maxV)
        {
            Min = minV;
            Max = maxV;
            Mid = (Min + Max) * 0.5f;
            Size = (Max - Mid);
            Zmin = Zmax = 0;
        }

        public void ToMidSize()
        {
            Mid = (Min + Max) * 0.5f;
            Size = (Max - Mid);
        }

        public void ToMinMax()
        {
            Min = Mid - Size;
            Max = Mid + Size;
        }

        public void Begin()
        {
            Min = Vector3.one * float.MaxValue;
            Max = Vector3.one * float.MinValue;
        }

        public void Add(Vector3 v)
        {
            Min = Vector3.Min(Min, v);
            Max = Vector3.Max(Max, v);
        }
    }
}

