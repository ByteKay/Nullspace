
namespace Nullspace
{
    // 延迟
    public class DelayCallback : BehaviourCallback
    {
        internal DelayCallback(float startTime, float duration) : base(null, null, null)
        {
            SetStartDurationTime(startTime, duration);
        }
    }

    // 等待
    public class WaitCallback : BehaviourCallback
    {
        internal WaitCallback(float startTime, float duration) : base(null, null, null)
        {
            SetStartDurationTime(startTime, duration);
        }
    }
}
