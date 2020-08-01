
using System.Diagnostics;

namespace UnityEngine
{
    public struct Vector2
    {
        public static Vector2 zero = new Vector2(0, 0);
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public struct Vector3
    {
        public static Vector3 zero = new Vector3();
        public static Vector3 up = new Vector3(0, 1, 0);
        public float x;
        public float y;
        public float z;
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public struct Vector3Int
    {
        public static Vector3Int zero = new Vector3Int();
        public int x;
        public int y;
        public int z;
        public Vector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public struct Vector4
    {
        public static Vector4 zero = new Vector4();
        public float x;
        public float y;
        public float z;
        public float w;
        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }

    public struct Quaternion
    {
        public static Quaternion identity = new Quaternion(0, 0, 0, 1);
        public float x;
        public float y;
        public float z;
        public float w;
        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static Quaternion AngleAxis(float angle, Vector3 asix)
        {
            // not do it
            return Quaternion.identity;
        }
    }

    public struct Matrix4x4
    {
        public static Matrix4x4 zero = new Matrix4x4();
        public float m00;
        public float m01;
        public float m02;
        public float m03;
        public float m10;
        public float m11;
        public float m12;
        public float m13;
        public float m20;
        public float m21;
        public float m22;
        public float m23;
        public float m30;
        public float m31;
        public float m32;
        public float m33;

        public void SetColumn(int col, Vector4 v)
        {
            Debug.Assert(col < 4 && col > -1, "");
            switch (col)
            {
                case 0:
                    m00 = v.x;
                    m01 = v.y;
                    m02 = v.z;
                    m03 = v.w;
                    break;
                case 1:
                    m10 = v.x;
                    m11 = v.y;
                    m12 = v.z;
                    m13 = v.w;
                    break;
                case 2:
                    m20 = v.x;
                    m21 = v.y;
                    m22 = v.z;
                    m23 = v.w;
                    break;
                case 3:
                    m30 = v.x;
                    m31 = v.y;
                    m32 = v.z;
                    m33 = v.w;
                    break;
            }

        }

        public Vector4 GetColumn(int col)
        {
            Debug.Assert(col < 4 && col > -1, "");
            switch (col)
            {
                case 0:
                    return new Vector4(m00, m01, m02, m03);
                case 1:
                    return new Vector4(m10, m11, m12, m13);
                case 2:
                    return new Vector4(m20, m21, m22, m23);
                case 3:
                    return new Vector4(m30, m31, m32, m33);
            }
            return Vector4.zero;
        }
    }


    public struct Color
    {
        public static Color black = new Color(0, 0, 0, 1);
        public float r;
        public float g;
        public float b;
        public float a;
        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }

    public struct Color32
    {
        public static Color black = new Color(0, 0, 0, 255);

        public byte r;
        public byte g;
        public byte b;
        public byte a;
        public Color32(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }
}
