
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Nullspace
{
    public class GeoCircleUtils
    {
        public static bool IsInCircle(Vector2 center, float r, Vector2 p)
        {
            return (p - center).sqrMagnitude <= r * r;
        }

        public static bool IsInSphere(Vector3 center, float r, Vector3 p)
        {
            return (p - center).sqrMagnitude <= r * r;
        }

        public static bool IsCircleInsectCircle2(Vector2 center1, float r1, Vector2 center2, float r2, ref GeoInsectPointArrayInfo insect)
        {
            // MatrixUtils
            Vector2 cc1 = center2 - center1;
            float cc1d = cc1.magnitude;
            if (cc1d > (r1 + r2))
                return false;
            if (r1 > r2 && IsInCircle(center1, r1, center2)) // 内部
            {
                if (r1 > (r2 + cc1d))
                {
                    return false;
                }
            }
            if (r1 < r2 && IsInCircle(center2, r2, center1))
            {
                if (r2 > (r1 + cc1d))
                {
                    return false;
                }
            }
            Vector2 cc = center1 + cc1 * 0.5f;
            if (cc1d == (r1 + r2))
            {
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint.mPointArray.Add(new Vector3(cc.x, cc.y, 0.0f));
                return true;
            }      
            float dist = cc1.sqrMagnitude * 0.25f;
            cc1.Normalize();
            Vector2 v1, v2;
            MatrixUtils.CreateMatrix2D(90, out v1, out v2);
            float x = Vector2.Dot(v1, cc1);
            float y = Vector2.Dot(v2, cc1);
            Vector2 vertD = new Vector2(x, y);
            vertD.Normalize();
            float len = Mathf.Sqrt(r1 * r1 - dist);
            Vector2 p1 = cc + len * vertD;
            Vector2 p2 = cc - len * vertD;
            insect.mIsIntersect = true;
            insect.mHitGlobalPoint.mPointArray.Add(new Vector3(p1.x, p1.y, 0.0f));
            insect.mHitGlobalPoint.mPointArray.Add(new Vector3(p2.x, p2.y, 0.0f));
            return false;
        }
        // 3 维 共面
        public static bool IsCircleInsectCirclePlane2(Vector3 center1, float r1, Vector3 center2, float r2, GeoPlane plane, ref GeoInsectPointArrayInfo insect)
        {
            Vector3 c1 = plane.TransformToLocal(center1);
            Vector3 c2 = plane.TransformToLocal(center2);
            bool isInsect = IsCircleInsectCircle2(new Vector2(c1.x, c1.z), r1, new Vector2(c2.x, c2.z), r2, ref insect);
            if (isInsect)
            {
                List<Vector3> t3 = new List<Vector3>();
                foreach (Vector3 v in insect.mHitGlobalPoint.mPointArray)
                {
                    Vector3 tmp = new Vector3(v.x, 0.0f, v.y);
                    Vector3 glo = plane.TransformToGlobal(tmp);
                    t3.Add(glo);
                }
                insect.mIsIntersect = true;
                insect.mHitGlobalPoint.mPointArray = t3;
            }
            return insect.mIsIntersect;
        }

        public static bool IsCircleInsectCircle3(Vector3 center1, float r1, GeoPlane plane1, Vector3 center2, float r2, GeoPlane plane2, ref GeoInsectPointArrayInfo insect)
        {
            float dot = Mathf.Abs(Vector3.Dot(plane1.mNormal, plane2.mNormal));
            if (1 - dot < 1e-5f)
            {
                if (GeoPlaneUtils.IsPointOnPlane(plane2.mNormal, plane2.mD, center1)) // 共面
                {
                    return IsCircleInsectCirclePlane2(center1, r1, center2, r2, plane2, ref insect);
                }
                return false;
            }
            GeoInsectPointArrayInfo tmp = new GeoInsectPointArrayInfo();
            bool isInsect = GeoPlaneUtils.IsPlaneInsectCircle(plane1.mNormal, plane1.mD, center2, r2, plane2, ref tmp);
            if (isInsect)
            {
                Vector3 lin1 = tmp.mHitGlobalPoint.mPointArray[0];
                Vector3 lin2 = lin1 + tmp.mHitGlobalPoint.mPointArray[1];
                GeoLineUtils.IsLineInsectCirclePlane2(lin1, lin2, center1, r1, plane1, ref insect);
            }
            return insect.mIsIntersect;
        }

        public static bool IsSphereInsectSphere(Vector3 center1, float r1, Vector3 center2, float r2, ref GeoInsectPointArrayInfo insect)
        {
            Vector3 c1c2 = center2 - center1;
            float tmp = c1c2.magnitude;
            if (tmp > (r1 + r2))
            {
                return false;
            }
            if (r1 > r2 && IsInSphere(center1, r1, center2)) // 内部
            {
                if (r1 > (r2 + tmp))
                {
                    return false;
                }
            }
            if (r1 < r2 && IsInSphere(center2, r2, center1))
            {
                if (r2 > (r1 + tmp))
                {
                    return false;
                }
            }
            // to do
            Vector3 p = center1 + c1c2 * 0.5f;
            float r3 = Mathf.Sqrt(r1 * r1 - p.sqrMagnitude);
            insect.mHitGlobalPoint.mPointArray.Add(p);
            insect.mHitGlobalPoint.mPointArray.Add(new Vector3(r3, r3, r3));
            insect.mHitGlobalPoint.mPointArray.Add(c1c2.normalized); // 法向量
            insect.mIsIntersect = true;
            return true;
        }

    }
}
