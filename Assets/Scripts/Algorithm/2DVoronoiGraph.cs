
using System.Collections.Generic;

using UnityEngine;

namespace Nullspace
{
    class XCompare : IComparer<Vector2>
    {
        public int Compare(Vector2 x, Vector2 y)
        {
            return x[0].CompareTo(y[0]);
        }
    }
    class YCompare : IComparer<Vector2>
    {
        public int Compare(Vector2 x, Vector2 y)
        {
            return x[1].CompareTo(y[1]);
        }
    }

    public class RegionPoints
    {
        private Vector2 mMin;
        private Vector2 mMax;
        private bool isMax;
        private List<Vector2> mPointsIn;
        public RegionPoints(Vector2 min, Vector2 max)
        {
            mMin = min;
            mMax = max;
            isMax = false;
            mPointsIn = new List<Vector2>();
        }

        public bool IsMax
        {
            get
            {
                return isMax;
            }
            set
            {
                isMax = value;
            }
        }

        public void Initialize(ref List<Vector2> datas)
        {
            mPointsIn = datas.FindAll((Vector2 v) =>
            {
                return v[1] >= mMin[1] && v[1] <= mMax[1]; // [a, b]
            });
            // 剔除
            datas.RemoveAll((Vector2 v) =>
            {
                return v[1] >= mMin[1] && v[1] <= mMax[1]; // [a, b]
            });
            XCompare com = new XCompare();
            mPointsIn.Sort(com);
        }

    }

    public class VoronoiGraph
    {
        public static void VoronoiGraphScatter(List<Vector2> datas)
        {
            // 计算凸包
            List<Vector2> jarvis = JarvisConvex.BuildHull(datas); // 逆时针
            // 剔除凸包上的点
            List<Vector2> leftPoints = Remove(datas, jarvis);
            // 寻找左半凸包
            List<Vector2> leftJarvis = LeftJarvis(jarvis);
            // 扫描线分区
            List<RegionPoints> regions = GroupRegions(ref leftPoints, leftJarvis);
            // 剩余的点 连成不相交的折线，合并到最后一个折线里去
            if (leftPoints.Count > 0)
            {

            }
            // 查找折线
            FindPolyline(jarvis, regions);
        }

        private static List<Vector2> LeftJarvis(List<Vector2> jarvis)
        {
            List<Vector2> result = new List<Vector2>();
            Vector2 maxY = new Vector2(1e10f,-1e10f);
            Vector2 minY = new Vector2(1e10f, 1e10f);

            foreach (Vector2 v in jarvis)
            {
                if (v[1] < minY[1])
                {
                    minY = v;
                }
                else if (v[1] == minY[1])
                {
                    if (v[0] < minY[0])
                    {
                        minY = v;
                    }
                }
                if (v[1] > maxY[1])
                {
                    maxY = v;
                }
                else if (v[1] == maxY[1])
                {
                    if (v[0] < maxY[0])
                    {
                        maxY = v;
                    }
                }
            }
            result.Sort(new YCompare());
            return result;
        }

        private static List<Vector2> Remove(List<Vector2> datas, List<Vector2> jarvis)
        {
            List<Vector2> tempData = new List<Vector2>();
            tempData.AddRange(datas);
            tempData.RemoveAll((Vector2 v) =>
            {
                int index = jarvis.FindIndex((Vector2 vv) =>
                {
                    return vv == v;
                });
                return index != -1;
            });
            return tempData;
        }

        private static void FindPolyline(List<Vector2> jarvis, List<RegionPoints> regions)
        {
            // 查找x值最小的点
            int index = FindMinXIndex(jarvis);
            int count = 0;
            FindPolyline(index, count, jarvis, regions);
        }

        private static void FindPolyline(int index, int count, List<Vector2> jarvis, List<RegionPoints> regions)
        {
            int greater = (index + count) % jarvis.Count;
            int less = (index + jarvis.Count - count) % jarvis.Count;
            for (int start = less; ; ++start)
            {
                int temp = start % jarvis.Count;
                if (temp > greater)
                {
                    break;
                }
            }
            // to do

        }

        private static int FindMinXIndex(List<Vector2> points)
        {
            int res = -1;
            float minx = 1e10f;
            for (int i = 0; i < points.Count; ++i)
            {
                if (points[i][0] < minx)
                {
                    minx = points[i][0];
                    res = i;
                }
            }
            return res;
        }

        private static List<RegionPoints> GroupRegions(ref List<Vector2> leftPoints, List<Vector2> leftJarvis)
        {
            List<RegionPoints> regionPoints = new List<RegionPoints>();
            for (int i = 1; i < leftJarvis.Count; ++i)
            {
                int j = i - 1;
                RegionPoints region = new RegionPoints(leftJarvis[j], leftJarvis[i]);
                region.Initialize(ref leftPoints);
                regionPoints.Add(region);
            }
            return regionPoints;
        }

        public static void VoronoiGraphPolygon(List<Vector2> datas)
        {

        }

        public static void VoronoiGraphSurface(List<Vector3> datas)
        {

        }

        public VoronoiGraph(List<Vector2> datas)
        {

        }

        public static implicit operator bool(VoronoiGraph a)
        {
            return a != null;
        }
    }
}
