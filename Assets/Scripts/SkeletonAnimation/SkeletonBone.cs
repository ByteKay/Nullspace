
namespace Animation
{
    public class SkeletonBone : SkeletonBoneList
    {
        private SkeletonBoneList mOwner;
        private uint mBoneId;
        private string mBoneName;

        public SkeletonBone(SkeletonBoneList owner, string name, uint id, uint groupId = 0) : base(null)
        {
            SetOwner(owner);
            mBoneName = name;
            mBoneId = id;
            mBoneGroupId = groupId;
        }


        public void SetOwner(SkeletonBoneList owner)
        {

        }
    }
}
