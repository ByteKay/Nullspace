using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Animation
{
    public class SkeletonCollider
    {
        private SkeletonColliderList mOwner;
        private SkeletonBone mSkeletonBone;
        private uint mBoneId;
        private Matrix4x4 mLocalMatrix;
        private Matrix4x4 mGlobalMatrix;
        private bool mAutoUpdate;
    }


    public class SkeletonColliderList
    {
        private Skeleton mOwner;
        protected List<SkeletonCollider> mColliders;
    }
    
}
