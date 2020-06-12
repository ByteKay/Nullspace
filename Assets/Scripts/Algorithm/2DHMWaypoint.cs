
using System.Collections.Generic;

using UnityEngine;

namespace Nullspace
{
    public class HMWaypoint
    {
        private class HMConvex
        {
            private HashSet<KeyValuePair<int, int>> mBorders;
            public Vector2 mCenter;
            public HMConvex()
            {
                mBorders = new HashSet<KeyValuePair<int, int>>();
            }
            public HMConvex(List<int> convex, Vector2 center)
            {
                for (int i = 0; i < convex.Count; ++i)
                {
                    int j = i + 1;
                    if (j == convex.Count)
                    {
                        j = 0;
                    }
                    mBorders.Add(GetBorder(convex[i], convex[j]));
                }
                mCenter = center;
            }

            private KeyValuePair<int, int> GetBorder(int i, int j)
            {
                int ii = i > j ? j : i;
                int jj = i > j ? i : j;
                return new KeyValuePair<int, int>(ii, jj);
            }

            public bool Contain(KeyValuePair<int, int> border)
            {
                return mBorders.Contains(border);
            }
            
            public bool IsEdgeShared(HMConvex other, out int i, out int j)
            {
                foreach (KeyValuePair<int, int> bord in mBorders)
                {
                    if (other.Contain(bord))
                    {
                        i = bord.Key;
                        j = bord.Value;
                        return true;
                    }
                }
                i = -1;
                j = -1;
                return false;
            }
        }

        private class HMShared
        {
            public int mShareI;
            public int mShareJ;
            public int mI;
            public int mJ;
            public HMShared(int i, int j, int si, int sj)
            {
                mI = i;
                mJ = j;
                mShareI = si;
                mShareJ = sj;
            }
        }

        public static List<List<Vector2>> Waypoint(EarPolygon poly, out List<List<Vector2>> lines) // 简单多边形 点组
        {
            List<Vector2> vert = new List<Vector2>();
            List<List<int>> convexes = ConvexPolygonDecompose.Decompose(poly, ref vert);
            List<List<Vector2>> convexBorder;
            lines = Calculate(vert, convexes, out convexBorder);
            return convexBorder;
        }

        public static List<List<Vector2>> Waypoint(List<Vector2> triangles, bool isTriangle, out List<List<Vector2>> lines) // 简单多边形 点组
        {
            List<Vector2> vert = new List<Vector2>();
            List<List<int>> convexes = ConvexPolygonDecompose.Decompose(triangles, !isTriangle, ref vert);
            List<List<Vector2>> convexBorder;
            lines = Calculate(vert, convexes, out convexBorder);
            return convexBorder;
        }

        public static List<List<Vector2>> Waypoint(List<List<Vector2>> triangles, out List<List<Vector2>> lines) // 简单多边形 点组
        {
            List<Vector2> vert = new List<Vector2>();
            List<List<int>> convexes = ConvexPolygonDecompose.Decompose(triangles, ref vert);
            List<List<Vector2>> convexBorder;
            lines = Calculate(vert, convexes, out convexBorder);
            return convexBorder;
        }

        private static List<List<Vector2>> Calculate(List<Vector2> vert, List<List<int>> convexes, out List<List<Vector2>> convexBorders)
        {
            List<List<Vector2>> lines;
            convexBorders = new List<List<Vector2>>();

            List<HMConvex> hmConvex = new List<HMConvex>();
            foreach (List<int> convex in convexes)
            {
                Vector2 center = new Vector2(0, 0);
                List<Vector2> border = new List<Vector2>();
                foreach (int idx in convex)
                {
                    center += vert[idx];
                    border.Add(vert[idx]);
                }
                hmConvex.Add(new HMConvex(convex, center * 1.0f / convex.Count));
                convexBorders.Add(border);
            }
            HashSet<HMShared> tmp = new HashSet<HMShared>();
            int count1 = hmConvex.Count;
            int count2 = count1 - 1;
            for (int i = 0; i < count2; ++i)
            {
                for (int j = i + 1; j < count1; ++j)
                {
                    int sharei = -1;
                    int sharej = -1;
                    if (hmConvex[i].IsEdgeShared(hmConvex[j], out sharei, out sharej))
                    {
                        tmp.Add(new HMShared(i, j, sharei, sharej));
                    }
                }
            }
            lines = new List<List<Vector2>>();
            // 如果 边 很长， 需要 分段
            foreach (HMShared share in tmp)
            {
                List<Vector2> line = new List<Vector2>();
                Vector2 vsi = hmConvex[share.mI].mCenter;
                Vector2 vsj = hmConvex[share.mJ].mCenter;
                Vector2 v = (vert[share.mShareI] + vert[share.mShareJ]) * 0.5f;       
                line.Add(vsi);
                line.Add(v);
                lines.Add(line);
                line = new List<Vector2>();
                line.Add(v);
                line.Add(vsj);
                lines.Add(line);
            }
            return lines;
        }
    }
}
