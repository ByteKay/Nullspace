
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class GeoPolygonUtils
    {
        public static bool IsPointInPolygon2(GeoPointsArray2 poly, ref Vector2 point)
        {
            bool res = false;
            int j = poly.Count - 1;
            for (int i = 0; i < poly.Count; i++)
            {
                if ((((poly[i][1] <= point[1]) && (point[1] < poly[j][1])) || ((poly[j][1] <= point[1]) && (point[1] < poly[i][1])))
                    && (point[0] < (poly[j][0] - poly[i][0]) * (point[1] - poly[i][1]) / (poly[j][1] - poly[i][1]) + poly[i][0]))
                    res = !res;
                j = i;
            }
            return res;
        }
        // poly  [][2]
        public static bool IsPointInPolygon2(float[][] poly, int npoints, float xt, float yt)
        {
            bool res = false;
            int j = npoints - 1;
            for (int i = 0; i < npoints; i++)
            {
                if ((((poly[i][1] <= yt) && (yt < poly[j][1])) || ((poly[j][1] <= yt) && (yt < poly[i][1])))
                    && (xt < (poly[j][0] - poly[i][0]) * (yt - poly[i][1]) / (poly[j][1] - poly[i][1]) + poly[i][0]))
                    res = !res;
                j = i;
            }
            return res;
        }
        // poly  [][2]
        public static bool IsPointInPolygon2(int[][] poly, int npoints, int xt, int yt)
        {
            bool res = false;
            int j = npoints - 1;
            for (int i = 0; i < npoints; i++)
            {
                if ((((poly[i][1] <= yt) && (yt < poly[j][1])) || ((poly[j][1] <= yt) && (yt < poly[i][1])))
                    && (xt < (poly[j][0] - poly[i][0]) * (yt - poly[i][1]) / (poly[j][1] - poly[i][1]) + poly[i][0]))
                    res = !res;
                j = i;
            }
            return res;
        }

        // poly  [xp][yp]
        public static bool IsPointInPolygon2(float[] xp, float[] yp, int count, float x, float y)
        {
            bool res = false;
            int j = count - 1;
            for (int i = 0; i < count; i++)
            {
                if ((((yp[i] <= y) && (y < yp[j])) || ((yp[j] <= y) && (y < yp[i])))
                    && (x < (xp[j] - xp[i]) * (y - yp[i]) / (yp[j] - yp[i]) + xp[i]))
                    res = !res;
                j = i;
            }
            return res;
        }

        public static float CalcualetArea(List<Vector2> polyPoints)
        {
            float area = 0.0f;
            int count = polyPoints.Count;
            for (int i = 0; i < count; ++i)
            {
                int j = (i + 1) % count;
                area += (polyPoints[i][0] * polyPoints[j][1]) - (polyPoints[j][0] * polyPoints[i][1]);
            }
            area *= 0.5f;
            return area;
        }

        public static void ReverseIfCW(ref List<Vector2> polyPoints)
        {
            if (CalcualetArea(polyPoints) < 0)
            {
                polyPoints.Reverse();
            }
        }

        public static float CalcualetArea(GeoPointsArray2 poly)
        {
            float area = 0.0f;
            int count = poly.mPointArray.Count;
            for (int i = 0; i < count; ++i)
            {
                int j = (i + 1) % count;
                area += (poly[i][0] * poly[j][1]) - (poly[j][0] * poly[i][1]);
            }
            area *= 0.5f;
            return area;
        }

        // 判断 p3 是否在 p1 -> p2 的左边，即逆时针方向
        public static float CounterClockwiseGL0(Vector2 p1, Vector2 p2, Vector2 p)
        {
            return (p2[0] - p1[0]) * (p[1] - p1[1]) - (p[0] - p1[0]) * (p2[1] - p1[1]);
        }
        public static bool IsConvexAngle(Vector2 p1, Vector2 p2, Vector2 p)
        {
            return CounterClockwiseGL0(p1, p2, p) >= 0;
        }

        public static bool IsConvex(Vector2 p1, Vector2 p2, Vector2 p)
        {
            return CounterClockwiseGL0(p1, p2, p) >= 0;
        }

        public static bool IsConvexPolygon2(GeoPointsArray2 points)
        {
            int count = points.Count;
            for (int i = 0; i < points.Count; ++i)
            {
                int pre = (i - 1 + count) % count;
                int next = (i + 1) % count;
                if (!IsConvex(points[pre], points[i], points[next]))
                {
                    return false;
                }
            }
            return true;
        }

        public static GeoAABB2 CalculateAABB(GeoPointsArray2 points)
        {
            Vector2 min = points[0];
            Vector2 max = points[0];
            for (int i = 0; i < points.Count; ++i)
            {
                min = Vector2.Min(points[i], min);
                max = Vector2.Max(points[i], max);
            }
            return new GeoAABB2(min, max);
        }

        public static bool IsPointInConvexPolygon2(GeoPointsArray2 poly, ref Vector2 point)
        {
            int n = poly.mPointArray.Count;
            float t1 = CounterClockwiseGL0(poly.mPointArray[0], poly.mPointArray[1], point);
            float t2 = CounterClockwiseGL0(poly.mPointArray[0], poly.mPointArray[n - 1], point);
            if (t1 < -1e-5f || t2 > 1e-5f)
            {
                return false;
            }
            int low = 2;
            int high = n - 1;
            while (low < high)
            {
                int mid = (low + high) >> 1;
                if (CounterClockwiseGL0(poly.mPointArray[0], poly.mPointArray[mid], point) < -1e-5f)
                {
                    high = mid;
                }
                else
                {
                    low = mid + 1;
                }
            }
            float t3 = CounterClockwiseGL0(poly.mPointArray[low], poly.mPointArray[low - 1], point);
            if (t3 > 1e-5f)
            {
                return false;
            }
            return true;
        }

        public static PolygonDirection CalculatePolygonArea(GeoPointsArray2 poly, ref float area)
        {
            area = CalcualetArea(poly);
            if (area > 0)
                return PolygonDirection.CCW;
            else
            {
                return PolygonDirection.CW;
            }
        }

    }
}
