
using UnityEngine;

namespace Nullspace
{
    // 按照 左手坐标系来计算角度的方向, 顺时针方向为正
    public class MatrixUtils
    {
        public static float Vector2Angle(Vector2 v)
        {
            float angle = Vector2.Angle(v, Vector2.right);
            if (v.x > 0 && v.y > 0)
            {
                return 360 - angle;
            }
            else if (v.x > 0 && v.y < 0)
            {
                return angle;
            }
            else if (v.x < 0 && v.y > 0)
            {
                return 360 - angle;
            }
            else
            {
                return angle;
            }
        }

        public static float VectorAngleVector(Vector2 v1, Vector2 v2)
        {
            float angle = Vector2.Angle(v1, v2);
            if (GeoPolygonUtils.IsConvex(Vector2.zero, v1, v2)) // v2 在 v1 向量的左边
            {
                return angle;
            }
            else
            {
                return -angle;
            }
        }

        public static float VectorAngleVector1(Vector2 v1, Vector2 v2)
        {
            float angle1 = Vector2Angle(v1);
            float angle2 = Vector2Angle(v2);
            if (angle1 > angle2)
            {
                if (angle1 - angle2 < 180) // 逆时针，为负数
                {
                    return angle2 - angle1;
                }
                else
                {
                    return angle1 - angle2;
                }
            }
            else
            {
                if (angle2 - angle1 < 180) // 顺时针，为正数
                {
                    return angle2 - angle1;
                }
                else
                {
                    return angle1 - angle2;
                }
            }
        }

        public static Matrix2x2 CreateMatrix2D(Vector2 from, Vector2 to)
        {
            float degree = VectorAngleVector(from, to);
            Vector2 oneRow;
            Vector2 twoRow;
            CreateMatrix2D(degree, out oneRow, out twoRow);
            Matrix2x2 res = new Matrix2x2(oneRow, twoRow);
            return res;
        }

        public static Matrix2x2 CreateMatrix2D(Vector2 from, Vector2 to, Vector2 center)
        {
            float degree = VectorAngleVector(from, to);
            Vector2 oneRow;
            Vector2 twoRow;
            CreateMatrix2D(degree, out oneRow, out twoRow);
            Matrix2x2 res = new Matrix2x2(oneRow, twoRow);
            res.Translate = center;
            return res;
        }

        public static Matrix2x2 CreateMatrix2D(float degree, Vector2 center)
        {
            Vector2 oneRow;
            Vector2 twoRow;
            CreateMatrix2D(degree, out oneRow, out twoRow);
            Matrix2x2 res = new Matrix2x2(oneRow, twoRow);
            res.Translate = center;
            return res;
        }

        public static void CreateMatrix2D(float degree, out Vector2 oneRow, out Vector2 twoRow)
        {
            float rad = Mathf.Deg2Rad * degree;
            oneRow = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            twoRow = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));
        }

        public static void CreateMatrixX3D(float degree, out Matrix4x4 mat)
        {
            Vector2 oneRow; 
            Vector2 twoRow;
            CreateMatrix2D(degree, out oneRow, out twoRow);
            Vector4 one = new Vector4(1, 0, 0, 0.0f);
            Vector4 two = new Vector4(0, oneRow.x, oneRow.y, 0.0f);
            Vector4 three = new Vector4(0.0f, twoRow.x, twoRow.y, 0.0f);
            Vector4 four = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            mat = new Matrix4x4();
            mat.SetRow(0, one);
            mat.SetRow(1, two);
            mat.SetRow(2, three);
            mat.SetRow(3, four);
        }

        public static void CreateMatrixY3D(float degree, out Matrix4x4 mat)
        {
            Vector2 oneRow;
            Vector2 twoRow;
            CreateMatrix2D(degree, out oneRow, out twoRow);
            Vector4 one = new Vector4(oneRow.x, 0.0f, oneRow.y, 0.0f);
            Vector4 two = new Vector4(0, 1, 0, 0.0f);
            Vector4 three = new Vector4(twoRow.x, 0, twoRow.y, 0.0f);
            Vector4 four = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            mat = new Matrix4x4();
            mat.SetRow(0, one);
            mat.SetRow(1, two);
            mat.SetRow(2, three);
            mat.SetRow(3, four);
        }

        public static void CreateMatrixZ3D(float degree, out Matrix4x4 mat)
        {
            Vector2 oneRow;
            Vector2 twoRow;
            CreateMatrix2D(degree, out oneRow, out twoRow);
            Vector4 one = new Vector4(oneRow.x, oneRow.y, 0, 0.0f);
            Vector4 two = new Vector4(twoRow.x, twoRow.y, 0.0f, 0.0f);
            Vector4 three = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
            Vector4 four = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            mat = new Matrix4x4();
            mat.SetRow(0, one);
            mat.SetRow(1, two);
            mat.SetRow(2, three);
            mat.SetRow(3, four);
        }

        public static Matrix4x4 CreateMatrix3D(Vector3 from, Vector3 to)
        {
            Quaternion q = Quaternion.FromToRotation(from, to);
            Matrix4x4 mat = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
            return mat;
        }
    }
}
