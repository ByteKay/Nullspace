
namespace Nullspace
{
    /// <summary>
    /// 按时间调用
    /// </summary>
    public class TimeCallback : BehaviourCallback
    {
        private int mCurrentSeconds = 0;

        internal TimeCallback(float startTime, float duration, AbstractCallback begin = null, AbstractCallback process = null, AbstractCallback end = null) : base(startTime, duration, begin, process, end)
        {

        }

        internal TimeCallback(AbstractCallback begin = null, AbstractCallback process = null, AbstractCallback end = null) : base( begin, process, end)
        {

        }

        internal override void Reset()
        {
            base.Reset();
            mCurrentSeconds = 0;
        }

        internal override void Process()
        {
            int elappsedSeconds = (int)mTimeElappsed - (int)StartTime;
            if (elappsedSeconds > mCurrentSeconds)
            {
                mCurrentSeconds = elappsedSeconds;
                base.Process();
            }
        }
    }

}
