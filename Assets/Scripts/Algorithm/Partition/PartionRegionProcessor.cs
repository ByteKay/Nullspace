
using System.Collections.Generic;
using UnityEngine;
using Nullspace;

namespace Partition
{
    public abstract class AbstractRegionProcessor
    {
        public abstract bool CanContinue();
        public abstract GeoAABB2 GetRectangle(float sizew, float sizeh);

        public abstract void AddRect(ref GeoAABB2 aabb);
    }

    public class RegionUnitProcessor : AbstractRegionProcessor
    {
        HashSet<Vector2i> mCacheWH = new HashSet<Vector2i>();
        public PolygonWrapper mWrapper;
        int mMissCount = 0;
        public float mMissProportion = 0.9f;
        public RegionUnitProcessor(List<Vector2> parent, List<Vector2> child, float scale = 4.0f, bool shuffle = true)
        {
            Initialize(parent, child, shuffle, scale);
        }

        public override void AddRect(ref GeoAABB2 aabb)
        {
            mWrapper.AddRectIfCan(ref aabb, false);
        }

        override
        public bool CanContinue()
        {
            return !mWrapper.IsFull() && mWrapper.Proportion(mMissCount) < mMissProportion;
        }

        override
        public GeoAABB2 GetRectangle(float sizew, float sizeh)
        {
            Vector2i temp = new Vector2i((int)(sizew * 10), (int)(sizeh * 10));
            if (!mCacheWH.Contains(temp))
            {
                GeoAABB2 aabb = new GeoAABB2();
                if (!mWrapper.GetRectangle(sizew, sizeh, ref aabb))
                {
                    mCacheWH.Add(temp);
                }
                else
                {
                    mMissCount = 0;
                    return aabb;
                }
            }
            mMissCount++;
            return null;
        }
        private void Initialize(List<Vector2> parent, List<Vector2> child, bool shuffle, float scale = 4.0f)
        {
            mWrapper = PolygonWrapper.Create(parent, child, shuffle, scale);
        }

        private void InitializeOld(List<Vector2> parent, List<Vector2> child)
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
        }
    }
    public class RegionUnitsProcessor : AbstractRegionProcessor
    {
        List<RegionUnitProcessor> mUnitsProcessor = new List<RegionUnitProcessor>();

        public void Add(RegionUnitProcessor processor)
        {
            mUnitsProcessor.Add(processor);
        }

        override
        public void AddRect(ref GeoAABB2 aabb)
        {
            // to do
        }

        
        public override bool CanContinue()
        {
            foreach (RegionUnitProcessor unit in mUnitsProcessor)
            {
                if (unit.CanContinue())
                {
                    return true;
                }
            }
            return false;
        }


        public override GeoAABB2 GetRectangle(float sizew, float sizeh)
        {
            foreach (AbstractRegionProcessor unit in mUnitsProcessor)
            {
                GeoAABB2 aabb = unit.GetRectangle(sizew, sizeh);
                if (aabb != null)
                {
                    return aabb;
                }
            }
            return null;
        }
    }

    public class RegionProcessor
    {
        List<AbstractRegionProcessor> mRegionPeocessors = new List<AbstractRegionProcessor>();
        public RegionProcessor()
        {

        }

        public AbstractRegionProcessor this[int index]
        {
            get
            {
                return mRegionPeocessors[index];
            }
        }

        public void Add(List<List<Vector2>> units, float scale = 4.0f)
        {
            RegionUnitsProcessor unitsProcessor = new RegionUnitsProcessor();
            foreach (List<Vector2> unit in units)
            {
                unitsProcessor.Add(new RegionUnitProcessor(unit, null, scale));
            }
            mRegionPeocessors.Add(unitsProcessor);
        }

        public void Add(List<Vector2> parent, List<Vector2> child, float scale = 4.0f)
        {
            mRegionPeocessors.Add(new RegionUnitProcessor(parent, child, scale));
        }

        public bool CanContinue()
        {
            foreach (var item in mRegionPeocessors)
            {
                if (item.CanContinue())
                {
                    return true;
                }
            }
            return false;
        }

        public GeoAABB2 GetRectangle(float sw, float sh, int index)
        {
            if (index < mRegionPeocessors.Count)
                return mRegionPeocessors[index].GetRectangle(sw, sh);
            return null;
        }
        public GeoAABB2 GetRectangle(float sw, float sh)
        {
            for (int i = 0; i < mRegionPeocessors.Count; ++i)
            {
                GeoAABB2 aabb = mRegionPeocessors[i].GetRectangle(sw, sh);
                if (aabb != null)
                    return aabb;
            }
            return null;
        }
    }
}
