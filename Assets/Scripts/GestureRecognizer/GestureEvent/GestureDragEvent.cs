
namespace Nullspace
{
    public class GestureDragEvent : BaseGestureEvent
    {
        public GestureDragEvent(int x, int y, long time, int touchCount) : base(x, y, time, touchCount)
        {
            mEventType = GestureEventType.GESTURE_DRAG;
        }
        public override bool IsValid() { return true; }
    }
}
