
using System;
using System.Collections.Generic;

using UnityEngine;

namespace Nullspace
{
    public class MeshVertexMerge
    {
        class Vertex3Hash : ObjectHash
        {
            public int mIndex;
            public Vector3 mVertex;
            public Vertex3Hash(int i, Vector3 v)
            {
                mIndex = i;
                mVertex = v;
            }
            
            public override string String()
            {
                return string.Format("{0}_{1}_{2}", Math.Round(mVertex[0], 2), Math.Round(mVertex[1], 2), Math.Round(mVertex[2], 2));
            }

        }
        class Vertex2Hash : ObjectHash
        {
            public int mIndex;
            public Vector2 mVertex;
            public Vertex2Hash(int i, Vector2 v)
            {
                mIndex = i;
                mVertex = v;
            }
            
            public override string String()
            {
                return string.Format("{0}_{1}", Math.Round(mVertex[0], 1), Math.Round(mVertex[1], 1));
            }

        }

        public static void MergeVertex(List<Vector3> vertes, List<int> indices, out List<Vector3> retained, out List<int> indexLst)
        {
            retained = new List<Vector3>();
            indexLst = new List<int>();
            GroupHash<Vertex3Hash> groups = new GroupHash<Vertex3Hash>();
            for (int i = 0; i < vertes.Count; ++i)
            {
                groups.Add(new Vertex3Hash(i, vertes[i]));
            }
            List<List<Vertex3Hash>> results = groups.GetResult();
            Dictionary<int, int> tmp = new Dictionary<int, int>();
            for (int i = 0; i < results.Count; ++i)
            {
                Vector3 tmpV = new Vector3(0.0f, 0.0f, 0.0f);
                foreach (Vertex3Hash obj in results[i])
                {
                    tmp.Add(obj.mIndex, i);
                    tmpV += obj.mVertex;
                }
                retained.Add(tmpV * 1.0f / results[i].Count);
            }
            foreach (int ind in indices)
            {
                indexLst.Add(tmp[ind]);
            }
            int count = indexLst.Count / 3;
            List<int> remove = new List<int>();
            for (int i = 0; i < count; ++i)
            {
                int i1 = indexLst[i * 3];
                int i2 = indexLst[i * 3 + 1];
                int i3 = indexLst[i * 3 + 2];
                if (i1 == i2 || i2 == i3 || i1 == i3)
                {
                    remove.Add(i * 3);
                    remove.Add(i * 3 + 1);
                    remove.Add(i * 3 + 2);
                }
            }
            for (int i = remove.Count - 1; i >= 0; --i)
            {
                indexLst.RemoveAt(remove[i]);
            }
        }

        public static void MergeVertex(List<Vector2> vertes, List<int> indices, out List<Vector2> retained, out List<int> indexLst)
        {
            retained = new List<Vector2>();
            indexLst = new List<int>();
            GroupHash<Vertex2Hash> groups = new GroupHash<Vertex2Hash>();
            for(int i = 0; i < vertes.Count; ++i)
            {
                groups.Add(new Vertex2Hash(i, vertes[i]));
            }
            List<List<Vertex2Hash>> results = groups.GetResult();
            Dictionary<int, int> tmp = new Dictionary<int, int>();
            for (int i = 0; i < results.Count; ++i)
            {
                Vector2 tmpV = new Vector2(0.0f, 0.0f);
                foreach (Vertex2Hash obj in results[i])
                {
                    tmp.Add(obj.mIndex, i);
                    tmpV += obj.mVertex;
                }
                retained.Add(tmpV * 1.0f / results[i].Count);
            }
            foreach (int ind in indices)
            {
                indexLst.Add(tmp[ind]);
            }
            int count = indexLst.Count / 3;
            List<int> remove = new List<int>();
            for (int i = 0; i < count; ++i)
            {
                int i1 = indexLst[i * 3];
                int i2 = indexLst[i * 3 + 1];
                int i3 = indexLst[i * 3 + 2];
                if (i1 == i2 || i2 == i3 || i1 == i3)
                {
                    remove.Add(i * 3);
                    remove.Add(i * 3 + 1);
                    remove.Add(i * 3 + 2);
                }
            }
            for (int i = remove.Count - 1; i >= 0; --i)
            {
                indexLst.RemoveAt(remove[i]);
            }
        }
    }
}
