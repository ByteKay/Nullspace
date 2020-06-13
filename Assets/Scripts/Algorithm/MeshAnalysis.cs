
using System.Collections.Generic;

using UnityEngine;

namespace Nullspace
{
    public enum VertexGroupType
    {
        DISTANCE, // 基于距离
        NEIBOR // 基于互相邻性
    }

    public class MeshVertex : ClosestObject
    {
        public int mIndex;
        public Vector3 mPoint;
        public HashSet<int> mNeiborPoints;
        public HashSet<int> mSharedEdges;
        public HashSet<int> mSharedFaces;
        public MeshVertex(int index, Vector3 v)
        {
            mPoint = v;
            mIndex = index;
            mNeiborPoints = new HashSet<int>();
            mSharedEdges = new HashSet<int>();
            mSharedFaces = new HashSet<int>();
        }
        public override bool IsCloseTo(ClosestObject other)
        {
            // to do
            return false;
        }

        public void AddNeiborPoint(int index)
        {
            mNeiborPoints.Add(index);
        }

        public void AddSharedEdges(int index)
        {
            mSharedEdges.Add(index);
        }
        public void AddSharedFaces(int index)
        {
            mSharedFaces.Add(index);
        }
    }

    public class MeshEdge : ClosestObject
    {
        public int mIndex;
        public Vector2i mPointIndex;
        public HashSet<int> mSharedFaces;
        public MeshEdge(int index, int start, int end)
        {
            mIndex = index;
            mPointIndex = new Vector2i(start, end);
            mSharedFaces = new HashSet<int>();
        }
        public MeshEdge(int index, Vector2i e)
        {
            mIndex = index;
            mPointIndex = e;
            mSharedFaces = new HashSet<int>();
        }

        public override bool IsCloseTo(ClosestObject other)
        {
            MeshEdge o = (MeshEdge)other;
            return IsPartialEqual(o.mPointIndex);
        }
        public void AddSharedFaces(int index)
        {
            mSharedFaces.Add(index);
        }

        public bool IsPartialEqual(Vector2i other)
        {
            bool flag = mPointIndex[0] == other[0] || mPointIndex[1] == other[1];
            flag = flag && (mPointIndex[0] == other[1] || mPointIndex[1] == other[0]);
            return flag;
        }

        public bool IsEqual(Vector2i other, bool useDirection = false)
        {
            bool flag = mPointIndex[0] == other[0] && mPointIndex[1] == other[1];
            if (useDirection)
            {
                return flag;
            }
            if (!flag)
            {
                flag = mPointIndex[0] == other[1] && mPointIndex[1] == other[0];
            }
            return flag;
        }
        public bool IsEqual(MeshEdge other)
        {
            return mIndex == other.mIndex;
        }
    }
    public class MeshFace : ClosestObject
    {
        public int mIndex;
        public List<int> mPointIndex;
        public List<int> mEdgeIndex;
        public HashSet<int> mNeiborFaceByEdge;
        public HashSet<int> mNeiborFaceByPoint;
        public MeshFace(int index, int pi, int pj, int pk, int ei, int ej, int ek)
        {
            mIndex = index;
            mPointIndex = new List<int>();
            mPointIndex.Add(pi);
            mPointIndex.Add(pj);
            mPointIndex.Add(pk);
            mEdgeIndex = new List<int>();
            mEdgeIndex.Add(ei);
            mEdgeIndex.Add(ej);
            mEdgeIndex.Add(ek);
            mNeiborFaceByEdge = new HashSet<int>();
            mNeiborFaceByPoint = new HashSet<int>();
        }
        public override bool IsCloseTo(ClosestObject other)
        {
            // to do
            return false;
        }

        public bool IsEqual(int p1, int p2, int p3)
        {
            return mPointIndex.Contains(p1) && mPointIndex.Contains(p2) && mPointIndex.Contains(p3);
        }

        public void AddNeiborFaceByEdge(int index)
        {
            mNeiborFaceByEdge.Add(index);
        }
        public void AddNeiborFaceByEdge(HashSet<int> indexes)
        {
            foreach (int i in indexes)
            {
                mNeiborFaceByEdge.Add(i);
            }
        }

        public void AddNeiborFaceByPoint(int index)
        {
            mNeiborFaceByPoint.Add(index);
        }
        public void AddNeiborFaceByPoint(HashSet<int> indexes)
        {
            foreach (int i in indexes)
            {
                mNeiborFaceByPoint.Add(i);
            }
        }

    }
    public class MeshAnalysis
    {
        public List<MeshVertex> mVertexes;
        public List<MeshEdge> mEdges;
        public List<MeshFace> mFaces;
        public MeshAnalysis(List<Vector3> triangles)
        {
            MeshAnalysis1(triangles);
        }
        public MeshAnalysis(List<Vector3> vertices, List<int> faces)
        {
            MeshAnalysis2(vertices, faces);
        }

        public MeshAnalysis(List<List<Vector3>> triangles)
        {
            List<Vector3> temp = new List<Vector3>();
            GeoAlgorithmUtils.FlatList(triangles, ref temp);
            MeshAnalysis1(temp);
        }

        private void MeshAnalysis1(List<Vector3> triangles)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> faces = new List<int>();
            GeoUtils.MeshVertexPrimitiveType(triangles, ref vertices, ref faces);
            MeshAnalysis2(vertices, faces);
        }
        private void MeshAnalysis2(List<Vector3> vertices, List<int> faces)
        {
            mVertexes = new List<MeshVertex>();
            mEdges = new List<MeshEdge>();
            mFaces = new List<MeshFace>();
            Initialize(vertices, faces);
        }
        private void Initialize(List<Vector3> vertices, List<int> faces)
        {
            for (int i = 0; i < vertices.Count; ++i)
            {
                mVertexes.Add(new MeshVertex(i, vertices[i]));
            }
            int count = faces.Count / 3;
            for (int i = 0; i < count; ++i)
            {
                int p1 = faces[i * 3 + 0];
                int p2 = faces[i * 3 + 1];
                int p3 = faces[i * 3 + 2];
                Vector2i e1 = new Vector2i(p1, p2);
                Vector2i e2 = new Vector2i(p2, p3);
                Vector2i e3 = new Vector2i(p3, p1);
                int i1 = mEdges.FindIndex((MeshEdge e) => { return e.mPointIndex == e1; });
                int i2 = mEdges.FindIndex((MeshEdge e) => { return e.mPointIndex == e2; });
                int i3 = mEdges.FindIndex((MeshEdge e) => { return e.mPointIndex == e3; });
                int index = mEdges.Count;
                if (i1 == -1)
                {
                    i1 = index++;
                    mEdges.Add(new MeshEdge(i1, e1));
                }
                if (i2 == -1)
                {
                    i2 = index++;
                    mEdges.Add(new MeshEdge(i2, e2));
                }
                if (i3 == -1)
                {
                    i3 = index++;
                    mEdges.Add(new MeshEdge(i3, e3));
                }
                mVertexes[p1].AddSharedEdges(i1);
                mVertexes[p2].AddSharedEdges(i2);
                mVertexes[p3].AddSharedEdges(i3);

                mVertexes[p1].AddSharedFaces(i);
                mVertexes[p2].AddSharedEdges(i);
                mVertexes[p3].AddSharedEdges(i);

                mVertexes[p1].AddNeiborPoint(p2);
                mVertexes[p1].AddNeiborPoint(p3);
                mVertexes[p2].AddNeiborPoint(p3);
                mVertexes[p2].AddNeiborPoint(p1);
                mVertexes[p3].AddNeiborPoint(p1);
                mVertexes[p3].AddNeiborPoint(p2);

                mEdges[i1].AddSharedFaces(i);
                mEdges[i2].AddSharedFaces(i);
                mEdges[i3].AddSharedFaces(i);

                MeshFace face = new MeshFace(i, p1, p2, p3, i1, i2, i3);
                mFaces.Add(face);
            }

            for (int i = 0; i < mFaces.Count; ++i)
            {
                foreach (int pi in mFaces[i].mPointIndex)
                {
                    mFaces[i].AddNeiborFaceByPoint(mVertexes[pi].mSharedFaces);
                }

                foreach (int ei in mFaces[i].mEdgeIndex)
                {
                    mFaces[i].AddNeiborFaceByEdge(mEdges[ei].mSharedFaces);
                }
            }
        }

    }

    public class MeshAnalysisExt
    {
        MeshAnalysis mMeshAnalysis;

        public MeshAnalysisExt(List<Vector3> triangles)
        {
            mMeshAnalysis = new MeshAnalysis(triangles);
        }

        public MeshAnalysisExt(List<Vector3> vertices, List<int> faces)
        {
            mMeshAnalysis = new MeshAnalysis(vertices, faces);
        }

        public MeshAnalysisExt(List<List<Vector3>> triangles)
        {
            mMeshAnalysis = new MeshAnalysis(triangles);
        }

        public static void RemoveInvalidPoint(ref List<Vector3> vertices, ref List<int> faces)
        {
            List<Vector3> allValid = new List<Vector3>();
            for (int i = 0; i < faces.Count; ++i)
            {
                allValid.Add(vertices[faces[i]]);
                allValid.Add(vertices[faces[i + 1]]);
                allValid.Add(vertices[faces[i + 2]]);
            }
            vertices.Clear();
            faces.Clear();
            GeoUtils.MeshVertexPrimitiveType(allValid, ref vertices, ref faces);
        }

        // 查找 边 被面共享的次数count，如果是 边界，次数为 1
        public List<MeshEdge> FindEdgeSharedCountByFace(int count)
        {
            List<MeshEdge> results = mMeshAnalysis.mEdges.FindAll((MeshEdge e) => { return e.mSharedFaces.Count == count; });
            return results;
        }

        public List<List<MeshEdge>> GroupEdgeByPointShared(List<MeshEdge> src)
        {
            List<List<MeshEdge>> result = new List<List<MeshEdge>>();
            GroupClosest<MeshEdge> group = new GroupClosest<MeshEdge>();
            group.AddRange(src);
            result = group.Group();
            return result;
        }

        public void RemoveBoundEdge(List<MeshEdge> src, List<Vector3> polygon)
        {
            List<int> temp = new List<int>();
            for (int i = 0; i < polygon.Count; ++i)
            {
                int j = i + 1;
                if (j == polygon.Count)
                    j = 0;
                int index = GetEdgeIndex(polygon[i], polygon[j]);
                if (index != -1)
                {
                    index = src.FindIndex((MeshEdge edge) => { return edge.mIndex == index; });
                    if (index != -1)
                    {
                        temp.Add(index);
                    }
                }
            }
            // 大到小
            temp.Sort((x, y) => {return -x.CompareTo(y);});
            foreach (int idx in temp)
            {
                src.RemoveAt(idx);
            }
        }

        // 返回顶点索引
        public List<int> LinkEdge(List<MeshEdge> src)
        {
            List<MeshEdge> temp = new List<MeshEdge>(src);
            List<int> pointIndex = new List<int>();
            if (temp.Count > 0)
            {
                pointIndex.Add(temp[0].mPointIndex[0]);
                pointIndex.Add(temp[0].mPointIndex[1]);
                temp.RemoveAt(0);
                int lastIndex = pointIndex[pointIndex.Count - 1];
                while (temp.Count > 0)
                {
                    for (int i = 0; i < temp.Count; ++i)
                    {
                        int flag = -1;
                        if (temp[i].mPointIndex[0] == lastIndex)
                        {
                            flag = 0;
                        }
                        else if (temp[i].mPointIndex[1] == lastIndex)
                        {
                            flag = 1;
                        }
                        if (flag != -1)
                        {
                            int other = 1 - flag;
                            other = temp[i].mPointIndex[other];
                            pointIndex.Add(other);
                            lastIndex = other;
                            temp.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            return pointIndex;
        }

        // 以点分组
        public List<List<MeshFace>> GroupFaceByPoint()
        {
            List<List<MeshFace>> results = new List<List<MeshFace>>();
            return results;
        }
        // 以边分组
        public List<List<MeshFace>> GroupFaceByEdge()
        {
            List<List<MeshFace>> results = new List<List<MeshFace>>();
            return results;
        }
        // 以面分组
        public List<List<MeshFace>> GroupFaceByFace()
        {
            List<List<MeshFace>> results = new List<List<MeshFace>>();
            return results;
        }

        public int GetVertexIndex(Vector3 p)
        {
            return mMeshAnalysis.mVertexes.FindIndex((MeshVertex v) => { return v.mPoint == p; });
        }
        public int GetEdgeIndex(Vector3 p1, Vector3 p2)
        {
            int result = -1;
            int index1 = GetVertexIndex(p1);
            if (index1 != -1)
            {
                int index2 = GetVertexIndex(p2);
                if (index2 != -1)
                {
                    Vector2i temp = new Vector2i(index1, index2);
                    result = mMeshAnalysis.mEdges.FindIndex((MeshEdge e) => { return e.IsEqual(temp); });
                }
            }
            return result;
        }
        public int GetFaceIndex(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            int result = -1;
            int index1 = GetVertexIndex(p1);
            if (index1 != -1)
            {
                int index2 = GetVertexIndex(p2);
                if (index2 != -1)
                {
                    int index3 = GetVertexIndex(p3);
                    if (index3 != -1)
                    {
                        Vector2i temp = new Vector2i(index1, index2);
                        result = mMeshAnalysis.mFaces.FindIndex((MeshFace f) => { return f.IsEqual(index1, index2, index3); });
                    }
                }
            }
            return result;
        }

        public List<int> GetVertexIndex(List<Vector3> pLst)
        {
            List<int> index = new List<int>();
            foreach (Vector3 p in pLst)
            {
                int idx = GetVertexIndex(p);
                index.Add(idx);
            }
            return index;
        }
        public static List<Vector3> CalculateConvexHull(List<Vector3> vertices, bool use2d, int ignore = 1)
        {
            List<Vector3> temp = new List<Vector3>();
            if (use2d)
            {
                int x = 0;
                int y = 1;
                if (ignore == 0)
                {
                    x = 1;
                    y = 2;
                }
                else if (ignore == 1)
                {
                    x = 0;
                    y = 2;
                }
                else
                {
                    x = 0;
                    y = 1;
                }
                List<Vector2> temp2 = new List<Vector2>();
                foreach (Vector3 v in vertices)
                {
                    temp2.Add(new Vector2(v[x], v[y]));
                }
                temp2 = JarvisConvex.BuildHull(temp2);
                GeoPolygonUtils.ReverseIfCW(ref temp2);
                foreach (Vector2 v in temp2)
                {
                    temp.Add(vertices.Find((Vector3 v3) => { return v3[x] == v[0] && v3[y] == v[1]; }));
                }
            }
            else
            {
                temp = QuickHull.BuildHull(vertices);

            }
            return temp;
        }
        public List<List<int>> SplitVertex(List<int> group)
        {
            List<List<int>> result = new List<List<int>>();
            for (int i = 0; i < group.Count; ++i)
            {
                int j = i + 1;
                if (j == group.Count)
                    j = 0;

            }
            return result;
        }
        private List<int> SplitVertex(int start, int end)
        {
            List<int> temp = new List<int>();
            // to do
            return temp;
        }
    }
}
