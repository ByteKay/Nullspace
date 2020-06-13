
namespace Nullspace
{
    public class GestureArcEvent : BaseGestureEvent
    {
        public GestureArcEvent(int x, int y, long time, int touchCount, TouchArcShape arcShape, TouchDirection direction) : base(x, y, time, touchCount)
        {
            mEventType = GestureEventType.GESTURE_LONG_TAP;
            mIntParameter = (int)arcShape;
            mIntParameter1 = (int)direction;
        }
        public TouchArcShape GetArcShape() { return EnumUtils.IntToEnum<TouchArcShape>(mIntParameter); }
        public TouchDirection GetDirection() { return EnumUtils.IntToEnum<TouchDirection>(mIntParameter1); }
        public override bool IsValid() { return true; }
    }
}
