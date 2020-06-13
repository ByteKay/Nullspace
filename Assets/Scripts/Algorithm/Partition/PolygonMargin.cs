
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Nullspace;
/*
 * 边缘的填充：先选择区域，再选择物件
 * 其他区域： 先选择物件， 再选择区域
 */
namespace Partition
{
    public class MarginParameter
    {
        public const float mRes = 0.1f;
        public const float mAngle1 = 5;
        public const float mAngle2 = 5;
        public const float mExpandLen = 2.0f;
        public const float mMinExpandLen = 0.5f;
        public const float mBinaryAngle = 10;

        public const float mInterval = 0.2f;
        public const float mStartOffset = 0.1f;
        public const int mInfinite = 100000;

        public const float mMarginLenThreshold = 1.0f;
        public const float mMarginMax = 2.0f;
        public const float mMarginMin = 0.5f;
        public const float mMarginMinAngle = 30f;

        public const float mMarginIntersectProportion = 0.3f;
    }
    public class MarginPoint
    {
        public int mRectIndex;
        public Vector2 mPoint;          // 点
        public bool isConvex;           // 是否是凸角 
        public Vector2 mDirection;      // 两垂直线的平分线
        public float mAngle;            // 角平分线到两向量的夹角
    }
    public class MarginPointPriorityCompare : IComparer<MarginPointPriority>
    {
        public int Compare(MarginPointPriority a, MarginPointPriority b)
        {
            int result = -a.mPriority.CompareTo(b.mPriority);
            if (result == 0)
            {
                int aw = (int)(a.mSize[0] + a.mSize[1]) * 100;
                int ah = (int)(a.mSize[2] + a.mSize[3]) * 100;
                int bw = (int)(b.mSize[0] + b.mSize[1]) * 100;
                int bh = (int)(b.mSize[2] + b.mSize[3]) * 100;
                int aa = aw * ah;
                int ba = bw * bh;
                result = -aa.CompareTo(ba);
                if (result == 0)
                {
                    aw = (int)(a.mSize[0] - a.mSize[1]) * 100;
                    ah = (int)(a.mSize[2] - a.mSize[3]) * 100;
                    bw = (int)(a.mSize[0] - a.mSize[1]) * 100;
                    bh = (int)(a.mSize[2] - a.mSize[3]) * 100;
                    aw = Math.Abs(aw);
                    ah = Math.Abs(ah);
                    bw = Math.Abs(bw);
                    bh = Math.Abs(bh);
                    aa = aw * ah;
                    ba = bw * bh;
                    // 越小越靠前
                    result = aa.CompareTo(ba);
                }
            }
            return result;
        }
    }
    public class MarginPointPriority
    {
        public Vector2 mPoint;
        public Vector2 mYDirection;
        public Vector2 mXDirection;
        public Vector4 mSize = new Vector4(); // 0 x+ 1 x- 2 y+ 3 y- // 计算 交点，没交点为 10000000，表示无穷。 主要为 左 和 右 宽
        public int mPriority; // 每个点共享的次数.  MarginPoint 优先级最高  。先使用，并且越要靠近边缘
        public bool mMarked = false;

        public bool CheckOther(MarginPointPriority other, float threshold)
        {
            if ((mPoint - other.mPoint).sqrMagnitude < threshold)
            {
                mPriority++;
                mYDirection += other.mYDirection;
                mPoint += other.mPoint;
                mPoint *= 0.5f;
                return true;
            }
            return false;
        }
    }
    //********************************************************
    public enum MarginPointType
    {
        MP_CORNER,
        MP_RECT,
        MP_ANGLE
    }
    public class SegmentsIntersectCheck
    {
        List<GeoSegment2> mSegments;
        public SegmentsIntersectCheck(List<Vector2> poly)
        {
            mSegments = new List<GeoSegment2>();
            for (int i = 0; i < poly.Count; ++i)
            {
                int j = i + 1;
                j = j == poly.Count ? 0 : j;
                mSegments.Add(new GeoSegment2(poly[i], poly[j]));
            }
        }

        public GeoInsectPointInfo CheckIntersect(Vector2 origin, Vector2 direction)
        {
            float minLen = 1e10f;
            bool flag = false;
            GeoInsectPointInfo current = new GeoInsectPointInfo();
            foreach (GeoSegment2 seg in mSegments)
            {
                current.Clear();
                if (GeoRayUtils.IsRayInsectSegment2(origin, direction, seg.mP1, seg.mP2, ref current) && current.mLength < minLen)
                {
                    minLen = current.mLength;
                    flag = true;
                }
            }
            GeoInsectPointInfo info = new GeoInsectPointInfo();
            info.mLength = minLen;
            info.mIsIntersect = flag;
            return info;
        }
    }
    public class AbstractMargin
    {
        public MarginPointType mType;
        public Vector2 mXDirection;
        public Vector2 mYDirection;
        public int mPriority;
        protected bool isOuterMargin;
        protected Quaternion mQuaternion;
        public AbstractMargin()
        {

        }
        public AbstractMargin(MarginPointType type, int priority, Vector2 xdir, Vector2 ydir, bool isOuter = true)
        {
            mType = type;
            mXDirection = xdir;
            mYDirection = ydir;
            mPriority = priority;
            isOuterMargin = isOuter;
        }
        public Quaternion Q
        {
            get
            {
                return mQuaternion;
            }
        }
        public virtual void GeneratorPoint(GeoPointsArray2 poly)
        {

        }
        public virtual void CalculateSize(BVHTree2D bvh)
        {

        }
        public virtual void CalculateSize(SegmentsIntersectCheck bvh)
        {

        }
        public virtual void DebugDraw(Color clr)
        {

        }

        public virtual List<MarginSplitUnit> Split(int index, float min, float max)
        {
            return null;
        }
    }
    public class RectMarginPoint : AbstractMargin
    {
        public Vector2 mStartPoint;
        public Vector2 mEndPoint;
        public float mLength;
        List<Vector2> mSamplers = new List<Vector2>();
        List<Vector4> mSamplersSize = new List<Vector4>();
        bool isGreed = true;
        public int Count
        {
            get
            {
                return mSamplers.Count;
            }
        }
        public Vector2 Pos(int index)
        {
            return mSamplers[index];
        }
        public Vector4 Size(int index)
        {
            return mSamplersSize[index];
        }
        public RectMarginPoint(Vector2 start, Vector2 end, float len, int priority, Vector2 xdir, Vector2 ydir, bool isOuter = true)
            : base(MarginPointType.MP_RECT, priority, xdir, ydir, isOuter)
        {
            mStartPoint = start;
            mEndPoint = end;
            mLength = len;
            mQuaternion = Quaternion.FromToRotation(Vector3.forward, new Vector3(-mYDirection[0], 0.0f, -mYDirection[1]));
        }
        public Vector2 Size(int samplerIndex, float max, float min)
        {
            float total = 0.0f;
            Vector4 size0 = Size(samplerIndex);
            Vector2 size = new Vector2(size0[0], size0[2]);
            if (size[0] > max)
            {

            }
            for (int i = samplerIndex + 1; samplerIndex < Count; ++i)
            {
                if (total > max)
                    break;
                size0 = Size(i);
                size = Vector2.Min(size0, size);
            }
            return size;
        }
        public override void DebugDraw(Color clr)
        {
            //GeoDebugDrawUtils.DrawLine(mStartPoint, mStartPoint + mYDirection, clr);
            for (int i = 0; i < mSamplers.Count; ++i)
            {
                //GeoDebugDrawUtils.DrawLine(mSamplers[i], mSamplers[i] + mXDirection * mSamplersSize[i][0], clr);
                //GeoDebugDrawUtils.DrawLine(mSamplers[i], mSamplers[i] - mXDirection * mSamplersSize[i][1], clr);
                GeoDebugDrawUtils.DrawLine(mSamplers[i], mSamplers[i] + mYDirection * mSamplersSize[i][2], clr);
                //GeoDebugDrawUtils.DrawLine(mSamplers[i], mSamplers[i] - mYDirection * mSamplersSize[i][3], clr);
            }
            //GeoDebugDrawUtils.DrawLine(mEndPoint, mEndPoint + mYDirection, clr);
        }
        public override void GeneratorPoint(GeoPointsArray2 poly)
        {
            Vector2 ystep = MarginParameter.mStartOffset * mYDirection;
            Vector2 xstep = MarginParameter.mInterval * mXDirection;
            Vector2 start = mStartPoint + ystep;
            mSamplers.Add(start);
            start += xstep;
            float increase = MarginParameter.mInterval;
            while (increase < mLength && (
                (isOuterMargin && !GeoPolygonUtils.IsPointInPolygon2(poly, ref start)) // 外部 在外面
                || (!isOuterMargin && GeoPolygonUtils.IsPointInPolygon2(poly, ref start)) // 内部 在里面
                )
                )
            {
                mSamplers.Add(start);
                start += xstep;
                increase += MarginParameter.mInterval;
            }
            mSamplers.Add(mEndPoint + ystep);
        }
        public override void CalculateSize(SegmentsIntersectCheck bvh)
        {
            if (mSamplers.Count > 0)
            {
                for (int i = 0; i < mSamplers.Count; ++i)
                {
                    Vector4 size = new Vector4(0, 0, 0, MarginParameter.mStartOffset);
                    GeoInsectPointInfo info = bvh.CheckIntersect(mSamplers[i], mXDirection);
                    if (info.mIsIntersect)
                    {
                        size[0] = info.mLength;
                    }
                    else
                    {
                        size[0] = MarginParameter.mInfinite;
                    }
                    info = bvh.CheckIntersect(mSamplers[i], -mXDirection);
                    if (info.mIsIntersect)
                    {
                        size[1] = info.mLength;
                    }
                    else
                    {
                        size[1] = MarginParameter.mInfinite;
                    }
                    info = bvh.CheckIntersect(mSamplers[i], mYDirection);
                    if (info.mIsIntersect)
                    {
                        size[2] = info.mLength;
                    }
                    else
                    {
                        size[2] = MarginParameter.mInfinite;
                    }
                    mSamplersSize.Add(size);
                }
            }
        }
        public override void CalculateSize(BVHTree2D bvh)
        {
            if (mSamplers.Count > 0)
            {
                GeoRay2 ray = new GeoRay2(Vector2.zero, Vector2.zero);
                for (int i = 0; i < mSamplers.Count; ++i)
                {
                    Vector4 size = new Vector4(0, 0, 0, MarginParameter.mStartOffset);
                    ray.mOrigin = mSamplers[i];
                    ray.mDirection = mXDirection;
                    GeoInsectPointArrayInfo info = new GeoInsectPointArrayInfo();
                    if (bvh.GetIntersection(ray, ref info, false))
                    {
                        size[0] = info.mLength;
                    }
                    else
                    {
                        size[0] = MarginParameter.mInfinite;
                    }
                    info.Clear();
                    ray.mDirection = -mXDirection;
                    info = new GeoInsectPointArrayInfo();
                    if (bvh.GetIntersection(ray, ref info, false))
                    {
                        size[1] = info.mLength;
                    }
                    else
                    {
                        size[1] = MarginParameter.mInfinite;
                    }
                    info.Clear();
                    ray.mDirection = mYDirection;
                    info = new GeoInsectPointArrayInfo();
                    if (bvh.GetIntersection(ray, ref info, false))
                    {
                        size[2] = info.mLength;
                    }
                    else
                    {
                        size[2] = MarginParameter.mInfinite;
                    }
                    info.Clear();
                    mSamplersSize.Add(size);
                }
            }
        }
        public override List<MarginSplitUnit> Split(int index, float min, float max)
        {
            List<MarginSplitUnit> temp = new List<MarginSplitUnit>();
            if (mLength < min)
            {
                MarginSplitUnit unit = new MarginSplitUnit();
                unit.mRectIndex = index;
                unit.mPointStartIndex = 0;
                unit.mPointEndIndex = mSamplers.Count - 1;
                float sizeMin = Size(0, mSamplers.Count - 1);
                unit.mSize = new Vector2(mLength, Math.Min(sizeMin, min));
                temp.Add(unit);
            }
            else
            {
                // 如果存在某些点，y轴方向能到达的距离很短，可能要进行分段处理
                int start = 0;
                // 类似 邻接矩阵
                List<List<MarginSplitUnit>> adjacents = new List<List<MarginSplitUnit>>();
                for (int i = start; i < Count - 1; ++i)
                {
                    List<MarginSplitUnit> row = new List<MarginSplitUnit>();
                    for (int j = i + 1; j < Count; ++j)
                    {
                        Vector2 pi = Pos(i);
                        Vector2 pj = Pos(j);
                        float len = (pj - pi).magnitude;
                        // 边太短
                        if ((len < min && j < Count - 1))
                        {
                            continue;
                        }
                        // 计算合适的大小
                        MarginSplitUnit unit = new MarginSplitUnit();
                        unit.mRectIndex = index;
                        unit.mPointStartIndex = i;
                        unit.mPointEndIndex = j;
                        unit.mSize = Size(i, j, len);
                        if (unit.mSize[1] > max)
                        {
                            unit.mSize[1] = max;
                        }
                        row.Add(unit);
                        // 边太长
                        if (len > max)
                        {
                            break;
                        }
                        // 如果存在太大的变化，跳出
                        if (Size(j)[2] > Size(i)[2] * 2.0f)
                        {
                            break;
                        }
                    }
                    if (isGreed && row.Count > 0)
                    {
                        float maxArea = -1e10f;
                        int maxIndex = -1;
                        for (int k = 0; k < row.Count; ++k)
                        {
                            float area = row[k].Area;
                            if (area > maxArea)
                            {
                                maxArea = area;
                                maxIndex = k;
                            }
                        }
                        var ti = row[maxIndex];
                        row.Clear();
                        row.Add(ti);
                        int next = ti.Next();
                        i = next - 1; // 设置 i 的值为后一个
                    }
                    adjacents.Add(row);
                }
                if (!isGreed)
                {
                    List<List<MarginSplitUnit>> travelled = new List<List<MarginSplitUnit>>();
                    List<MarginSplitUnit> travel = new List<MarginSplitUnit>();
                    GeoAlgorithmUtils.AdjacentGraphTravel(adjacents, travelled, travel);
                    float maxArea = -1;
                    int maxIndex = -1;
                    for (int i = 0; i < travelled.Count; ++i)
                    {
                        float area = 0.0f;
                        foreach (MarginSplitUnit unit in travelled[i])
                        {
                            area += unit.Area;
                        }
                        if (area > maxArea)
                        {
                            maxArea = area;
                            maxIndex = i;
                        }
                    }
                    if (maxIndex >= 0 && maxIndex < travelled.Count)
                    {
                        temp.AddRange(travelled[maxIndex]);
                    }
                }
                else
                {
                    temp.Clear();
                    foreach (List<MarginSplitUnit> adj in adjacents)
                    {
                        if (adj.Count == 1) // 只保留最大的那一个
                        {
                            temp.Add(adj[0]);
                        }
                    }
                }
            }
            foreach (MarginSplitUnit unit in temp)
            {
                if (unit.mSize[1] > unit.mSize[0])
                {
                    unit.mSize[1] = unit.mSize[0];
                }
                unit.InitializeRect(mSamplers[unit.mPointStartIndex], mXDirection, mYDirection);
            }
            return temp;
        }
        private Vector2 Size(int i, int j, float len)
        {
            // 此处的计算，还需要根据之间的值来算
            float min = Size(i)[2];
            for (int k = i + 1; k <= j; ++k)
            {
                min = Math.Min(Size(k)[2], min);
            }
            return new Vector2(len, min);
        }
        private float Size(int i, int j)
        {
            // 只计算首尾两个
            Vector4 si = Size(i);
            Vector4 sj = Size(j);
            float min = Math.Min(si[2], sj[2]);
            return min;
        }
    }
    public class CornerMarginPoint : AbstractMargin
    {
        public Vector2 mPoint;
        public float mAngle;
        public Vector2 mBinaryDir;

        public Vector2 mGeneratorPos;
        public Vector4 mSize;

        public CornerMarginPoint(Vector2 p, float angle, int priority, Vector2 xdir, Vector2 ydir)
            : base(MarginPointType.MP_CORNER, priority, xdir, ydir)
        {
            mPoint = p;
            mAngle = angle;
            mBinaryDir = (xdir + ydir).normalized;
            if (mAngle > 180)
            {
                mBinaryDir = -mBinaryDir;
            }
            mQuaternion = Quaternion.FromToRotation(Vector3.forward, new Vector3(-mBinaryDir[0], 0.0f, -mBinaryDir[1]));
        }

        public override void DebugDraw(Color clr)
        {
            GeoDebugDrawUtils.DrawLine(mPoint, mPoint + mBinaryDir, clr);
        }
        public override void GeneratorPoint(GeoPointsArray2 poly)
        {
            mSize = new Vector4();
            float scale = 0.25f;
            mGeneratorPos = mPoint + scale * mBinaryDir;
            float threshold = 0.001f;
            while (scale > threshold && GeoPolygonUtils.IsPointInPolygon2(poly, ref mGeneratorPos))
            {
                scale = scale * 0.8f;
                mGeneratorPos = mPoint + scale * mBinaryDir;
            }
            if (scale < threshold)
            {
                throw new Exception("need ajust");
                // scale = MarginParameter.mStartOffset;
            }
            mSize[3] = scale;
        }
        public override void CalculateSize(SegmentsIntersectCheck bvh)
        {
            CalculateSize();
            GeoInsectPointInfo info = bvh.CheckIntersect(mGeneratorPos, mBinaryDir);
            if (info.mIsIntersect)
            {
                mSize[2] = info.mLength;
            }
            else
            {
                mSize[2] = MarginParameter.mInfinite;
            }
        }
        public override void CalculateSize(BVHTree2D bvh)
        {
            CalculateSize();
            GeoRay2 ray = new GeoRay2(mGeneratorPos, Vector2.zero);
            ray.mDirection = mBinaryDir;
            GeoInsectPointArrayInfo info = new GeoInsectPointArrayInfo();
            if (bvh.GetIntersection(ray, ref info, false))
            {
                mSize[2] = info.mLength;
            }
            else
            {
                mSize[2] = MarginParameter.mInfinite;
            }

        }
        private void CalculateSize()
        {
            float angle = mAngle * 0.5f;
            if (mAngle > 180)
            {
                angle = (360 - mAngle) * 0.5f;
            }
            angle = angle / 180.0f;
            float sin = Mathf.Sin(angle);
            sin = mSize[3] * sin;
            mSize[0] = sin;
            mSize[1] = sin;
        }
    }
    //*************************************************************************
    public enum AngleType
    {
        LESS180,
        GREATER180
    }
    public class AbstractAngle
    {
        public int mIndex;
        public float mAngle;
        public Vector2 mAnglePoint;
        public Vector2 mPreviousPoint;
        public Vector2 mNextPoint;
        public AngleType mAngleTyle;
        public Vector2 mBinaryDir;

        protected float mPreviousLength;
        protected float mNextLength;
        protected Vector2 mPreviousDir;
        protected Vector2 mNextDir;

        public Vector2 mPreNew;
        public Vector2 mAngleNew;
        public Vector2 mNextNew;
        public float mOffset;
        protected bool isOuterMargin;

        public List<Vector2> mPolygon = new List<Vector2>();

        Vector2 mCenter;
        Vector2 mXDir;
        public virtual void DebugDraw(Color clr)
        {
            //GeoDebugDrawUtils.DrawLine(mPreNew, mAngleNew, Color.white);
            //GeoDebugDrawUtils.DrawLine(mAngleNew, mNextNew, Color.white);
            //GeoDebugDrawUtils.DrawLine(mNextNew, mAnglePoint, Color.white);
            //GeoDebugDrawUtils.DrawLine(mAnglePoint, mPreNew, Color.white);
            GeoDebugDrawUtils.DrawPolygon(mPolygon, Color.white);
            GeoDebugDrawUtils.DrawLine(mCenter, mCenter + mXDir, Color.green);
            GeoDebugDrawUtils.DrawLine(mCenter, mCenter + mBinaryDir, Color.blue);
        }
        public virtual Vector2 First()
        {
            return mPreNew;
        }
        public virtual Vector2 Last()
        {
            return mNextNew;
        }
        public AbstractAngle(int i, Vector2 middle, Vector2 pre, Vector2 next, float angle, AngleType type, bool isOuter = true)
        {
            mIndex = i;
            mAngle = angle;
            mAngleTyle = type;
            mAnglePoint = middle;
            mPreviousPoint = pre;
            mNextPoint = next;
            isOuterMargin = isOuter;
            Initialize();
        }
        private void Initialize()
        {
            mPreviousDir = mPreviousPoint - mAnglePoint;
            mPreviousLength = mPreviousDir.magnitude;
            mPreviousDir.Normalize();
            mNextDir = mNextPoint - mAnglePoint;
            mNextLength = mNextDir.magnitude;
            mNextDir.Normalize();
            mBinaryDir = (mPreviousDir + mNextDir).normalized;
            if (mAngleTyle == AngleType.GREATER180)
            {
                mBinaryDir = -mBinaryDir;
            }
        }
        /// <summary>
        /// 先计算 CalculateOffset
        /// </summary>
        public virtual void Calculate(BVHTree2D bvh, float proportion)
        {
            float cos = CalculateCos() * mOffset;
            CalculateNew(bvh, cos, proportion);
            mCenter = (mAnglePoint + mAngleNew) * 0.5f;
            if (isOuterMargin)
            {
                mXDir = new Vector2(-mBinaryDir[1], mBinaryDir[0]);
            }
            else
            {
                mXDir = new Vector2(mBinaryDir[1], -mBinaryDir[0]);
            }
            if (CheckIntersect())
            {
                Debug.LogError("to do intersect");
            }
        }
        protected virtual void CalculateNew(BVHTree2D bvh, float cos, float proportion)
        {
            // 此处 只取边长的一半 进行比较，避免 相邻两个角处理时，出现边 重叠
            float minPre = Math.Min(cos, mPreviousLength * proportion);
            float minNext = Math.Min(cos, mNextLength * proportion);
            mAngleNew = mOffset * mBinaryDir + mAnglePoint;
            mPreNew = minPre * mPreviousDir + mAnglePoint;
            mNextNew = minNext * mNextDir + mAnglePoint;

            Vector2 tempPreDir = new Vector2(-mPreviousDir[1], mPreviousDir[0]); // 逆时针旋转90度
            Vector2 tempNextDir = new Vector2(mNextDir[1], -mNextDir[0]); // 顺时针旋转90度
            if (!isOuterMargin)
            {
                mPreviousDir = new Vector2(mPreviousDir[1], -mPreviousDir[0]); // 顺时针旋转90度
                mNextDir = new Vector2(-mNextDir[1], mNextDir[0]); // 逆时针旋转90度
            }
            GeoInsectPointInfo info = new GeoInsectPointInfo();
            bool test = GeoRayUtils.IsRayInsectRay2(mPreNew, tempPreDir, mNextNew, tempNextDir, ref info);
            mPolygon.Add(mPreNew);
            mPolygon.Add(mAnglePoint);
            mPolygon.Add(mNextNew);

            if (test)
            {
                Vector2 tv = info.mHitGlobalPoint;
                float pt = (tv - mPreNew).magnitude;
                float nt = (tv - mNextNew).magnitude;
                Vector2 pv = tv;
                Vector2 nv = tv;
                if (pt > MarginParameter.mMarginLenThreshold)
                {
                    pv = mPreNew + MarginParameter.mMarginLenThreshold * tempPreDir;
                }
                if (nt > MarginParameter.mMarginLenThreshold)
                {
                    nv = mNextNew + MarginParameter.mMarginLenThreshold * tempNextDir;
                }
                mPolygon.Add(nv);
                if (pv != nv)
                {
                    mPolygon.Add(pv);
                }
            }
            else
            {
                mPolygon.Add(mNextNew);
            }


        }
        public virtual void CalculateOffset(BVHTree2D bvh, float lengthThreshold)
        {
            GeoRay2 ray = new GeoRay2(mAnglePoint + MarginParameter.mStartOffset * mBinaryDir, mBinaryDir);
            GeoInsectPointArrayInfo info = new GeoInsectPointArrayInfo();
            bool test = bvh.GetIntersection(ray, ref info, false);
            mOffset = lengthThreshold;
            if (test)
            {
                mOffset = Math.Min(info.mLength, lengthThreshold);
            }
        }
        public virtual void OffsetMinWith(float min)
        {
            mOffset = Math.Min(mOffset, min);
        }
        protected virtual float CalculateCos()
        {
            double cos = Math.Cos(mAngle * 0.5f / 180.0f);
            return (float)cos;
        }
        protected virtual bool CheckIntersect()
        {
            // 更复杂度的图形，可能需要检查 新生成的边 是否与其他的边 相交
            // 此处暂不处理
            // to do
            return false;
        }
    }
    public class Less180Angle : AbstractAngle
    {
        public Less180Angle(int i, Vector2 middle, Vector2 pre, Vector2 next, float angle, AngleType type, bool isOuter = true)
            : base(i, middle, pre, next, angle, type, isOuter)
        {

        }

    }
    public class Greater180Angle : AbstractAngle
    {
        public Greater180Angle(int i, Vector2 middle, Vector2 pre, Vector2 next, float angle, AngleType type, bool isOuter = true)
            : base(i, middle, pre, next, angle, type, isOuter)
        {
            if (isOuter)
            {
                mPreviousDir = new Vector2(-mPreviousDir[1], mPreviousDir[0]); // 逆时针旋转90度
                mNextDir = new Vector2(mNextDir[1], -mNextDir[0]); // 顺时针旋转90度
            }
            else
            {
                mPreviousDir = new Vector2(mPreviousDir[1], -mPreviousDir[0]); // 顺时针旋转90度
                mNextDir = new Vector2(-mNextDir[1], mNextDir[0]); // 逆时针旋转90度
            }
        }

        protected override void CalculateNew(BVHTree2D bvh, float cos, float proportion)
        {
            float minPre = cos;
            float minNext = cos;
            GeoRay2 ray = new GeoRay2(mAnglePoint + MarginParameter.mStartOffset * mBinaryDir, mPreviousDir);
            GeoInsectPointArrayInfo info = new GeoInsectPointArrayInfo();
            bool test = bvh.GetIntersection(ray, ref info, false);
            if (test)
            {
                minPre = Math.Min(info.mLength, cos);
            }
            ray.mDirection = mNextDir;
            test = bvh.GetIntersection(ray, ref info, false);
            if (test)
            {
                minNext = Math.Min(info.mLength, minNext);
            }
            mAngleNew = mOffset * mBinaryDir + mAnglePoint;
            mPreNew = minPre * mPreviousDir + mAnglePoint;
            mNextNew = minNext * mNextDir + mAnglePoint;
            mPolygon.Add(mPreNew);
            mPolygon.Add(mAnglePoint);
            mPolygon.Add(mNextNew);
            mPolygon.Add(mAngleNew);
        }

        protected override float CalculateCos()
        {
            double cos = Math.Cos((mAngle - 180) * 0.5f / 180.0f);
            return (float)cos;
        }

        public override Vector2 First()
        {
            return mAnglePoint;
        }
        public override Vector2 Last()
        {
            return mAnglePoint;
        }
    }
    public class AngleUtils
    {
        public static AbstractAngle AngleHandle(int i, Vector2 p, Vector2 c, Vector2 n, bool isOuterMargin = true)
        {
            Vector2 pc = p - c;
            Vector2 nc = n - c;
            float angle = Vector2.Angle(pc, nc);
            bool isConvex = GeoPolygonUtils.IsConvexAngle(p, c, n);
            if (isOuterMargin && isConvex) // 外部 凸角计算
            {
                angle = 360 - angle;
            }
            else if (!isOuterMargin && !isConvex) // 内部 凹角计算
            {
                angle = 360 - angle;
            }
            // 角度已转化为 外部的 角度
            AbstractAngle angleProcessor = null;
            AngleType type = AngleTypeBy(angle);
            switch (type)
            {
                case AngleType.LESS180:
                    angleProcessor = new Less180Angle(i, c, p, n, angle, type, isOuterMargin);
                    break;
                case AngleType.GREATER180:
                    angleProcessor = new Greater180Angle(i, c, p, n, angle, type, isOuterMargin);
                    break;
            }
            return angleProcessor;
        }

        public static Vector2 LengthByBinaryRay(AbstractAngle i, AbstractAngle j)
        {
            GeoInsectPointInfo info = new GeoInsectPointInfo();
            bool test = GeoRayUtils.IsRayInsectRay2(i.mAnglePoint, i.mBinaryDir, j.mAnglePoint, j.mBinaryDir, ref info);
            if (test)
            {
                Vector2 insect = info.mHitGlobalPoint;
                float len1 = (insect - i.mAnglePoint).magnitude;
                float len2 = (insect - j.mAnglePoint).magnitude;
                return new Vector2(len1, len2);
            }
            return new Vector2(MarginParameter.mInfinite, MarginParameter.mInfinite);
        }

        private static AngleType AngleTypeBy(float angle)
        {
            if (angle < 180)
            {
                return AngleType.LESS180;
            }
            return AngleType.GREATER180;
        }
    }
    //**********************************************************************
    public enum UnitType
    {
        RECT,
        ANGLE,
        CONVEX
    }

    public class MarginSplitUnit : IAdjacent
    {
        public int mRectIndex;
        public int mPointStartIndex;
        public int mPointEndIndex;
        public Vector2 mSize;
        public GeoRect2 mRect;
        public UnitType isUnitType;

        public MarginSplitUnit()
        {
            isUnitType = UnitType.RECT;
        }

        public void InitializeRect(Vector2 pos, Vector2 xDir, Vector2 yDir)
        {
            Vector2 p2 = pos + mSize[0] * xDir;
            Vector2 p3 = p2 + mSize[1] * yDir;
            Vector2 p4 = pos + mSize[1] * yDir;
            mRect = new GeoRect2(pos, p2, p3, p4);
        }

        public void UpdateSize(float y)
        {
            mSize[1] = y;
            InitializeRect(mRect.mP1, mRect.mDir1, mRect.mDir2);
        }

        public virtual List<Vector2> NormalizeUnit()
        {
            List<Vector2> tranalste = new List<Vector2>();
            tranalste.Add(mRect.mP1);
            tranalste.Add(mRect.mP2);
            tranalste.Add(mRect.mP3);
            tranalste.Add(mRect.mP4);
            GeoAABB2 aabb = GeoUtils.GetAABB(tranalste);
            for (int i = 0; i < tranalste.Count; ++i)
            {
                tranalste[i] = tranalste[i] - aabb.Center();
            }
            tranalste.Add(aabb.Center()); // 最后一个时起始点
            tranalste.Add(aabb.Size);
            return tranalste;
        }

        public virtual void DebugDraw(Color clr)
        {
            GeoDebugDrawUtils.DrawAABB(mRect.mP1, mRect.mP2, mRect.mP3, mRect.mP4, Color.black);
            GeoDebugDrawUtils.DrawLine(mRect.mCenter, mRect.mCenter + mRect.mDir1 * mSize[0], Color.green);
            GeoDebugDrawUtils.DrawLine(mRect.mCenter, mRect.mCenter + mRect.mDir2 * mSize[1], Color.blue);
        }

        public virtual List<Vector2> Samples()
        {
            List<Vector2> samples = new List<Vector2>();
            int count = 4;
            float intervalx = mSize[0] / count;
            float intervaly = mSize[1] / count;
            for (int x = 0; x <= count; ++x)
            {
                Vector2 start = mRect.mP1 + x * intervalx * mRect.mDir1;
                for (int y = 0; y <= count; ++y)
                {
                    samples.Add(start + y * intervaly * mRect.mDir2);
                }
            }
            return samples;
        }
        public int Previous()
        {
            return mPointStartIndex;
        }
        public int Next()
        {
            return mPointEndIndex;
        }
        public float Area
        {
            get
            {
                return mSize[0] * mSize[1];
            }
        }

        public virtual List<Vector2> GetPolygon()
        {
            List<Vector2> result = new List<Vector2>();
            result.Add(mRect.mP1);
            result.Add(mRect.mP2);
            result.Add(mRect.mP3);
            result.Add(mRect.mP4);
            return result;
        }
    }
    public class AngleMarginUnit : MarginSplitUnit
    {
        public AbstractAngle mAngle;
        public AngleMarginUnit()
            : base()
        {
            mAngle = null;
            isUnitType = UnitType.ANGLE;
        }

        public AngleMarginUnit(AbstractAngle angle)
        {
            mAngle = angle;
            isUnitType = UnitType.ANGLE;
        }

        public override List<Vector2> GetPolygon()
        {
            List<Vector2> result = new List<Vector2>();
            result.AddRange(mAngle.mPolygon);
            return result;
        }

        public override List<Vector2> Samples()
        {
            List<Vector2> tranalste = new List<Vector2>();
            tranalste.Add(mAngle.mAnglePoint);
            tranalste.Add(mAngle.mNextNew);
            tranalste.Add(mAngle.mAngleNew);
            tranalste.Add(mAngle.mPreNew);
            List<List<Vector2>> results = ConvexPolygonScanning.DrawPolygonTranslate(tranalste, 10);
            tranalste.Clear();
            GeoUtils.FlatList(results, ref tranalste);
            return tranalste;
        }
        public override List<Vector2> NormalizeUnit()
        {
            List<Vector2> tranalste = new List<Vector2>();
            tranalste.Add(mAngle.mAnglePoint);
            tranalste.Add(mAngle.mNextNew);
            tranalste.Add(mAngle.mAngleNew);
            tranalste.Add(mAngle.mPreNew);
            GeoAABB2 aabb = GeoUtils.GetAABB(tranalste);
            for (int i = 0; i < tranalste.Count; ++i)
            {
                tranalste[i] = tranalste[i] - aabb.Center();
            }
            tranalste.Add(aabb.Center()); // 最后一个时起始点
            tranalste.Add(aabb.Size);
            return tranalste;
        }

        public override void DebugDraw(Color clr)
        {
            mAngle.DebugDraw(clr);
        }

        public bool IsLess(float angle)
        {
            float a = mAngle.mAngle;
            if (mAngle.mAngleTyle == AngleType.GREATER180)
            {
                a = mAngle.mAngle - 180;
            }
            return a < angle;
        }
    }
    public class ConvexMarginUnit : MarginSplitUnit
    {
        public List<Vector2> mConvex;
        public int mNumber;
        public ConvexMarginUnit()
            : base()
        {
            mConvex = new List<Vector2>();
            isUnitType = UnitType.CONVEX;
        }

        public ConvexMarginUnit(List<Vector2> angle, int num)
        {
            mConvex = angle;
            mNumber = num;
            isUnitType = UnitType.CONVEX;
        }

        public override List<Vector2> GetPolygon()
        {
            List<Vector2> result = new List<Vector2>();
            result.AddRange(mConvex);
            return result;
        }

        public override List<Vector2> Samples()
        {
            List<Vector2> tranalste = new List<Vector2>();
            tranalste.AddRange(mConvex);
            List<List<Vector2>> results = ConvexPolygonScanning.DrawPolygonTranslate(tranalste, 10);
            tranalste.Clear();
            GeoUtils.FlatList(results, ref tranalste);
            return tranalste;
        }
        public override List<Vector2> NormalizeUnit()
        {
            List<Vector2> tranalste = new List<Vector2>();
            tranalste.AddRange(mConvex);
            GeoAABB2 aabb = GeoUtils.GetAABB(tranalste);
            for (int i = 0; i < tranalste.Count; ++i)
            {
                tranalste[i] = tranalste[i] - aabb.Center();
            }
            tranalste.Add(aabb.Center()); // 最后一个时起始点
            tranalste.Add(aabb.Size);
            return tranalste;
        }

        public override void DebugDraw(Color clr)
        {
            GeoDebugDrawUtils.DrawPolygon(mConvex, clr);
        }

    }
    public class MarginSplitUnitGroup
    {
        List<MarginSplitUnit> mUnits;
        public bool isLow;
        public MarginSplitUnitGroup()
        {
            mUnits = new List<MarginSplitUnit>();
        }
        public void Add(MarginSplitUnit unit)
        {
            mUnits.Add(unit);
        }

        public List<List<Vector2>> GetPolygons()
        {
            List<List<Vector2>> results = new List<List<Vector2>>();
            foreach (MarginSplitUnit u in mUnits)
            {
                results.Add(u.GetPolygon());
            }
            return results;
        }

        public MarginSplitUnit this[int index]
        {
            get
            {
                return mUnits[index];
            }
        }
        public int Count
        {
            get
            {
                return mUnits.Count;
            }
        }
        public void DebugDraw(Color clr)
        {
            foreach (MarginSplitUnit unit in mUnits)
            {
                unit.DebugDraw(clr);
            }
        }

        public Vector2 Center()
        {
            if (isLow)
                throw new Exception("should be low");
            try
            {
                int index = mUnits.Count >> 1;
                return mUnits[index].mRect.mCenter;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void RemoveAt(int index)
        {
            mUnits.RemoveAt(index);
        }

        public void SetLow()
        {
            isLow = false;
            foreach (MarginSplitUnit u in mUnits)
            {
                if (u.isUnitType == UnitType.ANGLE || u.isUnitType == UnitType.CONVEX)
                {
                    isLow = true;
                    break;
                }
            }
        }


        public virtual List<Vector2> Samplers()
        {
            List<Vector2> results = new List<Vector2>();
            foreach (MarginSplitUnit unit in mUnits)
            {
                results.AddRange(unit.Samples());
            }
            List<Vector2> groups = new List<Vector2>();
            float threshold = isLow ? 0.2f : 0.5f;
            while (results.Count > 0)
            {
                Vector2 t = results[0];
                groups.Add(t);
                results.RemoveAt(0);
                List<int> removes = new List<int>();
                for (int j = 0; j < results.Count; ++j)
                {
                    float d = (results[j] - t).magnitude;
                    if (d < threshold)
                    {
                        removes.Add(j);
                    }
                }
                for (int k = removes.Count - 1; k >= 0; --k)
                {
                    results.RemoveAt(removes[k]);
                }
            }
            results = groups;
            return results;
        }
    }
    public class MarginSplitUnitGroups
    {
        public List<MarginSplitUnitGroup> mGroups;
        public MarginSplitUnitGroups()
        {
            mGroups = new List<MarginSplitUnitGroup>();
        }
        public void Add(MarginSplitUnitGroup group)
        {
            mGroups.Add(group);
        }

        public MarginSplitUnitGroup this[int index]
        {
            get
            {
                return mGroups[index];
            }
        }
        public int Count
        {
            get
            {
                return mGroups.Count;
            }
        }

        public void Clear()
        {
            mGroups.Clear();
        }
    }

    public class MarginPointFill
    {
        public List<List<Vector2>> mPoints = new List<List<Vector2>>();
        public void Add(List<Vector2> point)
        {
            mPoints.Add(point);
        }
    }
    public class MarginFillUtils
    {
        List<ConvexMarginUnit> mConvex = new List<ConvexMarginUnit>();
        List<AngleMarginUnit> mAngle = new List<AngleMarginUnit>();
        public List<MarginSplitUnit> mRect = new List<MarginSplitUnit>();
        public Dictionary<int, Quaternion> mQuaternionMap = new Dictionary<int, Quaternion>();
        public Dictionary<int, Quaternion> mMaxQuaternionMap = new Dictionary<int, Quaternion>();
        public List<MarginSplitUnit> mMaxRect = new List<MarginSplitUnit>();
        public RectPartion mRectPartition;
        public RegionProcessor mConvexPartition;
        public MarginPointFill mAngleFill;
        public MarginFillUtils(MarginSplitUnitGroups groups)
        {
            foreach (MarginSplitUnitGroup group in groups.mGroups)
            {
                for (int i = 0; i < group.Count; ++i)
                {
                    switch (group[i].isUnitType)
                    {
                        case UnitType.ANGLE:
                            mAngle.Add((AngleMarginUnit)group[i]);
                            break;
                        case UnitType.CONVEX:
                            mConvex.Add((ConvexMarginUnit)group[i]);
                            break;
                        case UnitType.RECT:
                            if (group[i].mSize[0] < MarginParameter.mMarginMax || group[i].mSize[1] < MarginParameter.mMarginMax)
                            {
                                mRect.Add(group[i]);
                            }
                            else
                            {
                                mMaxRect.Add(group[i]);
                            }
                            break;
                    }
                }
            }
            Debug.Log(mMaxRect.Count);
            Initialize();
        }

        private void Initialize()
        {
            InitializeRect();
            InitializeAngle();
            InitializeConvex();
            InitializeMaxRect();
        }
        private void InitializeRect()
        {
            mRectPartition = new RectPartion();
            int index = 0;
            foreach (MarginSplitUnit unit in mRect)
            {
                mRectPartition.AddRect(0, 0, unit.mSize[0], unit.mSize[1]);
                Vector2 look = -unit.mRect.mDir2;
                Quaternion q = Quaternion.FromToRotation(Vector3.forward, new Vector3(look[0], 0.0f, look[1]));
                mQuaternionMap.Add(index++, q);
            }
        }

        private void InitializeMaxRect()
        {
            int index = 0;
            foreach (MarginSplitUnit unit in mMaxRect)
            {
                Vector2 look = -unit.mRect.mDir2;
                Quaternion q = Quaternion.FromToRotation(Vector3.forward, new Vector3(look[0], 0.0f, look[1]));
                mMaxQuaternionMap.Add(index++, q);
            }
        }

        private void InitializeAngle()
        {
            mAngleFill = new MarginPointFill();
            foreach (AngleMarginUnit unit in mAngle)
            {
                Vector2 dir = unit.mAngle.mBinaryDir;
                Vector2 start = unit.mAngle.mAnglePoint;
                float d = unit.mAngle.mOffset;
                float lenStart = -0.1f;
                float interval = -lenStart * 2.0f;
                List<Vector2> temp = new List<Vector2>();
                while (lenStart < d)
                {
                    temp.Add(start + interval * dir);
                    lenStart += interval;
                }
                mAngleFill.Add(temp);
            }
        }
        private void InitializeConvex()
        {
            mConvexPartition = new RegionProcessor();
            foreach (ConvexMarginUnit unit in mConvex)
            {
                if (unit.mConvex.Count > 2)
                {
                    mConvexPartition.Add(unit.mConvex, null, 10);
                }
                else
                {
                    // to do
                }
            }
        }
    }
    public class MarginPolygonSplitNew
    {
        #region angle processor
        List<AbstractAngle> mAngleProcessors;
        #endregion

        #region edge processor
        List<List<MarginSplitUnit>> mSplits;
        #endregion

        #region
        // 测试
        public int mCursorIndex = 0;
        #endregion

        #region checker
        BVHTree2D mBVH;
        GeoPointsArray2 mBackPolyDatas;
        #endregion
        #region result
        List<MarginSplitUnit> mFinal;
        public MarginSplitUnitGroups mGroups;
        #endregion

        public void Reset()
        {
            mCursorIndex = 0;
        }

        public MarginSplitUnitGroup NextGroup()
        {
            if (mCursorIndex >= mGroups.Count)
            {
                return null;
            }
            return mGroups[mCursorIndex++];
        }

        bool isOuterMargin = false;
        public MarginPolygonSplitNew(List<Vector2> temp, bool outerMargin)
        {
            List<Vector2> temp1 = new List<Vector2>();
            temp1.AddRange(temp);
            isOuterMargin = outerMargin;
            GeoPolygonUtils.ReverseIfCW(ref temp1);
            mBackPolyDatas = new GeoPointsArray2(temp1);
            mSplits = new List<List<MarginSplitUnit>>();
        }
        public void Calculate(float min = MarginParameter.mMarginMin, float max = MarginParameter.mMarginMax)
        {
            InitializeChecker();
            HandleAngle();
            HandleRect(min, max);
            MergeSplit();
            Group(MarginParameter.mMarginMin);
            CleanGroup();
            Clear();
        }

        private void CleanGroup()
        {
            // truncate
            MarginSplitUnitGroups newGroups = new MarginSplitUnitGroups();
            for (int i = 0; i < mGroups.Count; ++i)
            {
                if (mGroups[i].Count > 1 && mGroups[i].isLow)
                {
                    // convex hull
                    List<Vector2> points = new List<Vector2>();
                    for (int j = 0; j < mGroups[i].Count; ++j)
                    {
                        if (mGroups[i][j].isUnitType == UnitType.ANGLE)
                        {
                            AngleMarginUnit unit = (AngleMarginUnit)mGroups[i][j];
                            AbstractAngle angle = (AbstractAngle)unit.mAngle;
                            points.AddRange(angle.mPolygon);
                        }
                        else if (mGroups[i][j].isUnitType == UnitType.RECT)
                        {
                            points.Add(mGroups[i][j].mRect.mP1);
                            points.Add(mGroups[i][j].mRect.mP2);
                            points.Add(mGroups[i][j].mRect.mP3);
                            points.Add(mGroups[i][j].mRect.mP4);
                        }
                    }
                    if (points.Count > 3)
                    {
                        points = GeoUtils.VertexMergeList(points, 1);
                        points = JarvisConvex.BuildHull(points);
                    }
                    MarginSplitUnitGroup group = new MarginSplitUnitGroup();
                    group.isLow = true;
                    group.Add(new ConvexMarginUnit(points, mGroups[i].Count));
                    newGroups.Add(group);
                }
                else
                {
                    newGroups.Add(mGroups[i]);
                }
            }
            mGroups = newGroups;
            // 合并单个小的, 先合并相邻的
            newGroups = new MarginSplitUnitGroups();
            MarginSplitUnitGroup temp = new MarginSplitUnitGroup();
            for (int i = 0; i < mGroups.Count; ++i)
            {
                if (mGroups[i].Count == 1 || mGroups[i].isLow)
                {
                    temp.Add(mGroups[i][0]);
                }
                else
                {
                    if (temp.Count > 0)
                    {
                        temp.isLow = true;
                        newGroups.Add(temp);
                        temp = new MarginSplitUnitGroup();
                    }
                    newGroups.Add(mGroups[i]);
                }
            }
            if (temp.Count > 0)
            {
                newGroups.Add(temp);
            }
            mGroups = newGroups;
            // 合并单个的为一组
            newGroups = new MarginSplitUnitGroups();
            temp = new MarginSplitUnitGroup();
            temp.isLow = true;
            for (int i = 0; i < mGroups.Count; ++i)
            {
                if (mGroups[i].Count == 1)
                {
                    temp.Add(mGroups[i][0]);
                }
                else
                {
                    // 合并 长度 大于 宽度 2倍的 
                    List<int> removes = new List<int>();
                    for (int j = 0; j < mGroups[i].Count; ++j)
                    {
                        if (mGroups[i][j].isUnitType == UnitType.RECT && (mGroups[i][j].mSize[0] > mGroups[i][j].mSize[1] * 2.0f))
                        {
                            removes.Add(j);
                            temp.Add(mGroups[i][j]);
                        }
                    }
                    for (int k = removes.Count - 1; k >= 0; --k)
                    {
                        mGroups[i].RemoveAt(removes[k]);
                    }
                    if (mGroups[i].Count > 0)
                    {
                        newGroups.Add(mGroups[i]);
                    }
                }
            }
            newGroups.Add(temp);
            mGroups = newGroups;
            foreach (MarginSplitUnitGroup g in mGroups.mGroups)
            {
                g.SetLow();
            }
        }
        private void InitializeChecker()
        {
            List<BVHObject2> segs = new List<BVHObject2>();
            for (int i = 0; i < mBackPolyDatas.mPointArray.Count; ++i)
            {
                int j = i + 1;
                j = (j == mBackPolyDatas.mPointArray.Count) ? 0 : j;
                segs.Add(new BVHSegment2Object(mBackPolyDatas.mPointArray[i], mBackPolyDatas.mPointArray[j]));
            }
            mBVH = new BVHTree2D(segs, 4);
        }
        private void HandleAngle()
        {
            var temp = mBackPolyDatas.mPointArray;
            int count = temp.Count;
            mAngleProcessors = new List<AbstractAngle>();
            for (int i = 0; i < count; ++i)
            {
                int p = i - 1;
                int n = i + 1;
                p = p == -1 ? count - 1 : p;
                n = n == count ? 0 : n;
                AbstractAngle processor = AngleUtils.AngleHandle(i, temp[p], temp[i], temp[n], isOuterMargin);
                processor.CalculateOffset(mBVH, MarginParameter.mMarginLenThreshold);
                mAngleProcessors.Add(processor);
            }
            // 计算 offset
            Dictionary<int, float> cacheOffset = new Dictionary<int, float>();
            for (int i = 0; i < mAngleProcessors.Count; ++i)
            {
                int j = i + 1;
                if (j == mAngleProcessors.Count)
                    j = 0;
                Vector2 len = AngleUtils.LengthByBinaryRay(mAngleProcessors[i], mAngleProcessors[j]);
                if (cacheOffset.ContainsKey(i))
                {
                    cacheOffset[i] = Math.Min(cacheOffset[i], len[0]);
                }
                else
                {
                    cacheOffset.Add(i, len[0]);
                }
                if (cacheOffset.ContainsKey(j))
                {
                    cacheOffset[j] = Math.Min(cacheOffset[j], len[1]);
                }
                else
                {
                    cacheOffset.Add(j, len[1]);
                }
            }
            for (int i = 0; i < mAngleProcessors.Count; ++i)
            {
                mAngleProcessors[i].OffsetMinWith(cacheOffset[i]);
            }
            cacheOffset.Clear();

            //// 计算分隔
            for (int i = 0; i < mAngleProcessors.Count; ++i)
            {
                int j = i + 1;
                if (j == mAngleProcessors.Count)
                    j = 0;
                float proportion = 0.25f;
                if (mAngleProcessors[j].mAngleTyle == AngleType.GREATER180)
                {
                    proportion = 0.5f;
                }
                mAngleProcessors[i].Calculate(mBVH, proportion);
                if (mAngleProcessors[j].mAngleTyle != AngleType.GREATER180)
                {
                    // may update bvh
                    //BVHObject2 obj = new BVHSegment2Object(mAngleProcessors[j].mPreNew, mAngleProcessors[j].mAngleNew);
                    //mBVH.AddObject(obj);
                    //obj = new BVHSegment2Object(mAngleProcessors[j].mNextNew, mAngleProcessors[j].mAngleNew);
                    //mBVH.AddObject(obj, true);
                }
            }

        }
        private void HandleRect(float min, float max)
        {
            List<RectMarginPoint> rectList = new List<RectMarginPoint>();
            // 矩形区域
            for (int i = 0; i < mAngleProcessors.Count; ++i)
            {
                int j = i + 1;
                if (j == mAngleProcessors.Count)
                    j = 0;
                Vector2 start = mAngleProcessors[i].Last();
                Vector2 end = mAngleProcessors[j].First();
                Vector2 xDir = end - start;
                float length = xDir.magnitude;
                xDir.Normalize();
                Vector2 yDir = isOuterMargin ? new Vector2(xDir[1], -xDir[0]) : new Vector2(-xDir[1], xDir[0]); // 外部 顺时针得到 Y；内部 逆时针得到 Y
                RectMarginPoint rect = new RectMarginPoint(start, end, length, 1, xDir, yDir, isOuterMargin);
                rectList.Add(rect);
            }
            // split
            for (int i = 0; i < rectList.Count; ++i)
            {
                rectList[i].GeneratorPoint(mBackPolyDatas);
                rectList[i].CalculateSize(mBVH);
                List<MarginSplitUnit> splits = rectList[i].Split(i, min, max);
                if (splits != null && splits.Count > 0)
                {
                    mSplits.Add(splits);
                    foreach (MarginSplitUnit unit in splits)
                    {
                        BVHObject2 seg = new BVHSegment2Object(unit.mRect.mP1, unit.mRect.mP2);
                        mBVH.AddObject(seg);
                        seg = new BVHSegment2Object(unit.mRect.mP1, unit.mRect.mP4);
                        mBVH.AddObject(seg);
                        seg = new BVHSegment2Object(unit.mRect.mP3, unit.mRect.mP2);
                        mBVH.AddObject(seg);
                        seg = new BVHSegment2Object(unit.mRect.mP3, unit.mRect.mP4);
                        mBVH.AddObject(seg, true);
                        unit.InitializeRect(unit.mRect.mP1 - MarginParameter.mStartOffset * unit.mRect.mDir2, unit.mRect.mDir1, unit.mRect.mDir2);
                    }
                }
            }
        }
        private void MergeSplit()
        {
            mFinal = new List<MarginSplitUnit>();
            if (mAngleProcessors.Count != mSplits.Count)
            {
                throw new Exception("wrong here");
            }
            for (int i = 0; i < mSplits.Count; ++i)
            {
                AngleMarginUnit angle = new AngleMarginUnit(mAngleProcessors[i]);
                mFinal.Add(angle);
                mFinal.AddRange(mSplits[i]);
            }
        }
        private void Clear()
        {
            mSplits.Clear();
            mSplits = null;
            mAngleProcessors.Clear();
            mAngleProcessors = null;
            //mBackPolyDatas.Clear();
            //mBackPolyDatas = null;
        }
        private void Group(float xLen)
        {
            mGroups = new MarginSplitUnitGroups();
            MarginSplitUnitGroup group = new MarginSplitUnitGroup();
            int start = 0;
            int end = start;
            List<Vector2i> splits = new List<Vector2i>();
            for (int i = 0; i < mFinal.Count; ++i)
            {
                if (mFinal[i].isUnitType == UnitType.ANGLE)
                {
                    AbstractAngle angle = ((AngleMarginUnit)mFinal[i]).mAngle;
                    if (angle.mAngleTyle == AngleType.GREATER180)
                    {
                        end = i;
                        splits.Add(Vector2i.GetVector2i(start, end));
                        start = i + 1;
                    }
                }
            }
            if (splits[splits.Count - 1][1] < mFinal.Count - 1)
            {
                splits.Add(Vector2i.GetVector2i(splits[splits.Count - 1][1] + 1, mFinal.Count - 1));
            }
            foreach (Vector2i v2i in splits)
            {
                List<int> flags = new List<int>();
                for (int i = v2i[0]; i <= v2i[1]; ++i)
                {
                    bool isLowState = mFinal[i].isUnitType == UnitType.ANGLE || mFinal[i].mSize[0] < xLen;
                    flags.Add(isLowState ? 1 : 0);
                }

                for (int i = 0; i < flags.Count;)
                {
                    group = new MarginSplitUnitGroup();
                    group.Add(mFinal[v2i[0] + i]);
                    int j = i + 1;
                    while (j < flags.Count && flags[i] == flags[j])
                    {
                        group.Add(mFinal[v2i[0] + j]);
                        j++;
                    }
                    group.isLow = flags[i] == 1;
                    mGroups.Add(group);
                    i = j;
                }
            }
        }
        public void DebugDraw(Color clr1, Color clr2)
        {
            GeoDebugDrawUtils.DrawPolygon(mBackPolyDatas.mPointArray, clr1);
            //DebugGroupCursor(clr2);
            //GeoDebugDrawUtils.DrawPolygon(mConvexHull, Color.cyan);
            DebugGroupCursor(clr2);
        }
        public void DebugAllUnits(Color clr)
        {
            foreach (MarginSplitUnit unit in mFinal)
            {
                unit.DebugDraw(clr);
            }
        }
        public void DebugUnitCursor(Color clr)
        {
            mFinal[mCursorIndex].DebugDraw(clr);
        }
        public void DebugGroupCursor(Color clr)
        {
            for (int i = 0; i < mGroups.Count; ++i)
            {
                mGroups[i].DebugDraw(clr);
            }
        }
        public void TravelCursor()
        {
            mCursorIndex++;
            if (mCursorIndex >= mGroups.Count)
            {
                mCursorIndex = 0;
            }
        }
    }
    public class MarginPolygonSplit
    {
        // 外部边界和内部边界的区分，方向不一样
        bool isOuterMargin;
        List<MarginSplitUnit> mSplits;
        List<AbstractMargin> mFinal;
        #region checker
        bool isBvhChecker;
        SegmentsIntersectCheck mIntersectChecker;
        BVHTree2D mBVH;
        GeoPointsArray2 mBackPolyDatas;
        #endregion

        #region sweep
        MarginSplitUnit mCurrentUnit;
        // for test
        const int mUnitMinIndex = 39;
        const int mUnitMaxIndex = 48;
        public int mUnitIndex;

        RectMarginPoint mCurrent;
        public int mRectIndex = 0;
        int mRectPointIndex = 0;
        #endregion


        public MarginPolygonSplit(List<Vector2> temp, bool isbvh = true, bool outerMargin = true)
        {
            mFinal = new List<AbstractMargin>();
            mSplits = new List<MarginSplitUnit>();
            List<Vector2> temp1 = new List<Vector2>();
            temp1.AddRange(temp);
            GeoPolygonUtils.ReverseIfCW(ref temp1);
            mBackPolyDatas = new GeoPointsArray2(temp1);
            isOuterMargin = outerMargin;
            isBvhChecker = isbvh;
        }

        public void Initialize(float min = 0.5f, float max = 2.0f)
        {
            InitializeChecker();// 必须使用 bvh 进行计算， 不然 HandleAngle 出错
            MakeSplits();
            InitializeSplites();
            Partition(min, max);
        }

        private void Partition(float min, float max)
        {
            for (int i = 0; i < mFinal.Count; ++i)
            {
                if (mFinal[i].mType == MarginPointType.MP_RECT)
                {
                    List<MarginSplitUnit> splits = mFinal[i].Split(i, min, max);
                    if (splits != null)
                    {
                        mSplits.AddRange(splits);
                    }
                }
            }
        }

        public void TravelCursor()
        {
            if (mSplits.Count > 0)
            {
                //if (mUnitIndex > mUnitMaxIndex)
                //{
                //    mUnitIndex = mUnitMinIndex;
                //}
                //if (mUnitIndex < mUnitMinIndex)
                //{
                //    mUnitIndex = mUnitMaxIndex;
                //}
                if (mUnitIndex >= mSplits.Count)
                {
                    mUnitIndex = 0;
                }
                mCurrentUnit = mSplits[mUnitIndex];
                mRectIndex = mCurrentUnit.mRectIndex;
                mCurrent = (RectMarginPoint)mFinal[mRectIndex];
                mUnitIndex++;
            }
        }
        public void DrawCursor(Color clr)
        {
            mCurrentUnit.DebugDraw(clr);
        }
        public void SetTranform(GameObject go)
        {
            if (mFinal.Count > 0)
            {
                while (true)
                {
                    if (mRectIndex >= mFinal.Count)
                    {
                        mRectIndex = 0;
                    }
                    if (mFinal[mRectIndex].mType == MarginPointType.MP_CORNER)
                    {
                        mRectIndex++;
                        continue;
                    }
                    mCurrent = (RectMarginPoint)mFinal[mRectIndex];
                    if (mRectPointIndex >= mCurrent.Count)
                    {
                        mRectIndex++;
                        mRectPointIndex = 0;
                        continue;
                    }

                    if (mRectPointIndex < mCurrent.Count)
                    {
                        Vector2 pos = mCurrent.Pos(mRectPointIndex);
                        go.transform.rotation = mCurrent.Q;
                        go.transform.position = pos;
                        mRectPointIndex++;
                        break;
                    }
                    mRectPointIndex++;
                }
            }
        }
        public void DebugDraw(Color clr1, Color clr2)
        {
            GeoDebugDrawUtils.DrawPolygon(mBackPolyDatas.mPointArray, clr1);
            //foreach (AbstractMargin p in mFinal)
            //{
            //    p.DebugDraw(clr2);
            //}
        }
        public void DrawMarginSplitUnit(Color clr)
        {
            for (int i = 0; i < mSplits.Count; ++i)
            {
                if (i % 2 == 1)
                {
                    mSplits[i].DebugDraw(Color.white);
                }
                else
                {
                    mSplits[i].DebugDraw(Color.black);
                }
            }
        }
        public void DrawDirection(GameObject go)
        {
            if (mRectPointIndex < mCurrent.Count)
            {
                Vector2 temp = go.transform.position;
                Vector4 size = mCurrent.Size(mRectPointIndex);
                GeoDebugDrawUtils.DrawLine(temp, temp + mCurrent.mXDirection * size[0], Color.green);
                GeoDebugDrawUtils.DrawLine(temp, temp + mCurrent.mYDirection * size[2], Color.blue);
            }
        }
        private void InitializeSplites()
        {
            for (int i = 0; i < mFinal.Count; ++i)
            {
                mFinal[i].GeneratorPoint(mBackPolyDatas);
                if (isBvhChecker)
                {
                    mFinal[i].CalculateSize(mBVH);
                }
                else
                {
                    mFinal[i].CalculateSize(mIntersectChecker);
                }
            }
        }
        private void SortByPriority()
        {

        }
        private void MakeSplits()
        {
            var temp = mBackPolyDatas.mPointArray;
            int count = temp.Count;
            for (int i = 0; i < count; ++i)
            {
                int p = i - 1;
                int n = i + 1;
                p = p == -1 ? count - 1 : p;
                n = n == count ? 0 : n;
                Vector2 xDir = temp[n] - temp[i];
                float length = xDir.magnitude;
                xDir.Normalize();
                Vector2 yDir = isOuterMargin ? new Vector2(xDir[1], -xDir[0]) : new Vector2(-xDir[1], xDir[0]);
                RectMarginPoint rect = new RectMarginPoint(temp[i], temp[n], length, 1, xDir, yDir);
                yDir = temp[p] - temp[i];
                yDir.Normalize();
                float angle = Vector2.Angle(xDir, yDir);
                bool isConvex = GeoPolygonUtils.IsConvexAngle(temp[p], temp[i], temp[n]);
                int priority = 5;
                if (isOuterMargin)
                {
                    if (isConvex)
                    {
                        angle = 360 - angle;
                    }
                    else
                    {
                        priority = 10;
                    }
                }
                else
                {
                    if (!isConvex)
                    {
                        angle = 360 - angle;
                    }
                    else
                    {
                        priority = 10;
                    }
                }
                CornerMarginPoint corner = new CornerMarginPoint(temp[i], angle, priority, xDir, yDir);
                mFinal.Add(corner);
                mFinal.Add(rect);
            }
        }
        private void InitializeChecker()
        {
            if (!isBvhChecker)
            {
                mIntersectChecker = new SegmentsIntersectCheck(mBackPolyDatas.mPointArray);
            }
            else
            {
                List<BVHObject2> segs = new List<BVHObject2>();
                for (int i = 0; i < mBackPolyDatas.mPointArray.Count; ++i)
                {
                    int j = i + 1;
                    j = (j == mBackPolyDatas.mPointArray.Count) ? 0 : j;
                    segs.Add(new BVHSegment2Object(mBackPolyDatas.mPointArray[i], mBackPolyDatas.mPointArray[j]));
                }
                mBVH = new BVHTree2D(segs, 4);
            }

        }
        public bool GetRect(float w, float h, ref GeoRect2 rect)
        {
            return false;
        }
    }
    //********************************************************
    public class MarginRect
    {
        GeoRect3 mRect;
        public List<Vector2> mSamples;
        public MarginRect(GeoRect3 rect)
        {
            mRect = rect;
        }

        public void Sample(float interval)
        {
            mSamples = new List<Vector2>();
            Vector3 direct = mRect.mP4 - mRect.mP1;
            Vector3 start = mRect.mP1 + mRect.mDir1 * MarginParameter.mStartOffset;
            float len = direct.magnitude;
            direct.Normalize();
            int count = (int)(len / interval);
            if (count < 1)
            {
                mSamples.Add(start);
                mSamples.Add(start + len * direct);
                return;
            }

            for (int i = 0; i < count; ++i)
            {
                mSamples.Add(start);
                start += interval * direct;
            }
            float leftLen = len - count * interval;
            Vector2 left = leftLen * direct;
            if (leftLen < interval * 0.5f)
            {

                mSamples[count - 1] = mSamples[count - 1] + left;
            }
            else
            {
                mSamples.Add(mSamples[count - 1] + left);
            }
        }
        public GeoRect3 Rect
        {
            get
            {
                return mRect;
            }

        }
        public Vector3 Center
        {
            get
            {
                return mRect.mCenter;
            }
        }
    }
    /// <summary>
    /// 0.5 米 对 矩形分段
    /// 判断点所属矩形(属于多个，则存在交叉，需要选择)
    /// </summary>
    public class MarginPolygon
    {
        List<MarginRect> mAllRects;
        Dictionary<int, MarginPoint> mPointAttibutes;
        BVHTree2D mBVH;
        List<Vector2> mBackPolyData = new List<Vector2>();
        public List<MarginPointPriority> mFinal = new List<MarginPointPriority>();
        public MarginPolygon()
        {
            mAllRects = new List<MarginRect>();
            mPointAttibutes = new Dictionary<int, MarginPoint>();
        }
        public void Initialize(List<Vector2> margin)
        {
            mBackPolyData.AddRange(margin);
            GeoPolygonUtils.ReverseIfCW(ref mBackPolyData);
            InitBvh(mBackPolyData);
            Angle(mBackPolyData);
            Final(0.1f);
            // castAll = mBVH.RayCastAll(mFinal[idx].mPoint, mFinal[idx].mXDirection);
        }
        private void InitBvh(List<Vector2> temp)
        {
            List<BVHObject2> objs = new List<BVHObject2>();
            for (int i = 0; i < temp.Count; ++i)
            {
                int j = i + 1;
                j = j == temp.Count ? 0 : j;
                BVHSegment2Object seg = new BVHSegment2Object(temp[i], temp[j]);
                objs.Add(seg);
            }
            mBVH = new BVHTree2D(objs, 1);
        }
        private void Final(float mergeThreshold = 0.1f)
        {
            mFinal.Clear();
            GeoPointsArray2 poly = new GeoPointsArray2(mBackPolyData);
            for (int i = 0; i < mAllRects.Count; ++i)
            {
                List<Vector2> temp = mAllRects[i].mSamples;
                foreach (Vector2 p1 in temp)
                {
                    Vector2 p = p1;
                    if (!GeoPolygonUtils.IsPointInPolygon2(poly, ref p))
                    {
                        MarginPointPriority pri = new MarginPointPriority();
                        pri.mYDirection = mAllRects[i].Rect.mDir1;
                        pri.mPriority = 1;
                        for (int j = 0; j < mAllRects.Count; ++j)
                        {
                            if (j != i)
                            {
                                if (GeoRect3.IsPointInRect(mAllRects[j].Rect, p))
                                {
                                    pri.mPriority++;
                                }
                            }
                        }
                        pri.mPoint = p;
                        mFinal.Add(pri);
                    }

                }
                if (mPointAttibutes.ContainsKey(i))
                {
                    MarginPointPriority pri = new MarginPointPriority();
                    pri.mYDirection = mPointAttibutes[i].mDirection;
                    pri.mPriority = 100;
                    pri.mPoint = mPointAttibutes[i].mPoint;
                    mFinal.Add(pri);
                }
            }
            Quaternion q = Quaternion.FromToRotation(Vector3.right, Vector3.up);
            Matrix4x4 mat = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
            for (int i = 0; i < mFinal.Count; ++i)
            {
                mFinal[i].mXDirection = mat.MultiplyVector(mFinal[i].mYDirection);
                mFinal[i].mXDirection.Normalize();
            }

            //HashSet<int> mark = new HashSet<int>();
            //for (int i = 0; i < mFinal.Count - 1; ++i)
            //{
            //    if (mark.Contains(i))
            //    {
            //        continue;
            //    }
            //    for (int j = i + 1; j < mFinal.Count; ++j)
            //    {
            //        bool merged = mFinal[i].CheckOther(mFinal[j], mergeThreshold);
            //        if (merged)
            //        {
            //            mark.Add(j);
            //        }
            //    }
            //}
            //foreach (int idx in mark)
            //{
            //    mFinal.RemoveAt(idx);
            //}
            TestSize();
            // SortPoint();
        }
        public void GetRect(float w, float y)
        {
            GeoRect3 rect = null;
            for (int i = 0; i < mFinal.Count; ++i)
            {
                if (!mFinal[i].mMarked)
                {
                    // to do
                }
            }
            if (rect != null)
            {
                MarkFinalPoint(rect);
            }
        }
        private void MarkFinalPoint(GeoRect3 rect)
        {
            for (int i = 0; i < mFinal.Count; ++i)
            {
                if (!mFinal[i].mMarked)
                {
                    if (GeoRect3.IsPointInRect(rect, mFinal[i].mPoint))
                    {
                        mFinal[i].mMarked = true;
                    }
                }
            }
        }
        private void TestSize()
        {
            GeoRay2 ray = new GeoRay2(Vector2.zero, Vector2.zero);
            GeoInsectPointArrayInfo info = new GeoInsectPointArrayInfo();
            for (int i = 0; i < mFinal.Count; ++i)
            {

                ray.mOrigin = mFinal[i].mPoint;
                Vector2 xPositiveAxis = mFinal[i].mXDirection;
                Vector2 xNegtiveAxis = -xPositiveAxis;
                Vector2 yPositiveAxis = mFinal[i].mYDirection;
                Vector2 yNegtiveAxis = -yPositiveAxis;

                // x positive
                ray.mDirection = xPositiveAxis;
                if (mBVH.GetIntersection(ray, ref info, false))
                {
                    mFinal[i].mSize[0] = info.mLength;
                }
                else
                {
                    mFinal[i].mSize[0] = MarginParameter.mInfinite;
                }
                info.Clear();
                // x negtive
                ray.mDirection = xNegtiveAxis;
                if (mBVH.GetIntersection(ray, ref info, false))
                {
                    mFinal[i].mSize[1] = info.mLength;
                }
                else
                {
                    mFinal[i].mSize[1] = MarginParameter.mInfinite;
                }
                info.Clear();
                // y positive
                ray.mDirection = yPositiveAxis;
                if (mBVH.GetIntersection(ray, ref info, false))
                {
                    mFinal[i].mSize[2] = info.mLength;
                }
                else
                {
                    mFinal[i].mSize[2] = MarginParameter.mInfinite;
                }
                info.Clear();
                // y negtive, should use follow code
                ray.mDirection = yNegtiveAxis;
                if (mBVH.GetIntersection(ray, ref info, false))
                {
                    mFinal[i].mSize[3] = info.mLength;
                }
                else
                {
                    mFinal[i].mSize[3] = MarginParameter.mInfinite;
                }
                info.Clear();

                // mFinal[i].mSize[3] = MarginParameter.mStartOffset;
            }



        }
        private void SortPoint()
        {
            mFinal.Sort(new MarginPointPriorityCompare());
        }
        public void DebugDraw()
        {
            //if (mAllRects.Count > 0)
            //{
            //    Color[] clrs = new Color[2] { Color.black, Color.white };
            //    for (int i = 0; i < mAllRects.Count; ++i)
            //    {
            //        if (mAllRects[i] != null)
            //        {
            //            GeoDebugDrawUtils.DrawAABB(mAllRects[i].Rect, clrs[(i & 1)]);
            //        }
            //    }
            //}
            if (mFinal.Count > 0)
            {
                for (int i = 0; i < mFinal.Count; ++i)
                {
                    int idx = i;
                    GeoDebugDrawUtils.DrawLine(mFinal[idx].mPoint, mFinal[idx].mPoint + mFinal[idx].mSize[0] * mFinal[idx].mXDirection, Color.green);
                    GeoDebugDrawUtils.DrawLine(mFinal[idx].mPoint, mFinal[idx].mPoint + mFinal[idx].mSize[1] * -mFinal[idx].mXDirection, Color.green);
                    GeoDebugDrawUtils.DrawLine(mFinal[idx].mPoint, mFinal[idx].mPoint + mFinal[idx].mSize[2] * mFinal[idx].mYDirection, Color.green);
                    GeoDebugDrawUtils.DrawLine(mFinal[idx].mPoint, mFinal[idx].mPoint + mFinal[idx].mSize[3] * -mFinal[idx].mYDirection, Color.green);
                }
            }
            if (mBVH != null)
            {
                // mBVH.DebugDraw();
            }
            //if (castAll != null && castAll.Count > 0)
            //{
            //    for (int i = 0; i < castAll.Count; i += 2)
            //    {
            //        GeoDebugDrawUtils.DrawAABB(castAll[i], castAll[i + 1], Color.white);
            //    }
            //}
            //if (mPointAttibutes.Count > 0)
            //{
            //    foreach (MarginPoint p in mPointAttibutes.Values)
            //    {
            //        GeoDebugDrawUtils.DrawLine(p.mPoint, p.mPoint + p.mDirection * MarginParameter.mExpandLen, Color.green);
            //    }
            //}
        }

        private void Angle(List<Vector2> poly)
        {
            List<Vector2> temp = new List<Vector2>();
            #region find first index not in condition
            //int start = -1;
            //for (int i = 0; i < poly.Count; ++i)
            //{
            //    int p = i - 1;
            //    int n = i + 1;
            //    if (p == -1)
            //        p = poly.Count - 1;
            //    if (n == poly.Count)
            //        n = 0;
            //    Vector2 pi = poly[p] - poly[i];
            //    Vector2 ni = poly[n] - poly[i];
            //    float angle = Vector2.Angle(pi.normalized, ni.normalized);
            //    if (angle > mParameters.mAngle1 && angle < (180 - mParameters.mAngle1))
            //    {
            //        start = i;
            //        break;
            //    }
            //}

            //for (int i = start; i < poly.Count; ++i)
            //{
            //    temp.Add(poly[i]);
            //}
            //for (int i = 0; i < start; ++i)
            //{
            //    temp.Add(poly[i]);
            //}
            #endregion
            temp.AddRange(poly);
            List<int> indexSeq = Split(temp);
            Dictionary<int, Vector3> dirMap = CalculateRect(poly, temp, indexSeq);
            CalculatePointAttr(temp, indexSeq, dirMap);
            RemoveUnimportantPoint();
        }
        private void RemoveUnimportantPoint()
        {
            List<int> removed = new List<int>();
            var tmp = mPointAttibutes.Values.ToList();
            for (int i = 0; i < tmp.Count; ++i)
            {
                MarginPoint point = tmp[i];
                if (point.isConvex && point.mAngle > MarginParameter.mBinaryAngle)
                {
                    Vector2 vp = point.mPoint + point.mDirection * (MarginParameter.mExpandLen * 0.5f);
                    foreach (MarginRect rect in mAllRects)
                    {
                        if (GeoRect3.IsPointInRect(rect.Rect, vp))
                        {
                            removed.Add(point.mRectIndex);
                            break;
                        }
                    }
                }
                else
                {
                    removed.Add(point.mRectIndex);
                }
            }
            for (int i = removed.Count - 1; i >= 0; --i)
            {
                mPointAttibutes.Remove(removed[i]);
            }
        }
        private List<int> Split(List<Vector2> temp)
        {
            List<int> indexSeq = new List<int>();
            //for (int i = 0; i < temp.Count; )
            //{
            //    indexSeq.Add(i);
            //    int j = i + 1;
            //    while (j < temp.Count && Calculate(i, j, temp))
            //    {
            //        j++;
            //    }
            //    i = j;
            //}
            //if (indexSeq[indexSeq.Count - 1] != temp.Count - 1)
            //{
            //    indexSeq.Add(temp.Count - 1);
            //}

            for (int i = 0; i < temp.Count; ++i)
            {
                indexSeq.Add(i);
            }
            return indexSeq;
        }
        private Dictionary<int, Vector3> CalculateRect(List<Vector2> poly, List<Vector2> temp, List<int> indexSeq)
        {
            Quaternion q = Quaternion.FromToRotation(Vector3.up, Vector3.right);
            Matrix4x4 mat = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
            GeoPointsArray2 polyArray = new GeoPointsArray2(poly);
            Dictionary<int, Vector3> result = new Dictionary<int, Vector3>();
            for (int i = 0; i < indexSeq.Count; ++i)
            {
                int j = i + 1;
                if (j == indexSeq.Count)
                    j = 0;
                int ii = indexSeq[i];
                int jj = indexSeq[j];
                Vector3 ss = temp[ii];
                Vector3 ee = temp[jj];
                Vector3 dir = ee - ss;
                dir.Normalize();
                dir = mat.MultiplyVector(dir);

                Vector2 rf;
                float len = MarginParameter.mExpandLen;
                GeoRect3 rect = null;
                bool needContinue = true;
                Vector3 p1 = ss;
                Vector3 p4 = ee;
                do
                {
                    Vector3 p2 = ss + dir * len;
                    Vector3 p3 = ee + dir * len;
                    rect = new GeoRect3(p1, p2, p3, p4);
                    rf = rect.mCenter;
                    if (GeoPolygonUtils.IsPointInPolygon2(polyArray, ref rf))
                    {
                        len *= 0.5f;
                    }
                    else
                    {
                        needContinue = false;
                    }
                } while (needContinue && len > MarginParameter.mMinExpandLen);
                if (!needContinue)
                {
                    MarginRect mr = new MarginRect(rect);
                    mr.Sample(MarginParameter.mInterval);
                    mAllRects.Add(mr);
                    if (result.ContainsKey(ii))
                    {
                        Vector3 back = result[ii];
                        float angle = Vector3.Angle(back, dir);
                        back = back + dir;
                        back.Normalize();
                        back[2] = angle;
                        result[ii] = back;
                    }
                    else
                    {
                        result.Add(indexSeq[i], dir);
                    }
                    if (result.ContainsKey(jj))
                    {
                        Vector3 back = result[jj];
                        float angle = Vector3.Angle(back, dir);
                        back = back + dir;
                        back.Normalize();
                        back[2] = angle;
                        result[jj] = back;
                    }
                    else
                    {
                        result.Add(jj, dir);
                    }
                }
            }
            return result;
        }
        private void CalculatePointAttr(List<Vector2> temp, List<int> indexSeq, Dictionary<int, Vector3> dirMap)
        {
            for (int i = 0; i < indexSeq.Count; ++i)
            {
                int ii = indexSeq[i];
                if (dirMap.ContainsKey(ii))
                {
                    int p = i - 1;
                    p = p < 0 ? indexSeq.Count - 1 : p;
                    int n = i + 1;
                    n = n == indexSeq.Count ? 0 : n;
                    p = indexSeq[p];
                    n = indexSeq[n];
                    MarginPoint margin = new MarginPoint();
                    Vector3 tmp = dirMap[ii];
                    margin.isConvex = GeoPolygonUtils.IsConvexAngle(temp[p], temp[ii], temp[n]);
                    margin.mAngle = tmp[2];
                    margin.mDirection = new Vector2(tmp[0], tmp[1]);
                    margin.mPoint = temp[ii];
                    margin.mRectIndex = i;
                    mPointAttibutes.Add(margin.mRectIndex, margin);
                }
            }
        }
        private bool Calculate(int startIndex, int endIndex, List<Vector2> points)
        {
            int p = endIndex - 1;
            int n = endIndex + 1;
            if (n >= points.Count)
            {
                return true;
            }
            Vector2 pi = points[p] - points[endIndex];
            Vector2 ni = points[n] - points[endIndex];
            float angle = Vector2.Angle(pi.normalized, ni.normalized);
            if (angle > MarginParameter.mAngle1 && angle < (180 - MarginParameter.mAngle1))
            {
                return false;
            }
            if ((endIndex - startIndex) > 1)
            {
                Vector2 se = points[endIndex] - points[startIndex];
                se.Normalize();
                for (int i = startIndex + 1; i < endIndex; ++i)
                {
                    Vector2 v = points[i] - points[startIndex];
                    angle = Vector2.Angle(v.normalized, se);
                    if (angle > MarginParameter.mAngle2 && angle < (180 - MarginParameter.mAngle2))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
