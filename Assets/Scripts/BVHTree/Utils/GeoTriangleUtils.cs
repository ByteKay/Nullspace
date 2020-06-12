
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class GeoTriangleUtils
    {
        // 三角面
        public static bool IsTriangleInsectSegment2(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 seg1, Vector2 seg2, ref GeoInsectPointArrayInfo insect)
        {
            return GeoSegmentUtils.IsSegmentInsectTriangle2(seg1, seg2, p1, p2, p3, ref insect);
        }

        public static bool IsTriangleInsectSegment3(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 seg1, Vector3 seg2, ref GeoInsectPointArrayInfo insect)
        {
            return GeoSegmentUtils.IsSegmentInsectTriangle3(seg1, seg2, p1, p2, p3, ref insect);
        }

        public static bool IsTriangleInsectRay2(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 rayOrigin, Vector2 rayDirection, ref GeoInsectPointInfo insect)
        {
            return GeoRayUtils.IsRayInsectLine2(rayOrigin, rayDirection, rayOrigin, rayDirection, ref insect);
        }

        public static bool IsTriangleInsectRay3(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 rayOrigin, Vector3 rayDirection, ref GeoInsectPointInfo insect)
        {
            return GeoRayUtils.IsRayInsectTriangle3(rayOrigin, rayDirection, p1, p2, p3, ref insect);
        }

        public static bool IsTriangleInsectLine2(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 line1, Vector2 line2, ref GeoInsectPointArrayInfo insect)
        {
            return GeoLineUtils.IsLineInsectTriangle2(line1, line2, p1, p2, p3, ref insect);
        }

        public static bool IsTriangleInsectLine3(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 line1, Vector3 line2, ref GeoInsectPointInfo insect)
        {
            return GeoLineUtils.IsLineInsectTriangle3(line1, line2, p1, p2, p3, ref insect);
        }

        public static bool IsTriangleInsectAABB2(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 min, Vector2 max, ref GeoInsectPointArrayInfo insect)
        {
            return GeoAABBUtils.IsAABBInsectTriangle2(min, max, p1, p2, p3, ref insect);
        }

        public static bool IsTriangleInsectAABB3(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 min, Vector2 max, ref GeoInsectPointArrayInfo insect)
        {
            return GeoAABBUtils.IsAABBInsectTriangle3(min, max, p1, p2, p3, ref insect);
        }

        public static bool IsTriangleInsectCircle2(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 center, float r, ref GeoInsectPointArrayInfo insect)
        {
            GeoInsectPointArrayInfo tmp = new GeoInsectPointArrayInfo();
            if (GeoSegmentUtils.IsSegmentInsectCircle2(p1, p2, center, r, ref tmp))
            {
                insect.mHitGlobalPoint.mPointArray.AddRange(tmp.mHitGlobalPoint.mPointArray);
            }
            tmp.Clear();
            if (GeoSegmentUtils.IsSegmentInsectCircle2(p3, p2, center, r, ref tmp))
            {
                insect.mHitGlobalPoint.mPointArray.AddRange(tmp.mHitGlobalPoint.mPointArray);
            }
            tmp.Clear();
            if (GeoSegmentUtils.IsSegmentInsectCircle2(p1, p3, center, r, ref tmp))
            {
                insect.mHitGlobalPoint.mPointArray.AddRange(tmp.mHitGlobalPoint.mPointArray);
            }
            tmp.Clear();
            return false;
        }

        public static bool IsTriangleInsectPlaneCircle2(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 center, float r, GeoPlane plane, ref GeoInsectPointArrayInfo insect)
        {
            Vector3 p11 = plane.TransformToLocal(p1);
            Vector3 p21 = plane.TransformToLocal(p2);
            Vector3 p31 = plane.TransformToLocal(p3);
            Vector3 c1 = plane.TransformToLocal(center);
            Vector2 p12 = new Vector2(p11.x, p11.z);
            Vector2 p22 = new Vector2(p21.x, p21.z);
            Vector2 p32 = new Vector2(p31.x, p31.z);
            Vector2 c2 = new Vector2(c1.x, c1.z);
            GeoInsectPointArrayInfo temp = new GeoInsectPointArrayInfo();
            bool isInsect1 = IsTriangleInsectCircle2(p12, p22, p32, c2, r, ref temp);
            if (isInsect1)
            {
                insect.mIsIntersect = true;
                foreach (Vector3 v in temp.mHitGlobalPoint.mPointArray)
                {
                    Vector3 vv = new Vector3(v.x, 0.0f, v.y);
                    vv = plane.TransformToGlobal(vv);
                    insect.mHitGlobalPoint.mPointArray.Add(vv);
                }
                return true;
            }
            return false;
        }

        public static bool IsTriangleInsectCircle3(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 center, float r, GeoPlane plane, ref GeoInsectPointArrayInfo insect)
        {
            bool isInsect = GeoPlaneUtils.IsPlaneInsectTriangle(plane.mNormal, plane.mD, p1, p2, p3, ref insect);
            if (isInsect)
            {
                if (insect.mHitGlobalPoint.mPointArray.Count == 3) // 共面， 转化成 2d
                {
                    insect.Clear();
                    return IsTriangleInsectPlaneCircle2(p1, p2, p3, center, r, plane, ref insect);
                }
                else if (insect.mHitGlobalPoint.mPointArray.Count == 1)
                {
                    if (GeoCircleUtils.IsInSphere(center, r, insect.mHitGlobalPoint.mPointArray[0]))
                    {
                        return true;
                    }
                }
                else if (insect.mHitGlobalPoint.mPointArray.Count == 2)
                {           
                    Vector3 seg1 = plane.TransformToLocal(insect.mHitGlobalPoint.mPointArray[0]);
                    Vector3 seg2 = plane.TransformToLocal(insect.mHitGlobalPoint.mPointArray[1]);
                    Vector3 c1 = plane.TransformToLocal(center);
                    Vector2 p12 = new Vector2(seg1.x, seg1.z);
                    Vector2 p22 = new Vector2(seg2.x, seg2.z);
                    Vector2 c2 = new Vector2(c1.x, c1.z);
                    insect.Clear();
                    GeoInsectPointArrayInfo temp = new GeoInsectPointArrayInfo();
                    if (GeoSegmentUtils.IsSegmentInsectCircle2(p12, p22, c2, r, ref temp))
                    {
                        insect.mIsIntersect = true;
                        foreach (Vector3 v in temp.mHitGlobalPoint.mPointArray)
                        {
                            Vector3 vv = new Vector3(v.x, 0.0f, v.y);
                            vv = plane.TransformToGlobal(vv);
                            insect.mHitGlobalPoint.mPointArray.Add(vv);
                        }
                        return true;
                    }
                }

            }
            return false;
        }

        public static bool IsTriangleInsectTriangle2(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p5, Vector2 p6, ref GeoInsectPointArrayInfo insect)
        {
            GeoInsectPointArrayInfo tmp = new GeoInsectPointArrayInfo();
            List<Vector2> tri = new List<Vector2>();
            tri.Add(p1);
            tri.Add(p2);
            tri.Add(p3);
            bool inSect = false;
            for (int i = 0; i < 3; ++i)
            {
                int j = (i + 1) % 3;
                inSect = GeoSegmentUtils.IsSegmentInsectOrOverlapTriangle2(tri[i], tri[j], p4, p5, p6, ref tmp) ||  inSect;
            }
            if (inSect)
            {
                insect.mHitGlobalPoint.mPointArray.AddRange(tmp.mHitGlobalPoint.mPointArray);
            }
            insect.mIsIntersect = insect.mHitGlobalPoint.mPointArray.Count > 0;
            if (insect.mIsIntersect)
            {
                insect.UniquePoint();
            }
            return insect.mIsIntersect;
        }

        // 两三角面 共面的情况
        private static bool IsTriangleInsectTrianglePlane2(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector3 p5, Vector3 p6, ref GeoInsectPointArrayInfo insect)
        {
            GeoPlane plane = GeoPlaneUtils.CreateFromTriangle(p1, p2, p3);
            Vector2 p11, p21, p31, p41, p51, p61;
            GeoPlaneUtils.PlaneTransformLocalTriangle(p1, p2, p3, plane, out p11, out p21, out p31);
            GeoPlaneUtils.PlaneTransformLocalTriangle(p4, p5, p6, plane, out p41, out p51, out p61);
            GeoInsectPointArrayInfo tmp = new GeoInsectPointArrayInfo();
            bool isInsect = IsTriangleInsectTriangle2(p11, p21, p31, p41, p51, p61, ref tmp);
            if (isInsect)
            {
                foreach (Vector3 v in tmp.mHitGlobalPoint.mPointArray)
                {
                    Vector3 vt = new Vector3(v.x, 0.0f, v.z);
                    vt = plane.TransformToGlobal(vt);
                    insect.mHitGlobalPoint.mPointArray.Add(vt);
                }
                insect.mIsIntersect = insect.mHitGlobalPoint.mPointArray.Count > 0;
            }
            return insect.mIsIntersect;
        }

        public static bool IsTriangleInsectTriangle3(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector3 p5, Vector3 p6, ref GeoInsectPointArrayInfo insect)
        {
            /*
             * // 外面进行 剔除 处理
            Vector3 min1 = Vector3.Min(p1, p2);
            min1 = Vector3.Min(min1, p3);
            Vector3 max1 = Vector3.Max(p1, p2);
            max1 = Vector3.Max(max1, p3);
            Vector3 min2 = Vector3.Min(p4, p5);
            min2 = Vector3.Min(min2, p6);
            Vector3 max2 = Vector3.Max(p4, p5);
            max2 = Vector3.Max(max2, p6);
            if (!GeoAABBUtils.IsAABBInsectAABB3(min1, max1, min2, max2))
            {
                return false;
            }
             */
            // 417
            Vector3 p12 = p2 - p1;
            Vector3 p13 = p3 - p1;
            Vector3 normal1 = Vector3.Cross(p12, p13);
            Vector3 p45 = p5 - p4;
            Vector3 p46 = p6 - p4;
            Vector3 normal2 = Vector3.Cross(p45, p46);
            normal1.Normalize();
            normal2.Normalize();
            float dot = Vector3.Dot(normal1, normal2);
            if (1 - Mathf.Abs(dot) < 1e-5f) // 共面
            {
                return IsTriangleInsectTrianglePlane2(p1, p2, p3, p4, p5, p6, ref insect);
            }
            float d1 = -Vector3.Dot(normal1, p1);
            float d2 = -Vector3.Dot(normal2, p4);
            GeoInsectPointArrayInfo temp1 = new GeoInsectPointArrayInfo();
            if (GeoPlaneUtils.IsPlaneInsectTriangle(normal1, d1, p4, p5, p6, ref temp1))
            {
                GeoInsectPointArrayInfo temp2 = new GeoInsectPointArrayInfo();
                if (GeoPlaneUtils.IsPlaneInsectTriangle(normal2, d2, p1, p2, p3, ref temp2))
                {
                    int count1 = temp1.mHitGlobalPoint.mPointArray.Count;
                    int count2 = temp2.mHitGlobalPoint.mPointArray.Count;
                    if (count1 == 1 && count2 == 1)
                    {
                        bool isEq = temp1.mHitGlobalPoint.mPointArray[0] == temp2.mHitGlobalPoint.mPointArray[0];
                        if (isEq)
                        {
                            insect.mIsIntersect = true;
                            insect.mHitGlobalPoint.mPointArray.Add(temp1.mHitGlobalPoint.mPointArray[0]);
                            return true;
                        }
                    }
                    else if (count1 == 2 && count2 == 2)
                    {
                        Vector3 s1 = temp1.mHitGlobalPoint.mPointArray[0];
                        Vector3 s2 = temp1.mHitGlobalPoint.mPointArray[1];
                        Vector3 s3 = temp2.mHitGlobalPoint.mPointArray[0];
                        Vector3 s4 = temp2.mHitGlobalPoint.mPointArray[1];
                        GeoSegmentUtils.IsSegmentOverlapSegment3(s1, s2, s3, s4, ref insect);
                    }
                }
            }
            return insect.mIsIntersect;
        }

        public static bool IsTriangleInsectSphere(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 center, float r, ref GeoInsectPointArrayInfo insect)
        {
            GeoPlane plane = GeoPlaneUtils.CreateFromTriangle(p1, p2, p3);
            bool isInsec = GeoPlaneUtils.IsPlaneInsectSphere(plane, center, r, ref insect);
            if (isInsec)
            {
                Vector3 c1 = insect.mHitGlobalPoint.mPointArray[0];
                float rad = insect.mHitGlobalPoint.mPointArray[0][0];
                insect.Clear();
                return IsTriangleInsectPlaneCircle2(p1, p2, p3, c1, rad, plane, ref insect);
            }
            return false;
        }

        public static bool IsPointInTriangle2(Vector2 p1, Vector2 p2, Vector2 p3, ref Vector2 p)
        {
            // Compute vectors
            Vector2 v0 = p2 - p1;
            Vector2 v1 = p3 - p1;
            Vector2 v2 = p - p1;
            // Compute dot products
            float dot00 = Vector2.Dot(v0, v0);
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v0, v2);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v1, v2);
            // Compute barycentric coordinates
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            // Check if point is in triangle
            return (u >= 0.0f) && (v >= 0.0f) && (u + v <= 1.0f);
        }
        public static bool IsPointInTriangle3(Vector3 p1, Vector3 p2, Vector3 p3, ref Vector3 p)
        {
            // Compute vectors
            Vector3 v0 = p2 - p1;
            Vector3 v1 = p3 - p1;
            Vector3 v2 = p - p1;
            // Compute dot products
            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);
            // Compute barycentric coordinates
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            // Check if point is in triangle
            return (u >= 0.0f) && (v >= 0.0f) && (u + v <= 1.0f);
        }

        public static float PointClosest2Triangle2(Vector2 p1, Vector2 p2, Vector2 p3, ref Vector2 p, ref Vector2 close)
        {
            if (IsPointInTriangle2(p1, p2, p3, ref p))
            {
                // 内部不处理
                close = p;
                return 0.0f;
            }
            else
            {
                Vector2 tmp = new Vector2();
                float min = float.MaxValue;
                float d = GeoSegmentUtils.PointClosest2Segment2(p1, p2, ref p, ref tmp);
                if (d < min)
                {
                    min = d;
                    close = tmp;
                }
                d = GeoSegmentUtils.PointClosest2Segment2(p3, p2, ref p, ref tmp);
                if (d < min)
                {
                    min = d;
                    close = tmp;
                }
                d = GeoSegmentUtils.PointClosest2Segment2(p1, p3, ref p, ref tmp);
                if (d < min)
                {
                    min = d;
                    close = tmp;
                }
                return min;
            }
        }

        public static float PointClosest2PlaneTriangle2(Vector3 p1, Vector3 p2, Vector3 p3, GeoPlane plane, ref Vector3 p, ref Vector3 close)
        {
            Vector2 p11, p21, p31, pt;
            GeoPlaneUtils.PlaneTransformLocalTriangle(p1, p2, p3, plane, out p11, out p21, out p31);
            Vector3 pp = plane.TransformToLocal(p);
            pt = new Vector2(pp.x, pp.z);
            Vector2 close1 = new Vector2();
            float d = PointClosest2Triangle2(p11, p21, p31, ref pt, ref close1);
            close.x = close1.x;
            close.y = 0;
            close.z = close1.y;
            close = plane.TransformToGlobal(close);
            return d;
        }

        public static float PointClosest2Triangle3(Vector3 p1, Vector3 p2, Vector3 p3, ref Vector3 p, ref Vector3 close)
        {
            GeoPlane plane = new GeoPlane(p1, p2, p3);
            float d = GeoPlaneUtils.PointClosestToPlaneAbs(plane.mNormal, plane.mD, p, ref close);
            if (GeoTriangleUtils.IsPointInTriangle3(p1, p2, p3, ref close))
            {
                return d;
            }
            else 
            {
                Vector3 close1 = new Vector3();
                float dt = PointClosest2PlaneTriangle2(p1, p2, p3, plane, ref p, ref close1);
                return (float)Math.Sqrt(d * d + dt * dt);
            }
        }

        public static float PointDistance2Triangle2(Vector2 p1, Vector2 p2, Vector2 p3, ref Vector2 p)
        {
            Vector2 close = new Vector2();
            return PointClosest2Triangle2(p1, p2, p3, ref p, ref close);
        }

        public static float PointDistance2Triangle3(Vector3 p1, Vector3 p2, Vector3 p3, ref Vector3 p)
        {
            Vector3 close = new Vector3();
            return PointClosest2Triangle3(p1, p2, p3, ref p, ref close);
        }

        public static PolygonDirection TriangleArea2(Vector2 p1, Vector2 p2, Vector2 p3, ref float area)
        {
            GeoPointsArray2 array = new GeoPointsArray2();
            array.mPointArray.Add(p1);
            array.mPointArray.Add(p2);
            array.mPointArray.Add(p3);
            PolygonDirection dir = GeoPolygonUtils.CalculatePolygonArea(array, ref area);
            area = area < 0 ? -area : area;
            return dir;
        }

        public static void TriangleArea3(Vector3 p1, Vector3 p2, Vector3 p3, ref float area)
        {
            area = Vector3.Cross(p2 - p1, p3 - p1).magnitude * 0.5f;
        }

    }
}
