
namespace Nullspace
{
    public class GestureDragMoveEvent : BaseGestureEvent
    {
        public GestureDragMoveEvent(int x, int y, long time, int touchCount) : base(x, y, time, touchCount)
        {
            mEventType = GestureEventType.GESTURE_DRAG_MOVE;
        }
        public override bool IsValid() { return true; }
    }
}
