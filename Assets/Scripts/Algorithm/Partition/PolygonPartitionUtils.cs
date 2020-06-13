
using System.Collections.Generic;
using Nullspace;
using UnityEngine;

namespace Partition
{
    // deprecated
    class PolygonPoint
    {
        public Vector2 mPoint;
        public Vector2 mMaxSize;
        public List<Vector2> mCache;
        public PolygonPoint(Vector2 point, Vector2 maxSize)
        {
            mPoint = point;
            mMaxSize = maxSize;
            mCache = new List<Vector2>();
            mCache.Add(maxSize);
        }

        public bool NotFound(float w, float h)
        {
            List<int> needRemoved = new List<int>();
            for (int i = 0; i < mCache.Count; ++i)
            {
                if (w >= mCache[i][0] && h >= mCache[i][1])
                {
                    return false;
                }
                if (w < mCache[i][0] && h < mCache[i][1])
                {
                    needRemoved.Add(i);
                }
            }
            for (int i = needRemoved.Count - 1; i >= 0; --i)
            {
                mCache.RemoveAt(needRemoved[i]);
            }
            needRemoved.Clear();
            mCache.Add(new Vector2(w, h));
            mCache.Sort((v1, v2) =>
            {
                if (v1[0] < v2[0])
                    return 1;
                if (v1[0] > v2[0])
                    return -1;
                return 0;
            });
            if (mMaxSize[0] != mCache[0].x)
            {
                mMaxSize[0] = mCache[0].x;
                mMaxSize[1] = mCache[0].y;
                return true;
            }
            return false;
        }
        public bool FastCheckCanFound(float w, float h)
        {
            int size = mCache.Count;
            if (size == 0)
                return false;
            if (w > mCache[0][0] && h > mCache[0][1])
            {
                return false;
            }
            int low = 0;
            int high = size;
            int mid;
            while (low < high)
            {
                mid = (low + high) >> 1;
                if (w < mCache[mid][0])
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid;
                }
            }
            if (low < size && h > mCache[low][1])
            {
                return false;
            }
            return true;
        }
        public static bool RayIntersect(Vector2 origin, Vector2 direction, List<GeoSegment2> segments, out float near, out float far)
        {
            GeoInsectPointInfo insect = new GeoInsectPointInfo();
            float minD = 1e10f;
            float maxD = -1e10f;
            bool flag = false;
            foreach (GeoSegment2 s in segments)
            {
                insect.mIsIntersect = false;
                if (GeoRayUtils.IsRayInsectSegment2(origin, direction, s.mP1, s.mP2, ref insect))
                {
                    flag = true;
                    if (insect.mLength < minD)
                    {
                        minD = insect.mLength;
                    }
                    if (insect.mLength > maxD)
                    {
                        maxD = insect.mLength;
                    }
                }
            }
            near = minD;
            far = maxD;
            return flag;
        }
        public static float RayIntersect(Vector2 origin, Vector2 direction, List<GeoSegment2> segments)
        {
            GeoInsectPointInfo insect = new GeoInsectPointInfo();
            float minD = 1e10f;
            bool flag = false;
            foreach (GeoSegment2 s in segments)
            {
                insect.mIsIntersect = false;
                if (GeoRayUtils.IsRayInsectSegment2(origin, direction, s.mP1, s.mP2, ref insect))
                {
                    if (insect.mLength < minD)
                    {
                        minD = insect.mLength;
                        flag = true;
                    }
                }
            }
            if (!flag)
                minD = -1;
            return minD;
        }
        public static string Key(Vector2 point)
        {
            return string.Format("{0}_{1}", point[0], point[1]);
        }
    }

    public class PolygonWrapper
    {
        // 带有空洞的多边形
        public static PolygonWrapper Create(List<Vector2> data, bool random, List<Vector4> removeEdge = null, float scale = 4.0f) // 单位之间间隔为 1 / scale
        {
            PolygonWrapper wrapper = new PolygonWrapper(data);
            wrapper.Initialize(scale);
            wrapper.InitializeSegmentsBvh(removeEdge);
            if (random)
            {
                wrapper.ShuffleCenterList();
            }
            return wrapper;
        }
        public static PolygonWrapper Create(List<Vector2> data, List<Vector2> innerBorder, bool random, float scale = 4.0f) // 单位之间间隔为 1 / scale
        {
            PolygonWrapper wrapper = new PolygonWrapper(data);
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

        private PolygonWrapper(List<Vector2> data)
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
                mSegmentsBvh = new BVHTree2D(tempObjects, 4);
            }
        }

        public void AddCondition(List<Vector2> polygon, bool exe = false)
        {
            if (polygon != null)
            {
                for (int i = 0; i < polygon.Count; ++i)
                {
                    int j = i + 1;
                    if (j == polygon.Count)
                        j = 0;
                    BVHSegment2Object seg = new BVHSegment2Object(polygon[i], polygon[j]);
                    mSegmentsBvh.AddObject(seg);
                }
                RemovePointsInPolygon(polygon);
            }
            if (exe)
            {
                mSegmentsBvh.Build();
            }
        }

        public void InitializeSegmentsBvh(List<Vector2> outer, List<Vector2> inner)
        {
            // if (!mIsConvex)
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
                mSegmentsBvh = new BVHTree2D(tempObjects, 4);
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
            bool isExist = ExistRectangle(w, h, out center);
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
                while (hs < he)
                {
                    if (ExistRectangle(wmid, hmid, out center))
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
        private bool ExistRectangle(float w, float h, out Vector2 center)
        {
            int count = mCenterList.Count;
            bool flag = false;
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
            //if (mIsConvex)
            //{
            //    bool isInconvex = GeoPolygonUtils.IsPointInConvexPolygon2(mPolygonData, ref max);
            //    if (!isInconvex)
            //        return false;
            //    isInconvex = GeoPolygonUtils.IsPointInConvexPolygon2(mPolygonData, ref min);
            //    if (!isInconvex)
            //        return false;
            //    isInconvex = GeoPolygonUtils.IsPointInConvexPolygon2(mPolygonData, ref p2);
            //    if (!isInconvex)
            //        return false;
            //    isInconvex = GeoPolygonUtils.IsPointInConvexPolygon2(mPolygonData, ref p4);
            //    if (!isInconvex)
            //        return false;
            //}
            //else
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

    public class PolygonPartitionUtils
    {
        private List<PolygonWrapper> mPolygonPartition;
        private PolygonCompare mCompare = new PolygonCompare();

        private static List<Vector2i> SizeList = new List<Vector2i>()
        {
            new Vector2i(20, 20),
            new Vector2i(18, 18),
            new Vector2i(16, 16),
            new Vector2i(14, 14),
            new Vector2i(12, 12),
            new Vector2i(10, 10),
            new Vector2i(8, 8),
            new Vector2i(6, 6),
            new Vector2i(5, 5),
            new Vector2i(4, 4),
            new Vector2i(3, 3),
            new Vector2i(2, 2), 
        };

        public int Count
        {
            get
            {
                return mPolygonPartition.Count;
            }
        }

        public PolygonWrapper this[int idx]
        {
            get
            {
                return mPolygonPartition[idx];
            }
        }

        public PolygonPartitionUtils()
        {
            mPolygonPartition = new List<PolygonWrapper>();
        }

        public void AddPolygon(PolygonWrapper poly)
        {
            mPolygonPartition.Add(poly);
        }

        public void AddPolygon(List<Vector2> data)
        {
            mPolygonPartition.Add(PolygonWrapper.Create(data, false));
        }

        public List<GeoAABB2> FindMax(bool recursive)
        {
            List<GeoAABB2> results = new List<GeoAABB2>();
            foreach (PolygonWrapper poly in mPolygonPartition)
            {
                poly.FindMaxBinary(recursive);
                if (poly.RectangleList.Count > 0)
                {
                    results.AddRange(poly.RectangleList);
                }
            }
            return results;
        }

        public void ResetPolygon()
        {
            foreach (PolygonWrapper poly in mPolygonPartition)
            {
                poly.Reset();
            }
        }

        public List<GeoAABB2> GetRect()
        {
            List<GeoAABB2> results = new List<GeoAABB2>();
            HashSet<Vector2i> tempSet = new HashSet<Vector2i>();
            int counter = 0;
            while (true)
            {
                counter = 0;
                for(int i = 0; i < SizeList.Count; ++i)
                {
                    Vector2i size = SizeList[i];
                    GeoAABB2 aabb = GetRect(size[0], size[1], i, ref tempSet);
                    if (aabb == null)
                    {
                        counter++;
                    }
                    else
                    {
                        results.Add(aabb);
                    }
                }
                if (counter == SizeList.Count)
                {
                    break;
                }
            }
            return results;
        }

        public GeoAABB2 GetRect(float w, float h, int i, ref HashSet<Vector2i> record)
        {
            GeoAABB2 rect = new GeoAABB2();
            for (int j = 0; j < mPolygonPartition.Count; ++j)
            {
                Vector2i key = Vector2i.GetVector2i(i, j);
                PolygonWrapper poly = mPolygonPartition[j];
                if (!record.Contains(key))
                {
                    if (poly.GetRectangle(w, h, ref rect))
                    {
                        return rect;
                    }
                    else
                    {
                        record.Add(key);
                    }
                }
            }
            return null;
        }

        public void SortPolygons()
        {
            mPolygonPartition.Sort(mCompare);
        }

        class PolygonCompare : IComparer<PolygonWrapper>
        {
            public int Compare(PolygonWrapper x, PolygonWrapper y)
            {
                return -x.MaxArea().CompareTo(y.MaxArea());
            }
        }
    }
}
