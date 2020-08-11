
using UnityEngine;

namespace Nullspace
{
    public enum NavPathFlipType
    {
        None = 0,
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        W = 1 << 3
    }

    public class NavPathFlipUtils
    {
        public static bool Flip(NavPathFlipType type, ref Vector2 value)
        {
            int idx = 0;
            int t = (int)type;
            while ((t & (1 << idx)) != 0 && idx < 2)
            {
                value[idx] = Flip((NavPathFlipType)(1 << idx), value[idx]);
                idx++;
            }
            return true;
        }
        public static bool Flip(NavPathFlipType type, ref Vector3 value)
        {
            int idx = 0;
            int t = (int)type;
            while ((t & (1 << idx)) != 0 && idx < 3)
            {
                value[idx] = Flip((NavPathFlipType)(1 << idx), value[idx]);
                idx++;
            }
            return true;

        }
        public static bool Flip(NavPathFlipType type, ref Vector4 value)
        {
            int idx = 0;
            int t = (int)type;
            while ((t & (1 << idx)) != 0 && idx < 4)
            {
                value[idx] = Flip((NavPathFlipType)(1 << idx), value[idx]);
                idx++;
            }
            return true;
        }

        public static bool Flip(NavPathFlipType type, Vector2 value, out Vector2 outV)
        {
            outV = value;
            int idx = 0;
            int t = (int)type;
            while ((t & (1 << idx)) != 0 && idx < 2)
            {
                outV[idx] = Flip((NavPathFlipType)(1 << idx), value[idx]);
                idx++;
            }
            return true;
        }
        public static bool Flip(NavPathFlipType type, Vector3 value, out Vector3 outV)
        {
            outV = value;
            int idx = 0;
            int t = (int)type;
            while ((t & (1 << idx)) != 0 && idx < 3)
            {
                outV[idx] = Flip((NavPathFlipType)(1 << idx), value[idx]);
                idx++;
            }
            return true;

        }
        public static bool Flip(NavPathFlipType type, Vector4 value, out Vector4 outV)
        {
            outV = value;
            int idx = 0;
            int t = (int)type;
            while ((t & (1 << idx)) != 0 && idx < 4)
            {
                outV[idx] = Flip((NavPathFlipType)(1 << idx), value[idx]);
                idx++;
            }
            return true;
        }

        private static float Flip(NavPathFlipType type, float v)
        {
            switch (type)
            {
                case NavPathFlipType.None:
                    return v;
                case NavPathFlipType.X:
                case NavPathFlipType.Y:
                case NavPathFlipType.Z:
                case NavPathFlipType.W:
                    return -v;
            }
            return v;
        }
    }
}
