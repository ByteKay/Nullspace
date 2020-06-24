using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class ooce_model
    {
        private List<Vector3> vertices;
        private List<Vector3> cvertices;
        private List<Vector4> tvertices;
        private List<Vector3i> faces;
        private int n_vertices;
        public int id;
        public int n_faces;
        public bbox b;

        public ooce_model()
        {

        }

        public void Define(List<Vector3> v, int nv, List<Vector3i> f, int nf)
        {

        }
        public void SetBox(ref Vector3 min, ref Vector3 max)
        {

        }
        public void DefineBox(Vector3 v, int nv)
        {

        }

        public void SetID(int setid)
        {

        }
    }
}
