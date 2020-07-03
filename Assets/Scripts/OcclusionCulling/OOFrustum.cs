
using UnityEngine;

namespace Nullspace
{
    public class OOFrustum
    {

        private Matrix4x4 mMtr;
        private Vector3 mPosition;
        private Vector4[] mPlanes;

        public OOFrustum()
        {
            mPlanes = new Vector4[6];
        }

        public void Set(ref Matrix4x4 m, ref Vector3 pos)
        {
            mMtr = m;
            mPosition = pos;
            GetPlanes();
        }

        public int Test(OOBox b)
        {
            int i;
            float m, n;
            Vector3 diff = b.Mid - mPosition;
            if ((Mathf.Abs(diff[0]) < b.Size[0]) &&
                (Mathf.Abs(diff[1]) < b.Size[1]) &&
                (Mathf.Abs(diff[2]) < b.Size[2]))
            {
                return 2;
            }

            for (i = 0; i < 6; i++)
            {
                m = b.Mid[0] * mPlanes[i][0] + b.Mid[1] * mPlanes[i][1] + b.Mid[2] * mPlanes[i][2] + mPlanes[i][3];
                n = b.Size[0] * Mathf.Abs(mPlanes[i][0]) +
                  b.Size[1] * Mathf.Abs(mPlanes[i][1]) +
                  b.Size[2] * Mathf.Abs(mPlanes[i][2]);
                if (m > n)
                {
                    return 0;
                }
            }
            return 1;
        }

        private void PlaneNormalize(int idx)
        {
            mPlanes[idx].Normalize();
            mPlanes[idx] = -mPlanes[idx];
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
            mPlanes[n][0] = mMtr[0, 3] + mMtr[0, m];
            mPlanes[n][1] = mMtr[1, 3] + mMtr[1, m];
            mPlanes[n][2] = mMtr[2, 3] + mMtr[2, m];
            mPlanes[n][3] = mMtr[3, 3] + mMtr[3, m];
        }

        public void pln_sub(int n, int m)
        {
            mPlanes[n][0] = mMtr[0, 3] - mMtr[0, m];
            mPlanes[n][1] = mMtr[1, 3] - mMtr[1, m];
            mPlanes[n][2] = mMtr[2, 3] - mMtr[2, m];
            mPlanes[n][3] = mMtr[3, 3] - mMtr[3, m];
        }
    }
}
