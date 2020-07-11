using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Animation
{
    public class SkeletonRootBoneList : SkeletonBoneList
    {
        public SkeletonRootBoneList(Skeleton owner) : base(owner)
        {
            
        }

        public virtual void PrepareGlobalMatrices()
        {

        }
    }
}
