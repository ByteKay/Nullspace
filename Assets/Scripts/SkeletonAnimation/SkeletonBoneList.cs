
using System.Collections.Generic;
using UnityEngine;

namespace Animation
{
    public class SkeletonBoneList
    {
        protected Skeleton mSkeleton;
        protected Matrix4x4 mInverseBindPos;
        protected Vector3 mBoneTranslation;
        protected Quaternion mBoneRotation;
        protected Vector4 mBoneColor;
        protected uint mBoneGroupId;
        protected List<SkeletonBone> mBoneList;

        public SkeletonBoneList(Skeleton owner)
        {
            mSkeleton = owner;
        }

    }
}
