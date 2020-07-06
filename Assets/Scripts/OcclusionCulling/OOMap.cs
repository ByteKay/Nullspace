
using System;

namespace Nullspace
{
    public class OOMap
    {
        private const short OOCE_NULL = -32768;
        private const byte OOCE_EMPTY =  0;
        private const byte OOCE_FULL  =  255;
        private const byte OOCE_PARTIAL = 1;
        private const byte OOCE_DIRTY = 2;
        private const int Y_MAX = 2000;

        private int mMapXres;
        private int mMapYres;
        private int mMapXresb;
        private int mMapYresb;
        private int[] mEt;
        private int[,] mBorder;
        private int mYmin;
        private int mYmax;
        public byte[] Blocks;
        public uint[] Map;

        public OOMap()
        {
            mBorder = new int[Y_MAX, 2];
            mEt = new int[Y_MAX];
            for (int i = 0; i < Y_MAX; i++)
            {
                mEt[i] = OOCE_NULL;
                mBorder[i, 0] = 100000;
                mBorder[i, 1] = 0;
            }
            mYmin = 100000;
            mYmax = 0;
            Map = null;
            Blocks = null;

        }

        public void SetResolution(int x, int y)
        {
            mMapXres = x;
            mMapYres = y;
            mMapXresb = (mMapXres + 31) >> 5;
            mMapYresb = (mMapYres + 31) >> 5;
            Map = new uint[mMapXresb * mMapYresb * 32 * 4];
            Blocks = new byte[mMapXresb * mMapYresb];
        }

        public void Clear()
        {
            for (int i = 0; i < Y_MAX; i++)
            {
                mEt[i] = OOCE_NULL;
                mBorder[i, 0] = 100000;
                mBorder[i, 1] = 0;
            }
            Array.Clear(Map, 0, mMapXresb * mMapYresb * 32 * 4 - 1);
            Array.Clear(Blocks, 0, mMapXresb * mMapYresb - 1);
        }

        public void DrawOPolygon(Vector2i[] vs, int vp)
        {
            int i, j;
            for (i = 0; i < vp; i++)
            {
                j = i + 1;
                if (j == vp)
                    j = 0;
                if (vs[i][1] < vs[j][1])
                    DrawEdge(vs, i, j);
                else
                    DrawEdge(vs, j, i);
            }
        }


        public void SetDirtyRectangle(int x1, int y1, int x2, int y2)
        {
            int i, j;
            x1 >>= 5;
            y1 >>= 5;
            x2 >>= 5;
            y2 >>= 5;
            x2--;
            y2--;
            x1 >>= 5;
            y1 >>= 5;
            x2 >>= 5;
            y2 >>= 5;
            for (i = y1; i <= y2; i++)
            {
                int idx = i * mMapXresb + x1;
                for (j = x1; j <= x2; j++)
                {
                    if (Blocks[idx] != OOCE_FULL)
                    {
                        Blocks[idx] = OOCE_DIRTY;
                    }
                    idx++;
                }
            }
        }

        public void UpdateBlock(int bptr, int mptr)
        {
            if (Blocks[bptr] == OOCE_FULL)
            {
                return;
            }
            if (Blocks[bptr] != OOCE_DIRTY)
            {
                return;
            }
            uint band = 0xffffffff;
            uint bor = 0;
            for (int i = 0; i < 32; i++)
            {
                band &= Map[mptr];
                bor |= Map[mptr];
                mptr += mMapXresb;
            }
            if (bor == 0)
            {
                Blocks[bptr] = OOCE_EMPTY;
                return;
            }
            if (band == 0xffffffff)
            {
                Blocks[bptr] = OOCE_FULL;
                return;
            }
            Blocks[bptr] = OOCE_PARTIAL;
        }

        private void DrawSpan(int min, int max)
        {
            int xl = min >> 5;
            uint ml = 0xffffffff >> (min & 31);
            int xr = max >> 5;
            uint mr = 0xffffffff >> (max & 31);
            int ptr = xl;
            if (xl == xr)
            {
                Map[ptr] |= (ml ^ mr);
            }
            else
            {
                int pte = xr;
                Map[ptr++] |= ml;
                while (ptr < pte)
                {
                    Map[ptr++] = 0xffffffff;
                }
                Map[ptr] |= (mr ^ 0xffffffff);
            }
        }

        private void DrawEdge(Vector2i[] vs, int i, int j)
        {
            int x, y, xe;
            int sxi, dxi;
            int ya = (vs[i][1] + 16) >> 5;
            int yb = ((vs[j][1] + 16) >> 5) - 1;
            if (ya > yb)
                return;

            dxi = (((vs[j][0] - vs[i][0]) << 15) / (vs[j][1] - vs[i][1]));
            sxi = 32768 + (vs[i][0] << 11) + dxi * ((16 - vs[i][1]) & 31) / 32;
            int adr = ya * mMapXresb;
            while (ya <= yb)
            {
                x = sxi >> 16;
                xe = mEt[ya];
                if (xe == OOCE_NULL)
                {
                    mEt[ya] = x;
                }
                else
                {
                    mEt[ya] = OOCE_NULL;
                    if (x < xe)
                    {
                        DrawSpan(x, xe);
                    }
                    else if (x > xe)
                    {
                        DrawSpan(xe, x);
                    }
                }
                sxi += dxi;
                ya++;
                adr += mMapXresb;
            }
        }

        private void ConvertEdge(Vector2i[] vs, int i, int j)
        {
            int ya = (vs[i][1] + 16) >> 5;
            int yb = ((vs[j][1] + 16) >> 5) - 1;
            if (ya < mYmin)
            {
                mYmin = ya;
            }
            if (yb > mYmax)
            {
                mYmax = yb;
            }
            if (ya > yb)
            {
                return;
            }
            int dxi = (((vs[j][0] - vs[i][0]) << 15) / (vs[j][1] - vs[i][1]));
            int sxi = 32768 + (vs[i][0] << 11) + dxi * ((16 - vs[i][1]) & 31) / 32;
            while (ya <= yb)
            {
                int x = sxi >> 16;
                if (x < mBorder[ya, 0])
                {
                    mBorder[ya, 0] = x;
                }
                if (x > mBorder[ya, 1])
                {
                    mBorder[ya, 1] = x;
                }
                sxi += dxi;
                ya++;
            }
        }

        private int TestPolygon()
        {
            for (int i = mYmin >> 5; i <= mYmax >> 5; i++)
            {
                int y1 = i << 5;
                int y2 = y1 + 32;
                if (y1 < mYmin)
                {
                    y1 = mYmin;
                }
                if (y2 > mYmax)
                {
                    y2 = mYmax;
                }
                int xmin = 10000;
                int xmax = 0;
                int xleft = 0;
                int xright = 100000;
                int bleft = 0;
                int bright = 0;
                for (int k = y1; k < y2; k++)
                {
                    bleft = mBorder[k,0];
                    bright = mBorder[k, 1] - 1;
                    if (bleft < xmin)
                    {
                        xmin = bleft;
                    }
                    if (bright > xmax)
                    {
                        xmax = bright;
                    }
                    if (bleft > xleft)
                    {
                        xleft = bleft;
                    }
                    if (bright < xright)
                    {
                        xright = bright;
                    }
                }
                xmin >>= 5;
                xmax >>= 5;
                xleft >>= 5;
                xright >>= 5;
                int vbound;
                if (mYmin > i << 5 || mYmax < (i << 5) + 31)
                {
                    vbound = 1;
                }
                else
                {
                    vbound = 0;
                }
                int bptr = i * mMapXresb + xmin;
                int w2ptr = i * 32 * mMapXresb + xmin;
                for (int j = xmin; j <= xmax; j++)
                {
                    UpdateBlock(bptr, w2ptr);
                    if (Blocks[bptr] == OOCE_EMPTY)
                    {
                        return 1;
                    }
                    if (Blocks[bptr] != OOCE_FULL)
                    {
                        int x1 = j << 5;
                        int x2 = x1 + 32;
                        if (vbound == 0 && j > xleft && j < xright)
                        {
                            if (Blocks[bptr] != OOCE_FULL)
                            {
                                return 1;
                            }
                        }
                        else
                        {
                            int wptr = y1 * mMapXresb + j;
                            for (int k = y1; k < y2; k++)
                            {
                                bleft = mBorder[k,0];
                                bright = mBorder[k,1];
                                if (bleft < x2 && bright >= x1)
                                {
                                    uint mask;
                                    if (bleft <= x1)
                                    {
                                        mask = 0xffffffff;
                                    }
                                    else
                                    {
                                        mask = 0xffffffff >> (bleft & 31);
                                    }
                                    if (bright < x2)
                                    {
                                        mask ^= (0xffffffff >> (bright & 31));
                                    }
                                    if ((Map[wptr] | (mask ^ 0xffffffff)) != 0xffffffff)
                                    {
                                        return 1;
                                    }
                                }
                                wptr += mMapXresb;
                            }
                        }
                    }
                    bptr++;
                    w2ptr++;
                }
            }
            return 0;
        }

        public int QueryOPolygon(Vector2i[] vs, int vp)
        {
            mYmin = 100000;
            mYmax = 0;
            for (int i = 0; i < vp; i++)
            {
                int j = i + 1;
                if (j == vp)
                {
                    j = 0;
                }
                if (vs[i][1] < vs[j][1])
                {
                    ConvertEdge(vs, i, j);
                }
                else
                {
                    ConvertEdge(vs, j, i);
                }
            }

            int res = TestPolygon();
            for (int i = mYmin; i <= mYmax; i++)
            {
                mBorder[i, 0] = 100000;
                mBorder[i, 1] = 0;
            }
            return res;
        }
    }
}
