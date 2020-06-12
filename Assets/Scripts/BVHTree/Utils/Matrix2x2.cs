
using UnityEngine;

namespace Nullspace
{
    // maybe use Matrix4x4 instead Matrix2x3 and Matrix2x2
    public class Matrix2x3
    {
        private Vector3 mFirst;
        private Vector3 mLast;
        public Matrix2x3(Vector2 one, Vector2 two)
        {
            mFirst = new Vector3(one[0], one[1], 0.0f);
            mLast = new Vector3(two[0], two[1], 0.0f);
        }
        public Vector2 Translate
        {
            set
            {
                mFirst[2] = value[0];
                mLast[2] = value[1];
            }
            get
            {
                return new Vector2(mFirst[2], mLast[2]);
            }
        }

    }

    public class Matrix2x2
    {
        private Vector2 mFirst;
        private Vector2 mLast;
        public Matrix2x2(Vector2 one, Vector2 two)
        {
            mFirst = one;
            mLast = two;
            Translate = Vector2.zero;
            Scale = Vector2.one;
        }

        public float Det()
        {
            return mFirst[0] * mLast[1] - mFirst[1] * mLast[0];
        }

        public Vector2 Scale { get; set; }
        public Vector2 Translate { get; set; }

        public Vector2 MultiplePoint(Vector2 point)
        {
            Vector2 temp = new Vector2(point.x, point.y);
            temp.Scale(Scale);
            temp[0] = Vector2.Dot(mFirst, temp);
            temp[1] = Vector2.Dot(mLast, temp);
            return temp + Translate;
        }
        public Vector2 Rotate(Vector2 point)
        {
            Vector2 temp = new Vector2(Vector2.Dot(mFirst, point), Vector2.Dot(mLast, point));
            return temp;
        }
        // just for rotation
        public Matrix2x2 Inverse()
        {
            Vector2 newOne = new Vector2(mLast[1], -mFirst[1]);
            Vector2 newTwo = new Vector2(-mLast[0], mFirst[0]);
            float rdet = 1.0f / Det();
            Matrix2x2 inv = new Matrix2x2(newOne * rdet, newTwo * rdet);
            return inv;
        }
    }
}
