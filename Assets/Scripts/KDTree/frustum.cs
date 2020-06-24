using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nullspace
{
    public class frustum
    {

        private Matrix4x4 mtr;
        private Vector3 position;
        private Vector4[] planes;

        public frustum()
        {
            planes = new Vector4[6];
        }

        public void Set(ref Matrix4x4 m, ref Vector3 pos)
        {

        }
        public int Test(bbox b)
        {
            return 0;
        }

        private void PlaneNormalize(Vector4 p)
        {

        }
        private void GetPlanes()
        {

        }

    }
}
