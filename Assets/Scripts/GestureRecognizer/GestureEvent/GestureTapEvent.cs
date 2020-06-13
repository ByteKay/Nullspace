
namespace Nullspace
{
    public class GestureTapEvent : BaseGestureEvent
    {
        public GestureTapEvent(int x, int y, long time, int touchCount) : base(x, y, time, touchCount)
        {
            mEventType = GestureEventType.GESTURE_TAP;
        }

        public override bool IsValid() { return true; }
    }
}
