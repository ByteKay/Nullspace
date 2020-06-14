
using System.Collections.Generic;

namespace Nullspace
{
    /// <summary>
    /// 触发器排序：按触发点长度值
    /// </summary>
    public class PathTriggerSort : IComparer<PathTrigger>
    {
        public static PathTriggerSort TriggerSortInstance = new PathTriggerSort();

        public int Compare(PathTrigger x, PathTrigger y)
        {
            return x.mTriggerLength.CompareTo(y.mTriggerLength);
        }
    }
}
