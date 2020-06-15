
namespace Nullspace
{
    public class TouchQueueInfomation
    {
        public TouchQueue touchQueue;
        public long releaseTime;
        public TouchQueueChangingMode lastChangingMode;
        public int repeatTimes;

        public TouchState curState;
        public TouchQueueInfomation()
        {
            touchQueue = null;
            releaseTime = 0;
            lastChangingMode = TouchQueueChangingMode.TQC_NONE;
            repeatTimes = 0;
            curState = TouchState.STATE_NONE;
        }

        public TouchQueueInfomation(TouchQueue queue, TouchQueueChangingMode changingMode, long time)
        {
            curState = TouchState.STATE_NONE;
            touchQueue = queue;
            releaseTime = time;
            lastChangingMode = changingMode;
            repeatTimes = 0;
            if (touchQueue.IsActived())
            {
                curState = TouchState.STATE_TAP;
            }
        }

        public bool IsEmpty() { return touchQueue == null || releaseTime == 0; }
    }
}
