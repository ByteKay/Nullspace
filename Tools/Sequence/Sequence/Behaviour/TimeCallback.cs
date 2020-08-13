
namespace Nullspace
{
    /// <summary>
    /// 按时间调用
    /// </summary>
    public class TimeCallback : BehaviourCallback
    {
        protected float mLastTimeLine = 0;
        protected float mInterval = float.MaxValue;
        // 
        protected bool mIsTimeContinuous = false;

        internal TimeCallback(float startTime, float duration, AbstractCallback begin = null, AbstractCallback process = null, AbstractCallback end = null) : base(startTime, duration, begin, process, end)
        {

        }

        internal TimeCallback(AbstractCallback begin = null, AbstractCallback process = null, AbstractCallback end = null) : base( begin, process, end)
        {

        }

        protected internal override void SetStartTime(float startTime, float duration)
        {
            base.SetStartTime(startTime, duration);
            mLastTimeLine = startTime;
        }

        internal override void Reset()
        {
            base.Reset();
            mLastTimeLine = StartTime;
        }

        internal override void Process()
        {
            if (CanProcess())
            {
                mLastTimeLine = mTimeElappsed;
                base.Process();
            }
        }

        internal bool CanProcess()
        {
            return (mTimeElappsed - mLastTimeLine) > mInterval;
        }
    }

}
