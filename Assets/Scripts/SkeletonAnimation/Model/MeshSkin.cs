
namespace Animation
{
    public class MeshSkin
    {
        struct BindingWeight
        {
            ushort count;
            float[] weights;
            ushort[] boneIds;
        }

        struct TrackTargetBinding
        {
            SkeletonTrack track;
            Joint target;
            bool enabled;
        }

    }
}
