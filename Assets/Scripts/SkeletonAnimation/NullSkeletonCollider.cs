using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullAnimation
{
    public class NullSkeletonCollider
    {
        private SkeletonColliderList mOwner;
        private NullSkeletonBone mSkeletonBone;
        private uint mBoneId;
        private Matrix4x4 mLocalMatrix;
        private Matrix4x4 mGlobalMatrix;
        private bool mAutoUpdate;
    }


    public class SkeletonColliderList
    {
        private NullSkeleton mOwner;
        protected List<NullSkeletonCollider> mColliders;
    }
    
}
