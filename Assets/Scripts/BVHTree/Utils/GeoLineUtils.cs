
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class GeoLineUtils
    {
        // 线
        public static bool IsLineInsectLine2(Vector2 line1, Vector2 line2, Vector2 line3, Vector2 line4, ref GeoInsectPointInfo insect)
        {
            Vector3 line11 = new Vector3(line1.x, line1.y, 0.0f);
            Vector3 line12 = new Vector3(line2.x, line2.y, 0.0f);
            Vector3 line21 = new Vector3(line3.x, line3.y, 0.0f);
            Vector3 line22 = new Vector3(line4.x, line4.y, 0.0f);
            return IsLineInsectLine3(line11, line12, line21, line22, ref insect);
        }

        public static bool IsLineInsectLine3(Vector3 line1, Vector3 line2, Vector3 line3, Vector3 line4, ref GeoInsectPointInfo insect)
        {
            Vector3 closest1, closest2;
            bool isInsect = GeoUtils.LineLineClosestPoint(line1, line2, line3, line4, out closest1, out closest2);
            if (isInsect)
            {
                Vector3 t = closest1 - closest2;
                if (t.magnitude < 1e-5f)
                {
                    insect.mHitGlobalPoint = closest1;
                    insect.mIsIntersect = true;
                    return true;
                }
            }
            insect.mIsIntersect = false;
            return false;
        }

        public static bool IsLineInsectSegment2(Vector2 line1, Vector2 line2, Vector2 seg1, Vector2 seg2, ref GeoInsectPointInfo insect)
        {
            Vector3 line11 = new Vector3(line1.x, line1.y, 0.0f);
            Vector3 line21 = new Vector3(line2.x, line2.y, 0.0f);
            Vector3 seg11 = new Vector3(seg1.x, seg1.y, 0.0f);
            Vector3 seg21 = new Vector3(seg2.x, seg2.y, 0.0f);
            return IsLineInsectSegment3(line11, line21, seg11, seg21, ref insect);
        }

        public static bool IsLineInsectSegment3(Vector3 line1, Vector3 line2, Vector3 seg1, Vector3 seg2, ref GeoInsectPointInfo insect)
        {
            Vector3 closest1, closest2;
            bool issect = GeoUtils.LineSegmentClosestPoint(line1, line2, seg1, seg2, out closest1, out closest2);
            if (issect)
            {
                Vector3 t = closest1 - closest2;
                if (t.magnitude < 1e-5f)
                {
                    insect.mHitGlobalPoint = closest1;
                    insect.mIsIntersect = true;
                    return true;
                }
            }
            insect.mIsIntersect = false;
            return false;
        }

        public static bool IsLineInsectRay2(Vector2 line1, Vector2 line2, Vector2 rayOrigin, Vector2 rayDirection, ref GeoInsectPointInfo insect)
        {
            Vector3 line11 = new Vector3(line1.x, line1.y, 0.0f);
            Vector3 line12 = new Vector3(line2.x, line2.y, 0.0f);
            Vector3 rayO = new Vector3(rayOrigin.x, rayOrigin.y, 0.0f);
            Vector3 rayD = new Vector3(rayDirection.x, rayDirection.y, 0.0f);
            return IsLineInsectRay3(line11, line12, rayO, rayD, ref insect);
        }

        public static bool IsLineInsectRay3(Vector3 line1, Vector3 line2, Vector3 rayOrigin, Vector3 rayDirection, ref GeoInsectPointInfo insect)
        {
            Vector3 close1, close2;
            bool issect = GeoUtils.RayLineClosestPoint(rayOrigin, rayDirection, line1, line2, out close1, out close2);
            if (issect)
            {
                Vector3 t = close1 - close2;
                if (t.magnitude < 1e-5f)
                {
                    insect.mHitGlobalPoint = close1;
                    insect.mIsIntersect = true;
                    return true;
                }
            }
            insect.mIsIntersect = false;
            return false;
        }

        public static bool IsLineInsectCircle2(Vector2 line1, Vector2 line2, Vector2 center, float r, ref GeoInsectPointArrayInfo insect)
        {
            // 389 公式存在问题， 按照代码来
            Vector2 direction = line2 - line1;
            float a = Vector2.Dot(direction, direction);
            Vector2 c1 = line1 - center;
            float b = 2 * Vector2.Dot(direction, c1);
            float c = Vector2.Dot(c1, c1) - r * r;
            float discrm = b * b - 4 * a * c;
            if (discrm < 0)
                return false;
            discrm = Mathf.Sqrt(discrm);
            float t1 = -b + discrm;
            float t2 = -b - discrm;
            insect.mIsIntersect = true;
            if (discrm == 0)
            {
                Vector2 temp = line1 + t1 * direction;
                insect.mHitGlobalPoint.mPointArray.Add(new Vector3(temp.x, temp.y, 0.0f));
            }
            else
            {
                Vector2 temp = line1 + t1 * direction;
                insect.mHitGlobalPoint.mPointArray.Add(new Vector3(temp.x, temp.y, 0.0f));
                temp = line1 + t2 * direction;
                insect.mHitGlobalPoint.mPointArray.Add(new Vector3(temp.x, temp.y, 0.0f));
            }
            return true;
        }

        public static bool IsLineInsectCirclePlane2(Vector3 line1, Vector3 line2, Vector3 center, float r, GeoPlane plane, ref GeoInsectPointArrayInfo insect)
        {
            Vector3 l1 = plane.TransformToLocal(line1);
            Vector3 l2 = plane.TransformToLocal(line2);
            Vector3 c1 = plane.TransformToLocal(center);
            bool isInsect = IsLineInsectCircle2(new Vector2(l1.x, l1.z), new Vector2(l2.x, l2.z), new Vector2(c1.x, c1.z), r, ref insect);
            if (isInsect)
            {
                insect.mHitGlobalPoint.mPointArray = GeoPlaneUtils.PlaneTransformGlobal(insect.mHitGlobalPoint.mPointArray, plane);
            }
            return insect.mIsIntersect;
        }

        public static bool IsLineInsectCircle3(Vector3 line1, Vector3 line2, Vector3 center, float r, GeoPlane plane, ref GeoInsectPointArrayInfo insect)
        {
            if (GeoPlaneUtils.IsPointOnPlane(plane.mNormal, plane.mD, line1) && GeoPlaneUtils.IsPointOnPlane(plane.mNormal, plane.mD, line2))
            {
                return IsLineInsectCirclePlane2(line1, line2, center, r, plane, ref insect);
            }
            GeoInsectPointInfo info = new GeoInsectPointInfo();
            bool isInsect = GeoPlaneUtils.IsPlaneInsectLine(plane.mNormal, plane.mD, line1, line2, ref info);
            if (isInsect)
            {
                if (GeoCircleUtils.IsInSphere(center, r, info.mHitGlobalPoint))
                {
                    insect.mIsIntersect = true;
                    insect.mHitGlobalPoint.mPointArray.Add(info.mHitGlobalPoint);
                    return true;
                }
                insect.mIsIntersect = false;
            }
            return false;
        }

        public static bool IsLineInsectTriangle2(Vector2 line1, Vector2 line2,  Vector2 p1, Vector2 p2, Vector2 p3, ref GeoInsectPointArrayInfo insect)
        {
            // 转化为 线 与 线段 相交
            List<Vector2> tri = new List<Vector2>();
            tri.Add(p1);
            tri.Add(p2);
            tri.Add(p3);
            List<Vector3> ins = new List<Vector3>();
            GeoInsectPointInfo info = new GeoInsectPointInfo();
            for (int i = 0; i < 3; ++i)
            {
                bool isSeg = IsLineInsectSegment2(line1, line2, tri[i], tri[(i + 1) % 3], ref info);
                if (isSeg)
                {
                    ins.Add(info.mHitGlobalPoint);
                }
            }
            if (ins.Count > 0)
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint.mPointArray = ins;
            }
            return insect.mIsIntersect;
        }

        public static bool IsLineInsectTriangle3(Vector3 line1, Vector3 line2, Vector3 p1, Vector3 p2, Vector3 p3, ref GeoInsectPointInfo insect)
        {
            // 376 计算几何
            Vector3 d = line2 - line1;
            Vector3 line1p1 = line1 - p1;
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
            float t = Vector3.Dot(Vector3.Cross(line1p1, v2v1), v3v1);
            float u = Vector3.Dot(Vector3.Cross(d, v3v1), line1p1);
            float v = Vector3.Dot(Vector3.Cross(line1p1, v2v1), d);
            if (u >= 0 && u <= 1 && v >= 0 && v <= 1 && (u + v) <= 1)
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint = line1p1 + t * d;
                return true;
            }
            return false;
        }

        /*
         *   矩形：R(t1, t2) = C + t1 * e1 + t2 * e2, -w < t1 < w, -h < t2 < h . C 为 矩形 中心点
         * 
            作者：张建龙
            链接：https://www.zhihu.com/question/31763307/answer/53259121
            来源：知乎
            轴对齐的矩形对线段的裁剪算法，大致有五种算法，可以解决轴对齐的矩形对线段的裁剪算法。
         * 最早由Danny Cohen&Ivan Sutherland提出的一种算法，称为Cohen-Sutherlad裁剪算法（简称CS算法），
         * 该算法把平面分成9个区域，根据点的位置确定与矩形的相交边。后来，Cyrus&Beck[1](1978)提出了Cyrus-Beck裁剪算法（简称CB算法），
         * 线段用参数方程来表示，计算与矩形的每条边所在的直线的交点，但是该算法的效率相对较低。
         * Liang&Barsky[2](1984)尝试对CS算法进行改进，提出了该算法的一种变种，称为Liang-Barsky裁剪算法（简称为LB算法），
         * Liang和Barsky[3](1992)进一步对LB算法进行了改进。Sobkow&Pospisil&Yang[4](1987)描述了另外一种线段的裁剪算法，称为快速裁剪算法（简称为SPY算法）。
         * Nicholl,Lee&Nicholl[5]对前面四种线段裁剪算法进行了较为深入地分析和比较，并且描述了一种更加高效的算法，称为Nicholl-Lee-Nicholl算法（简称为NLN算法）。
         * 对比五种算法的性能，它们并不是在数量级上的差别，只是算法中运算个数的差别，比如NLN算法拥有最少的除法次数。
         * CS、LB和SPY在不同机器、不同的数据集上，会显示出不同的算法性能。在算法实现上，NLN和SPY算法较为复杂，CS和LB相对简单。
         * 如果不会特别苛求算法的性能，建议选择CS或者LB算法
         * 
         * 
         * 很多实现并没有 垂直和水平的处理，因为默认相交的几何体都是在
         * 第一象限
         */
        public static bool IsLineInsectAABB2(Vector2 line1, Vector2 line2, Vector2 min, Vector2 max, ref GeoInsectPointArrayInfo insect)
        {
            // 转化为 线 与 线段 相交
            // 对角线 快速 判断
            Vector2 dir = line2 - line1;
            if (dir[0] == 0.0f) // 垂直
            {
                if (line1[0] >= min[0] && line1[0] <= max[0]) // 相交
                {
                    insect.mIsIntersect = true;
                    insect.mHitGlobalPoint.mPointArray.Add(new Vector3(line1[0], min[1], 0.0f));
                    insect.mHitGlobalPoint.mPointArray.Add(new Vector3(line1[0], max[1], 0.0f));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (dir[1] == 0.0f) // 平行
            {
                if (line1[1] >= min[1] && line1[1] <= max[1]) // 相交
                {
                    insect.mIsIntersect = true;
                    insect.mHitGlobalPoint.mPointArray.Add(new Vector3(min[0], line1[1], 0.0f));
                    insect.mHitGlobalPoint.mPointArray.Add(new Vector3(max[0], line1[1], 0.0f));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            float invx = 1.0f / dir[0];
            float t1;
            float t2;
            float dnear;
            float dfar;
            if (invx > 0)
            {
                t1 = (min[0] - line1[0]) * invx;
                t2 = (max[0] - line1[0]) * invx;
            }
            else
            {
                t2 = (min[0] - line1[0]) * invx;
                t1 = (max[0] - line1[0]) * invx;
            }
            dnear = t1;
            dfar = t2;
            if (dnear > dfar || dfar < 0.0f)
            {
                return false;
            }
            float invy = 1.0f / dir[1];
            if (invy > 0)
            {
                t1 = (min[1] - line1[1]) * invy;
                t2 = (max[1] - line1[1]) * invy;
            }
            else
            {
                t2 = (min[1] - line1[1]) * invy;
                t1 = (max[1] - line1[1]) * invy;
            }
            if (t1 > dnear)
            {
                dnear = t1;
            }
            if (t2 < dfar)
            {
                dfar = t2;
            }
            dnear = t1;
            dfar = t2;
            if (dnear > dfar || dfar < 0.0f)
            {
                return false;
            }
            insect.mIsIntersect = true;
            insect.mHitGlobalPoint.mPointArray.Add(line1 + dnear * dir);
            if (dnear != dfar)
            {
                insect.mHitGlobalPoint.mPointArray.Add(line1 + dfar * dir);
            }
            return true;
        }

        public static bool IsLineInsectAABBPlane2(Vector3 line1, Vector3 line2, Vector3 min, Vector3 max, GeoPlane plane, ref GeoInsectPointArrayInfo insect)
        {
            Vector3 min1 = plane.TransformToLocal(min);
            Vector3 max1 = plane.TransformToLocal(max);
            Vector3 l1 = plane.TransformToLocal(line1);
            Vector3 l2 = plane.TransformToLocal(line2);
            bool isInsect = IsLineInsectAABB2(new Vector2(l1.x, l1.z), new Vector2(l2.x, l2.z), new Vector2(min1.x, min1.z), new Vector2(max1.x, max1.z), ref insect);
            if (isInsect)
            {
                insect.mHitGlobalPoint.mPointArray = GeoPlaneUtils.PlaneTransformGlobal(insect.mHitGlobalPoint.mPointArray, plane);
            }
            return isInsect;
        }

        public static bool IsLineInsectAABB3(Vector3 line1, Vector3 line2, Vector3 min, Vector3 max, ref GeoInsectPointArrayInfo insect)
        {
            // 转化为 线 与 线段 相交
            // 对角线 快速 判断
            Vector3 dir = line2 - line1;
            if (dir[0] == 0.0f) // 平行 YOZ
            {
                if (line1[0] >= min[0] && line1[0] <= max[0]) // 相交 转换到 2d
                {
                    // (line1[0], min[1], min[2]) (line1[0], max[1], max[2]);
                    Vector2 line11 = new Vector2(line1[1], line1[2]);
                    Vector2 line21 = new Vector2(line2[1], line2[2]);
                    Vector2 min1 = new Vector2(min[1], min[2]);
                    Vector2 max1 = new Vector2(max[1], max[2]);
                    GeoInsectPointArrayInfo tmp = new GeoInsectPointArrayInfo();
                    bool isInsect = IsLineInsectAABB2(line11, line21, min1, max1, ref tmp);
                    if (isInsect)
                    {
                        insect.mIsIntersect = true;
                        foreach (Vector3 v in tmp.mHitGlobalPoint.mPointArray)
                        {
                            insect.mHitGlobalPoint.mPointArray.Add(new Vector3(line1[0], v[0], v[1]));
                        }
                    }
                    return isInsect;
                }
            }
            if (dir[1] == 0.0f) // 平行
            {
                if (line1[1] >= min[1] && line1[1] <= max[1]) // 相交 转换到 2d
                {
                    // (min[0], line1[1], min[2]) (max[0], line1[1], max[2]);
                    Vector2 line11 = new Vector2(line1[0], line1[2]);
                    Vector2 line21 = new Vector2(line2[0], line2[2]);
                    Vector2 min1 = new Vector2(min[0], min[2]);
                    Vector2 max1 = new Vector2(max[0], max[2]);
                    GeoInsectPointArrayInfo tmp = new GeoInsectPointArrayInfo();
                    bool isInsect = IsLineInsectAABB2(line11, line21, min1, max1, ref tmp);
                    if (isInsect)
                    {
                        insect.mIsIntersect = true;
                        foreach (Vector3 v in tmp.mHitGlobalPoint.mPointArray)
                        {
                            insect.mHitGlobalPoint.mPointArray.Add(new Vector3(v[0], line1[1], v[1]));
                        }
                    }
                    return isInsect;
                }
            }
            if (dir[2] == 0.0f) // 平行
            {
                if (line1[2] >= min[2] && line1[2] <= max[2]) // 相交 转换到 2d
                {
                    // (min[0], min[1], line1[2]) (max[0], max[1], line1[2]);
                    Vector2 line11 = new Vector2(line1[0], line1[1]);
                    Vector2 line21 = new Vector2(line2[0], line2[1]);
                    Vector2 min1 = new Vector2(min[0], min[1]);
                    Vector2 max1 = new Vector2(max[0], max[1]);
                    GeoInsectPointArrayInfo tmp = new GeoInsectPointArrayInfo();
                    bool isInsect = IsLineInsectAABB2(line11, line21, min1, max1, ref tmp);
                    if (isInsect)
                    {
                        insect.mIsIntersect = true;
                        foreach (Vector3 v in tmp.mHitGlobalPoint.mPointArray)
                        {
                            insect.mHitGlobalPoint.mPointArray.Add(new Vector3(v[0], v[1], line1[2]));
                        }
                    }
                    return isInsect;
                }
            }
            float invx = 1.0f / dir[0];
            float t1;
            float t2;
            float dnear;
            float dfar;
            if (invx > 0)
            {
                t1 = (min[0] - line1[0]) * invx;
                t2 = (max[0] - line1[0]) * invx;
            }
            else
            {
                t2 = (min[0] - line1[0]) * invx;
                t1 = (max[0] - line1[0]) * invx;
            }
            dnear = t1;
            dfar = t2;
            if (dnear > dfar || dfar < 0.0f)
            {
                return false;
            }
            float invy = 1.0f / dir[1];
            if (invy > 0)
            {
                t1 = (min[1] - line1[1]) * invy;
                t2 = (max[1] - line1[1]) * invy;
            }
            else
            {
                t2 = (min[1] - line1[1]) * invy;
                t1 = (max[1] - line1[1]) * invy;
            }
            if (t1 > dnear)
            {
                dnear = t1;
            }
            if (t2 < dfar)
            {
                dfar = t2;
            }
            dnear = t1;
            dfar = t2;
            if (dnear > dfar || dfar < 0.0f)
            {
                return false;
            }
            float invz = 1.0f / dir[2];
            if (invz > 0)
            {
                t1 = (min[2] - line1[2]) * invz;
                t2 = (max[2] - line1[2]) * invz;
            }
            else
            {
                t2 = (min[2] - line1[2]) * invz;
                t1 = (max[2] - line1[2]) * invz;
            }
            if (t1 > dnear)
            {
                dnear = t1;
            }
            if (t2 < dfar)
            {
                dfar = t2;
            }
            dnear = t1;
            dfar = t2;
            if (dnear > dfar || dfar < 0.0f)
            {
                return false;
            }
            insect.mIsIntersect = true;
            insect.mHitGlobalPoint.mPointArray.Add(line1 + dnear * dir);
            if (dnear != dfar)
            {
                insect.mHitGlobalPoint.mPointArray.Add(line1 + dfar * dir);
            }
            return true;
        }

        public static bool IsLineInsectSphere(Vector3 line1, Vector3 line2, Vector3 center, float r, ref GeoInsectPointArrayInfo insect)
        {
            // 389 公式存在问题， 按照代码来
            Vector3 direction = line2 - line1;
            float a = Vector3.Dot(direction, direction);
            Vector3 c1 = line1 - center;
            float b = 2 * Vector3.Dot(direction, c1);
            float c = Vector3.Dot(c1, c1) - r * r;
            float discrm = b * b - 4 * a * c;
            if (discrm < 0)
                return false;
            discrm = Mathf.Sqrt(discrm);
            float t1 = -b + discrm;
            float t2 = -b - discrm;
            insect.mIsIntersect = true;
            if (discrm == 0)
            {
                insect.mHitGlobalPoint.mPointArray.Add(line1 + t1 * direction);
            }
            else
            {
                insect.mHitGlobalPoint.mPointArray.Add(line1 + t1 * direction);
                insect.mHitGlobalPoint.mPointArray.Add(line1 + t2 * direction);
            }
            return true;
        }

        public static bool IsPointInLine2(Vector2 line1, Vector2 line2, ref Vector2 p)
        {
            return PointDistance2Line2(line1, line2, ref p) < 1e-5f;
        }

        public static bool IsPointInLine3(Vector3 line1, Vector3 line2, ref Vector3 p)
        {
            return PointDistance2Line3(line1, line2, ref p) < 1e-5f;
        }

        public static Vector2 PointClosest2Line2(Vector2 line1, Vector2 line2, ref Vector2 p)
        {
            Vector2 temp = line2 - line1;
            temp.Normalize();
            Vector2 p1 = p - line1;
            float dot = Vector2.Dot(temp, p1);
            return line1 + dot * temp;
        }

        public static Vector3 PointClosest2Line3(Vector3 line1, Vector3 line2, ref Vector3 p)
        {
            Vector3 temp = line2 - line1;
            temp.Normalize();
            Vector3 p1 = p - line1;
            float dot = Vector3.Dot(temp, p1);
            return line1 + dot * temp;
        }

        public static float PointDistance2Line2(Vector2 line1, Vector2 line2, ref Vector2 p)
        {
            Vector2 temp = PointClosest2Line2(line1, line2, ref p);
            return (p - temp).magnitude;
        }

        public static float PointDistance2Line3(Vector3 line1, Vector3 line2, ref Vector3 p)
        {
            Vector3 temp = PointClosest2Line3(line1, line2, ref p);
            return (p - temp).magnitude;
        }
    }
}
