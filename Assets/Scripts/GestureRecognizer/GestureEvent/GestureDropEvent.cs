
namespace Nullspace
{
    public class GestureDropEvent : BaseGestureEvent
    {
        public GestureDropEvent(int x, int y, long time, int touchCount) : base(x, y, time, touchCount)
        {
            mEventType = GestureEventType.GESTURE_DROP;
        }
        public override bool IsValid() { return true; }
    }
}
