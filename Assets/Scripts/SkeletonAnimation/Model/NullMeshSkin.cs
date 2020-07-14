
namespace NullAnimation
{
    public class NullMeshSkin
    {
        struct BindingWeight
        {
            ushort count;
            float[] weights;
            ushort[] boneIds;
        }

        struct TrackTargetBinding
        {
            NullSkeletonTrack track;
            NullJoint target;
            bool enabled;
        }

    }
}
