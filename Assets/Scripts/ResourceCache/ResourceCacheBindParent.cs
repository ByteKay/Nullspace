using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nullspace
{
    public class ResourceCacheBindParent
    {
        public static Transform CacheUnused = null;
        public static void Initialize()
        {
            CacheUnused = new GameObject("CacheUnused").transform;
        }

        public static bool IsCacheUnusedParent(GameObject go)
        {
            if (go != null)
            {
                return go.transform.parent.name == "CacheUnused";
            }
            return false;
        }

    }
}
