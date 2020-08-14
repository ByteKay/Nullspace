
namespace Nullspace
{
    // 按时间间隔执行
    public class TimeIntervalCallback : BehaviourCallback
    {
        // Process 执行时间间隔
        protected float mInterval = 0;
        // 是否连续执行。不连续，每执行一次置空
        protected bool mIsContinuous;
        // 执行Begin后，未超过一个Interval时，是否执行一次Process
        protected bool mIsFirstProcess;
        
        internal TimeIntervalCallback(float startTime, float duration, Callback begin = null, Callback process = null, Callback end = null) : base(startTime, duration, begin, process, end)
        {
            SetTimeInfo();
        }

        internal TimeIntervalCallback(Callback begin = null, Callback process = null, Callback end = null) : base( begin, process, end)
        {
            SetTimeInfo();
        }

        /// <summary>
        /// 默认每帧执行
        /// 连续执行
        /// 首次执行
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="isContinuous"></param>
        /// <param name="firstProcess"></param>
        internal void SetTimeInfo(float interval = 0, bool isContinuous = true, bool isFirstProcess = true)
        {
            mInterval = interval;
            mIsContinuous = isContinuous;
            mIsFirstProcess = isFirstProcess;
        }
    }
}
