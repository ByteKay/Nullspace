
using System.Collections.Generic;
using UnityEngine;
using Nullspace;

namespace Partition
{
    internal class MarginalExpandClass
    {
        class MarginalExpandClassCompare : IComparer<KeyValuePair<int, float>>
        {
            public int Compare(KeyValuePair<int, float> x, KeyValuePair<int, float> y)
            {
                return x.Value.CompareTo(y.Value);
            }
        }
        public int index;
        public Vector2 v1;
        public Vector2 v2;
        public List<Vector2> insectPoint = new List<Vector2>();
        public List<int> insectIndex = new List<int>();
        public void Sort()
        {
            if (insectPoint.Count < 1)
            {
                return;
            }
            List<KeyValuePair<int, float>> cache = new List<KeyValuePair<int, float>>();
            for (int i = 0; i < insectPoint.Count; ++i)
            {
                KeyValuePair<int, float> temp = new KeyValuePair<int, float>(i, (insectPoint[i] - v1).sqrMagnitude);
                cache.Add(temp);
            }
            cache.Sort(new MarginalExpandClassCompare());
            List<Vector2> insectPoint1 = new List<Vector2>();
            List<int> insectIndex1 = new List<int>();
            foreach (KeyValuePair<int, float> item in cache)
            {
                insectPoint1.Add(insectPoint[item.Key]);
                insectIndex1.Add(insectIndex[item.Key]);
            }
            insectPoint = insectPoint1;
            insectIndex = insectIndex1;
        }
    }
    internal class PartitionUnitCompare : IComparer<PartitionUnit>
    {
        public int Compare(PartitionUnit x, PartitionUnit y)
        {
            return -x.mProbablity.CompareTo(y.mProbablity);
        }
    }
    public class PartitionUnit
    {
        public float mProbablity;
        public float mMinW;
        public float mMaxW;
        public float mMinH;
        public float mMaxH;
        public System.Object mObject;
        public PartitionUnit()
        {

        }
    }
    public class PartitionParameter
    {
        public float mProbablity;
        public System.Object mObject;
        public PartitionParameter()
        {

        }
    }
    public class PolygonPartitionProcessor
    {
        #region 中间量
        HashSet<Vector2i> mCacheWH = new HashSet<Vector2i>();
        HashSet<int> mCacheIndex = new HashSet<int>();
        List<float> mResultsArea = new List<float>();
        List<float> mAreaProb = new List<float>();
        #endregion
        PolygonWrapper mWrapper;
        List<PartitionParameter> mProbs;
        int mProb;
        float mProbTotal = 0.0f;
        float mArea;
        int mInsertMaxCount;
        int mInsertedCount;
        // List<List<GeoAABB2>> mResults = new List<List<GeoAABB2>>();
        public PolygonPartitionProcessor(List<Vector2> parent, List<Vector2> child, List<PartitionParameter> probs)
        {
            mProbs = probs;
            mProb = mProbs.Count;
            mInsertMaxCount = mProb * 10;
            mInsertedCount = 0;
            for (int i = 0; i < mProb; ++i)
            {
                // mResults.Add(new List<GeoAABB2>());
                mResultsArea.Add(0.0f);
                mAreaProb.Add(mProbs[i].mProbablity);
                mProbTotal += mProbs[i].mProbablity;
            }
            Initialize(parent, child);
        }

        public int RandomIndex()
        {
            if (mProbTotal == 0)
                return -1;
            return RandomInt(mAreaProb, mProbTotal);
        }

        public System.Object GetObjectOfIndex(int index)
        {
            return mProbs[index].mObject;
        }

        public bool CanContinue()
        {
            return !mWrapper.IsFull() && mCacheIndex.Count < mProb && mInsertedCount < mInsertMaxCount && mProbTotal > 1e-5f;
        }

        public GeoAABB2 GetRectangle(float sizew, float sizeh, int index)
        {
            if (CanContinue())
            {              
                Vector2i temp = new Vector2i((int)(sizew * 100), (int)(sizeh * 100));
                if (!mCacheWH.Contains(temp))
                {
                    GeoAABB2 aabb = new GeoAABB2();
                    if (!mWrapper.GetRectangle(sizew, sizeh, ref aabb))
                    {
                        mCacheWH.Add(temp);
                        if (!mCacheIndex.Add(index))
                        {
                            mInsertedCount++;
                        }
                    }
                    else
                    {
                        // mResults[index].Add(aabb);
                        float aabbArea = aabb.Area();
                        mResultsArea[index] += aabbArea;
                        mCacheIndex.Clear();
                        mInsertedCount = 0;
                        mAreaProb.Clear();
                        mProbTotal = 0.0f;
                        for (int a = 0; a < mProb; ++a)
                        {
                            float newProb = mResultsArea[a] / mArea - mProbs[a].mProbablity;
                            newProb = newProb > 0 ? 0 : -newProb;
                            mAreaProb.Add(newProb);
                            mProbTotal += newProb;
                        }
                        return aabb;
                    }
                }
                else
                {
                    if (!mCacheIndex.Add(index))
                    {
                        mInsertedCount++;
                    }
                }
            }
            return null;
        }

        private void Initialize(List<Vector2> parent, List<Vector2> child)
        {
            List<Vector4> connects = null;
            List<Vector2> source = new List<Vector2>();
            if (child != null && child.Count > 0)
            {
                EarPolygon polygon = new EarPolygon();
                foreach (Vector2 t in parent)
                {
                    polygon.AddPoint(t[0], t[1]);
                }
                EarPolygon chlid = new EarPolygon(polygon);
                foreach (Vector2 t in child)
                {
                    chlid.AddPoint(t[0], t[1]);
                }
                connects = EarClipping.Merge(polygon);
                LinkedListNode<EarPoint> point = polygon.Get();
                while (point.Next != null)
                {
                    source.Add(point.Value.mPoint);
                    point = point.Next;
                }
            }
            else
            {
                source.AddRange(parent);
            }
            mWrapper = PolygonWrapper.Create(source, true, connects, 2);
            mArea = mWrapper.Area;
        }
        private int RandomInt(List<float> probs, float sumProb)
        {
            int prob = probs.Count;
            float rand = RandomUtils.GetRandomFloat(0, sumProb);
            int index = -1;
            for (int i = 0; i < prob; ++i)
            {
                float sub = rand - probs[i];
                if (sub > 1e-5f)
                {
                    rand = sub;
                }
                else
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }
    public class StaticPolygonPartition
    {
        /// <summary>
        /// 1 获取离散点
        /// 2 双概率获得大小
        /// 3 Voronoi图生成
        /// </summary>
        /// <param name="original">多边形</param>
        /// <param name="probablities">占比，0 1为最小最大范围，2位概率</param>
        public static void VoronoiPartition(List<Vector2> original, List<PartitionUnit> probablities) 
        {
            // to do
        }

        public static List<List<GeoAABB2>> PolygonPartition(List<Vector2> parentP, List<Vector2> childP, List<PartitionUnit> probablities)
        {
            List<Vector4> connects = null;
            List<Vector2> source = new List<Vector2>();
            if (childP != null && childP.Count > 0)
            {
                EarPolygon polygon = new EarPolygon();
                foreach (Vector2 t in parentP)
                {
                    polygon.AddPoint(t[0], t[1]);
                }     
                EarPolygon chlid = new EarPolygon(polygon);
                foreach (Vector2 t in childP)
                {
                    chlid.AddPoint(t[0], t[1]);
                }
                connects = EarClipping.Merge(polygon);
                LinkedListNode<EarPoint> point = polygon.Get();         
                while (point.Next != null)
                {
                    source.Add(point.Value.mPoint);
                    point = point.Next;
                }
            }
            else
            {
                source.AddRange(parentP);
            }
            return PolygonPartition(source, probablities, connects);
        }
        // 多边形逆时针,向外扩展 length 个单位, 放入 GeoPolygonUtils 里头
        public static List<Vector2> ConvexMarginalExpand(List<Vector2> original, float length = 1.0f)
        {
            List<Vector2> result = new List<Vector2>();
            Vector2 center = new Vector2(0.0f, 0.0f);
            foreach (Vector2 temp in original)
            {
                center += temp;
            }
            center *= (1.0f / original.Count);
            foreach (Vector2 temp in original)
            {
                Vector2 vp = temp - center;
                result.Add(temp + vp.normalized * length);
            }
            return result;
        }       

        // 对于环状问题，暂时没处理。处理起来，稍作修改即可。后期遇到时加入(检测环状，然后分离环状，及带孔的多边形环形区域。)
        public static void PolygonExpand(List<Vector2> original, List<Vector2> results, float length = 1.0f, bool useHull = true)
        {
            List<Vector2> origin = new List<Vector2>();
            origin.AddRange(original);
            GeoPointsArray2 hullArray = null;
            if (useHull)
            {
                List<Vector2> hull = JarvisConvex.BuildHull(original);
                GeoPolygonUtils.ReverseIfCW(ref hull);
                hullArray = new GeoPointsArray2(hull);
            }
            float len = length;
            while (results.Count == 0)
            {
                List<Vector2> segs = ExpandPolygonSegments(origin, len);
                float min = CheckSegmentsIntersect(segs);
                if (min > len)
                    min = len;
                if (min > 1e-5f)
                {
                    List<Vector2> expands = ExpandPolygon(origin, min);
                    //List<int> vLst = GeoUtils.VertexIndexList(expands);
                    //List<List<int>> circles = CommonUtils.CheckCircle(vLst);
                    // GeoUtils.Split(origin, circles);
                    origin = PolygonIntersectRemove(expands);
                    len = len - min;
                    if (len < 0.01f)
                    {
                        results.AddRange(origin);
                    }
                }
                else
                {
                    for (int i = 1; i < segs.Count; i += 2)
                    {
                        results.Add(segs[i]);
                    }
                }
                if (useHull && results.Count == 0 && hullArray != null)
                {
                    bool flag = false;
                    for (int i = 0; i < origin.Count; i++)
                    {
                        Vector2 refV = origin[i];
                        if (GeoPolygonUtils.IsPointInConvexPolygon2(hullArray, ref refV))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        List<Vector2> split = JarvisConvex.BuildHull(origin);
                        GeoPolygonUtils.ReverseIfCW(ref split);
                        split = ConvexMarginalExpand(split, len);
                        results.AddRange(split);
                        len = 0;
                    }
                }
            }
        }
        private static List<Vector2> ExpandPolygonSegments(List<Vector2> original, float length = 1.0f)
        {
            List<Vector2> tempOrigin = new List<Vector2>();
            tempOrigin.AddRange(original);
            if (GeoPolygonUtils.CalcualetArea(tempOrigin) < 0)
            {
                tempOrigin.Reverse();
            }
            List<Vector2> result = new List<Vector2>();
            for (int i = 0; i < tempOrigin.Count; ++i)
            {
                int j = i + 1;
                if (j == tempOrigin.Count)
                    j = 0;
                int k = i - 1;
                if (k == -1)
                    k = tempOrigin.Count - 1;
                Vector2 vk = tempOrigin[k] - tempOrigin[i];
                Vector2 vj = tempOrigin[j] - tempOrigin[i];
                Vector2 dir = vk.normalized + vj.normalized;
                dir.Normalize();
                if (GeoPolygonUtils.IsConvexAngle(tempOrigin[k], tempOrigin[i], dir + tempOrigin[i]))
                {
                    dir = -dir;
                }
                result.Add(tempOrigin[i]);
                result.Add(tempOrigin[i] + dir * length);
            }
            return result;

        }
        private static float CheckSegmentsIntersect(List<Vector2> segs)
        {
            int count = segs.Count >> 1;
            float min = 1e10f;
            bool flag = false;
            for (int i = 0; i < count - 1; ++i)
            {
                Vector2 iv1 = segs[2 * i];
                Vector2 iv2 = segs[2 * i + 1];
                for (int j = i + 1; j < count; ++j)
                {
                    Vector2 jv1 = segs[2 * j];
                    Vector2 jv2 = segs[2 * j + 1];
                    GeoInsectPointInfo infor = new GeoInsectPointInfo();
                    if (GeoSegmentUtils.IsSegmentInsectSegment2(iv1, iv2, jv1, jv2, ref infor))
                    {
                        Vector2 insect = infor.mHitGlobalPoint;
                        float isi = (insect - iv1).sqrMagnitude;
                        float isj = (insect - jv1).sqrMagnitude;
                        float maxT = isi > isj ? isi : isj;
                        if (maxT < min)
                        {
                            flag = true;
                            min = maxT;
                        }
                    }
                }
            }
            if (flag)
            {
                return min;
            }
            return -1.0f;
        }
        private static List<Vector2> ExpandPolygon(List<Vector2> original, float length = 1.0f)
        {
            List<Vector2> tempOrigin = new List<Vector2>();
            tempOrigin.AddRange(original);
            if (GeoPolygonUtils.CalcualetArea(tempOrigin) < 0)
            {
                tempOrigin.Reverse();
            }
            List<Vector2> result = new List<Vector2>();
            for (int i = 0; i < tempOrigin.Count; ++i)
            {
                int j = i + 1;
                if (j == tempOrigin.Count)
                    j = 0;
                int k = i - 1;
                if (k == -1)
                    k = tempOrigin.Count - 1;
                Vector2 vk = tempOrigin[k] - tempOrigin[i];
                Vector2 vj = tempOrigin[j] - tempOrigin[i];
                Vector2 dir = vk.normalized + vj.normalized;
                dir.Normalize();
                if (GeoPolygonUtils.IsConvexAngle(tempOrigin[k], tempOrigin[i], dir + tempOrigin[i]))
                {
                    dir = -dir;
                }
                result.Add(tempOrigin[i] + dir * length);
            }
            return result;
        }
        private static List<Vector2> PolygonIntersectRemove(List<Vector2> result)
        {
            int count = result.Count;
            List<Vector2> insects = new List<Vector2>();
            List<Vector2> checkPoints = CheckIntersect(result, ref insects);
            while (count != checkPoints.Count)
            {
                // 此处做删除 圆环时，需要考虑 圆环点数较多的情况。此时需要分隔
                RemoveCircle(ref checkPoints, insects);
                count = checkPoints.Count;
                insects.Clear();
                checkPoints = CheckIntersect(checkPoints, ref insects);
            }
            checkPoints = RemoveShortEdge(checkPoints, 0.1f);
            checkPoints = RemoveShortAngle(checkPoints, 15.0f);
            return checkPoints;
        }
        private static bool Vector3EqualVector2(Vector3 v3, Vector2 v2)
        {
            return v2[0] == v3[0] && v2[1] == v3[1];
        }
        private static List<Vector2> CheckIntersect(List<Vector2> result, ref List<Vector2> insects)
        {
            List<MarginalExpandClass> temp = new List<MarginalExpandClass>();
            for (int i = 0; i < result.Count; ++i)
            {
                MarginalExpandClass marginalPoint = new MarginalExpandClass();
                int j = i + 1;
                if (j == result.Count)
                    j = 0;
                marginalPoint.index = i;
                marginalPoint.v1 = result[i];
                marginalPoint.v2 = result[j];
                temp.Add(marginalPoint);
            }
            for (int i = 0; i < temp.Count - 1; ++i)
            {
                MarginalExpandClass cur = temp[i];
                for (int j = i + 1; j < temp.Count; ++j)
                {
                    MarginalExpandClass other = temp[j];
                    if (temp[j].v1 == cur.v2)
                    {
                        continue;
                    }
                    GeoInsectPointInfo info = new GeoInsectPointInfo();
                    if (GeoSegmentUtils.IsSegmentInsectSegment2(cur.v1, cur.v2, other.v1, other.v2, ref info))
                    {
                        Vector2 ttt = info.mHitGlobalPoint;
                        cur.insectPoint.Add(ttt);
                        cur.insectIndex.Add(other.index);
                        other.insectPoint.Add(ttt);
                        other.insectIndex.Add(cur.index);
                    }
                }
            }
            List<Vector2> allPointsWithIntersects = new List<Vector2>();
            for (int i = 0; i < temp.Count; ++i)
            {
                allPointsWithIntersects.Add(temp[i].v1);
                if (temp[i].insectPoint.Count > 0)
                {
                    temp[i].Sort();
                    foreach (Vector2 v2t in temp[i].insectPoint)
                    {
                        int index = insects.FindIndex((Vector2 v) =>
                            {
                                return v == v2t;
                            });
                        if (index == -1)
                        {
                            insects.Add(v2t);
                        }
                    }
                    allPointsWithIntersects.AddRange(temp[i].insectPoint);
                }
            }
            if (allPointsWithIntersects.Count < 2)
                return allPointsWithIntersects;
            List<Vector2> tempResult1 = new List<Vector2>(); 
            {
                int index = 0;
                for (int i = 1; i < allPointsWithIntersects.Count; ++i)
                {
                    if (allPointsWithIntersects[i] != allPointsWithIntersects[0])
                    {
                        index = i;
                        break;
                    }
                }
                List<Vector2> tempResult = new List<Vector2>();
                for (int i = index; i < allPointsWithIntersects.Count; ++i)
                {
                    tempResult.Add(allPointsWithIntersects[i]);
                }
                for (int i = 0; i < index; ++i)
                {
                    tempResult.Add(allPointsWithIntersects[i]);
                }   
                for (int i = 0; i < tempResult.Count; )
                {
                    tempResult1.Add(tempResult[i]);
                    int next = i + 1;
                    while (next < tempResult.Count && tempResult[i] == tempResult[next])
                    {
                        ++next;
                    }
                    i = next;
                }
            }
            return tempResult1;
        }
        private static List<Vector2> RemoveCircleOld(List<Vector2> allPointsWithIntersects)
        {
            List<Vector2> results = new List<Vector2>();
            LinkedList<Vector2> del = new LinkedList<Vector2>();
            foreach (Vector2 v in allPointsWithIntersects)
            {
                del.AddLast(v);
            }
            LinkedListNode<Vector2> first = del.First;
            LinkedListNode<Vector2> cur = first;
            while (cur != null)
            {
                LinkedListNode<Vector2> next = CheckCircle(del, cur);
                if (next != null)
                {
                    while (next != cur)
                    {
                        next = next.Previous;
                        del.Remove(next.Next);
                    }
                }
                cur = cur.Next;
                
            }
            while (first != null)
            {
                results.Add(first.Value);
                first = first.Next;
            }
            return results;
        }
        private static void RemoveCircle(ref List<Vector2> allPointsWithIntersects, List<Vector2> insects)
        {
            int mid = allPointsWithIntersects.Count >> 1;
            List<int> count = new List<int>();
            for (int i = 0; i < insects.Count; ++i)
            {
                for (int j = 0; j < allPointsWithIntersects.Count; ++j)
                {
                    if (allPointsWithIntersects[j] == insects[i])
                    {
                        count.Add(j);
                    }
                }
                if (count.Count > 1)
                {
                    int start = count[0];
                    int end = count[count.Count - 1];
                    if (end - start < mid)
                    {
                        allPointsWithIntersects.RemoveRange(start, end - start);
                    }
                    else
                    {
                        allPointsWithIntersects.RemoveRange(end, allPointsWithIntersects.Count - end);
                        allPointsWithIntersects.RemoveRange(0, start);
                    }
                }
                count.Clear();
            }
        }
        private static LinkedListNode<Vector2> CheckCircle(LinkedList<Vector2> del, LinkedListNode<Vector2> cur)
        {
            LinkedListNode<Vector2> next = cur.Next;
            while (next != null)
            {
                if (next.Value == cur.Value)
                {
                    return next;
                }
                next = next.Next;
            }
            return next;
        }
        public static List<Vector2> RemoveShortAngle(List<Vector2> original, float minAngle = 10.0f)
        {
            if (original.Count < 3)
                return original;
            
            DoubleCircleLinkedList<Vector2> dcel = new DoubleCircleLinkedList<Vector2>();
            foreach (Vector2 v in original)
            {
                dcel.AddLast(v);
            }
            LinkedListNode<Vector2> cur = dcel.First;
            while (true)
            {
                int count = dcel.Count;
                do
                {
                    Vector2 curPoint = cur.Value;
                    Vector2 pre = dcel.Previous(cur).Value;
                    Vector2 next = dcel.Next(cur).Value;
                    pre = curPoint - pre;
                    next = curPoint - next;
                    float angle = Vector2.Angle(pre, next);
                    cur = dcel.Next(cur); 
                    if (angle < minAngle)
                    {
                        dcel.Remove(dcel.Previous(cur));
                    }
                } while (cur != dcel.First);
                if (dcel.Count == count)
                {
                    break;
                }
            }
            List<Vector2> tempOrigin = new List<Vector2>();
            do
            {
                tempOrigin.Add(cur.Value);
                cur = dcel.Next(cur);
            } while (cur != dcel.First);
            return tempOrigin;
        }
        public static List<Vector2> RemoveShortEdge(List<Vector2> original, float distance = 0.1f) // 半径
        {
            DoubleCircleLinkedList<Vector2> dcel = new DoubleCircleLinkedList<Vector2>();
            foreach (Vector2 v in original)
            {
                dcel.AddLast(v);
            }
            LinkedListNode<Vector2> cur = dcel.First;
            List<Vector2> temp = new List<Vector2>();
            do
            {
                Vector2 curPoint = cur.Value;
                temp.Add(curPoint);
                // float totalLen = 0.0f;
                do
                {
                    cur = dcel.Next(cur);
                    // totalLen += (cur.Value - curPoint).magnitude;
                    float totalLen = (cur.Value - curPoint).magnitude;
                    if (totalLen > distance)
                    {
                        break;
                    }
                } while (cur != dcel.First);
                
            } while (cur != dcel.First);
            return temp;
        }
        private static int RandomInt(List<float> probablities, float sumProb)
        {
            int prob = probablities.Count;
            float rand = RandomUtils.GetRandomFloat(0, sumProb);
            int index = -1;
            for (int i = 0; i < prob; ++i)
            {
                float sub = rand - probablities[i];
                if (sub > 1e-5f)
                {
                    rand = sub;
                }
                else
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        /// <summary>
        /// 1 在某一个段内找不到时，并且找的概率为1，容易出现死循环 nowCount insertMaxCount 来控制 退出循环
        /// 2 wrapper.IsFull() 设置占比，退出循环
        /// 3 cacheIndex.Count < prob 退出循环
        /// </summary>
        /// <param name="original"></param>
        /// <param name="probablities"></param>
        /// <param name="connects"></param>
        /// <returns></returns>
        private static List<List<GeoAABB2>> PolygonPartition(List<Vector2> original, List<PartitionUnit> probablities, List<Vector4> connects = null)
        {
            List<PartitionUnit> newUnits = new List<PartitionUnit>();
            newUnits.AddRange(probablities);
            PolygonWrapper wrapper = PolygonWrapper.Create(original, true, connects, 2);
            float total = wrapper.Area;
            int prob = newUnits.Count;
            List<List<GeoAABB2>> results = new List<List<GeoAABB2>>();
            List<float> resultsArea = new List<float>();
            List<float> areaProb = new List<float>();
            // 更新概率
            float proSum = 0.0f;
            for (int i = 0; i < prob; ++i)
            {
                results.Add(new List<GeoAABB2>());
                resultsArea.Add(0.0f);
                areaProb.Add(newUnits[i].mProbablity);
                proSum += newUnits[i].mProbablity;
            }
            int insertMaxCount = prob * 10;
            int nowCount = 0;
            HashSet<Vector2i> cacheWH = new HashSet<Vector2i>();
            HashSet<int> cacheIndex = new HashSet<int>();
            int index = RandomInt(areaProb, proSum);
            while (!wrapper.IsFull() && cacheIndex.Count < prob && nowCount < insertMaxCount)
            {
                float sizew = RandomUtils.GetRandomFloat(newUnits[index].mMinW, newUnits[index].mMaxW);
                float sizeh = RandomUtils.GetRandomFloat(newUnits[index].mMinH, newUnits[index].mMaxH);
                Vector2i temp = new Vector2i((int)(sizew * 100), (int)(sizeh * 100));
                if (!cacheWH.Contains(temp))
                {
                    GeoAABB2 aabb = new GeoAABB2();
                    if (!wrapper.GetRectangle(sizew, sizeh, ref aabb))
                    {
                        cacheWH.Add(temp);
                        if (!cacheIndex.Add(index))
                        {
                            nowCount++;
                        }
                    }
                    else
                    {
                        results[index].Add(aabb);
                        float aabbArea = aabb.Area();
                        resultsArea[index] += aabbArea;
                        cacheIndex.Clear();
                        nowCount = 0;
                        areaProb.Clear();
                        proSum = 0.0f;
                        for (int a = 0; a < prob; ++a)
                        {
                            float newProb = resultsArea[a] / total - newUnits[a].mProbablity;
                            newProb = newProb > 0 ? 0 : -newProb;
                            areaProb.Add(newProb);
                            proSum += newProb;
                        }
                        if (proSum == 0)
                            break;
                    }
                }
                else
                {
                    if (!cacheIndex.Add(index))
                    {
                        nowCount++;
                    }
                }
                index = RandomInt(areaProb, proSum);
            }
            for (int a = 0; a < resultsArea.Count; ++a)
            {
                float abs = resultsArea[a] / total;
                Debug.Log(string.Format("count: {0}, xLen: {1}", results[a].Count, abs));
            }
            return results;
        }
    }
}
