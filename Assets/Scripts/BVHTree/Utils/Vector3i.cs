
namespace Nullspace
{
    public class Vector3i
    {
        private int[] mPos = new int[3];
        public Vector3i()
        {
            mPos[0] = 0;
            mPos[1] = 0;
            mPos[2] = 0;
        }
        public Vector3i(int x, int y, int z)
        {
            mPos[0] = x;
            mPos[1] = y;
            mPos[2] = z;
        }

        public string GetString()
        {
            return string.Format("{0}_{1}_{2}", mPos[0], mPos[1], mPos[2]);
        }

        public int this[int idx]
        {
            get
            {
                return mPos[idx];
            }
            set
            {
                mPos[idx] = value;
            }
        }

        public static Vector3i Min(Vector3i v1, Vector3i v2)
        {
            Vector3i min = new Vector3i();
            min[0] = v1[0] > v2[0] ? v2[0] : v1[0];
            min[1] = v1[1] > v2[1] ? v2[1] : v1[1];
            min[2] = v1[2] > v2[2] ? v2[2] : v1[2];
            return min;
        }
       
        public static Vector3i Max(Vector3i v1, Vector3i v2)
        {
            Vector3i max = new Vector3i();
            max[0] = v1[0] < v2[0] ? v2[0] : v1[0];
            max[1] = v1[1] < v2[1] ? v2[1] : v1[1];
            max[2] = v1[2] < v2[2] ? v2[2] : v1[2];
            return max;
        }

        public static Vector3i operator + (Vector3i v1, Vector3i v2)
        {
            return new Vector3i(v1[0] + v2[0], v1[1] + v2[1], v1[2] + v2[2]);
        }

        public static Vector3i operator - (Vector3i v1, Vector3i v2)
        {
            return new Vector3i(v1[0] - v2[0], v1[1] - v2[1], v1[2] - v2[2]);
        }
        public static Vector3i operator *(Vector3i v1, Vector3i v2)
        {
            return new Vector3i(v1[0] * v2[0], v1[1] * v2[1], v1[2] * v2[2]);
        }

        public static Vector3i operator * (Vector3i v1, int scale)
        {
            return new Vector3i(v1[0] * scale, v1[1] * scale, v1[2] * scale);
        }
        public static Vector3i operator * (int scale, Vector3i v1)
        {
            return v1 * scale;
        }

        public static bool operator != (Vector3i v1, Vector3i v2)
        {
            return (v1[0] != v2[0]) || (v1[1] != v2[1]);
        }

        public static bool operator == (Vector3i v1, Vector3i v2)
        {
            return v1[0] == v2[0] && v1[1] == v2[1];
        }

        public override bool Equals(object obj)
        {
            return this == (Vector3i)obj;
        }

        public override int GetHashCode()
        {
            return GetString().GetHashCode();
        }
    }
}
