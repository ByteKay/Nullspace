
using System.Collections.Generic;


namespace Nullspace
{
    public abstract class ClosestObject
    {
        public abstract bool IsCloseTo(ClosestObject other);
    }

    public class ClosestUnion
    {

        private Dictionary<int, List<int>> mGroups;
        private List<int> mIndexTable;
        public ClosestUnion()
        {
            mGroups = new Dictionary<int, List<int>>();
            mIndexTable = new List<int>();
        }

        public void Initialize(int size)
        {
            for (int i = 0; i < size; ++i)
            {
                mIndexTable.Add(i);
            }
        }

        public int Find(int x)
        {
            while (mIndexTable[x] != x)
            {
                int id = mIndexTable[mIndexTable[x]];
                mIndexTable[x] = id;
                x = id;
            }
            return x;
        }

        public void Unite(int i, int j)
        {
            int ii = Find(i);
		    int jj = Find(j);
		    if (ii != jj)
		    {
			    mIndexTable[ii] = jj;
		    }
        }

        public void Group()
        {
            int size = mIndexTable.Count;
		    for (int i = 0; i < size; ++i)
		    {
			    int groupId = Find(i);
			    if (!mGroups.ContainsKey(groupId))
			    {
				    mGroups.Add(groupId, new List<int>());
			    }
			    mGroups[groupId].Add(i);
		    }
        }

        public Dictionary<int, List<int>> GetResult()
        {
            return mGroups;
        }

    }

    // 可以认为是  基于密度的聚类
    public class GroupClosest<T> where T : ClosestObject
    {
        private ClosestUnion mUnion;
        private List<T> mObjects;
        class ClosestPair
        {
            public int mI;
            public int mJ;
            public ClosestPair(int i, int j)
            {
                mI = i;
                mJ = j;
            }
        }
        public GroupClosest()
        {
            mUnion = new ClosestUnion();
            mObjects = new List<T>();
        }

        public GroupClosest(List<T> t)
        {
            mUnion = new ClosestUnion();
            mObjects = t;
        }

        public void Add(T t)
        {
            mObjects.Add(t);
        }
        public List<List<T>> Group()
        {
            mUnion.Initialize(mObjects.Count);
            List<ClosestPair> pairs = new List<ClosestPair>();
            GeneratorPairs(ref pairs);
            int size = pairs.Count;
            for (int i = 0; i < size; ++i)
            {
                mUnion.Unite(pairs[i].mI, pairs[i].mJ);
            }
            pairs.Clear();
            mUnion.Group();
            Dictionary<int, List<int>> rst = mUnion.GetResult();
            List<List<T>> groupResult = new List<List<T>>();
            foreach (List<int> group in rst.Values)
            {
                List<T> tmp = new List<T>();
                foreach (int idx in group)
                {
                    tmp.Add(mObjects[idx]);
                }
                groupResult.Add(tmp);
            }
            return groupResult;
        }
        private void GeneratorPairs(ref List<ClosestPair> pairs)
        {
            pairs.Clear();
            int size = mObjects.Count;
            for (int i = 0; i < size - 1; ++i)
            {
                T objectI = mObjects[i];
                for (int j = i + 1; j < size; ++j)
                {
                    if (objectI.IsCloseTo(mObjects[j]))
                    {
                        pairs.Add(new ClosestPair(i, j));
                    }
                }
            }
        }

    }



}
