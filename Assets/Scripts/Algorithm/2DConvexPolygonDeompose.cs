
/*
 
   多边形的凸分解 H-M Algorithm
 
 */

using System.Collections.Generic;
using UnityEngine;
using EdgeID = System.Collections.Generic.KeyValuePair<int, int>;

namespace Nullspace
{
    public class SharedTriangle
    {
        public int mFaceIndex;
        public int[] mVertexIndex;
        public SharedEdge[] mEdgeList;
        public SharedTriangle(int index, int v1, int v2, int v3)
        {
            mFaceIndex = index;
            mVertexIndex = new int[3]{v1, v2, v3};
            mEdgeList = new SharedEdge[3] { new SharedEdge(mFaceIndex, v1, v2, v3), new SharedEdge(mFaceIndex, v1, v3, v2), new SharedEdge(mFaceIndex, v3, v2, v1) };
        }
    }
    public class SharedEdge : ClosestObject
    {
        public int mFaceIndex;
        public int mOtherVertex;
        public Vector2i mEdgeVertexIndex;
        Vector2i mSharedTriangles;
        public SharedEdge(int index, int i, int j, int other)
        {
            mEdgeVertexIndex = Vector2i.GetVector2i(i, j);
            mFaceIndex = index;
            mOtherVertex = other;
        }

        public void SharedFace(int f1, int f2)
        {
            mSharedTriangles = Vector2i.GetVector2i(f1, f2);
        }

        public override bool IsCloseTo(ClosestObject other)
        {
            SharedEdge o = (SharedEdge)other;
            return mEdgeVertexIndex == o.mEdgeVertexIndex;
        }
    }

    public class HalfEdge
    {
        public int mKey;
        public int mIndex;
        public HalfEdge mNext;
        public HalfEdge mPartner;
    }

    public class ConvexPolygonDecompose
    {
        /// <summary>
        /// 简单多边形或者三角面点数组
        /// </summary>
        /// <param name="tries"></param>
        /// <param name="isPolygon">是否为简单多边形</param>
        /// <param name="vert"></param>
        /// <returns></returns>
        public static List<List<int>> Decompose(List<Vector2> tries, bool isPolygon, ref List<Vector2> vert) 
        {
            if (isPolygon)
            {
                EarPolygon poly = new EarPolygon();
                foreach (Vector2 v in tries)
                {
                    poly.AddPoint(v.x, v.y);
                }
                EarClipping.Clip(poly);
                List<Vector2> triangles = new List<Vector2>();
                GeoUtils.FlatList(poly.mResults, ref triangles);
                List<int> indices = new List<int>();
                GeoUtils.MeshVertexPrimitiveType(triangles, ref vert, ref indices);
                return HMDecompose(vert, indices);
            }
            else
            {
                List<int> indices = new List<int>();
                GeoUtils.MeshVertexPrimitiveType(tries, ref vert, ref indices);
                return HMDecompose(vert, indices);
            }
        }
        /// <summary>
        ///  三角面数组 每个子List为一个三角面
        /// </summary>
        /// <param name="triangleLst"></param>
        /// <param name="vert"></param>
        /// <returns></returns>
        public static List<List<int>> Decompose(List<List<Vector2>> triangleLst, ref List<Vector2> vert)
        {
            List<Vector2> tries = new List<Vector2>();
            GeoUtils.FlatList(triangleLst, ref tries);
            List<int> indices = new List<int>();
            GeoUtils.MeshVertexPrimitiveType(tries, ref vert, ref indices);    
            return HMDecompose(vert, indices);
        }

        public static List<List<int>> Decompose(EarPolygon poly, ref List<Vector2> vert)
        {
            EarClipping.Clip(poly);
            List<Vector2> triangles = new List<Vector2>();
            GeoUtils.FlatList(poly.mResults, ref triangles);
            List<int> indices = new List<int>();
            GeoUtils.MeshVertexPrimitiveType(triangles, ref vert, ref indices);
            return HMDecompose(vert, indices);
        }

        private static List<List<int>> HMDecompose(List<Vector2> vertes, List<int> indices)
        {
            List<HalfEdge> edges = BuilderEdge(indices);
            PriorityQueue<int, HalfEdge, float> remoableEdge = new PriorityQueue<int, HalfEdge, float>();
            GetRemovableEdgeQueue(vertes, edges, ref remoableEdge);
            HashSet<EdgeID> st = DeleteEdges(remoableEdge, vertes);
            return ExtractPolygonList(edges, st);
        }

        private static List<List<int>> ExtractPolygonList(List<HalfEdge> graph, HashSet<EdgeID> deletedEdgeSet)
        {
	        List<List<int>> resultList = new List<List<int>>();
	        HashSet<HalfEdge> visited = new HashSet<HalfEdge>();
            foreach (HalfEdge edge in graph)
            {
                if (visited.Contains(edge))
                {
                    continue;
                }
                if (deletedEdgeSet.Contains(GetEdgeId(edge)))
                {
                    continue;
                }
                List<int> polygon = new List<int>();
                HalfEdge current = edge;
                do
                {
                    if (!visited.Contains(current))
                    {
                        visited.Add(current);
                        polygon.Add(current.mIndex);
                    }
                    current = current.mNext;
                    while (current.mPartner != null && deletedEdgeSet.Contains(GetEdgeId(current)))
                    {
                        current = current.mPartner.mNext;
                    }
                } while (current.mKey != edge.mKey);
                resultList.Add(polygon);
            }
	        return resultList;
        }

        private static HashSet<EdgeID> DeleteEdges(PriorityQueue<int, HalfEdge, float> priorityQueue, List<Vector2> vertes)
        {
            HashSet<EdgeID> deletedEdgeSet = new HashSet<EdgeID>();
	        while (priorityQueue.Size > 0)
	        {
                HalfEdge edgeToRemove = priorityQueue.Dequeue();
		        deletedEdgeSet.Add(GetEdgeId(edgeToRemove.mIndex, edgeToRemove.mNext.mIndex));
                UpdateEdge(edgeToRemove, priorityQueue, deletedEdgeSet, vertes);
                UpdateEdge(edgeToRemove.mPartner, priorityQueue, deletedEdgeSet, vertes);
	        }
	        return deletedEdgeSet;
        }
        private static HalfEdge RepresentActive(HalfEdge e)
        {
            if (e.mPartner != null && e.mIndex > e.mPartner.mIndex)
            {
                return e.mPartner;
            }
            return e;
        }
        private static void UpdateEdge(HalfEdge edgeToRemove, PriorityQueue<int, HalfEdge, float> priorityQueue, HashSet<EdgeID> deletedEdgeSet, List<Vector2> pointList)
        {
            HalfEdge left = GetUndeletedLeft(edgeToRemove, deletedEdgeSet);
            HalfEdge right = GetUndeletedRight(edgeToRemove, deletedEdgeSet);
            HalfEdge reLeft = RepresentActive(left);
            if (priorityQueue.Contain(reLeft.mKey))
	        {
		        // Check if this is still removable
                HalfEdge leftOfLeft = GetUndeletedLeft(left.mPartner, deletedEdgeSet);
                if ((leftOfLeft.mPartner != null && (leftOfLeft.mPartner.mKey == right.mKey || leftOfLeft.mPartner.mKey == edgeToRemove.mKey)) ||
                    !GeoPolygonUtils.IsConvex(pointList[edgeToRemove.mIndex], pointList[right.mNext.mIndex], pointList[leftOfLeft.mIndex]))
                {
                    priorityQueue.Remove(reLeft.mKey);
                }
                else
                {
                    // Need to update the priority
                    float pri = GetSmallestAdjacentAngleOnEdge(left, deletedEdgeSet, pointList);
                    priorityQueue.Remove(reLeft.mKey);
                    priorityQueue.Enqueue(reLeft.mKey, reLeft, pri);
                }
	        }
            HalfEdge reRight = RepresentActive(right);
            if (priorityQueue.Contain(reRight.mKey))
	        {
                HalfEdge rightOfRight = GetUndeletedRight(right, deletedEdgeSet);
                if ((rightOfRight.mPartner != null && (rightOfRight.mPartner.mKey == left.mKey || rightOfRight.mKey == edgeToRemove.mKey)) ||
                    !GeoPolygonUtils.IsConvex(pointList[edgeToRemove.mIndex], pointList[rightOfRight.mNext.mIndex], pointList[left.mIndex]))
                {
                    priorityQueue.Remove(reRight.mKey);
                }
                else
                {
                    priorityQueue.Remove(reRight.mKey);
                    priorityQueue.Enqueue(reRight.mKey, reRight, GetSmallestAdjacentAngleOnEdge(right, deletedEdgeSet, pointList));
                }
	        }
        }
        private static EdgeID GetEdgeId(int a, int b)
        {
            return (b < a) ? new EdgeID(b, a) : new EdgeID(a, b);
        }

        private static EdgeID GetEdgeId(HalfEdge edge)
        {
            return GetEdgeId(edge.mIndex, edge.mNext.mIndex);
        }

        private static List<HalfEdge> BuilderEdge(List<int> triangleList)
        {
	        List<HalfEdge> halfEdgeList = new List<HalfEdge>();
	        Dictionary<EdgeID, HalfEdge> openEdgeList = new Dictionary<EdgeID,HalfEdge>();
            int n = triangleList.Count;
	        for (int i = 0; i < n; ++i)
            {
                halfEdgeList.Add(new HalfEdge());
            }
            n = n / 3;
	        for (int i = 0; i < n; ++i)
	        {
                int start = i * 3;
		        for (int j = 0; j < 3; ++j)
		        {
			        int a = start + j;
			        int b = start + (j + 1) % 3;

			        halfEdgeList[a].mIndex = triangleList[a];
			        halfEdgeList[a].mNext = halfEdgeList[b];
                    halfEdgeList[a].mKey = a;
			        EdgeID edgeID = GetEdgeId(triangleList[a], triangleList[b]);                  
                    if (openEdgeList.ContainsKey(edgeID))
                    {
                        HalfEdge edge = openEdgeList[edgeID];
                        edge.mPartner = halfEdgeList[a];
                        halfEdgeList[a].mPartner = edge;
                        openEdgeList.Remove(edgeID);
                    }
                    else
                    {
                        openEdgeList.Add(edgeID, halfEdgeList[a]);
                        halfEdgeList[a].mPartner = null;
                    }
		        }
	        }
	        return halfEdgeList;
        }
        private static void GetRemovableEdgeQueue(List<Vector2> vertes, List<HalfEdge> edges, ref PriorityQueue<int, HalfEdge, float> remoableEdge)
        {
            HashSet<EdgeID> deleteSet = new HashSet<EdgeID>();
            foreach (HalfEdge edge in edges)
	        {
                if (edge.mIndex > edge.mNext.mIndex)
                {
                    continue;
                }
		        if (IsEdgeRemovable(vertes, edge))
		        {
                    remoableEdge.Enqueue(edge.mKey, edge, GetSmallestAdjacentAngleOnEdge(edge, deleteSet, vertes));
		        }
	        }
        }
        private static bool IsEdgeRemovable(List<Vector2> vertes, HalfEdge edge)
        {
            if (edge.mPartner == null)
            {
                return false;
            }
            return GeoPolygonUtils.IsConvex(vertes[edge.mIndex], vertes[edge.mPartner.mNext.mNext.mIndex], vertes[edge.mNext.mNext.mIndex]) &&
                GeoPolygonUtils.IsConvex(vertes[edge.mPartner.mIndex], vertes[edge.mNext.mNext.mIndex], vertes[edge.mPartner.mNext.mNext.mIndex]);
        }
        private static float GetSmallestAdjacentAngleOnEdge(HalfEdge edge, HashSet<EdgeID> deleteSet, List<Vector2> vertes)
        {
            float em = GetSmallestAdjacentAngleOnHalfEdge(edge, deleteSet, vertes);
            float pm = GetSmallestAdjacentAngleOnHalfEdge(edge.mPartner, deleteSet, vertes);
            return Mathf.Max(em, pm);
        }
        private static float GetSmallestAdjacentAngleOnHalfEdge(HalfEdge edge, HashSet<EdgeID> deleteSet, List<Vector2> vertes)
        {
            HalfEdge leftEdge = GetUndeletedLeft(edge, deleteSet);
            HalfEdge rightEdge = GetUndeletedRight(edge, deleteSet);

            Vector2 centerPoint = vertes[edge.mIndex];
            Vector2 forwardPoint = vertes[edge.mNext.mIndex];
            Vector2 leftPoint = vertes[leftEdge.mIndex];
            Vector2 rightPoint = vertes[rightEdge.mNext.mIndex];

            return GetSmallestAdjacentAngleOnHalfEdge(centerPoint, forwardPoint, leftPoint, rightPoint);
        }
        private static HalfEdge GetUndeletedLeft(HalfEdge edge, HashSet<EdgeID> deleteSet)
        {
            HalfEdge edge_right = edge;
            HalfEdge edge_left = edge.mNext.mNext;
            while (deleteSet.Contains(GetEdgeId(edge_left.mIndex, edge_right.mIndex)))
            {
                edge_right = edge_left.mPartner;
                edge_left = edge_right.mNext.mNext;
            }
            return edge_left;
        }
        private static HalfEdge GetUndeletedRight(HalfEdge edge, HashSet<EdgeID> deleteSet)
        {
            do
            {
                edge = edge.mPartner.mNext;
            } while (deleteSet.Contains(GetEdgeId(edge.mIndex, edge.mNext.mIndex)));
            return edge;
        }
        private static float GetSmallestAdjacentAngleOnHalfEdge(Vector2 centerPoint, Vector2 forwardPoint, Vector2 leftPoint, Vector2 rightPoint)
        {
	        Vector2 forwardDirection = (forwardPoint-centerPoint).normalized;
            return Mathf.Max(Vector2.Dot((leftPoint - centerPoint).normalized, forwardDirection),
                            Vector2.Dot((rightPoint - centerPoint).normalized, forwardDirection));
        }
    }
}
