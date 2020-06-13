
using System.Collections.Generic;

using UnityEngine;

namespace Nullspace
{
    public class GeoAABBUtils
    {
        // aabb
        public static bool IsAABBInsectSegment2(Vector2 min, Vector2 max, Vector2 seg1, Vector2 seg2, ref GeoInsectPointArrayInfo insect)
        {
            return GeoSegmentUtils.IsSegmentInsectAABB2(seg1, seg2, min, max, ref insect);
        }

        public static bool IsAABBInsectSegment3(Vector3 min, Vector3 max, Vector3 seg1, Vector3 seg2, ref GeoInsectPointArrayInfo insect)
        {
            return GeoSegmentUtils.IsSegmentInsectAABB3(seg1, seg2, min, max, ref insect);
        }

        public static bool IsAABBInsectRay2(Vector2 min, Vector2 max, Vector2 rayOrigin, Vector2 rayDirection, ref GeoInsectPointArrayInfo insect)
        {
            return GeoRayUtils.IsRayInsectAABB2(rayOrigin, rayDirection, min, max, ref insect);
        }

        public static bool IsAABBInsectRay3(Vector3 min, Vector3 max, Vector3 rayOrigin, Vector3 rayDirection, ref GeoInsectPointArrayInfo insect)
        {
            return GeoRayUtils.IsRayInsectAABB3(rayOrigin, rayDirection, min, max, ref insect);
        }

        public static bool IsAABBInsectLine2(Vector2 min, Vector2 max, Vector2 line1, Vector2 line2, ref GeoInsectPointArrayInfo insect)
        {
            return GeoLineUtils.IsLineInsectAABB2(line1, line2, min, max, ref insect);
        }

        public static bool IsAABBInsectLine3(Vector3 min, Vector3 max, Vector3 line1, Vector3 line2, ref GeoInsectPointArrayInfo insect)
        {
            return GeoLineUtils.IsLineInsectAABB3(line1, line2, min, max, ref insect);
        }

        public static bool IsAABBInsectCircle2(Vector2 min, Vector2 max, Vector2 center, float r, ref GeoInsectPointArrayInfo insect)
        {
            /*
                rect : p(x, y) = p1 + u * d1 + v * d2 (0 < u < 1, 0 < v < 1)
             *  (p - center).magnitude = r
             */

            // float x = min[0] + u * d1[0];
            // float y = min[1] + v * d2[1];

            // (x - center[0]) * (x - center[0]) + (y - center[1]) * (y - center[1]) = r * r;
            List<Vector3> tmp = new List<Vector3>();
            // x = minx u = 0
            float d = r * r - (min[0] - center[0]) * (min[0] - center[0]);
            if (d >= 0)
            {
                d = Mathf.Sqrt(d);
                float y1 = center[1] + d;
                if (y1 >= min[1] && y1 <= max[1])
                {
                    tmp.Add(new Vector3(min[0], y1, 0.0f));
                }
                if (d > 0)
                {
                    float y2 = center[1] - d;
                    if (y2 >= min[1] && y2 <= max[1])
                    {
                        tmp.Add(new Vector3(min[0], y2, 0.0f));
                    }
                }
            }
            // x = maxx u = 1
            d = r * r - (max[0] - center[0]) * (max[0] - center[0]);
            if (d >= 0)
            {
                d = Mathf.Sqrt(d);
                float y1 = center[1] + d;
                if (y1 >= min[1] && y1 <= max[1])
                {
                    tmp.Add(new Vector3(max[0], y1, 0.0f));
                }
                if (d > 0)
                {
                    float y2 = center[1] - d;
                    if (y2 >= min[1] && y2 <= max[1])
                    {
                        tmp.Add(new Vector3(max[0], y2, 0.0f));
                    }
                }
            }
            // y = miny v = 0
            d = r * r - (min[1] - center[1]) * (min[1] - center[1]);
            if (d >= 0)
            {
                d = Mathf.Sqrt(d);
                float x1 = center[0] + d;
                if (x1 >= min[0] && x1 <= max[1])
                {
                    tmp.Add(new Vector3(x1, min[1], 0.0f));
                }
                if (d > 0)
                {
                    float x2 = center[0] - d;
                    if (x2 >= min[0] && x2 <= max[1])
                    {
                        tmp.Add(new Vector3(x2, min[1], 0.0f));
                    }
                }
            }
            // y = maxy v = 1
            d = r * r - (max[1] - center[1]) * (max[1] - center[1]);
            if (d >= 0)
            {
                d = Mathf.Sqrt(d);
                float x1 = center[0] + d;
                if (x1 >= min[0] && x1 <= max[1])
                {
                    tmp.Add(new Vector3(x1, min[1], 0.0f));
                }
                if (d > 0)
                {
                    float x2 = center[0] - d;
                    if (x2 >= min[0] && x2 <= max[1])
                    {
                        tmp.Add(new Vector3(x2, max[1], 0.0f));
                    }
                }
            }
            insect.mIsIntersect = tmp.Count > 0;
            if (insect.mIsIntersect)
            {
                insect.mHitGlobalPoint.mPointArray = tmp;
            }
            return insect.mIsIntersect;
        }

        public static bool IsAABBInsectCirclePlane2(Vector3 min, Vector3 max, Vector3 center, float r, GeoPlane plane, ref GeoInsectPointArrayInfo insect)
        {
            Vector3 min1 = plane.TransformToLocal(min);
            Vector3 max1 = plane.TransformToLocal(max);
            Vector3 c1 = plane.TransformToLocal(center);
            bool isInsect = IsAABBInsectCircle2(min1, max1, c1, r, ref insect);
            if (isInsect)
            {
                insect.mHitGlobalPoint.mPointArray = GeoPlaneUtils.PlaneTransformGlobal(insect.mHitGlobalPoint.mPointArray, plane);
            }
            return isInsect;
        }

        public static bool IsAABBInsectCircle3(Vector3 min, Vector3 max, GeoPlane plane1, Vector3 center, float r, GeoPlane plane2, ref GeoInsectPointArrayInfo insect)
        {
            float dot = Vector3.Dot(plane1.mNormal, plane2.mNormal);
            if (1 - Mathf.Abs(dot) < 1e-5f)
            {
                if (GeoPlaneUtils.IsPointOnPlane(plane2.mNormal, plane2.mD, min))
                {
                    return IsAABBInsectCirclePlane2(min, max, center, r, plane1, ref insect); // should use plane1
                }
                return false;
            }
            GeoInsectPointArrayInfo tmp = new GeoInsectPointArrayInfo();
            bool isInsect = GeoPlaneUtils.IsPlaneInsectCircle(plane1.mNormal, plane1.mD, center, r, plane2, ref tmp);
            if (isInsect)
            {
                if (tmp.mHitGlobalPoint.mPointArray.Count == 1)
                {
                    Vector3 line1 = tmp.mHitGlobalPoint.mPointArray[0];
                    if (GeoAABBUtils.IsPointInAABB2Plane(min, max, plane1, ref line1))
                    {
                        insect.mIsIntersect = true;
                        insect.mHitGlobalPoint.mPointArray.Add(line1);
                        return true;
                    }
                }
                else
                {
                    Vector3 seg1 = tmp.mHitGlobalPoint.mPointArray[0];
                    Vector3 seg2 = seg1 + tmp.mHitGlobalPoint.mPointArray[1];
                    return GeoSegmentUtils.IsSegmentInsectAABB2Plane(seg1, seg2, min, max, plane1, ref insect);
                }
            }
            return false;
        }

        public static bool IsAABBInsectTriangle2(Vector2 min, Vector2 max, Vector2 p1, Vector2 p2, Vector2 p3, ref GeoInsectPointArrayInfo insect)
        {
            // x = minx x = maxx
            // y = miny y = maxy
            List<Vector2> res = new List<Vector2>();
            Vector2 v = new Vector2();
            List<Vector2> tmp = new List<Vector2>();
            tmp.Add(p1);
            tmp.Add(p2);
            tmp.Add(p3);
            for (int i = 0; i < 3; ++i)
            {
                int j = (i + 1) % 3;
                if (GeoSegmentUtils.InterpolateX2(tmp[i], tmp[j], min[0], ref v))
                {
                    if (v.y <= max[1] && v.y >= min[1])
                    {
                        res.Add(v);
                    }
                }
                v = new Vector2();
                if (GeoSegmentUtils.InterpolateX2(tmp[i], tmp[j], max[0], ref v))
                {
                    if (v.y <= max[1] && v.y >= min[1])
                    {
                        res.Add(v);
                    }
                }
                v = new Vector2();
                if (GeoSegmentUtils.InterpolateY2(tmp[i], tmp[j], min[1], ref v))
                {
                    if (v.x <= max[0] && v.x >= min[0])
                    {
                        res.Add(v);
                    }
                }
                v = new Vector2();
                if (GeoSegmentUtils.InterpolateY2(tmp[i], tmp[j], max[1], ref v))
                {
                    if (v.x <= max[0] && v.x >= min[0])
                    {
                        res.Add(v);
                    }
                }
            }
            insect.mIsIntersect = res.Count > 0;
            if (insect.mIsIntersect)
            {
                foreach (Vector2 vt in res)
                {
                    insect.mHitGlobalPoint.mPointArray.Add(new Vector3(vt.x, vt.y, 0.0f));
                }
            }
            return insect.mIsIntersect;
        }

        public static bool IsAABBInsectTriangle2Plane(Vector3 min, Vector3 max, GeoPlane plane, Vector3 p1, Vector3 p2, Vector3 p3, ref GeoInsectPointArrayInfo insect)
        {
            Vector2 p11, p21, p31;
            GeoPlaneUtils.PlaneTransformLocalTriangle(p1, p2, p3, plane, out p11, out p21, out p31);
            Vector3 vmin = plane.TransformToLocal(min);
            Vector3 vmax = plane.TransformToLocal(max);
            bool isInsect = IsAABBInsectTriangle2(vmin, vmax, p11, p21, p31, ref insect);
            if (isInsect)
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint.mPointArray = GeoPlaneUtils.PlaneTransformGlobal(insect.mHitGlobalPoint.mPointArray, plane);
            }
            return insect.mIsIntersect;
        }

        public static bool IsAABBInsectTriangle3(Vector3 min, Vector3 max, Vector3 p1, Vector3 p2, Vector3 p3, ref GeoInsectPointArrayInfo insect)
        {
            // to do
            return false;
        }

        public static bool IsAABBInsectSphere(Vector3 min, Vector3 max, Vector3 center, float r, ref GeoInsectPointArrayInfo insect)
        {
            // to do
            return false;
        }
      
        public static bool IsPointInAABB2Plane(Vector3 min, Vector3 max, GeoPlane plane, ref Vector3 p)
        {
            if (!GeoPlaneUtils.IsPointOnPlane(plane.mNormal, plane.mD, p))
            {
                return false;
            }
            Vector3 size = max - min;
            float sx = Vector3.Dot(size, plane.mAxisX);
            float sz = Vector3.Dot(size, plane.mAxisZ);
            Vector3 pc = p - min;
            float d1 = Vector3.Dot(pc, plane.mAxisX);
            float d2 = Vector3.Dot(pc, plane.mAxisZ);
            if (d1 < sx && d2 < sz)
            {
                return true;
            }
            return false;
        }

        public static bool IsAABBInsectAABB2(Vector2 min1, Vector2 max1, Vector2 min2, Vector2 max2)
        {
            /*
             * //若相交，求得为一个重叠的矩形区域
               minx   =   max(minx1,   minx2)   
               miny   =   max(miny1,   miny2)   
               maxx   =   min(maxx1,   maxx2)   
               maxy   =   min(maxy1,   maxy2)   
               minx   >   maxx   或者     miny   >   maxy
             */
            Vector2 min = Vector2.Max(min1, min2);
            Vector2 max = Vector2.Min(max1, max2);
            if (min.x > max.x || min.y > max.y)
                return false;
            return true;
        }

        public static bool IsAABBInsectAABB2(Vector2 min1, Vector2 max1, Vector2 min2, Vector2 max2, ref GeoInsectPointArrayInfo insect)
        {
            /*
             * //若相交，求得为一个重叠的矩形区域
               minx   =   max(minx1,   minx2)   
               miny   =   max(miny1,   miny2)   
               maxx   =   min(maxx1,   maxx2)   
               maxy   =   min(maxy1,   maxy2)   
               minx   >   maxx   或者     miny   >   maxy
             */
            Vector2 min = Vector2.Max(min1, min2);
            Vector2 max = Vector2.Min(max1, max2);
            if (min.x > max.x || min.y > max.y)
                return false;
            insect.mIsIntersect = true;
            insect.mHitGlobalPoint.mPointArray.Add(new Vector3(min.x, min.y, 0.0f));
            insect.mHitGlobalPoint.mPointArray.Add(new Vector3(max.x, max.y, 0.0f));
            return true;
        }

        public static bool IsAABBInsectAABB3(Vector3 min1, Vector3 max1, Vector3 min2, Vector3 max2, ref GeoInsectPointArrayInfo insect)
        {
            Vector3 min = Vector3.Max(min1, min2);
            Vector3 max = Vector3.Min(max1, max2);
            if (min.x > max.x || min.y > max.y || min.z > max.z)
                return false;
            insect.mIsIntersect = true;
            insect.mHitGlobalPoint.mPointArray.Add(min);
            insect.mHitGlobalPoint.mPointArray.Add(max);
            return true;
        }

        public static bool IsPointInAABB2(Vector2 min, Vector2 max, ref Vector2 p)
        {
            return p.x > min.x && p.x < max.x && p.y > min.y && p.y < max.y;
        }

        public static bool IsPointInAABB3(Vector3 min, Vector3 max, ref Vector3 p)
        {
            return p.x >= min.x && p.x <= max.x && p.y >= min.y && p.y <= max.y && p.z >= min.z && p.z <= max.z;
        }

        public static float PointClosest2AABB2(Vector2 min, Vector2 max, ref Vector2 p, ref Vector2 close1)
        {
            if (IsPointInAABB2(min, max, ref p))
            {
                close1 = p;
                return 0.0f;
            }
            else
            {
                /*
                                 |                  |   
                               4 |        3         |  2
                             -------------------------------
                                 |                  |
                               5 |        0         |  1
                 *               |                  |
                             -------------------------------
                               6 |        7         |  8
                                 |                  |
         
                 */
                if (p.x < min[0] && p.y < min[1]) // 6
                {
                    close1 = min;
                    return (p - close1).magnitude;
                }
                else if (p.x < min[0] && p.y < max[1] && p.y > min[1]) // 5
                {
                    close1 = new Vector2(min[0], p.y);
                    return (min[0] - p.x);
                }
                else if (p.x < min[0] && p.y > max[1]) // 4
                {
                    close1 = new Vector2(min[0], max[1]);
                    return (p - close1).magnitude;
                }
                else if (p.x > max[0] && p.y > max[1]) // 2
                {
                    close1 = max;
                    return (p - max).magnitude;
                }
                else if (p.x > max[0] && p.y > min[1] && p.y < max[1]) // 1
                {
                    close1 = new Vector2(max[0], p.y);
                    return (p.x - max[0]);
                }
                else if (p.x > max[0] && p.y < min[1]) // 8
                {
                    close1 = new Vector2(max[0], min[1]);
                    return (p - close1).magnitude;
                }
                else if (p.x > min[0] && p.x < max[0] && p.y > max[1]) // 3
                {
                    close1 = new Vector2(p.x, max[1]);
                    return (p.y - max[1]);
                }
                else if (p.x > min[0] && p.x < max[0] && p.y < min[1]) // 7
                {
                    close1 = new Vector2(p.x, min[1]);
                    return (min[1] - p.y);
                }
            }
            return 0.0f;
        }

        public static float PointClosest2PlaneAABB2(Vector3 min, Vector3 max, GeoPlane plane, ref Vector3 p, ref Vector3 close1)
        {
            Vector3 min1 = plane.TransformToLocal(min);
            Vector3 max1 = plane.TransformToLocal(max);
            Vector3 p1 = plane.TransformToLocal(p);
            Vector2 min2 = new Vector2(min1.x, min1.z);
            Vector2 max2 = new Vector2(max1.x, max1.z);
            Vector2 p2 = new Vector2(p1.x, p1.z);
            Vector2 close = new Vector2();
            float temp = PointClosest2AABB2(min2, max2, ref p2, ref close);
            if (p1.y != 0)
            {
                temp = Mathf.Sqrt(temp * temp + p1.y * p1.y);
            }
            p1 = new Vector3(close.x, 0.0f, close.y);
            close1 = plane.TransformToGlobal(p1);
            return temp;
        }

        public static float PointClosest2AABB3(Vector3 min, Vector3 max, ref Vector3 p, ref Vector3 close1)
        {
            /*
         
             * 
             * 
             *         6   7
             *        
             *         5   4
                
             * 3   2  
                
             * 0   1 
             */
            if (IsPointInAABB3(min, max, ref p))
            {
                close1 = p;
                return 0.0f;
            }
            else
            {
                Vector3 v = max - min;
                Vector3 diff = p - min;
                float x = diff[0];
                float y = diff[1];
                float z = diff[2];

                Vector3 v1 = new Vector3(max[0], min[1], min[2]);
                Vector3 v2 = new Vector3(max[0], max[1], min[2]);
                Vector3 v3 = new Vector3(min[0], max[1], min[2]);
                Vector3 v4 = new Vector3(max[0], min[1], max[2]);
                Vector3 v5 = new Vector3(min[0], min[1], max[2]);
                Vector3 v6 = new Vector3(min[0], max[1], max[2]);
                Vector3 v7 = max;
                /*
                    diff[0]   [0, v[0]]
                 *  diff[1]   [0, v[1]]
                    diff[2]   [0, v[2]]
                 */
                if (x < 0)
                {
                    // (0,3,5,6)
                    if (y < 0)
                    {
                        if (z < 0)
                        {
                            close1 = min;
                        }
                        else if (z > v[1])
                        {
                            close1 = v5;
                        }
                        else // 0 < z < v[2]
                        {
                            close1 = new Vector3(max[0], min[1], diff[2]);
                        }
                    }
                    else if (y > v[1])
                    {
                        if (z < 0)
                        {
                            close1 = v3;
                        }
                        else if (z > v[1])
                        {
                            close1 = v6;
                        }
                        else // 0 < z < v[2]
                        {
                            close1 = new Vector3(min[0], max[1], diff[2]);
                        }
                    }
                    else // 0 < y < v[1]
                    {
                        
                        if (z < 0)
                        {
                            close1 = new Vector3(min[0], diff[1], min[2]);
                        }
                        else if (z > v[1])
                        {
                            close1 = new Vector3(min[0], diff[1], max[2]);
                        }
                        else // 0 < z < v[2]
                        {
                            close1 = new Vector3(min[0], diff[1], diff[2]);
                        }
                    }
                }
                else if (x > v[0])
                {
                    // {1,2,7,4}
                    if (y < 0)
                    {
                        // (1,4) down
                        if (z < 0)
                        {
                            close1 = v1;
                        }
                        else if (z > v[1])
                        {
                            close1 = v4;
                        }
                        else // 0 < z < v[2]
                        {
                            close1 = new Vector3(max[0], min[1], diff[2]);
                        }
                    }
                    else if (y > v[1])
                    {
                        // (2, 7)
                        if (z < 0)
                        {
                            close1 = v2;
                        }
                        else if (z > v[1])
                        {
                            close1 = v7;
                        }
                        else // 0 < z < v[2]
                        {
                            close1 = new Vector3(max[0], max[1], diff[2]);
                        }
                    }
                    else // 0 < y < v[1]
                    {
                        if (z < 0) // (1, 2)
                        {
                            close1 = new Vector3(max[0], diff[1], min[2]);
                        }
                        else if (z > v[1]) // (4, 7)
                        {
                            close1 = new Vector3(max[0], diff[1], max[2]);
                        }
                        else // 0 < z < v[2]
                        {
                            close1 = new Vector3(max[0], diff[1], diff[2]);
                        }
                    }
                }
                else // 0 < x < v[0]
                {
                    
                    if (y < 0) // (0, 1, 4, 5)
                    {
                        if (z < 0) 
                        {
                            close1 = new Vector3(diff[0], min[1], min[2]);
                        }
                        else if (z > v[1])
                        {
                            close1 = new Vector3(diff[0], min[1], max[2]);
                        }
                        else // 0 < z < v[2]
                        {
                            close1 = new Vector3(diff[0], min[1], diff[2]);
                        }
                    }
                    else if (y > v[1]) // (2,3,6,7)
                    {
                        if (z < 0)
                        {
                            close1 = new Vector3(diff[0], max[1], min[2]);
                        }
                        else if (z > v[1])
                        {
                            close1 = new Vector3(diff[0], max[1], max[2]);
                        }
                        else // 0 < z < v[2]
                        {
                            close1 = new Vector3(diff[0], max[1], diff[2]);
                        }
                    }
                    else // 0 < y < v[1]
                    {
                        if (z < 0)
                        {
                            close1 = new Vector3(diff[0], diff[1], min[2]);
                        }
                        else if (z > v[1])
                        {
                            close1 = new Vector3(diff[0], diff[1], max[2]);
                        }
                        else // 0 < z < v[2] // 内部 排除
                        {
                            
                        }
                    }
                }
                float dist = close1.magnitude;
                close1 = close1 + min;
                return dist;
            }
        }

        public static float PointDistance2AABB2(Vector2 min, Vector2 max, ref Vector2 p)
        {
            Vector2 close = new Vector2();
            return PointClosest2AABB2(min, max, ref p, ref close);
        }

        public static float PointDistance2AABB3(Vector3 min, Vector3 max, ref Vector3 p)
        {
            Vector3 close = new Vector3();
            return PointClosest2AABB3(min, max, ref p, ref close);
        }
    }
}
