
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class GeoPlaneUtils
    {
        public static GeoPlane CreateFromTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return new GeoPlane(p1, p2, p3);
        }

        public static GeoPlane CreateFromRectangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            Vector3 ax = (p2 - p1).normalized;
            Vector3 az = (p3 - p1).normalized;
            Vector3 ay = Vector3.Cross(ax, az);
            ay.Normalize();
            ay = -ay;
            return new GeoPlane(ax, ay, az, p1);
        }

        public static Vector3 CalculateNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 p12 = p2 - p1;
            Vector3 p13 = p3 - p1;
            return Vector3.Cross(p12, p13);
        }

        public static float PointDistanceToPlane(Vector3 normal, float d, Vector3 p)
        {
            // float dot = Vector3.Dot(plane.mNormal, point - plane.mPoint);
            // return dot; 
            // or
            float dot = Vector3.Dot(normal, p);
            return dot + d;
        }

        public static float PointClosestToPlaneAbs(Vector3 normal, float d, Vector3 p, ref Vector3 close)
        {
            float dot = PointDistanceToPlane(normal, d, p);
            close = p - dot * normal;
            return Mathf.Abs(dot);
        }

        public static float PointDistanceToPlaneAbs(Vector3 normal, float d, Vector3 p)
        {
            float dot = Vector3.Dot(normal, p);
            return Mathf.Abs(dot + d);
        }

        public static float PointDistanceToPlane(GeoPlane plane, Vector3 p)
        {
            return PointDistanceToPlane(plane.mNormal, plane.mD, p);
        }

        public static bool IsPointOnPlane(Vector3 normal, float d, Vector3 p)
        {
            float dist = PointDistanceToPlane(normal, d, p);
            return dist > -1e-5f && dist < 1e-5f;
        }

        public static bool IsPointInPositiveHalf(Vector3 normal, float d, Vector3 p)
        {
            float d1 = PointDistanceToPlane(normal, d, p);
            return d1 > 1e-5f;
        }

        public static bool IsPointInNegativeHalf(Vector3 normal, float d, Vector3 p)
        {
            float d1 = PointDistanceToPlane(normal, d, p);
            return d1 < -1e-5f;
        }

        public static bool IsPointInPositiveHalf(GeoPlane plane, Vector3 p)
        {
            float d = PointDistanceToPlane(plane, p);
            return d > 1e-5f;
        }

        public static Vector3 GetPointFromPlane(Vector3 normal, float d)
        {
            Vector3 point = new Vector3(0, 0, 0);
            int axis = 0;
            if (normal[0] != 0)
                axis = 0;
            if (normal[1] != 0)
                axis = 1;
            if (normal[2] != 0)
                axis = 2;
            point[axis] = d / normal[axis];
            return point;
        }

        public static bool IsPlaneInsectRay(Vector3 normal, float d, Vector3 origin, Vector3 direction, ref GeoInsectPointInfo insect)
        {
            float t = Vector3.Dot(normal, direction);
            if (t == 0) // 平行 或 在平面内
            {
                if (IsPointOnPlane(normal, d, origin))
                {
                    // 在平面内
                    return false;
                }
                else
                {
                    // 平行
                    return false;
                }

            }
            float up = -Vector3.Dot(normal, origin) + d;
            t = up / t;
            if (t > 0)
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint = origin + t * direction;
            }
            return insect.mIsIntersect;
        }

        public static bool IsPlaneInsectSegment(Vector3 normal, float d, Vector3 p1, Vector3 p2, ref GeoInsectPointInfo insect)
        {
            Vector3 direction = p2 - p1;
            float t = Vector3.Dot(normal, direction);
            if (t == 0) // 平行 或 在平面内
            {
                if (IsPointOnPlane(normal, d, p1))
                {
                    // 在平面内
                    return false;
                }
                else
                {
                    // 平行
                    return false;
                }
            }
            float up = -Vector3.Dot(normal, p1) + d;
            t = up / t;
            if (t > 0 && t < 1)
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint = p1 + t * direction;
            }
            return insect.mIsIntersect;
        }

        public static bool IsPlaneInsectLine(Vector3 normal, float d, Vector3 p1, Vector3 p2, ref GeoInsectPointInfo insect)
        {
            Vector3 direction = p2 - p1;
            direction.Normalize(); // not required
            float t = Vector3.Dot(normal, direction);
            if (t == 0) // 平行 或 在平面内
            {
                if (IsPointOnPlane(normal, d, p1))
                {
                    // 在平面内
                    return false;
                }
                else
                {
                    // 平行
                    return false; 
                } 
            }
            float up = -Vector3.Dot(normal, p1) + d;
            t = up / t;
            insect.mIsIntersect = true;
            insect.mHitGlobalPoint = p1 + t * direction;
            return true;
        }

        public static bool IsPlaneInsectPlane(Vector3 normal1, float d1, Vector3 normal2, float d2, ref GeoInsectPointArrayInfo insect)
        {
            // 410 所有的法向量必须先单位化
            Vector3 lineDirection = Vector3.Cross(normal1, normal2);
            if (lineDirection.magnitude < 1e-5f) // 平行
            {
                return false;
            }
            float n1n2 = Vector3.Dot(normal1, normal2);
            float m = n1n2 * n1n2 - 1;
            float a = (-d2 * n1n2 + d1) / m;
            float b = (-d1 * n1n2 + d2) / m;
            Vector3 p = a * normal1 + b * normal2;
            insect.mIsIntersect = true;
            insect.mHitGlobalPoint.mPointArray.Add(p); // 直线 上一点
            insect.mHitGlobalPoint.mPointArray.Add(lineDirection); // 直线 方向
            return true;
        }

        public static bool IsPlaneInsectCircle(Vector3 normal, float d, Vector3 center, float r, GeoPlane plane, ref GeoInsectPointArrayInfo insect)
        {
            float tmp = Mathf.Abs(Vector3.Dot(plane.mNormal.normalized, normal.normalized));
            if (1 - tmp < 1e-5f)
            {
                return false; // 平行 或 共面
            }
            GeoInsectPointArrayInfo tmp1 =  new GeoInsectPointArrayInfo();
            bool inSect = IsPlaneInsectPlane(normal,d, plane.mNormal, plane.mD, ref tmp1);
            if (inSect)
            {
                Vector3 line1 = tmp1.mHitGlobalPoint.mPointArray[0] + tmp1.mHitGlobalPoint.mPointArray[1];
                GeoLineUtils.IsLineInsectCirclePlane2(tmp1.mHitGlobalPoint.mPointArray[0], line1, center, r, plane, ref insect);
            }
            return insect.mIsIntersect;
        }

        public static bool IsPlaneInsectTriangle(Vector3 normal, float d, Vector3 p1, Vector3 p2, Vector3 p3, ref GeoInsectPointArrayInfo insect)
        {
            // 414
            List<Vector3> tri = new List<Vector3>();
            tri.Add(p1);
            tri.Add(p2);
            tri.Add(p3);
            List<Vector3> pPos = new List<Vector3>();
            List<Vector3> pOn = new List<Vector3>();
            foreach (Vector3 p in tri)
            {
                bool p1P = IsPointInPositiveHalf(normal, d, p);
                if (p1P)
                {
                    pPos.Add(p);
                }
                else
                {
                    p1P = IsPointOnPlane(normal, d, p);
                    if (p1P)
                    {
                        pOn.Add(p);
                    }
                }
            }
            if (pPos.Count == 3 && pOn.Count == 0) // 三点都在 一侧 
            {
                return false;
            }
            if (pPos.Count == 2 && pOn.Count == 1) // 一点在上，另两点同侧
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint.mPointArray.Add(pOn[0]);
                return true;
            }
            if (pPos.Count == 1 && pOn.Count == 2)
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint.mPointArray.Add(pOn[0]);
                insect.mHitGlobalPoint.mPointArray.Add(pOn[1]);
                return true;
            }
            if (pPos.Count == 0 && pOn.Count == 3) // 共面
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint.mPointArray.Add(pOn[0]);
                insect.mHitGlobalPoint.mPointArray.Add(pOn[1]);
                insect.mHitGlobalPoint.mPointArray.Add(pOn[2]);
                return true;
            }
            insect.mIsIntersect = true;
            GeoInsectPointInfo temp = new GeoInsectPointInfo();
            for (int i = 0; i < 3; ++i)
            {
                if (IsPlaneInsectSegment(normal, d, tri[i], tri[(i + 1) % 3], ref temp))
                {
                    insect.mHitGlobalPoint.mPointArray.Add(temp.mHitGlobalPoint);
                }
            }
            return true;
        }

        public static bool IsPlaneInsectAABB2(Vector3 normal, float d, Vector3 min, Vector3 max, GeoPlane plane, ref GeoInsectPointArrayInfo insect)
        {
            float dot = Vector3.Dot(normal, plane.mNormal);
            if (1 - Mathf.Abs(dot) < 1e-5f)
            {
                return false;
            }
            GeoInsectPointArrayInfo tmp = new GeoInsectPointArrayInfo();
            bool isInsect = GeoPlaneUtils.IsPlaneInsectPlane(normal, d, plane.mNormal, plane.mD, ref tmp);
            if (isInsect)
            {
                Vector3 l1 = tmp.mHitGlobalPoint.mPointArray[0];
                Vector3 l2 = l1 + tmp.mHitGlobalPoint.mPointArray[1];
                isInsect = GeoLineUtils.IsLineInsectAABBPlane2(l1, l2, min, max, plane, ref insect);
            }
            return insect.mIsIntersect;
        }

        public static bool IsPlaneInsectAABB3(Vector3 normal, float d, Vector3 min, Vector3 max, ref GeoInsectPointArrayInfo insect)
        {

            return false;
        }

        public static bool IsPlaneInsectSphere(GeoPlane plane, Vector3 center, float r, ref GeoInsectPointArrayInfo insect)
        {
            Vector3 close = new Vector3();
            float dist = PointClosestToPlaneAbs(plane.mNormal, plane.mD, center, ref close);
            if (dist > r)
            {
                return false;
            }
            float rad = Mathf.Sqrt(r * r - dist * dist);
            insect.mIsIntersect = true;
            insect.mHitGlobalPoint.mPointArray.Add(close);
            insect.mHitGlobalPoint.mPointArray.Add(Vector3.one * rad);  // 相交为一个圆，所在平明为 plane
            return true;
        }

        public static void PlaneTransformLocalTriangle(Vector3 p1, Vector3 p2, Vector3 p3, GeoPlane plane, out Vector2 p11, out Vector2 p21, out Vector2 p31)
        {
            Vector3 p1t = plane.TransformToLocal(p1);
            Vector3 p2t = plane.TransformToLocal(p2);
            Vector3 p3t = plane.TransformToLocal(p3);
            p11 = new Vector2(p1t.x, p1t.z);
            p21 = new Vector2(p2t.x, p2t.z);
            p31 = new Vector2(p3t.x, p3t.z);
        }
        public static void PlaneTransformGlobaleTriangle(Vector3 p1, Vector3 p2, Vector3 p3, GeoPlane plane, out Vector3 p11, out Vector3 p21, out Vector3 p31)
        {
            p11 = plane.TransformToGlobal(p1);
            p21 = plane.TransformToGlobal(p2);
            p31 = plane.TransformToGlobal(p3);
        }

        public static void PlaneTransformGlobaleTriangle(Vector3 p1, Vector3 p2, Vector3 p3, GeoPlane plane, out Vector2 p11, out Vector2 p21, out Vector2 p31)
        {
            Vector3 p1t = plane.TransformToGlobal(p1);
            Vector3 p2t = plane.TransformToGlobal(p2);
            Vector3 p3t = plane.TransformToGlobal(p3);
            p11 = new Vector2(p1t.x, p1t.z);
            p21 = new Vector2(p2t.x, p2t.z);
            p31 = new Vector2(p3t.x, p3t.z);
        }

        public static List<Vector3> PlaneTransformGlobal(List<Vector3> ps, GeoPlane plane)
        {
            List<Vector3> tmp = new List<Vector3>();
            foreach (Vector3 p in ps)
            {
                Vector3 v = new Vector3(p.x, 0.0f, p.y);
                tmp.Add(plane.TransformToGlobal(v));
            }
            return tmp;
        }

        public static List<Vector3> PlaneTransformGlobal(List<Vector2> ps, GeoPlane plane)
        {
            List<Vector3> tmp = new List<Vector3>();
            foreach (Vector3 p in ps)
            {
                Vector3 v = new Vector3(p.x, 0.0f, p.y);
                tmp.Add(plane.TransformToGlobal(v));
            }
            return tmp;
        }

        public static List<Vector2> PlaneTransformLocal(List<Vector3> ps, GeoPlane plane)
        {
            List<Vector2> tmp = new List<Vector2>();
            foreach (Vector3 p in ps)
            {
                Vector3 p1 = plane.TransformToLocal(p);
                tmp.Add(new Vector2(p.x, p.z));
            }
            return tmp;
        }
    }
}
