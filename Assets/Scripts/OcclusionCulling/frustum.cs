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
            mtr = m;
            position = pos;
            GetPlanes();
        }

        public int Test(bbox b)
        {
            int i;
            float m, n;
            Vector3 diff = b.mid - position;
            if ((Mathf.Abs(diff[0]) < b.size[0]) &&
                (Mathf.Abs(diff[1]) < b.size[1]) &&
                (Mathf.Abs(diff[2]) < b.size[2]))
            {
                return 2;
            }

            for (i = 0; i < 6; i++)
            {
                m = b.mid[0] * planes[i][0] + b.mid[1] * planes[i][1] + b.mid[2] * planes[i][2] + planes[i][3];
                n = b.size[0] * Mathf.Abs(planes[i][0]) +
                  b.size[1] * Mathf.Abs(planes[i][1]) +
                  b.size[2] * Mathf.Abs(planes[i][2]);
                if (m > n)
                {
                    return 0;
                }
            }
            return 1;
        }

        private void PlaneNormalize(int idx)
        {
            planes[idx].Normalize();
            planes[idx] = -planes[idx];
        }

        private void GetPlanes()
        {
            pln_add(0, 0);
            pln_sub(1, 0);
            pln_add(2, 1);
            pln_sub(3, 1);
            pln_add(4, 2);
            pln_sub(5, 2);
            PlaneNormalize(0);
            PlaneNormalize(1);
            PlaneNormalize(2);
            PlaneNormalize(3);
            PlaneNormalize(4);
            PlaneNormalize(5);
        }

        public void pln_add(int n, int m)
        {
            planes[n][0] = mtr[0, 3] + mtr[0, m];
            planes[n][1] = mtr[1, 3] + mtr[1, m];
            planes[n][2] = mtr[2, 3] + mtr[2, m];
            planes[n][3] = mtr[3, 3] + mtr[3, m];
        }

        public void pln_sub(int n, int m)
        {
            planes[n][0] = mtr[0, 3] - mtr[0, m];
            planes[n][1] = mtr[1, 3] - mtr[1, m];
            planes[n][2] = mtr[2, 3] - mtr[2, m];
            planes[n][3] = mtr[3, 3] - mtr[3, m];
        }
    }
}
