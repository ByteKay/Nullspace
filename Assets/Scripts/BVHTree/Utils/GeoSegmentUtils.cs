
using System.Collections.Generic;

using UnityEngine;

namespace Nullspace
{
    public class GeoSegmentUtils
    {

        // 线段 相交 和 重叠 分开 计算 （线段重叠 不认为 相交）
        public static bool IsSegmentInsectSegment2(Vector2 seg1, Vector2 seg2, Vector2 seg3, Vector2 seg4, ref GeoInsectPointInfo insect)
        {
            Vector3 seg11 = GeoUtils.ToVector2(seg1);
            Vector3 seg21 = GeoUtils.ToVector2(seg2);
            Vector3 seg31 = GeoUtils.ToVector2(seg3);
            Vector3 seg41 = GeoUtils.ToVector2(seg4);
            return IsSegmentInsectSegment3(seg11, seg21, seg31, seg41, ref insect);
        }

        public static bool IsSegmentInsectSegment3(Vector3 seg1, Vector3 seg2, Vector3 seg3, Vector3 seg4, ref GeoInsectPointInfo insect)
        {
            Vector3 close1, close2;
            bool isinsect = GeoUtils.SegmentSegmentClosestPoint(seg1, seg2, seg3, seg4, out close1, out close2);
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
            insect.mIsIntersect = false;
            return false;
        }

        public static bool IsSegmentInsectLine2(Vector2 seg1, Vector2 seg2, Vector2 line1, Vector2 line2, ref GeoInsectPointInfo insect)
        {
            return GeoLineUtils.IsLineInsectSegment2(line1, line2, seg1, seg2, ref insect);
        }

        public static bool IsSegmentInsectLine3(Vector3 seg1, Vector3 seg2, Vector3 line1, Vector3 line2, ref GeoInsectPointInfo insect)
        {
            return GeoLineUtils.IsLineInsectSegment3(line1, line2, seg1, seg2, ref insect);
        }

        public static bool IsSegmentInsectRay2(Vector2 seg1, Vector2 seg2, Vector2 rayOrigin, Vector2 rayDirection, ref GeoInsectPointInfo insect)
        {
            Vector3 seg11 = GeoUtils.ToVector2(seg1);
            Vector3 seg21 = GeoUtils.ToVector2(seg2);
            Vector3 rayOrigin1 = GeoUtils.ToVector2(rayOrigin);
            Vector3 rayDirection1 = GeoUtils.ToVector2(rayDirection);
            return IsSegmentInsectRay3(seg11, seg21, rayOrigin1, rayDirection1, ref insect);
        }

        public static bool IsSegmentInsectRay3(Vector3 seg1, Vector3 seg2, Vector3 rayOrigin, Vector3 rayDirection, ref GeoInsectPointInfo insect)
        {
            Vector3 close1, close2;
            bool isinsect = GeoUtils.RaySegmentClosestPoint(rayOrigin, rayDirection, seg1, seg2, out close1, out close2);
            if (isinsect)
            {
                Vector3 t = close1 - close2;
                if (t.magnitude < 1e-5f)
                {
                    insect.mIsIntersect = true;
                    insect.mHitGlobalPoint = close1;
                    insect.mLength = (rayOrigin - close1).magnitude;
                }
            }
            return insect.mIsIntersect;
        }

        public static bool IsSegmentInsectCircle2(Vector2 seg1, Vector2 seg2, Vector2 center, float r, ref GeoInsectPointArrayInfo insect)
        {
            if (GeoCircleUtils.IsInCircle(center, r, seg1) && GeoCircleUtils.IsInCircle(center, r, seg2)) // 都在外面，不一定
                return false;
            List<Vector3> array = insect.mHitGlobalPoint.mPointArray;
            List<int> removeIndex = new List<int>();
            for (int i = 0; i < array.Count; ++i)
            {
                Vector2 temp = new Vector2(array[i].x, array[i].y);
                if (!IsPointInSegment2(seg1, seg2, ref temp))
                {
                    removeIndex.Add(i);
                }
            }
            for (int i = removeIndex.Count - 1; i >= 0; --i)
            {
                array.RemoveAt(i);
            }
            insect.mHitGlobalPoint.mPointArray = array;
            insect.mIsIntersect = insect.mHitGlobalPoint.mPointArray.Count > 0;
            return insect.mIsIntersect;
        }

        public static bool IsSegmentInsectCircle3(Vector3 seg1, Vector3 seg2, Vector3 center, float r, GeoPlane plane, ref GeoInsectPointInfo insect)
        {
            bool isInsect = GeoPlaneUtils.IsPlaneInsectSegment(plane.mNormal, plane.mD, seg1, seg2, ref insect);
            if (isInsect)
            {
                if (GeoCircleUtils.IsInSphere(center, r, insect.mHitGlobalPoint))
                {
                    return true;
                }
            }
            insect.mIsIntersect = isInsect;
            return isInsect;
        }

        public static bool IsSegmentInsectAABB2(Vector2 seg1, Vector2 seg2, Vector2 min, Vector2 max, ref GeoInsectPointArrayInfo insect)
        {
            bool isInsect = GeoLineUtils.IsLineInsectAABB2(seg1, seg2, min, max, ref insect);
            if (isInsect)
            {
                List<Vector3> tmp = new List<Vector3>();
                foreach (Vector3 v in insect.mHitGlobalPoint.mPointArray)
                {
                    Vector2 v2 = new Vector2(v.x, v.y);
                    if (GeoSegmentUtils.IsPointInSegment2(seg1, seg2, ref v2))
                    {
                        tmp.Add(v);
                    }
                }
                insect.mHitGlobalPoint.mPointArray = tmp;
            }
            return insect.mIsIntersect;
        }

        public static bool IsSegmentInsectAABB2Plane(Vector3 seg1, Vector3 seg2, Vector3 min, Vector2 max, GeoPlane plane, ref GeoInsectPointArrayInfo insect)
        {
            Vector3 s1 = plane.TransformToLocal(seg1);
            Vector3 s2 = plane.TransformToLocal(seg2);
            Vector3 min1 = plane.TransformToLocal(min);
            Vector3 max1 = plane.TransformToLocal(max);
            bool isInsect = IsSegmentInsectAABB2(s1, s2, min1, max1, ref insect);
            if (isInsect)
            {
                insect.mHitGlobalPoint.mPointArray = GeoPlaneUtils.PlaneTransformGlobal(insect.mHitGlobalPoint.mPointArray, plane);
            }
            return isInsect;
        }

        public static bool IsSegmentInsectAABB3(Vector3 seg1, Vector3 seg2, Vector3 min, Vector2 max, ref GeoInsectPointArrayInfo insect)
        {
            bool isInsect = GeoLineUtils.IsLineInsectAABB3(seg1, seg2, min, max, ref insect);
            if (isInsect)
            {
                List<Vector3> tmp = new List<Vector3>();
                foreach (Vector3 v in insect.mHitGlobalPoint.mPointArray)
                {
                    Vector3 v2 = v;
                    if (GeoSegmentUtils.IsPointInSegment3(seg1, seg2, ref v2))
                    {
                        tmp.Add(v);
                    }
                }
                insect.mIsIntersect = false;
                if (tmp.Count > 0)
                {
                    insect.mIsIntersect = true;
                    insect.mHitGlobalPoint.mPointArray = tmp;
                }
            }
            return insect.mIsIntersect;
        }

        public static bool IsSegmentInsectTriangle2(Vector2 seg1, Vector2 seg2, Vector2 p1, Vector2 p2, Vector2 p3, ref GeoInsectPointArrayInfo insect)
        {
            GeoInsectPointInfo point = new GeoInsectPointInfo();
            List<Vector2> tri = new List<Vector2>();
            tri.Add(p1);
            tri.Add(p2);
            tri.Add(p3);
            List<Vector3> ret = new List<Vector3>();
            for (int i = 0; i < 3; ++i)
            {
                bool isS = IsSegmentInsectSegment2(seg1, seg2, p1, p2, ref point);
                if (isS)
                {
                    ret.Add(point.mHitGlobalPoint);
                }
            }
            if (ret.Count > 0)
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint.mPointArray = ret;
            }
            return insect.mIsIntersect;
        }

        public static bool IsSegmentInsectOrOverlapTriangle2(Vector2 seg1, Vector2 seg2, Vector2 p1, Vector2 p2, Vector2 p3, ref GeoInsectPointArrayInfo insect)
        {
            // 在三角形内部
            if (GeoTriangleUtils.IsPointInTriangle2(p1, p2, p3, ref seg1) && GeoTriangleUtils.IsPointInTriangle2(p1, p2, p3, ref seg2))
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint.mPointArray.Add(new Vector3(seg1.x, seg1.y, 0.0f));
                insect.mHitGlobalPoint.mPointArray.Add(new Vector3(seg2.x, seg2.y, 0.0f));
                return true;
            }
            // 相交测试(不包含 边的重叠)
            bool isInsect = GeoSegmentUtils.IsSegmentInsectTriangle2(seg1, seg2, p1, p2, p3, ref insect);
            if (!isInsect)
            {
                // 边重叠 测试
                isInsect = GeoSegmentUtils.IsSegmentOverlapTriangle2(seg1, seg2, p1, p2, p3, ref insect);
            }
            return insect.mIsIntersect;
        }

        // 共面的情况
        public static bool IsSegmentInsectTrianglePlane3(Vector3 seg1, Vector3 seg2, Vector3 p1, Vector3 p2, Vector3 p3, ref GeoInsectPointArrayInfo insect)
        {
            GeoInsectPointInfo point = new GeoInsectPointInfo();
            List<Vector3> tri = new List<Vector3>();
            tri.Add(p1);
            tri.Add(p2);
            tri.Add(p3);
            List<Vector3> ret = new List<Vector3>();
            for (int i = 0; i < 3; ++i)
            {
                bool isS = IsSegmentInsectSegment3(seg1, seg2, p1, p2, ref point);
                if (isS)
                {
                    ret.Add(point.mHitGlobalPoint);
                }
            }
            if (ret.Count > 0)
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint.mPointArray = ret;
            }
            return insect.mIsIntersect;
        }

        public static bool IsSegmentInsectTriangle3(Vector3 seg1, Vector3 seg2, Vector3 p1, Vector3 p2, Vector3 p3, ref GeoInsectPointArrayInfo insect)
        {
            // 376 计算几何
            Vector3 d = seg2 - seg1;
            Vector3 line1p1 = seg1 - p1;
            Vector3 v2v1 = p2 - p1;
            Vector3 v3v1 = p3 - p1;
            Vector3 normal = Vector3.Cross(v2v1, v3v1);
            float dot = Vector3.Dot(normal, d.normalized);
            if (dot < 1e-5f && dot > -1e-5f)
            {
                // 平行 或 共面
                dot = Vector3.Dot(normal, line1p1);
                if (dot < 1e-5f && dot > -1e-5f)
                {
                    // 共面  此处应该转化为 线与线段 相交
                    return IsSegmentInsectTrianglePlane3(seg1, seg2, p1, p2, p3, ref insect);
                }
                else
                {
                    // 平行
                    return false;
                }
            }

            Vector3 d31 = Vector3.Cross(d, v3v1);
            float d31v2v1 = Vector3.Dot(d31, v2v1);
            // float tmp = 1.0f / d31v2v1;
            float t = Vector3.Dot(Vector3.Cross(line1p1, v2v1), v3v1);
            float u = Vector3.Dot(Vector3.Cross(d, v3v1), line1p1);
            float v = Vector3.Dot(Vector3.Cross(line1p1, v2v1), d);
            if (u >= 0 && u <= 1 && v >= 0 && v <= 1 && (u + v) <= 1 && t >= 0 && t <= 1)
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint.mPointArray.Add(seg1 + t * d);
                return true;
            }
            return false;
        }

        public static bool IsSegmentInsectSphere(Vector3 seg1, Vector3 seg2, Vector3 center, float r, ref GeoInsectPointArrayInfo insect)
        {
            // 389 公式存在问题， 按照代码来
            Vector3 direction = seg2 - seg1;
            float a = Vector3.Dot(direction, direction);
            Vector3 c1 = seg1 - center;
            float b = 2 * Vector3.Dot(direction, c1);
            float c = Vector3.Dot(c1, c1) - r * r;
            float discrm = b * b - 4 * a * c;
            if (discrm < 0)
                return false;
            discrm = Mathf.Sqrt(discrm);
            float t1 = -b + discrm;
            float t2 = -b - discrm;
            if ((t1 > 0 && t1 * t1 < a) || (t2 > 0 && t2 * t2 < a))
            {
                insect.mIsIntersect = true;
                if (discrm == 0)
                {
                    insect.mHitGlobalPoint.mPointArray.Add(seg1 + t1 * direction);
                }
                else
                {
                    if (t1 > 0)
                        insect.mHitGlobalPoint.mPointArray.Add(seg1 + t1 * direction);
                    if (t2 > 0)
                        insect.mHitGlobalPoint.mPointArray.Add(seg1 + t2 * direction);
                }
            }
            return insect.mIsIntersect;
        }

        public static bool IsSegmentOverlapSegment2(Vector2 seg1, Vector2 seg2, Vector2 seg3, Vector2 seg4, ref GeoInsectPointArrayInfo insect)
        {
            // 4 点共线
            if (GeoLineUtils.IsPointInLine2(seg3, seg4, ref seg1) && GeoLineUtils.IsPointInLine2(seg3, seg4, ref seg2))
            {
                Vector2 seg12 = seg2 - seg1;
                Vector2 seg34 = seg4 - seg3;
                Vector2 seg13 = seg3 - seg1;
                Vector2 p1 = new Vector2();
                Vector2 p2 = new Vector2();
                bool isOver = false;
                if (seg34.magnitude < seg12.magnitude)
                {
                    isOver = IsSegmentOverlapSegment2(seg1, seg2, seg3, seg4, ref p1, ref p2);
                }
                else
                {
                    isOver = IsSegmentOverlapSegment2(seg3, seg4, seg1, seg2, ref p1, ref p2);
                }
                if (isOver)
                {
                    insect.mIsIntersect = true;
                    insect.mHitGlobalPoint.mPointArray.Add(p1);
                    insect.mHitGlobalPoint.mPointArray.Add(p2);
                }
                return insect.mIsIntersect;
            }
            return false;
        }

        public static bool IsSegmentOverlapSegment3(Vector3 seg1, Vector3 seg2, Vector3 seg3, Vector3 seg4, ref GeoInsectPointArrayInfo insect)
        {
            Vector3 seg12 = seg2 - seg1;
            Vector3 seg34 = seg4 - seg3;
            Vector3 seg13 = seg3 - seg1;

            float mag = Vector3.Cross(seg12, seg34).magnitude;
            if (mag > 1e-5f) // 不平行的 排除
            {
                return false;
            }
            mag = Vector3.Cross(seg13, seg34).magnitude;
            if (mag > 1e-5f) // 不共线的 排除
            {
                return false;
            }
            Vector3 p1 = new Vector3();
            Vector3 p2 = new Vector3();
            bool isOver = false;
            if (seg34.magnitude < seg12.magnitude)
            {
                isOver = IsSegmentOverlapSegment3(seg1, seg2, seg3, seg4, ref p1, ref p2);
            }
            else
            {
                isOver = IsSegmentOverlapSegment3(seg3, seg4, seg1, seg2, ref p1, ref p2);
            }
            if (isOver)
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint.mPointArray.Add(p1);
                insect.mHitGlobalPoint.mPointArray.Add(p2);
            }
            return insect.mIsIntersect;
        }

        // 共线 的 处理
        public static bool IsSegmentOverlapTriangle2(Vector2 seg1, Vector2 seg2, Vector2 p1, Vector2 p2, Vector2 p3, ref GeoInsectPointArrayInfo insect)
        {
            List<Vector3> tri = new List<Vector3>();
            for (int i = 0; i < 3; ++i)
            {
                IsSegmentOverlapSegment2(seg1, seg2, tri[i], tri[(i + 1) % 3], ref insect);
            }
            return insect.mIsIntersect;
        }

        public static bool IsSegmentOverlapTriangle3(Vector3 seg1, Vector3 seg2, Vector3 p1, Vector3 p2, Vector3 p3, ref GeoInsectPointArrayInfo insect)
        {
            List<Vector3> tri = new List<Vector3>();
            for (int i = 0; i < 3; ++i)
            {
                IsSegmentOverlapSegment3(seg1, seg2, tri[i], tri[(i + 1) % 3], ref insect);
            }
            return insect.mIsIntersect;
        }
        private static bool IsSegmentOverlapSegment2(Vector2 seg1, Vector2 seg2, Vector2 seg3, Vector2 seg4, ref Vector2 p1, ref Vector2 p2)
        {
            bool isOn3 = IsPointInSegment2(seg1, seg2, ref seg3);
            bool isOn4 = IsPointInSegment2(seg1, seg2, ref seg4);
            if (isOn3 && isOn4)
            {
                p1 = seg3;
                p2 = seg4;
                return true;
            }
            else if (isOn3 && !isOn4)
            {
                p1 = seg3;
                if ((seg4 - seg1).sqrMagnitude < (seg4 - seg2).sqrMagnitude)
                {
                    p2 = seg1;
                }
                else
                {
                    p2 = seg2;
                }
                return true;
            }
            else if (!isOn3 && isOn4)
            {
                p2 = seg4;
                if ((seg3 - seg1).sqrMagnitude < (seg3 - seg2).sqrMagnitude)
                {
                    p1 = seg1;
                }
                else
                {
                    p1 = seg2;
                }
                return true;
            }
            return false;
        }

        private static bool IsSegmentOverlapSegment3(Vector3 seg1, Vector3 seg2, Vector3 seg3, Vector3 seg4, ref Vector3 p1, ref Vector3 p2)
        {
            bool isOn3 = IsPointInSegment3(seg1, seg2, ref seg3);
            bool isOn4 = IsPointInSegment3(seg1, seg2, ref seg4);
            if (isOn3 && isOn4)
            {
                p1 = seg3;
                p2 = seg4;
                return true;
            }
            else if (isOn3 && !isOn4)
            {
                p1 = seg3;
                if ((seg4 - seg1).sqrMagnitude < (seg4 - seg2).sqrMagnitude)
                {
                    p2 = seg1;
                }
                else
                {
                    p2 = seg2;
                }
                return true;
            }
            else if (!isOn3 && isOn4)
            {
                p2 = seg4;
                if ((seg3 - seg1).sqrMagnitude < (seg3 - seg2).sqrMagnitude)
                {
                    p1 = seg1;
                }
                else
                {
                    p1 = seg2;
                }
                return true;
            }
            return false;
        }

        public static bool IsPointInSegment2(Vector2 seg1, Vector2 seg2, ref Vector2 p)
        {
            float p1 = PointDistance2Segment2(seg1, seg2, ref p);
            return p1 < 1e-5;
        }

        public static bool IsPointInSegment3(Vector3 seg1, Vector3 seg2, ref Vector3 p)
        {
            float p1 = PointDistance2Segment3(seg1, seg2, ref p);
            return p1 < 1e-5;
        }

        public static float PointClosest2Segment2(Vector2 seg1, Vector2 seg2, ref Vector2 p, ref Vector2 close)
        {
            Vector2 pt = p - seg1;
            Vector2 seg12 = seg2 - seg1;
            float len = seg12.magnitude;
            seg12.Normalize();
            float dot = Vector2.Dot(pt, seg12);
            if (dot < 0)
                dot = 0;
            if (dot > len)
                dot = 1;
            close = seg1 + dot * seg12;
            return (p - close).magnitude;
        }

        public static float PointClosest2Segment3(Vector3 seg1, Vector3 seg2, ref Vector3 p, ref Vector3 close)
        {
            Vector3 pt = p - seg1;
            Vector3 seg12 = seg2 - seg1;
            float len = seg12.magnitude;
            seg12.Normalize();
            float dot = Vector3.Dot(pt, seg12);
            if (dot < 0)
                dot = 0;
            if (dot > len)
                dot = 1;
            close = seg1 + dot * seg12;
            return (p - close).magnitude;
        }

        public static float PointDistance2Segment2(Vector2 seg1, Vector2 seg2, ref Vector2 p)
        {
            Vector2 pt = new Vector2();
            return PointClosest2Segment2(seg1, seg2, ref p, ref pt);
        }

        public static float PointDistance2Segment3(Vector3 seg1, Vector3 seg2, ref Vector3 p)
        {
            Vector3 pt = new Vector3();
            return PointClosest2Segment3(seg1, seg2, ref p, ref pt);
        }

        public static bool InterpolateX2(Vector2 seg1, Vector2 seg2, float x, ref Vector2 result)
        {
            Vector2 d = seg2 - seg1;
            if (d[0] == 0) // 垂直 x
                return false;
            float rd = 1.0f / d[0];
            float t = (x - seg1[0]) * rd;
            if (t >= 0 && t <= 1)
            {
                result = seg1 + t * d;
                return true;
            }
            return false;
        }
        public static bool InterpolateX3(Vector3 seg1, Vector3 seg2, float x, ref Vector2 result)
        {
            Vector3 d = seg2 - seg1;
            if (d[0] == 0) // 垂直 x
                return false;
            float rd = 1.0f / d[0];
            float t = (x - seg1[0]) * rd;
            if (t >= 0 && t <= 1)
            {
                result = seg1 + t * d;
                return true;
            }
            return false;
        }
        public static bool InterpolateY2(Vector2 seg1, Vector2 seg2, float y, ref Vector2 result)
        {
            Vector2 d = seg2 - seg1;
            if (d[0] == 0) // 垂直 x
                return false;
            float rd = 1.0f / d[1];
            float t = (y - seg1[1]) * rd;
            if (t >= 0 && t <= 1)
            {
                result = seg1 + t * d;
                return true;
            }
            return false;
        }
        public static bool InterpolateY3(Vector3 seg1, Vector3 seg2, float y, ref Vector2 result)
        {
            Vector3 d = seg2 - seg1;
            if (d[1] == 0) // 垂直 x
                return false;
            float rd = 1.0f / d[1];
            float t = (y - seg1[1]) * rd;
            if (t >= 0 && t <= 1)
            {
                result = seg1 + t * d;
                return true;
            }
            return false;
        }

        public static bool InterpolateZ3(Vector3 seg1, Vector3 seg2, float z, ref Vector2 result)
        {
            Vector3 d = seg2 - seg1;
            if (d[2] == 0) // 垂直 x
                return false;
            float rd = 1.0f / d[2];
            float t = (z - seg1[2]) * rd;
            if (t >= 0 && t <= 1)
            {
                result = seg1 + t * d;
                return true;
            }
            return false;
        }

    }
}
