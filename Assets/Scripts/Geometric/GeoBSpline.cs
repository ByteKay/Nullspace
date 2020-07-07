/*
 
    另一种实现方式可参考： http://www.cnblogs.com/liuyunfeifei/articles/3110140.html
 *                      利用微分形式，建立方程组
 *                      利用追赶法，求解Ax = b  A为三对角矩阵
    
 *  另一种，使用 最小二乘法实现
 *                      N * x = b  ==》 Nt * N * x = Nt * b  而后Nt为N的转置矩阵
 *                      最后求逆矩阵可得解
 *                      
 *  BSpline Surface 暂不处理
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Nullspace
{
    enum KNOT_PARAMETER
    {
        Centripetal,
        Chord,
        Uniformly
    }

    enum KNOT_FUNCTION
    {
        Average,   
        Uniformly, 
        Universal
    }

    public class GeoBSpline2
    {
        int mK = 3;
        float mDelta = 0.01f;
        float mTotal = 0.0f;
        List<Vector2> mControlPnts;
        List<float> mKontPoints = new List<float>();
        public List<Vector2> mSamplers = new List<Vector2>();
        public GeoBSpline2()
        {
            mControlPnts = new List<Vector2>();
        }
        public GeoBSpline2(List<Vector2> contol)
        {
            mControlPnts = contol;
        }

        public int K
        {
            get
            {
                return mK;
            }
            set
            {
                mK = value;
            }
        }
        public float Delta
        {
            get
            {
                return mDelta;
            }
            set
            {
                mDelta = value;
            }
        }


        public void AddPoint(Vector2 p)
        {
            mControlPnts.Add(p);
        }

        public void CreateSamples()
        {
            float knot = 0.0f;
            Vector2 res = new Vector2(10f, 10f);
            while (knot < 1)
            {
                res = CalculatePoint(knot);
                if (res == Vector2.zero)
                    break;
                if (mSamplers.Count == 0 || mSamplers[mSamplers.Count - 1] != res)
                {
                    mSamplers.Add(res);
                }
                knot += mDelta;
            }
            res = CalculatePoint(1.0f);
            if (res != Vector2.zero && mSamplers[mSamplers.Count - 1] != res)
            {
                mSamplers.Add(res);
            }
        }

        private Vector2 CalculatePoint(float knot)
        {
            int count = 0;
            Vector2 v = new Vector2(0.0f, 0.0f);
            foreach (Vector2 it in mControlPnts)
            {
                float value = CalculateDeboorValue(count, mK, knot);
                count++;
                v += it * value;   
            }
            return v;
        }

        private float CalculateDeboorValue(int i, int k, float knot)
        {
            if (k != 0)
            {
                float lup = knot - mKontPoints[i];
                float ldown = mKontPoints[i + k] - mKontPoints[i];
                float l = 0.0f;
                if (ldown > 0 && lup > 0)
                {
                    l = lup / ldown * CalculateDeboorValue(i, k - 1, knot);
                }
                float rup = mKontPoints[i + 1 + k] - knot;
                float rdown = mKontPoints[i + 1 + k] - mKontPoints[i + 1];
                float r = 0.0f;
                if (rdown > 0 && rup > 0)
                {
                    r = rup / rdown * CalculateDeboorValue(i + 1, k - 1, knot);
                }
                return r + l;
            }
            else
            {
                if ((mKontPoints[i] <= knot) && (knot < mKontPoints[i + 1]))
                {
                    return 1.0000f;
                }
                else
                {
                    return 0.0000f;
                }
            }
        }

        public void CalculateKnots()
        {
            int count = mControlPnts.Count;
            if (count < mK + 1)
            {
                return;
            }
            // parameters t
            float[] distance = new float[count];
            distance[0] = 0;
            for (int i = 1; i < count; ++i)
            {
                float tmp = (mControlPnts[i] - mControlPnts[i - 1]).magnitude;
                distance[i] = distance[i - 1] + tmp;
                mTotal += tmp;
            }
            float rt = 1.0f / mTotal;
            for (int i = 1; i < count; ++i) 
            {
                distance[i] *= rt;
            }
            // knots
            mKontPoints.Clear();
            for (int i = 0; i < mK + 1; ++i)
            {
                mKontPoints.Add(0.0f);
            }     
            float avera = 1.0f / mK;
            for (int i = 1; i < count - mK; ++i)
            {
                float u = 0.0f;
                for (int j = i; j < i + mK; ++j)
                {
                    u += distance[j];
                }
                mKontPoints.Add(u * avera);
            }
            for (int i = count + 1; i < count + mK + 2; ++i)
            {
                mKontPoints.Add(1.0f);
            }
        }
    }

    public class GeoBSpline3
    {
        int mK = 3;
        float mDelta = 0.01f;
        List<Vector3> mControlPnts;
        List<float> mKontPoints = new List<float>();
        // KNOT_FUNCTION mKnotFunc = KNOT_FUNCTION.Average;
        public List<Vector3> mSamplers = new List<Vector3>();
        public GeoBSpline3()
        {
            mControlPnts = new List<Vector3>();
        }
        public GeoBSpline3(List<Vector3> contol)
        {
            mControlPnts = contol;
        }

        public int K
        {
            get
            {
                return mK;
            }
            set
            {
                mK = value;
            }
        }
        public float Delta
        {
            get
            {
                return mDelta;
            }
            set
            {
                mDelta = value;
            }
        }


        public void AddPoint(Vector3 p)
        {
            mControlPnts.Add(p);
        }

        public void CreateSamples()
        {
            float knot = 0.0f;
            Vector3 res = new Vector3(10f, 10f);
            while (res != Vector3.zero)
            {
                res = CalculatePoint(knot);
                if (res == Vector3.zero)
                    break;
                if (mSamplers.Count == 0 || mSamplers[mSamplers.Count - 1] != res)
                {
                    mSamplers.Add(res);
                }
                knot += mDelta;
                if (knot > 1.0f)
                {
                    break;
                }
            }
            res = CalculatePoint(1.0f);
            if (res != Vector3.zero && mSamplers[mSamplers.Count - 1] != res)
            {
                mSamplers.Add(res);
            }
        }

        private Vector3 CalculatePoint(float knot)
        {
            int count = 0;
            Vector3 v = new Vector3(0.0f, 0.0f);
            foreach (Vector3 it in mControlPnts)
            {
                float value = CalculateDeboorValue(count, mK, knot);
                v += it * value;
                count++;
            }
            return v;
        }

        private float CalculateDeboorValue(int i, int k, float knot)
        {
            if (k != 0)
            {
                float lup = knot - mKontPoints[i];
                float ldown = mKontPoints[i + k] - mKontPoints[i];
                float l = 0.0f;
                if (ldown > 0 && lup > 0)
                {
                    l = lup / ldown * CalculateDeboorValue(i, k - 1, knot);
                }
                float rup = mKontPoints[i + 1 + k] - knot;
                float rdown = mKontPoints[i + 1 + k] - mKontPoints[i + 1];
                float r = 0.0f;
                if (rdown > 0 && rup > 0)
                {
                    r = rup / rdown * CalculateDeboorValue(i + 1, k - 1, knot);
                }
                return r + l;
            }
            else
            {
                if ((mKontPoints[i] <= knot) && (knot < mKontPoints[i + 1]))
                {
                    return 1.0000f;
                }
                else
                {
                    return 0.0000f;
                }
            }
        }

        public void CalculateKnots()
        {
            int count = mControlPnts.Count;
            if (count < mK + 1)
            {
                return;
            }
            // parameters t
            float[] distance = new float[count];
            float total = 0.0f;
            distance[0] = 0;
            for (int i = 1; i < count; ++i)
            {
                float tmp = (mControlPnts[i] - mControlPnts[i - 1]).magnitude;
                distance[i] = distance[i - 1] + tmp;
                total += tmp;
            }
            float rt = 1.0f / total;
            for (int i = 1; i < count; ++i) 
            {
                distance[i] *= rt;
            }
            // knots
            mKontPoints.Clear();
            for (int i = 0; i < mK + 1; ++i)
            {
                mKontPoints.Add(0.0f);
            }     
            float avera = 1.0f / mK;
            for (int i = 1; i < count - mK; ++i)
            {
                float u = 0.0f;
                for (int j = i; j < i + mK; ++j)
                {
                    u += distance[j];
                }
                mKontPoints.Add(u * avera);
            }
            for (int i = count + 1; i < count + mK + 2; ++i)
            {
                mKontPoints.Add(1.0f);
            }
        }
    }
}
