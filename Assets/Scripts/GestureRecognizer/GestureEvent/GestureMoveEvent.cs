
namespace Nullspace
{
    public class GestureMoveEvent : BaseGestureEvent
    {
        public GestureMoveEvent(int x, int y, long time, int touchCount) : base(x, y, time, touchCount)
        {
            mEventType = GestureEventType.GESTURE_MOVE;
        }

        public override bool IsValid() { return true; }
    }
}
