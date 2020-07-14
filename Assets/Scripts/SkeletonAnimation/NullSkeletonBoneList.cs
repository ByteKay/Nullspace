
using System.Collections.Generic;
using UnityEngine;

namespace NullAnimation
{
    public class NullSkeletonBoneList
    {
        protected NullSkeleton mSkeleton;
        protected Matrix4x4 mInverseBindPos;
        protected Vector3 mBoneTranslation;
        protected Quaternion mBoneRotation;
        protected Vector4 mBoneColor;
        protected uint mBoneGroupId;
        protected List<NullSkeletonBone> mBoneList;

        public NullSkeletonBoneList(NullSkeleton owner)
        {
            mSkeleton = owner;
        }

    }
}
