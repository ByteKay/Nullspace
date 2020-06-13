
namespace Nullspace
{
    public class BaseGestureEvent
    {
        protected byte mEventType;
        protected int mEventX;
        protected int mEventY;
        protected int mTouchCount;
        protected long mEventTime;
        protected float mFloatParameter;
        protected int mIntParameter;
        protected int mIntParameter1;

        public BaseGestureEvent()
        {
            mEventX = mEventY = 0;
            mTouchCount = 1;
            mEventType = GestureEventType.GESTURE_UNKNOWN;
            mFloatParameter = 0.0f;
            mIntParameter = 0;
        }

        public BaseGestureEvent(int x, int y, long time, int touchCount)
        {
            mEventX = x;
            mEventY = y;
            mEventTime = time;
            mTouchCount = touchCount;
            mEventType = GestureEventType.GESTURE_UNKNOWN;
            mFloatParameter = 0.0f;
            mIntParameter = 0;
        }

        public byte GetEventType() { return mEventType; }
        public int GetEventX() { return mEventX; }
        public int GetEventY() { return mEventY; }
        public void GetEventCoordinate(ref int x, ref int y) { x = mEventX; y = mEventY; }
        public long GetEventTime() { return mEventTime; }
        public int GetTouchCount() { return mTouchCount; }
        public virtual bool IsValid() { return false; }
    }
}
