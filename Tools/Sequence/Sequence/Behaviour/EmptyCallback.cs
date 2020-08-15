
namespace Nullspace
{
    public class EmptyCallback : BehaviourCallback
    {
        internal EmptyCallback(float startTime, float duration) : base(null, null, null)
        {
            SetStartDurationTime(startTime, duration);
        }
    }
}
