using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nullspace
{
    public class MathUtils
    {
        public static float Interpolation(float left, float right, float t)
        {
            bool needRevert = left > right;

            if (needRevert)
            {
                left = -left;
                right = -right;
            }

            if (t < 0) t = 0;
            else if (t > 1) t = 1;

            float v = left + t * (right - left);
            return needRevert ? -v : v;
        }
        public static Vector3 Interpolation(Vector3 left, Vector3 right, float t)
        {
            if (t < 0) t = 0;
            else if (t > 1) t = 1;
            return left + t * (right - left);
        }
    }
}
