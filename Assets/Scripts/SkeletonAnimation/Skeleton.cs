
using System.Collections.Generic;

namespace Animation
{
    public class Skeleton
    {
        private string mSkeletonName;
        private SkeletonRootBoneList mRootBones;
        private SkeletonAnimation mSkeletonAnimation;
        private List<SkeletonBone> mBonesByIdCache;
        private bool mRagDollEnabled;
        protected List<MeshMorpherDefine> mCachedAnimations;

        public class MeshMorpherDefine
        {
            protected MeshMorpher mMeshMorpher;
            protected List<uint> mBonesKeeped;
            protected Skeleton mOwner;
        }


    }
}
