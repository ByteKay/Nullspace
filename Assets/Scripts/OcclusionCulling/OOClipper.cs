
using UnityEngine;

namespace Nullspace
{
    public class OOClipper
    {
        public Vector4[] mClipSpaceVertices;
        public Vector4[] mClipOutVertices;
        public Vector2i[] mScreenSpaceVertices;
        private float[] mClipLerpFactor;
        private int mClipVerticesNumber;
        private float mScaleX;
        private float mScaleY;
        public OOClipper()
        {
            mClipSpaceVertices = new Vector4[64];
            mClipOutVertices = new Vector4[64];
            mClipLerpFactor = new float[64];
            mScreenSpaceVertices = new Vector2i[8]
                                    {
                                        new Vector2i(),
                                        new Vector2i(),
                                        new Vector2i(),
                                        new Vector2i(),
                                        new Vector2i(),
                                        new Vector2i(),
                                        new Vector2i(),
                                        new Vector2i()
                                    };
        }

        public void SetResolution(int x, int y)
        {
            mScaleX = 32 * x / 2;
            mScaleY = 32 * y / 2;
        }

        /// <summary>
        /// 齐次坐标是一个四元素 P = (x, y, z, w)。现在我们假设在规范化对称立方体.
        /// (-1 <= x, y, z <= 1) 的边界做裁剪,
        /// 点 P 满足如下不等式时该点位于规范化观察体内：
        /// -1 <= x / w <= 1, 
        /// -1 <= y / w <= 1, 
        /// -1 <= z / w <= 1
        /// 即 
        /// w > 0, -w <= x, y, z <= w，
        /// w < 0, w <= x, y, z <= -w，
        /// 暂且认为 w = mClipSpaceVertices[i][3] > 0
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public int ClipAndProject(int n)
        {
            mClipVerticesNumber = n;
            // 记录点所在的区域码
            uint clipOr = 0;
            // 所有点是否在裁剪区域外且为同一侧
            uint clipAnd = 0xffffffff;
            for (int i = 0; i < mClipVerticesNumber; i++)
            {
                uint clipmask = 0;
                //左平面 左边
                if (mClipSpaceVertices[i][0] < -mClipSpaceVertices[i][3])
                {
                    clipmask |= 1;
                }
                //右平面 右边
                if (mClipSpaceVertices[i][0] > mClipSpaceVertices[i][3])
                {
                    clipmask |= 2;
                }
                //底平面 下边
                if (mClipSpaceVertices[i][1] < -mClipSpaceVertices[i][3])
                {
                    clipmask |= 4;
                }
                //顶平面 上边
                if (mClipSpaceVertices[i][1] > mClipSpaceVertices[i][3])
                {
                    clipmask |= 8;
                }
                //近平面 前边
                if (mClipSpaceVertices[i][2] < -mClipSpaceVertices[i][3])
                {
                    clipmask |= 16;
                }
                //远平面 后边
                if (mClipSpaceVertices[i][2] > mClipSpaceVertices[i][3])
                {
                    clipmask |= 32;
                }
                clipOr |= clipmask;
                clipAnd &= clipmask;
            }
            // 同一侧且在裁剪区域外
            if (clipAnd > 0)
            {
                mClipVerticesNumber = 0;
                return 0;
            }
            // 不都在同一侧,即至少存在跨区域的两个点.需要做裁剪计算
            if (clipOr > 0)
            {
                if ((clipOr & 1) > 0)
                {
                    for (int i = 0; i < mClipVerticesNumber; i++)
                    {
                        mClipLerpFactor[i] = mClipSpaceVertices[i][3] + mClipSpaceVertices[i][0];
                    }
                    Clip();
                    if (mClipVerticesNumber < 3)
                    {
                        return 0;
                    }
                }
                if ((clipOr & 2) > 0)
                {
                    for (int i = 0; i < mClipVerticesNumber; i++)
                    {
                        mClipLerpFactor[i] = mClipSpaceVertices[i][3] - mClipSpaceVertices[i][0];
                    }
                    Clip();
                    if (mClipVerticesNumber < 3)
                    {
                        return 0;
                    }
                }
                if ((clipOr & 4) > 0)
                {
                    for (int i = 0; i < mClipVerticesNumber; i++)
                    {
                        mClipLerpFactor[i] = mClipSpaceVertices[i][3] + mClipSpaceVertices[i][1];
                    }
                    Clip();
                    if (mClipVerticesNumber < 3)
                    {
                        return 0;
                    }
                }
                if ((clipOr & 8) > 0)
                {
                    for (int i = 0; i < mClipVerticesNumber; i++)
                    {
                        mClipLerpFactor[i] = mClipSpaceVertices[i][3] - mClipSpaceVertices[i][1];
                    }
                    Clip();
                    if (mClipVerticesNumber < 3)
                    {
                        return 0;
                    }
                }
                if ((clipOr & 16) > 0)
                {
                    for (int i = 0; i < mClipVerticesNumber; i++)
                    {
                        mClipLerpFactor[i] = mClipSpaceVertices[i][3] + mClipSpaceVertices[i][2];
                    }
                    Clip();
                    if (mClipVerticesNumber < 3)
                    {
                        return 0;
                    }
                }
                if ((clipOr & 32) > 0)
                {
                    for (int i = 0; i < mClipVerticesNumber; i++)
                    {
                        mClipLerpFactor[i] = mClipSpaceVertices[i][3] - mClipSpaceVertices[i][2];
                    }
                    Clip();
                    if (mClipVerticesNumber < 3)
                    {
                        return 0;
                    }
                }
            }
            for (int i = 0; i < mClipVerticesNumber; i++)
            {
                // 透视除法
                float invh = 1 / mClipSpaceVertices[i][3];
                mClipSpaceVertices[i] = mClipSpaceVertices[i] * invh;
                // 视口变换 屏幕:左下角为原点(0, 0)
                mClipSpaceVertices[i][0] = (mClipSpaceVertices[i][0] + 1) * mScaleX;
                mClipSpaceVertices[i][1] = (mClipSpaceVertices[i][1] + 1) * mScaleY;
                mScreenSpaceVertices[i][0] = ((int)(mClipSpaceVertices[i][0])) | 1;
                mScreenSpaceVertices[i][1] = ((int)(mClipSpaceVertices[i][1])) | 1;
            }
            return mClipVerticesNumber;
        }

        private void Clip()
        {
            int j;
            int k = 0;
            for (int i = 0; i < mClipVerticesNumber; i++)
            {
                j = i + 1;
                if (j == mClipVerticesNumber)
                {
                    j = 0;
                }
                if (mClipLerpFactor[i] >= 0)
                {
                    if (mClipLerpFactor[j] < 0)
                    {
                        mClipOutVertices[k++] = Vector4.Lerp(mClipSpaceVertices[i], mClipSpaceVertices[j], mClipLerpFactor[i] / (mClipLerpFactor[i] - mClipLerpFactor[j]));
                    }
                }
                else if (mClipLerpFactor[j] >= 0)
                {
                    mClipOutVertices[k++] = Vector4.Lerp(mClipSpaceVertices[j], mClipSpaceVertices[i], mClipLerpFactor[j] / (mClipLerpFactor[j] - mClipLerpFactor[i]));
                }
            }
            mClipVerticesNumber = k;
            Vector4[] tmp = mClipSpaceVertices;
            mClipSpaceVertices = mClipOutVertices;
            mClipOutVertices = tmp;
        }

    }
}
