
namespace NullAnimation
{
    public class NullSkeletonBone : NullSkeletonBoneList
    {
        private NullSkeletonBoneList mOwner;
        private uint mBoneId;
        private string mBoneName;

        public NullSkeletonBone(NullSkeletonBoneList owner, string name, uint id, uint groupId = 0) : base(null)
        {
            SetOwner(owner);
            mBoneName = name;
            mBoneId = id;
            mBoneGroupId = groupId;
        }


        public void SetOwner(NullSkeletonBoneList owner)
        {

        }
    }
}
