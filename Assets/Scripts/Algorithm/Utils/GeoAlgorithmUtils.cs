
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Nullspace
{
    public class GeoAlgorithmUtils
    {
        public static bool IsOnSamePlane(GeoPointsArray3 points, ref GeoPlane plane)
        {
            if (points.Size() < 4)
            {
                return true;
            }
            int count = points.Size();
            int i = 2;
            Vector3 p = points[0];
            for (; i < count; ++i)
            {
                p = points[i];
                if (!GeoLineUtils.IsPointInLine3(points[0], points[1], ref p))
                {
                    break;
                }
            }
            if (i == count)
            {
                return true;
            }
            plane = GeoPlaneUtils.CreateFromTriangle(points[0], points[1], p);
            i = 2;
            int c = 2;
            for (; i < count; ++i)
            {
                if (GeoPlaneUtils.IsPointOnPlane(plane.mNormal, plane.mD, points[i]))
                {
                    c++;
                }
            }
            return c == count;
        }

        public static GeoPointsArray2 BuildConvexHull(GeoPointsArray2 points)
        {
            points.Distinct();
            return new GeoPointsArray2(JarvisConvex.BuildHull(points.mPointArray));
        }

        public static GeoPointsArray3 BuildConvexHull(GeoPointsArray3 points)
        {
            points.Distinct();
            GeoPlane plane = new GeoPlane(Vector3.zero, 0);
            if (IsOnSamePlane(points, ref plane))
            {
                GeoPointsArray2 point2 = new GeoPointsArray2(GeoPlaneUtils.PlaneTransformLocal(points.mPointArray, plane));
                point2 = BuildConvexHull(point2);
                return new GeoPointsArray3(GeoPlaneUtils.PlaneTransformGlobal(point2.mPointArray, plane));
            }
            else
            {
                return new GeoPointsArray3(QuickHull.BuildHull(points));
            }
        }


        public static void FlatList<T>(List<List<T>> vertes, ref List<T> points)
        {
            points.Clear();
            foreach (List<T> lst in vertes)
            {
                points.AddRange(lst);
            }
        }

        // 邻接表 遍历
        // 当层次很多时，递归较慢，可能内存溢出
        public static void AdjacentGraphTravel<T>(List<List<T>> graph, List<List<T>> groups, List<T> group, int index = 0) where T : IAdjacent
        {
            List<T> temp = graph[index];
            for (int i = 0; i < temp.Count; ++i)
            {
                T adj = temp[i];
                group.Add(adj);
                int idx = adj.Next();
                if (idx < graph.Count)
                {
                    AdjacentGraphTravel(graph, groups, group, idx);
                }
                else
                {
                    List<T> final = new List<T>();
                    final.AddRange(group);
                    groups.Add(final);
                }
                if (group.Count > 0)
                {
                    group.RemoveAt(group.Count - 1);
                }
            }
        }

        public static List<List<int>> CheckCircle(List<int> indexList)
        {
            List<List<int>> results = new List<List<int>>();
            List<int> unique = indexList.Distinct().ToList();
            foreach (int v in unique)
            {
                List<int> finda = indexList.FindAll((int value) => { return value == v; });
                finda.Sort();
                //int max = finda.Max();
                //int min = finda.Min();
                //finda.Clear();
                //finda.Add(min);
                //if (min != max)
                //{
                //    finda.Add(max);
                //}
                results.Add(finda);
            }
            return results;
        }

        public static int BinarySearch(List<float> list, float value)
        {
            int count = list.Count;
            if (count > 0)
            {
                int low = 0;
                int high = list.Count - 1;
                while (low < high)
                {
                    int mid = (low + high) >> 1;
                    if (list[mid] > value)
                    {
                        high = mid;
                    }
                    else if (list[mid] < value)
                    {
                        low = mid + 1;
                    }
                    else
                    {
                        return mid;
                    }
                }
                return low;
            }
            return -1;
        }
        public static void TestMinSquare()
        {
            List<Vector2> testMinSquare = new List<Vector2>();
            float k = 1.0f;
            float b = 0.01f;
            float startX = 0;
            for (int i = 0; i < 10; ++i)
            {
                float y = k * startX + b;
                float jitter = RandomUtils.GetRandomFloat(-0.001f, 0.001f);
                testMinSquare.Add(new Vector2(startX, y + jitter));
                startX += 0.5f;
            }
            testMinSquare = MinLeastSquare(testMinSquare);
        }

        public static List<Vector2> MinLeastSquare(List<Vector2> polygonPoints, float threshold = 0.1f)
        {
            GeoPolygonUtils.ReverseIfCW(ref polygonPoints);
            List<Vector2> temp = new List<Vector2>();
            List<Vector2> result = new List<Vector2>();
            temp.AddRange(polygonPoints);
            int count = polygonPoints.Count;
            for (int i = 0; i < temp.Count;)
            {
                result.Add(temp[i]);
                int n = i + 2;
                while (n < temp.Count && CalculateLeastSquare(i, n, temp, threshold))
                {
                    ++n;
                }
                i = n - 1;
            }
            if (result.Count != count)
            {
                result = MinLeastSquare(result);
            }
            return result;
        }
        // 后期可以替换这个做法，直接使用点到线段的距离，最大的距离值设置一个 阈值 处理
        private static bool CalculateLeastSquare(int start, int end, List<Vector2> polygonPoints, float threshold)
        {
            float a00 = 0.0f;
            float a01 = 0.0f;
            float a10 = 0.0f;
            float a11 = 0.0f;
            float b0 = 0.0f;
            float b1 = 0.0f;
            for (int i = start; i <= end; ++i)
            {
                a00 += polygonPoints[i].x * polygonPoints[i].x;
                a01 += polygonPoints[i].x;
                a10 += polygonPoints[i].x;
                a11 += 1;
                b0 += polygonPoints[i].x * polygonPoints[i].y;
                b1 += polygonPoints[i].y;
            }
            float det = (a00 * a11) - (a01 * a10);
            if (det < 1e-5f && det > -1e-5f)
                return false;
            det = 1.0f / det;
            float temp = a00;
            a00 = a11 * det;
            a11 = temp * det;
            a01 = -a01 * det;
            a10 = -a10 * det;
            float k = a00 * b0 + a01 * b1;
            float b = a10 * b0 + a11 * b1;
            float sum = 0.0f;
            for (int i = start; i <= end; ++i)
            {
                float dy = (polygonPoints[i].x * k + b) - polygonPoints[i].y;
                sum += dy * dy;
            }
            if (sum > threshold)
            {
                return false;
            }
            return true;
        }
    }
}
