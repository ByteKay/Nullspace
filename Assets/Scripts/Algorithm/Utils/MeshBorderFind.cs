
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Nullspace
{
    class EdgeObject : ClosestObject
    {
        public Vector2i mEdge;
        public int mCount;
        public EdgeObject(int i, int j)
        {
            mEdge = Vector2i.GetVector2i(i, j);
            mCount = 1;
        }

        public void CountPlus()
        {
            ++mCount;
        }

        public bool Include(int idx)
        {
            return mEdge[0] == idx || mEdge[1] == idx;
        }

        public int IndexOf(int idx)
        {
            return mEdge[0] == idx ? 0 : (mEdge[1] == idx ? 1 : -1);
        }

        public EdgeObject(Vector2i vi)
        {
            mEdge = vi;
            mCount = 1;
        }

        
        public override bool IsCloseTo(ClosestObject other)
        {
            EdgeObject oth = (EdgeObject)other;
            return mEdge.PartialEqual(oth.mEdge);
        }


    }
    public class MeshBorderFind
    {

        public static void FindMeshBorder(List<Vector2> vertes, List<int> indices, out List<List<Vector2>> borderLst)
        {
            int count = indices.Count / 3;
            Dictionary<string, EdgeObject> mEdgeDict = new Dictionary<string, EdgeObject>();
            for (int i = 0; i < count; ++i)
            {
                int st = i * 3;
                for (int j = st; j < (st + 3); ++j)
                {
                    int next = j + 1;
                    if (next == (st + 3))
                    {
                        next = st;
                    }
                    Vector2i edge = Vector2i.GetVector2i(indices[j], indices[next]);
                    string str1 = edge.GetString();
                    if (!mEdgeDict.ContainsKey(str1))
                    {
                        mEdgeDict.Add(str1, new EdgeObject(edge));
                    }
                    else
                    {
                        mEdgeDict[str1].CountPlus();
                    }
                }
            }
            List<EdgeObject> oneList = mEdgeDict.Values.ToList().FindAll(t => { return t.mCount == 1; });
            GroupClosest<EdgeObject> group = new GroupClosest<EdgeObject>(oneList);
            List<List<EdgeObject>> results = group.Group();
            borderLst = new List<List<Vector2>>();
            foreach (List<EdgeObject> res in results)
            {
                List<Vector2> tmp = new List<Vector2>();
                EdgeObject obj0 = res[0];
                int first = obj0.mEdge[0];
                int last = obj0.mEdge[1];
                tmp.Add(vertes[first]);
                tmp.Add(vertes[last]);
                res.RemoveAt(0);
                while (res.Count > 0)
                {
                    int index1 = res.FindIndex(o => { return o.Include(last); });
                    if (index1 == -1)
                    {
                        tmp.Clear();
                        break;
                    }
                    EdgeObject obj = res[index1];
                    int index = obj.IndexOf(last);
                    index = 1 - index;
                    last = obj.mEdge[index];
                    tmp.Add(vertes[last]);
                    res.RemoveAt(index1);
                }
                if (tmp.Count > 2)
                {
                    tmp.RemoveAt(tmp.Count - 1);
                    borderLst.Add(tmp);
                }
            }
        }

        public static void FindMeshBorder(List<Vector3> vertes, List<int> indices, out List<List<Vector3>> borderLst)
        {        
            int count = indices.Count / 3;
            Dictionary<string, EdgeObject> mEdgeDict = new Dictionary<string, EdgeObject>();
            for (int i = 0; i < count; ++i)
            {
                int st = i * 3;
                for (int j = st; j < (st + 3); ++j)
                {
                    int next = j + 1;
                    if (next == (st + 3))
                    {
                        next = st;
                    }
                    Vector2i edge = Vector2i.GetVector2i(indices[j], indices[next]);
                    string str1 = edge.GetString();
                    if (!mEdgeDict.ContainsKey(str1))
                    {
                        mEdgeDict.Add(str1, new EdgeObject(edge));
                    }
                    else
                    {
                        mEdgeDict[str1].CountPlus();
                    }
                }
            }
            List<EdgeObject> oneList = mEdgeDict.Values.ToList().FindAll(t => { return t.mCount == 1; });
            GroupClosest<EdgeObject> group = new GroupClosest<EdgeObject>(oneList);
            List<List<EdgeObject>> results = group.Group();
            borderLst = new List<List<Vector3>>();
            foreach (List<EdgeObject> res in results)
            {
                List<Vector3> tmp = new List<Vector3>();
                EdgeObject obj0 = res[0];
                int first = obj0.mEdge[0];
                int last = obj0.mEdge[1];
                tmp.Add(vertes[first]);
                tmp.Add(vertes[last]);
                res.RemoveAt(0);
                while (res.Count > 0)
                {
                    int index1 = res.FindIndex(o => { return o.Include(last); });
                    if (index1 == -1)
                    {
                        tmp.Clear();
                        break;
                    }
                    EdgeObject obj = res[index1];
                    int index = obj.IndexOf(last);
                    index = 1 - index;
                    last = obj.mEdge[index];
                    tmp.Add(vertes[last]);
                    res.RemoveAt(index1);
                }
                if (tmp.Count > 2)
                {
                    tmp.RemoveAt(tmp.Count - 1);
                    borderLst.Add(tmp);
                }
            }
        }

    }
}
