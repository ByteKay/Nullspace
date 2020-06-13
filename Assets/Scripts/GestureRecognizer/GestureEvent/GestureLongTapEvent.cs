
namespace Nullspace
{
    public class GestureLongTapEvent : BaseGestureEvent
    {
        public GestureLongTapEvent(int x, int y, long time, int touchCount, long duration) : base(x, y, time, touchCount)
        {
            mEventType = GestureEventType.GESTURE_LONG_TAP;
            mIntParameter = (int)duration;
        }
        public long GetDuration() { return mIntParameter; }
        public override bool IsValid() { return true; }
    }
}
