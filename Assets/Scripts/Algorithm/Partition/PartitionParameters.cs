
using System;
using System.Collections.Generic;
using Nullspace;
using UnityEngine;

namespace Partition
{
    public class PartitionParameters
    {
        public static PartitionParameters globalParamter = new PartitionParameters();

        public List<Vector2> mFourDirs;
        public List<Vector2> mHalfCircleDirs;
        public float posStep;
        public float lenStep;
        public float selfArea;
        public float maxArea;
        public float minArea;
        private float degree;
        public float interval;
        public PartitionParameters()
        {
            InitializeBase();
            InitializeFourDir();
            InitializeHalfCircleDir();
        }
        public void InitializeBase()
        {
            posStep = 0.4f;
            lenStep = 0.1f;
            maxArea = 0.95f;
            selfArea = 0.05f;
            minArea = 3.0f;
            degree = 5.0f;
            interval = 0.3f;
        }
        private void InitializeFourDir()
        {
            mFourDirs = new List<Vector2>();
            mFourDirs.Add(new Vector2(0, -1)); // down
            mFourDirs.Add(new Vector2(1, 0));  // right
            mFourDirs.Add(new Vector2(0, 1));  // top
            mFourDirs.Add(new Vector2(-1, 0)); // left
        }
        private void InitializeHalfCircleDir()
        {
            mHalfCircleDirs = new List<Vector2>();
            float rad = Mathf.Deg2Rad * degree;
            int count = (int)(180 / degree);
            double f0 = Math.Cos(rad);
            double f1 = -Math.Sin(rad);
            double f2 = -f1;
            double f3 = f0;
            Vector2 start = new Vector2(0.0000f, 1.0000f);
            mHalfCircleDirs.Add(start);
            for (int i = 0; i < count; ++i)
            {
                float d1 = (float)(f0 * start[0] + f1 * start[1]);
                float d2 = (float)(f2 * start[0] + f3 * start[1]);
                mHalfCircleDirs.Add(new Vector2(d1, d2));
                start[0] = d1;
                start[1] = d2;
            }
            int size = mHalfCircleDirs.Count;
            for (int i = 0; i < size; ++i)
            {
                mHalfCircleDirs.Add(-mHalfCircleDirs[i]);
            }
        }
    }
}
