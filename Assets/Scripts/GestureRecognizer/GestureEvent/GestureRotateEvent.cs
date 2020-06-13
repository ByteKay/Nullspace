
namespace Nullspace
{
    public class GestureRotateEvent : BaseGestureEvent
    {
        public GestureRotateEvent(int x, int y, long time, int touchCount, float angle) : base(x, y, time, touchCount)
        {
            mEventType = GestureEventType.GESTURE_ROTATE;
            mFloatParameter = angle;
        }
        public float GetAngle() { return mFloatParameter; }
        public override bool IsValid() { return true; }
    }
}
