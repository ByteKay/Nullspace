
namespace Nullspace
{
    public class GestureEndMoveEvent : BaseGestureEvent
    {
        public GestureEndMoveEvent(int x, int y, long time, int touchCount) : base(x, y, time, touchCount)
        {
            mEventType = GestureEventType.GESTURE_END_MOVE;
        }

        public override bool IsValid() { return true; }
    }
}
