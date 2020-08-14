
namespace Nullspace
{
    /// <summary>
    /// 按帧数调用：注意最后一帧可能发生在 End 回调
    /// 
    /// mTargetFrameCount = 0 时，表示 Process 不执行
    /// 
    /// </summary>
    public class FrameCountCallback : BehaviourCallback
    {
        // 已执行帧数
        protected int mElappsedFrameCount = 0;
        // 目标帧数
        protected int mTargetFrameCount = 0;
        // 每秒多少帧， 提升精度，使用 double 
        protected double mFrameCountPerSecond = 0;

        /// <summary>
        /// duration 与 targetFrameCount 对应
        /// </summary>
        /// <param name="startTime">起始时间点</param>
        /// <param name="duration">持续时长</param>
        /// <param name="targetFrameCount">持续帧数</param>
        /// <param name="begin">开始时回调</param>
        /// <param name="process">过程中回调</param>
        /// <param name="end">结束时回调</param>
        internal FrameCountCallback(float startTime, float duration, int targetFrameCount, Callback begin = null, Callback process = null, Callback end = null) : base(startTime, duration, begin, process, end)
        {
            SetFrameCount(targetFrameCount);
        }

        internal FrameCountCallback(Callback begin = null, Callback process = null, Callback end = null) : base( begin, process, end)
        {
            SetFrameCount(0);
        }

        internal void SetFrameCount(int targetFrameCount)
        {
            mTargetFrameCount = targetFrameCount;
            ResetFrameCountPerSecond();
        }

        protected internal override void SetStartTime(float startTime, float duration)
        {
            base.SetStartTime(startTime, duration);
            ResetFrameCountPerSecond();
        }

        private void ResetFrameCountPerSecond()
        {
            if (mDuration > 0)
            {
                mFrameCountPerSecond = mTargetFrameCount / mDuration;
            }
            else
            {
                mFrameCountPerSecond = 0;
            }
        }

        internal override void Reset()
        {
            base.Reset();
            mElappsedFrameCount = 0;
        }

        /// <summary>
        /// 走过的总时间：时间减起始时间
        /// 目前的总帧数：走过的总时间 * 帧率(mFrameCountPerSecond)
        /// 目前可走帧数：走过的总帧数 - 已走过的帧数
        /// </summary>
        internal override void Process()
        {
            // mTimeElappsed 肯定 小于 mEndTime
            float elappsedTime = mTimeElappsed - StartTime;
            int elappsedFrames = (int)(elappsedTime * mFrameCountPerSecond);
            if (elappsedFrames < mTargetFrameCount)
            {
                elappsedFrames = elappsedFrames - mElappsedFrameCount;
                for (int i = 0; i < elappsedFrames; ++i)
                {
                    base.Process();
                }
                mElappsedFrameCount += elappsedFrames;
            }
            else
            {
                DebugUtils.Log(InfoType.Warning, "Process elappsedFrames > mTargetFrameCount");
            }
        }

    }

}
