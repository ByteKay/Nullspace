using System;
using UnityEngine;

namespace Nullspace
{
    public class GeoRect2
    {
        public Vector2 mP1;
        public Vector2 mP2;
        public Vector2 mP3;
        public Vector2 mP4;
        public Vector2 mDir1;
        public Vector2 mDir2;
        public Vector2 mCenter;
        public Vector2 mSize;
        public GeoRect2(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            mP1 = p1;
            mP2 = p2;
            mP3 = p3;
            mP4 = p4;
            mDir1 = mP2 - mP1;
            mDir2 = mP4 - mP1;
            mSize = new Vector2(mDir1.magnitude * 0.5f, mDir2.magnitude * 0.5f);
            mDir1.Normalize();
            mDir2.Normalize();
            mCenter = (p1 + p3) * 0.5f;
        }
        public static bool IsPointInRect(GeoRect2 rect, Vector2 p)
        {
            Vector2 pc = p - rect.mCenter;
            float pj = Vector2.Dot(rect.mDir1, pc);
            if (pj > rect.mSize[0])
                return false;
            pj = Vector2.Dot(rect.mDir2, pc);
            if (pj > rect.mSize[1])
                return false;
            return true;
        }

        public float IsRayIntersect(Vector2 pos, Vector2 dir)
        {
            float d = 1e10f;
            GeoInsectPointInfo info = new GeoInsectPointInfo();
            if (GeoRayUtils.IsRayInsectSegment2(pos, dir, mP1, mP2, ref info))
            {
                d = info.mLength;
            }
            info = new GeoInsectPointInfo();
            if (GeoRayUtils.IsRayInsectSegment2(pos, dir, mP3, mP2, ref info))
            {
                d = Math.Min(d, info.mLength);
            }
            info = new GeoInsectPointInfo();
            if (GeoRayUtils.IsRayInsectSegment2(pos, dir, mP1, mP4, ref info))
            {
                d = Math.Min(d, info.mLength);
            }
            info = new GeoInsectPointInfo();
            if (GeoRayUtils.IsRayInsectSegment2(pos, dir, mP3, mP4, ref info))
            {
                d = Math.Min(d, info.mLength);
            }
            return d;
        }

    }
    public class GeoRect3
    {
        public Vector3 mP1;
        public Vector3 mP2;
        public Vector3 mP3;
        public Vector3 mP4;
        public Vector3 mDir1;
        public Vector3 mDir2;
        public Vector3 mCenter;
        public Vector2 mSize;
        public GeoRect3(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            mP1 = p1;
            mP2 = p2;
            mP3 = p3;
            mP4 = p4;
            mDir1 = mP2 - mP1;
            mDir2 = mP4 - mP1;
            mSize = new Vector2(mDir1.magnitude, mDir2.magnitude);
            mDir1.Normalize();
            mDir2.Normalize();
            mCenter = (p1 + p3) * 0.5f;
        }

        public static bool IsPointInRect(GeoRect3 rect, Vector3 p)
        {
            Vector3 pc = p - rect.mP1;
            float pj = Vector3.Dot(rect.mDir1, pc);
            if (pj < 0 || pj > rect.mSize[0])
                return false;
            pj = Vector3.Dot(rect.mDir2, pc);
            if (pj < 0 || pj > rect.mSize[1])
                return false;
            return true;
        }
    }
}
