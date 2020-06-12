
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class GeoRayUtils
    {
        // 射线
        public static bool IsRayInsectSegment2(Vector2 rayOrigin, Vector2 rayDirection, Vector2 seg1, Vector2 seg2, ref GeoInsectPointInfo insect)
        {
            return GeoSegmentUtils.IsSegmentInsectRay2(seg1, seg2, rayOrigin, rayDirection, ref insect);
        }

        public static bool IsRayInsectSegment3(Vector3 rayOrigin, Vector3 rayDirection, Vector3 seg1, Vector3 seg2, ref GeoInsectPointInfo insect)
        {
            return GeoSegmentUtils.IsSegmentInsectRay3(seg1, seg2, rayOrigin, rayDirection, ref insect);
        }

        public static bool IsRayInsectLine2(Vector2 rayOrigin, Vector2 rayDirection, Vector2 line1, Vector2 line2, ref GeoInsectPointInfo insect)
        {
            return GeoLineUtils.IsLineInsectRay2(line1, line2, rayOrigin, rayDirection, ref insect);
        }

        public static bool IsRayInsectLine3(Vector3 rayOrigin, Vector3 rayDirection, Vector3 line1, Vector3 line2, ref GeoInsectPointInfo insect)
        {
            return GeoLineUtils.IsLineInsectRay3(line1, line2, rayOrigin, rayDirection, ref insect);
        }

        public static bool IsRayInsectRay2(Vector2 rayOrigin1, Vector2 rayDirection1, Vector2 rayOrigin2, Vector2 rayDirection2, ref GeoInsectPointInfo insect)
        {
            Vector3 rayOrigin11 = GeoUtils.ToVector2(rayOrigin1);
            Vector3 rayDirection11 = GeoUtils.ToVector2(rayDirection1);
            Vector3 rayOrigin21 = GeoUtils.ToVector2(rayOrigin2);
            Vector3 rayDirection21 = GeoUtils.ToVector2(rayDirection2);
            return IsRayInsectRay3(rayOrigin11, rayDirection11, rayOrigin21, rayDirection21, ref insect);
        }

        public static bool IsRayInsectRay3(Vector3 rayOrigin1, Vector3 rayDirection1, Vector3 rayOrigin2, Vector3 rayDirection2, ref GeoInsectPointInfo insect)
        {
            Vector3 close1, close2;
            bool isinsect = GeoUtils.RayRayClosestPoint(rayOrigin1, rayDirection1, rayOrigin2, rayDirection2, out close1, out close2);
            if (isinsect)
            {
                Vector3 t = close1 - close2;
                if (t.magnitude < 1e-5f)
                {
                    insect.mIsIntersect = true;
                    insect.mHitGlobalPoint = close1;
                    return true;
                }
            }
            return false;
        }

        public static bool IsRayInsectCircle2(Vector2 rayOrigin, Vector2 rayDirection, Vector2 center, float r, ref GeoInsectPointArrayInfo insect)
        {
            // 389 公式存在问题， 按照代码来
            Vector2 direction = rayDirection;
            float a = Vector2.Dot(direction, direction);
            Vector2 c1 = rayOrigin - center;
            float b = 2 * Vector2.Dot(direction, c1);
            float c = Vector2.Dot(c1, c1) - r * r;
            float discrm = b * b - 4 * a * c;
            if (discrm < 0)
                return false;
            discrm = Mathf.Sqrt(discrm);
            float t1 = -b + discrm;
            float t2 = -b - discrm;
            if (t1 > 0 || t2 > 0)
            {
                insect.mIsIntersect = true;
                if (discrm == 0)
                {
                    insect.mHitGlobalPoint.mPointArray.Add(rayOrigin + t1 * direction);
                }
                else
                {
                    if (t1 > 0)
                        insect.mHitGlobalPoint.mPointArray.Add(rayOrigin + t1 * direction);
                    if (t2 > 0)
                        insect.mHitGlobalPoint.mPointArray.Add(rayOrigin + t2 * direction);
                }
            }
            return insect.mIsIntersect;
        }

        public static bool IsRayInsectCircle3(Vector3 rayOrigin, Vector3 rayDirection, Vector3 center, float r, GeoPlane plane, ref GeoInsectPointInfo insect)
        {
            if (GeoPlaneUtils.IsPlaneInsectRay(plane.mNormal, plane.mD, rayOrigin, rayDirection, ref insect))
            {
                if (GeoCircleUtils.IsInSphere(center, r, insect.mHitGlobalPoint))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsRayInsectTriangle2(Vector2 rayOrigin, Vector2 rayDirection, Vector2 p1, Vector2 p2, Vector2 p3, ref GeoInsectPointArrayInfo insect)
        {
            if (GeoLineUtils.IsLineInsectTriangle2(rayOrigin, rayOrigin + rayDirection, p1, p2, p3, ref insect))
            {
                List<Vector3> tmp = insect.mHitGlobalPoint.mPointArray;
                List<Vector3> tmp1 = new List<Vector3>();
                for (int i = 0; i < tmp.Count; ++i)
                {
                    Vector2 v = new Vector2(tmp[i].x, tmp[i].y);
                    if (IsPointInRay2(rayOrigin, rayDirection, ref v))
                    {
                        tmp1.Add(tmp[i]);
                    }
                }
                if (tmp1.Count > 0)
                {
                    insect.mHitGlobalPoint.mPointArray = tmp1;
                }
            }
            return insect.mIsIntersect;
        }

        public static bool IsRayInsectTriangle3(Vector3 rayOrigin, Vector3 rayDirection, Vector3 p1, Vector3 p2, Vector3 p3, ref GeoInsectPointInfo insect)
        {
            // 376 计算几何
            Vector3 d = rayDirection;
            Vector3 line1p1 = rayOrigin - p1;
            Vector3 v2v1 = p2 - p1;
            Vector3 v3v1 = p3 - p1;
            Vector3 normal = Vector3.Cross(v2v1, v3v1);
            float dot = Vector3.Dot(normal, d.normalized);
            if (dot < 1e-5f && dot > -1e-5f)
            {
                // 平行或 共面
                dot = Vector3.Dot(normal, line1p1);
                if (dot < 1e-5f && dot > -1e-5f)
                {
                    // 共面  此处应该转化为 线与线段 相交
                    // 暂时处理为 false
                    return false;
                }
                else
                {
                    // 平行
                    return false;
                }
            }

            Vector3 d31 = Vector3.Cross(d, v3v1);
            float d31v2v1 = Vector3.Dot(d31, v2v1);
            float tmp = 1.0f / d31v2v1;
            float t = Vector3.Dot(Vector3.Cross(line1p1, v2v1), v3v1) * tmp;
            float u = Vector3.Dot(Vector3.Cross(d, v3v1), line1p1) * tmp;
            float v = Vector3.Dot(Vector3.Cross(line1p1, v2v1), d) * tmp;
            if (u >= 0 && u <= 1 && v >= 0 && v <= 1 && (u + v) <= 1 && t > 0)
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint = rayOrigin + t * d;
                return true;
            }
            return false;
        }

        public static bool IsRayInsectAABB2(Vector2 rayOrigin, Vector2 rayDirection, Vector2 min, Vector2 max, ref GeoInsectPointArrayInfo insect)
        {
            bool isInsect = GeoLineUtils.IsLineInsectAABB2(rayOrigin, rayDirection + rayOrigin, min, max, ref insect);
            if (isInsect)
            {
                List<Vector3> tmp = new List<Vector3>();
                foreach (Vector3 v in insect.mHitGlobalPoint.mPointArray)
                {
                    Vector2 v2 = new Vector2(v.x, v.y);
                    if (GeoRayUtils.IsPointInRay2(rayOrigin, rayDirection, ref v2))
                    {
                        tmp.Add(v);
                    }
                }
                if (tmp.Count > 0)
                {
                    insect.mHitGlobalPoint.mPointArray = tmp;
                }
                else
                {
                    insect.mIsIntersect = false;
                }
            }
            return insect.mIsIntersect;
        }

        public static bool IsRayInsectAABB3(Vector3 rayOrigin, Vector3 rayDirection, Vector3 min, Vector3 max, ref GeoInsectPointArrayInfo insect)
        {
            bool isInsect = GeoLineUtils.IsLineInsectAABB3(rayOrigin, rayDirection + rayOrigin, min, max, ref insect);
            if (isInsect)
            {
                List<Vector3> tmp = new List<Vector3>();
                List<Vector3> tmp1 = insect.mHitGlobalPoint.mPointArray;
                for (int i = 0; i < tmp1.Count; ++i)
                {
                    Vector3 v = tmp1[i];
                    if (GeoRayUtils.IsPointInRay3(rayOrigin, rayDirection, ref v))
                    {
                        tmp.Add(v);
                    }
                }
                if (tmp.Count > 0)
                {
                    insect.mHitGlobalPoint.mPointArray = tmp;
                    insect.mHitGlobalPoint.mPointArray.Sort((v1, v2) =>
                    {
                        return (v1 - rayOrigin).sqrMagnitude.CompareTo((v2 - rayOrigin).sqrMagnitude);
                    });
                }
                else
                {
                    insect.mIsIntersect = false;
                }
            }
            return insect.mIsIntersect;
        }

        public static bool IsRayInsectSphere(Vector3 rayOrigin, Vector3 rayDirection, Vector3 center, float r, ref GeoInsectPointArrayInfo insect)
        {
            // 389 公式存在问题， 按照代码来
            Vector3 direction = rayDirection;
            float a = Vector3.Dot(direction, direction);
            Vector3 c1 = rayOrigin - center;
            float b = 2 * Vector3.Dot(direction, c1);
            float c = Vector3.Dot(c1, c1) - r * r;
            float discrm = b * b - 4 * a * c;
            if (discrm < 0)
                return false;
            discrm = Mathf.Sqrt(discrm);
            float t1 = -b + discrm;
            float t2 = -b - discrm;
            if (t1 > 0 || t2 > 0)
            {
                insect.mIsIntersect = true;
                if (discrm == 0)
                {
                    insect.mHitGlobalPoint.mPointArray.Add(rayOrigin + t1 * direction);
                }
                else
                {
                    if (t1 > 0)
                        insect.mHitGlobalPoint.mPointArray.Add(rayOrigin + t1 * direction);
                    if (t2 > 0)
                        insect.mHitGlobalPoint.mPointArray.Add(rayOrigin + t2 * direction);
                }
            }
            return insect.mIsIntersect;
        }

        public static bool IsPointInRay2(Vector2 rayOrigin, Vector2 rayDirection, ref Vector2 p)
        {
            return PointDistance2Ray2(rayOrigin, rayDirection, ref p) < 1e-5f;
        }
        public static bool IsPointInRay3(Vector3 rayOrigin, Vector3 rayDirection, ref Vector3 p)
        {
            return PointDistance2Ray3(rayOrigin, rayDirection, ref p) < 1e-5f;
        }

        public static Vector2 PointClosest2Ray2(Vector2 rayOrigin, Vector2 rayDirection, ref Vector2 p)
        {
            Vector2 pt = p - rayOrigin;
            float dot = Vector2.Dot(pt, rayDirection);
            if (dot < 0)
                return rayOrigin;
            return rayOrigin + dot * rayDirection;
        }

        public static Vector3 PointClosest2Ray3(Vector3 rayOrigin, Vector3 rayDirection, ref Vector3 p)
        {
            Vector3 pt = p - rayOrigin;
            float dot = Vector3.Dot(pt, rayDirection);
            if (dot < 0)
                return rayOrigin;
            return rayOrigin + dot * rayDirection;
        }

        public static float PointDistance2Ray2(Vector2 rayOrigin, Vector2 rayDirection, ref Vector2 p)
        {
            Vector2 pc = PointClosest2Ray2(rayOrigin, rayDirection, ref p);
            return (pc - p).magnitude;
        }

        public static float PointDistance2Ray3(Vector3 rayOrigin, Vector3 rayDirection, ref Vector3 p)
        {
            Vector3 pc = PointClosest2Ray3(rayOrigin, rayDirection, ref p);
            return (pc - p).magnitude;
        }
    }
}
