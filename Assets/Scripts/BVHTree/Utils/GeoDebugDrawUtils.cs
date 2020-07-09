
using System.Collections.Generic;
using UnityEngine;


namespace Nullspace
{
    public class GeoDebugDrawUtils
    {
        public static void DrawLine(Vector2 p1, Vector2 p2, Color clr)
        {
            Debug.DrawLine(p1, p2, clr);
        }

        public static void DrawLine(Vector3 p1, Vector3 p2, Color clr)
        {
            Debug.DrawLine(p1, p2, clr);
        }

        public static void DrawPoints(List<Vector2> points, Color clr)
        {
            for (int i = 0; i < points.Count; ++i)
            {
                Debug.DrawLine(points[i], points[i] + Vector2.right * 0.1f, clr);
            }
        }
        public static void DrawAABB(GeoRect2 rect, Color clr)
        {
            Debug.DrawLine(rect.mP1, rect.mP2, clr);
            Debug.DrawLine(rect.mP3, rect.mP2, clr);
            Debug.DrawLine(rect.mP1, rect.mP4, clr);
            Debug.DrawLine(rect.mP3, rect.mP4, clr);
        }
        public static void DrawAABB(GeoRect3 rect, Color clr)
        {
            Debug.DrawLine(rect.mP1, rect.mP2, clr);
            Debug.DrawLine(rect.mP3, rect.mP2, clr);
            Debug.DrawLine(rect.mP1, rect.mP4, clr);
            Debug.DrawLine(rect.mP3, rect.mP4, clr);
        }

        public static void DrawAABB(Vector2 min, Vector2 max, Color clr)
        {
            Vector2 p2 = new Vector2(max[0], min[1]);
            Vector2 p4 = new Vector2(min[0], max[1]);
            Debug.DrawLine(min, p2, clr);
            Debug.DrawLine(max, p2, clr);
            Debug.DrawLine(max, p4, clr);
            Debug.DrawLine(min, p4, clr);
        }


        public static void DrawAABB(Vector3 min, Vector3 max)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "cube";
            go.transform.position = (min + max) * 0.5f;
            go.transform.localScale = (max - min);
        }

        public static void DrawAABB(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Color clr)
        {
            Debug.DrawLine(p1, p2, clr);
            Debug.DrawLine(p3, p2, clr);
            Debug.DrawLine(p1, p4, clr);
            Debug.DrawLine(p3, p4, clr);
        }
        public static void DrawAABB(List<Vector2> aabbs, Color clr)
        {
            for (int i = 0; i < aabbs.Count; ++i)
            {
                int start = i * 4;
                Debug.DrawLine(aabbs[start], aabbs[start + 1], clr);
                Debug.DrawLine(aabbs[start + 1], aabbs[start + 2], clr);
                Debug.DrawLine(aabbs[start + 2], aabbs[start + 3], clr);
                Debug.DrawLine(aabbs[start + 3], aabbs[start], clr);
            }
        }
        public static void DrawAABB(List<GeoAABB2> aabbs, Color clr)
        {
            foreach (GeoAABB2 aabb in aabbs)
            {
                DrawAABB(aabb, clr);
            }
        }


        private static Color[] Colors = new Color[] { Color.red, Color.green, Color.blue, Color.white };
        /// <summary>
        ///                    max
        ///         6 --------- 7
        ///       / |         / |
        ///     2 --------- 3   |
        ///     |   |       |   |
        ///     |   |       |   |
        ///     |   4 --------- 5
        ///     | /         | /
        ///     0 --------- 1
        ///    min
        /// </summary>
        public static void DrawAABB(Vector3 Min, Vector3 Max, int level)
        {
            int idx = level % Colors.Length;
            // 6个面
            // 8顶点索引和数据对应
            Vector3[] vxt = new Vector3[8];
            vxt[0] = new Vector3(Min[0], Min[1], Min[2]);
            vxt[1] = new Vector3(Max[0], Min[1], Min[2]);
            vxt[2] = new Vector3(Min[0], Max[1], Min[2]);
            vxt[3] = new Vector3(Max[0], Max[1], Min[2]);
            vxt[4] = new Vector3(Min[0], Min[1], Max[2]);
            vxt[5] = new Vector3(Max[0], Min[1], Max[2]);
            vxt[6] = new Vector3(Min[0], Max[1], Max[2]);
            vxt[7] = new Vector3(Max[0], Max[1], Max[2]);

            List<Vector3> polygon = new List<Vector3>();
            // 近
            polygon.Add(vxt[0]);
            polygon.Add(vxt[1]);
            polygon.Add(vxt[3]);
            polygon.Add(vxt[2]);
            GeoDebugDrawUtils.DrawPolygon(polygon, Colors[idx]);

            // 左
            polygon.Clear();
            polygon.Add(vxt[0]);
            polygon.Add(vxt[4]);
            polygon.Add(vxt[6]);
            polygon.Add(vxt[2]);
            GeoDebugDrawUtils.DrawPolygon(polygon, Colors[idx]);

            // 底
            polygon.Clear();
            polygon.Add(vxt[0]);
            polygon.Add(vxt[1]);
            polygon.Add(vxt[5]);
            polygon.Add(vxt[4]);
            GeoDebugDrawUtils.DrawPolygon(polygon, Colors[idx]);

            // 右
            polygon.Clear();
            polygon.Add(vxt[1]);
            polygon.Add(vxt[5]);
            polygon.Add(vxt[7]);
            polygon.Add(vxt[3]);
            GeoDebugDrawUtils.DrawPolygon(polygon, Colors[idx]);

            // 远
            polygon.Clear();
            polygon.Add(vxt[4]);
            polygon.Add(vxt[5]);
            polygon.Add(vxt[7]);
            polygon.Add(vxt[6]);
            GeoDebugDrawUtils.DrawPolygon(polygon, Colors[idx]);

            // 上
            polygon.Clear();
            polygon.Add(vxt[2]);
            polygon.Add(vxt[3]);
            polygon.Add(vxt[7]);
            polygon.Add(vxt[6]);
            GeoDebugDrawUtils.DrawPolygon(polygon, Colors[idx]);
        }

        public static void DrawAABB(GeoAABB2 aabb, Color clr)
        {
            Vector2 min = aabb.mMin;
            Vector2 max = aabb.mMax;
            Vector2 p2 = new Vector2(max[0], min[1]);
            Vector2 p4 = new Vector2(min[0], max[1]);
            Debug.DrawLine(min, p2, clr);
            Debug.DrawLine(max, p2, clr);
            Debug.DrawLine(max, p4, clr);
            Debug.DrawLine(min, p4, clr);
        }
        public static void DrawPolygon(List<Vector2> poly, Color clr)
        {
            for (int i = 0; i < poly.Count; ++i)
            {
                int j = i + 1;
                if (j == poly.Count)
                    j = 0;
                Debug.DrawLine(poly[i], poly[j], clr);
            }
        }
        public static void DrawPolygon(List<Vector3> poly, Color clr)
        {
            for (int i = 0; i < poly.Count; ++i)
            {
                int j = i + 1;
                if (j == poly.Count)
                    j = 0;
                Debug.DrawLine(poly[i], poly[j], clr);
            }
        }
        public static void DrawConvex(List<List<int>> faces, List<Vector2> vertes, Color clr)
        {
            for (int i = 0; i < faces.Count; ++i)
            {
                for (int j = 0; j < faces[i].Count; ++j)
                {
                    int k = j + 1;
                    if (k == faces[i].Count)
                        k = 0;
                    Debug.DrawLine(vertes[faces[i][j]], vertes[faces[i][k]], clr);
                }
            }
        }
        public static void DrawTriangles(List<int> faces, List<Vector2> vertes, Color clr)
        {
            for (int i = 0; i < faces.Count; i += 3)
            {
                Debug.DrawLine(vertes[faces[i]], vertes[faces[i + 1]], clr);
                Debug.DrawLine(vertes[faces[i + 1]], vertes[faces[i + 2]], clr);
                Debug.DrawLine(vertes[faces[i]], vertes[faces[i + 2]], clr);
            }
        }
        public static void DrawTriangles(List<List<Vector2>> triangles, Color clr)
        {
            foreach (List<Vector2> tmp in triangles)
            {
                Debug.DrawLine(tmp[0], tmp[1], clr);
                Debug.DrawLine(tmp[1], tmp[2], clr);
                Debug.DrawLine(tmp[2], tmp[0], clr);
            }
        }
        public static void DrawTriangles(List<List<Vector3>> triangles, Color clr)
        {
            foreach (List<Vector3> tmp in triangles)
            {
                Debug.DrawLine(tmp[0], tmp[1], clr);
                Debug.DrawLine(tmp[1], tmp[2], clr);
                Debug.DrawLine(tmp[2], tmp[0], clr);
            }
        }
        public static void DrawTriangles(List<Vector3> triangles, Color clr)
        {
            for (int i = 0; i < triangles.Count; i += 3)
            {
                Debug.DrawLine(triangles[i], triangles[i + 1], clr);
                Debug.DrawLine(triangles[i + 1], triangles[i + 2], clr);
                Debug.DrawLine(triangles[i + 2], triangles[i], clr);
            }
        }
        public static void DrawTriangles(List<int> faces, List<Vector3> vertes, Color clr)
        {
            for (int i = 0; i < faces.Count; i += 3)
            {
                Debug.DrawLine(vertes[faces[i]], vertes[faces[i + 1]], clr);
                Debug.DrawLine(vertes[faces[i + 1]], vertes[faces[i + 2]], clr);
                Debug.DrawLine(vertes[faces[i]], vertes[faces[i + 2]], clr);
            }
        }
    }
}
