
using System.Collections.Generic;
using UnityEngine;
using Nullspace;


namespace Partition
{
    public class PolygonWrapperRotation
    {
        // 带有空洞的多边形
        public static PolygonWrapperRotation Create(List<Vector2> data, bool random, List<Vector4> removeEdge = null, float scale = 4.0f) // 单位之间间隔为 1 / scale
        {
            PolygonWrapperRotation wrapper = new PolygonWrapperRotation(data);
            wrapper.Initialize(scale);
            wrapper.InitializeSegmentsBvh(removeEdge);
            if (random)
            {
                wrapper.ShuffleCenterList();
            }
            return wrapper;
        }
        public static PolygonWrapperRotation Create(List<Vector2> data, List<Vector2> innerBorder, bool random, float scale = 4.0f) // 单位之间间隔为 1 / scale
        {
            PolygonWrapperRotation wrapper = new PolygonWrapperRotation(data);
            wrapper.Initialize(scale);
            wrapper.InitializeSegmentsBvh(data, innerBorder);
            if (innerBorder != null)
            {
                wrapper.RemovePointsInPolygon(innerBorder);
            }
            if (random)
            {
                wrapper.ShuffleCenterList();
            }
            return wrapper;
        }
        private void RemovePointsInPolygon(List<Vector2> innerPoly)
        {
            List<Vector2> temp = new List<Vector2>();
            temp.AddRange(innerPoly);
            GeoPolygonUtils.ReverseIfCW(ref temp);
            GeoPointsArray2 poly = new GeoPointsArray2(temp);
            List<int> removeIndex = new List<int>();
            for (int i = 0; i < mCenterList.Count; ++i)
            {
                Vector2 t = mCenterList[i];
                if (GeoPolygonUtils.IsPointInPolygon2(poly, ref t))
                {
                    removeIndex.Add(i);
                }
            }
            for (int i = removeIndex.Count - 1; i >= 0; --i)
            {
                mCenterList.RemoveAt(removeIndex[i]);
            }
        }
        private PolygonWrapperRotation(List<Vector2> data)
        {
            mPolygonData = new GeoPointsArray2(data);
            mCenterList = new List<Vector2>();
            mRectangleList = new List<GeoAABB2>();
            mOccupied = 0.0f;
            mScale = 4.0f;
        }
        public void ShuffleCenterList()
        {
            mCenterList = RandomUtils.RandomShuffle(mCenterList);
        }
        public float Proportion(int count)
        {
            return count * 1.0f / mCenterList.Count;
        }
        public void InitializeSegmentsBvh(List<Vector4> removeEdge)
        {
            if (!mIsConvex && mPolygonData.Count > 8)
            {
                List<BVHObject2> tempObjects = new List<BVHObject2>();
                for (int i = 0; i < mPolygonData.Count; ++i)
                {
                    int j = i + 1;
                    if (j == mPolygonData.Count)
                        j = 0;
                    
                    if (removeEdge != null)
                    {
                        Vector4 v1 = new Vector4(mPolygonData[i][0], mPolygonData[i][1], mPolygonData[j][0], mPolygonData[j][1]);
                        int index = removeEdge.FindIndex((Vector4 v) =>
                            {
                                return v == v1;
                            });
                        if (index != -1)
                            continue;
                        Vector4 v2 = new Vector4(mPolygonData[j][0], mPolygonData[j][1], mPolygonData[i][0], mPolygonData[i][1]);
                        index = removeEdge.FindIndex((Vector4 v) =>
                        {
                            return v == v2;
                        });
                        if (index != -1)
                            continue;
                    }   
                    tempObjects.Add(new BVHSegment2Object(mPolygonData[i], mPolygonData[j]));
                }
                mSegmentsBvh = new BVHTree2D(tempObjects, 1);
            }
        }
        public void InitializeSegmentsBvh(List<Vector2> outer, List<Vector2> inner)
        {
            if (!mIsConvex)
            {
                List<BVHObject2> tempObjects = new List<BVHObject2>();
                for (int i = 0; i < outer.Count; ++i)
                {
                    int j = i + 1;
                    if (j == outer.Count)
                        j = 0;
                    tempObjects.Add(new BVHSegment2Object(outer[i], outer[j]));
                }
                if (inner != null)
                {
                    for (int i = 0; i < inner.Count; ++i)
                    {
                        int j = i + 1;
                        if (j == inner.Count)
                            j = 0;
                        tempObjects.Add(new BVHSegment2Object(inner[i], inner[j]));
                    }
                }
                mSegmentsBvh = new BVHTree2D(tempObjects, 1);
            }
        }
        public void Initialize(float scale)
        {
            mScale = scale;
            mArea = GeoPolygonUtils.CalcualetArea(mPolygonData);
            if (mArea < 0)
            {
                mPolygonData.Reverse();
                mArea = -mArea;
            }
            mAABB = GeoPolygonUtils.CalculateAABB(mPolygonData);
            mMaxSize = mAABB.mMax - mAABB.mMin;
            mIsConvex = IsAllConvex();
            Decompose();
            mLeftRectangle.Clear();
        }
        public bool IsFull()
        {
            return Occupied() > PartitionParameters.globalParamter.maxArea;
        }
        public float Area
        {
            get
            {
                return mArea;
            }
        }
        public bool GetRectangle(float w, float h, ref GeoAABB2 rect)
        {
            if (IsFull())
            {
                return false;
            }
            Vector2 center;
            float angle;
            bool isExist = ExistRectangle(w, h, out angle, out center);
            if (isExist)
            {
                Vector2 temp = new Vector2(w * 0.5f, h * 0.5f);
                rect.mMin = center - temp;
                rect.mMax = center + temp;
                AddRectangle(ref rect);
            }
            return isExist;
        }

        int mMaxDepth = 1000;
        public void FindMaxBinary(bool recursive)
        {
            float ws = 0.3f;
            float we = mMaxSize[0];
            Vector2 center = new Vector2();
            Vector2 maxCenter = new Vector2();
            Vector2 maxSize = new Vector2();
            float maxArea = -1.0f;
            
            while (ws < we)
            {
                float wmid = (ws + we) * 0.5f;
                float hs = 0.3f;
                float he = mMaxSize[1];
                bool flag = false;
                Vector2 lastCenter = new Vector2();
                float hmid = (hs + he) * 0.5f;
                float angle;
                while (hs < he)
                {
                    if (ExistRectangle(wmid, hmid, out angle, out center))
                    {
                        flag = true;
                        lastCenter = center;
                        hs = hmid + 0.01f;
                    }
                    else
                    {
                        he = hmid - 0.01f;
                    }
                    hmid = (hs + he) * 0.5f;
                }
                if (flag)
                {
                    if (wmid * hmid > maxArea)
                    {
                        maxArea = wmid * hs;
                        maxCenter = lastCenter;
                        maxSize[0] = wmid;
                        maxSize[1] = hmid;
                    }
                    ws = wmid + 0.01f;
                }
                else
                {
                    we = wmid - 0.01f;
                }
            }
            if (maxArea > 0)
            {
                bool flag = false;
                if ((maxArea / mArea) > PartitionParameters.globalParamter.selfArea)
                {
                    GeoAABB2 maxRect = new GeoAABB2(maxCenter - maxSize * 0.5f, maxCenter + maxSize * 0.5f);
                    mMaxSize = maxSize;
                    float total = AddRectangle(ref maxRect);
                    flag = recursive && total < PartitionParameters.globalParamter.maxArea;
                }
                if (flag)
                {
                    mMaxDepth--;
                    if (mMaxDepth > 0)
                        FindMaxBinary(recursive);
                }
            }
        }
        public bool AddRectIfCan(ref GeoAABB2 rect, bool needCheck)
        {
            bool can = true;
            if (needCheck)
            {
                Vector2 size = rect.mMax - rect.mMin;
                can = CheckRectangleValid(rect.mMin, rect.mMax, size[0], size[1]);
            }
            if (can)
            {
                AddRectangle(ref rect);
            }
            return can;
        }
        public List<GeoAABB2> RectangleList
        {
            get
            {
                return mRectangleList;
            }
        }
        public float MaxArea()
        {
            return mMaxSize[0] * mMaxSize[1];
        }
        public void Reset()
        {
            mRectangleList.Clear();
            mCenterList.Clear();
            mOccupied = 0.0f;
            mMaxSize = mAABB.mMax - mAABB.mMin;
            Decompose();
            mLeftRectangle = new List<GeoAABB2>();
            mBvh = new BVHTree2D();
        }
        #region bvh
        private BVHTree2D mBvh = new BVHTree2D();
        private BVHTree2D mSegmentsBvh = null;
        private List<GeoAABB2> mLeftRectangle = new List<GeoAABB2>();
        private int mMaxLeftCount = 6;
        #endregion
        #region private member datas and methods
        public List<Vector2> mCenterList;
        private List<GeoAABB2> mRectangleList;
        private float mOccupied;
        public GeoPointsArray2 mPolygonData;
        private float mArea;
        private GeoAABB2 mAABB;
        private Vector2 mMaxSize;
        private bool mIsConvex;
        private float mScale;
        private bool IsAllConvex()
        {
            return GeoPolygonUtils.IsConvexPolygon2(mPolygonData);
        }
        private float AddRectangle(ref GeoAABB2 rect)
        {
            Vector2 temp = rect.mMax - rect.mMin;
            float area = temp[0] * temp[1];
            mOccupied += area;
            mRectangleList.Add(rect);     
            bool isbeyond = mLeftRectangle.Count > mMaxLeftCount;
            mBvh.AddObject(new BVHAABB2Object(rect.mMin, rect.mMax), isbeyond);
            if (isbeyond)
            {
                mLeftRectangle.Clear();
            }
            else
            {
                mLeftRectangle.Add(rect);
            }
            RemovePointsInRectangle(rect);
            return mOccupied / mArea;
        }


        private bool ExistRectangle(float w, float h, out float angle, out Vector2 center)
        {
            int count = mCenterList.Count;
            bool flag = false;
            angle = 0.0f;
            center = new Vector2();
            for (int i = 0; i < count; ++i)
            {
                Vector2 diag = new Vector2(w, h);
                Vector2 min = mCenterList[i] - diag * 0.5f;
                Vector2 max = mCenterList[i] + diag * 0.5f;
                flag = CheckRectangleValid(min, max, w, h);
                if (flag)
                {
                    center = mCenterList[i];
                    return true;
                }
            }
            return flag;
        }

        private bool CheckRectangleValid(Vector2 min, Vector2 max, float w, float h)
        {
            Vector2 p4 = new Vector2(min[0], max[1]);
            Vector2 p2 = new Vector2(max[0], min[1]);
            // 点都在polygon 内部  // 使用 bvh 判断
            if (mIsConvex)
            {
                bool isInconvex = GeoPolygonUtils.IsPointInConvexPolygon2(mPolygonData, ref max);
                if (!isInconvex)
                    return false;
                isInconvex = GeoPolygonUtils.IsPointInConvexPolygon2(mPolygonData, ref min);
                if (!isInconvex)
                    return false;
                isInconvex = GeoPolygonUtils.IsPointInConvexPolygon2(mPolygonData, ref p2);
                if (!isInconvex)
                    return false;
                isInconvex = GeoPolygonUtils.IsPointInConvexPolygon2(mPolygonData, ref p4);
                if (!isInconvex)
                    return false;
            }
            else
            {
                if (mSegmentsBvh != null)
                {
                    bool insect = mSegmentsBvh.TestIntersection(new GeoAABB2(min, max));
                    if (insect)
                        return false;
                }
                else
                {
                    bool isInconvex = GeoPolygonUtils.IsPointInPolygon2(mPolygonData, ref max);
                    if (!isInconvex)
                        return false;
                    isInconvex = GeoPolygonUtils.IsPointInPolygon2(mPolygonData, ref min);
                    if (!isInconvex)
                        return false;
                    isInconvex = GeoPolygonUtils.IsPointInPolygon2(mPolygonData, ref p2);
                    if (!isInconvex)
                        return false;
                    isInconvex = GeoPolygonUtils.IsPointInPolygon2(mPolygonData, ref p4);
                    if (!isInconvex)
                        return false;
                }
            }
            // 矩形与矩形是否有交叉
            bool isInsectAABB = IsIntersectOtherAABB(min, max);
            if (isInsectAABB)
                return false;
            return true;
        }
        private bool IsIntersectOtherAABB(Vector2 v, Vector2 vm)
        {
            foreach (GeoAABB2 aabb in mLeftRectangle)
            {
                if (GeoAABBUtils.IsAABBInsectAABB2(v, vm, aabb.mMin, aabb.mMax))
                {
                    return true;
                }
            }
            return mBvh.TestIntersection(new GeoAABB2(v, vm));
        }
        private void Decompose()
        {
            List<List<Vector2>> tempCenters = new List<List<Vector2>>();
            if (mIsConvex)
            {
                tempCenters = ConvexPolygonScanning.DrawPolygonTranslate(mPolygonData.mPointArray, mScale);
            }
            else
            {
                tempCenters = PolygonScanning.DrawPolygonScale(mPolygonData.mPointArray, mScale);
            }
            foreach (List<Vector2> vv in tempCenters)
            {
                foreach (Vector2 v in vv)
                {
                    Vector2 p = v;
                    if (mIsConvex && GeoPolygonUtils.IsPointInConvexPolygon2(mPolygonData, ref p))
                    {
                        mCenterList.Add(p);
                    }
                    else if (!mIsConvex && GeoPolygonUtils.IsPointInPolygon2(mPolygonData, ref p))
                    {
                        mCenterList.Add(p);
                    }
                }
            }
        }
        private float Occupied()
        {
            return mOccupied / mArea;
        }
        private void RemovePointsInRectangle(GeoAABB2 rect)
        {
            List<int> needRemoved = new List<int>();
            for (int i = 0; i < mCenterList.Count; ++i)
            {
                Vector2 v = mCenterList[i];
                if (GeoAABBUtils.IsPointInAABB2(rect.mMin, rect.mMax, ref v))
                {
                    needRemoved.Add(i);
                }
            }
            for (int i = needRemoved.Count - 1; i >= 0; --i)
            {
                mCenterList.RemoveAt(needRemoved[i]);
            }
        }
        #endregion
    }
}
