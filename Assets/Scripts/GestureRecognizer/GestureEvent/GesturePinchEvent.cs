
namespace Nullspace
{
    public class GesturePinchEvent : BaseGestureEvent
    {
        public GesturePinchEvent(int x, int y, long time, int touchCount, float scale) : base(x, y, time, touchCount)
        {
            mEventType = GestureEventType.GESTURE_PINCH;
            mFloatParameter = scale;
        }

        public float GetScale() { return mFloatParameter; }
        public override bool IsValid() { return true; }
    }
}
