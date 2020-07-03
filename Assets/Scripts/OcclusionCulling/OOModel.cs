
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class OOModel
    {
        public List<Vector3> Vertices;
        public List<Vector3> CVertices;
        public List<Vector4> TVertices;
        public List<Vector3i> Faces;
        public int NVertices;
        public int Id;
        public int NFaces;
        public OOBox Box;

        public OOModel()
        {
            Vertices = null;
            Faces = null;
            TVertices = null;
            CVertices = null;
            Box = new OOBox(Vector3.zero, Vector3.zero);
        }

        public void Define(List<Vector3> v, int nv, List<Vector3i> f, int nf)
        {
            int i;
            NFaces = nf;
            NVertices = nv;
            Vertices = v;
            Faces = f;
            TVertices = new List<Vector4>(nv);
            CVertices = new List<Vector3>(nv);

            Box.Begin();
            for (i = 0; i < nv; i++)
                Box.Add(v[i]);
            Box.ToMidSize();
        }

        public void SetBox(Vector3 min, Vector3 max)
        {
            Box = new OOBox(min, max);
            Box.ToMidSize();
        }

        public void DefineBox(List<Vector3> v, int nv)
        {
            Box.Begin();
            for (int i = 0; i < nv; i++)
            {
                Box.Add(v[i]);
            }
            Box.ToMidSize();
        }

        public void SetID(int setid)
        {
            Id = setid;
        }
    }
}
