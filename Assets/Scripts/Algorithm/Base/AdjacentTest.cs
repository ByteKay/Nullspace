
using System.Collections.Generic;

namespace Nullspace
{
    public class AdjacentTest : IAdjacent
    {
        public int mIndex;
        public int mNext;
        public float mWeight;
        public AdjacentTest(int idx, int next, float weight)
        {
            mIndex = idx;
            mNext = next;
            mWeight = weight;
        }

        public int Next()
        {
            return mNext;
        }
        public int Previous()
        {
            return mIndex;
        }

        public static void Test()
        {
            List<List<AdjacentTest>> all = new List<List<AdjacentTest>>();
            int count = 20;
            for (int i = 0; i < count - 1; ++i)
            {
                List<AdjacentTest> row = new List<AdjacentTest>();
                if (i != 1 && i != 10 && i != 15)
                {
                    for (int j = i + 1; j < count; ++j)
                    {
                        AdjacentTest test = new AdjacentTest(i, j, i * count + j);
                        row.Add(test);
                    }
                }
                all.Add(row);
            }
            List<AdjacentTest> group = new List<AdjacentTest>();
            List<List<AdjacentTest>> groups = new List<List<AdjacentTest>>();
            GeoAlgorithmUtils.AdjacentGraphTravel(all, groups, group);

            foreach (List<AdjacentTest> adj in groups)
            {
                string str = "";
                for (int i = 0; i < adj.Count; ++i)
                {
                    str = string.Format("{0} {1} {2}", str, adj[i].Previous(), adj[i].Next());
                }
                DebugUtils.Info("", "{0}", str);
            }
        }

    }
}
