using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullAnimation
{
    public class NullSkeletonRootBoneList : NullSkeletonBoneList
    {
        public NullSkeletonRootBoneList(NullSkeleton owner) : base(owner)
        {
            
        }

        public virtual void PrepareGlobalMatrices()
        {

        }
    }
}
