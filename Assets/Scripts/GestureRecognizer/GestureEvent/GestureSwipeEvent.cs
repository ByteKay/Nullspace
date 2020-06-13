
namespace Nullspace
{
    public class GestureSwipeEvent : BaseGestureEvent
    {
        public GestureSwipeEvent(int x, int y, long time, int touchCount, TouchDirection direction) : base(x, y, time, touchCount)
        {
            mEventType = GestureEventType.GESTURE_LONG_TAP;
            mIntParameter = (int)direction;
        }
        public TouchDirection GetDirection() { return EnumUtils.IntToEnum<TouchDirection>(mIntParameter); }
        public override bool IsValid() { return true; }
    }
}
