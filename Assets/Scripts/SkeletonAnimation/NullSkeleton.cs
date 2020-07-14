
using System.Collections.Generic;

namespace NullAnimation
{
    public class NullSkeleton
    {
        private string mSkeletonName;
        private NullSkeletonRootBoneList mRootBones;
        private NullSkeletonAnimation mSkeletonAnimation;
        private List<NullSkeletonBone> mBonesByIdCache;
        private bool mRagDollEnabled;
        protected List<NullMeshMorpherDefine> mCachedAnimations;

        public class NullMeshMorpherDefine
        {
            protected NullMeshMorpher mMeshMorpher;
            protected List<uint> mBonesKeeped;
            protected NullSkeleton mOwner;
        }


    }
}
