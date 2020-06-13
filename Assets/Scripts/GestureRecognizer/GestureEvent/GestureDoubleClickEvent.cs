
namespace Nullspace
{
    public class GestureDoubleClickEvent : BaseGestureEvent
    {
        public GestureDoubleClickEvent(int x, int y, long time, int touchCount) : base(x, y, time, touchCount)
        {
            mEventType = GestureEventType.GESTURE_DOUBLE_CLICK;
        }
        public override bool IsValid() { return true; }
    }
}
