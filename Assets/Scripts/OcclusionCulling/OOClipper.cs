
using UnityEngine;

namespace Nullspace
{
    public class OOClipper
    {
        public Vector4[] Vi;
        public Vector4[] Vo;
        public Vector2i[] Vs;

        private float mScaleX;
        private float mScaleY;
        private float mDeltaX;
        private float mDeltaY;
        private float mMapXres;
        private float mMapYres;
        private Vector4[] mVi;
        private Vector4[] mVo;
        private float[] mDd;
        private int mVp;
        public OOClipper()
        {
            mVi = new Vector4[64];
            mVo = new Vector4[64];
            Vi = mVi;
            Vo = mVo;
            mDd = new float[64];
        }

        public void SetResolution(int x, int y)
        {
            mScaleX = 32 * x / 2;
            mScaleY = 32 * y / 2;
            mDeltaX = 32 * x / 2;
            mDeltaY = 32 * y / 2;
        }

        public int ClipAndProject(int n)
        {
            mVp = n;
            uint clipor = 0;
            uint clipand = 0xffffffff;
            for (int i = 0; i < mVp; i++)
            {
                uint clipmask = 0;
                if (Vi[i][0] < -Vi[i][3])
                    clipmask |= 1;
                if (Vi[i][0] > Vi[i][3])
                    clipmask |= 2;
                if (Vi[i][1] < -Vi[i][3])
                    clipmask |= 4;
                if (Vi[i][1] > Vi[i][3])
                    clipmask |= 8;
                if (Vi[i][2] < -Vi[i][3])
                    clipmask |= 16;
                if (Vi[i][2] > Vi[i][3])
                    clipmask |= 32;
                clipor |= clipmask;
                clipand &= clipmask;
            }

            if (clipand > 0)
            {
                mVp = 0;
                return 0;
            }

            if (clipor > 0)
            {
                if ((clipor & 1) > 0)
                {
                    for (int i = 0; i < mVp; i++)
                        mDd[i] = Vi[i][3] + Vi[i][0];
                    Clip();
                    if (mVp < 3)
                        return 0;
                }
                if ((clipor & 2) > 0)
                {
                    for (int i = 0; i < mVp; i++)
                        mDd[i] = Vi[i][3] - Vi[i][0];
                    Clip();
                    if (mVp < 3) return 0;
                }
                if ((clipor & 4) > 0)
                {
                    for (int i = 0; i < mVp; i++)
                        mDd[i] = Vi[i][3] + Vi[i][1];
                    Clip(); if (mVp < 3)
                        return 0;
                }
                if ((clipor & 8) > 0)
                {
                    for (int i = 0; i < mVp; i++)
                        mDd[i] = Vi[i][3] - Vi[i][1];
                    Clip();
                    if (mVp < 3)
                        return 0;
                }
                if ((clipor & 16) > 0)
                {
                    for (int i = 0; i < mVp; i++)
                        mDd[i] = Vi[i][3] + Vi[i][2];
                    Clip();
                    if (mVp < 3)
                        return 0;
                }
                if ((clipor & 32) > 0)
                {
                    for (int i = 0; i < mVp; i++)
                        mDd[i] = Vi[i][3] - Vi[i][2];
                    Clip();
                    if (mVp < 3)
                        return 0;
                }
            }

            for (int i = 0; i < mVp; i++)
            {
                float invh = 1 / Vi[i][3];
                Vi[i][0] = Vi[i][0] * invh * mScaleX + mDeltaX;
                Vi[i][1] = mDeltaY - Vi[i][1] * invh * mScaleY;
                Vs[i][0] = ((int)(Vi[i][0])) | 1;
                Vs[i][1] = ((int)(Vi[i][1])) | 1;
            }
            return mVp;
        }

        private void Clip()
        {
            int j;
            int k = 0;
            for (int i = 0; i < mVp; i++)
            {
                j = i + 1;
                if (j == mVp)
                    j = 0;
                if (mDd[i] >= 0)
                {
                    if (mDd[j] < 0)
                    {
                        Vo[k++] = Vector4.Lerp(Vi[i], Vi[j], mDd[i] / (mDd[i] - mDd[j]));
                    }
                }
                else if (mDd[j] >= 0)
                {
                    Vo[k++] = Vector4.Lerp(Vi[j], Vi[i], mDd[j] / (mDd[j] - mDd[i]));
                }
            }
            mVp = k;
            Vector4[] tmp = Vi;
            Vi = Vo;
            Vo = tmp;
        }

    }
}
