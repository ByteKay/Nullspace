
using System.Collections.Generic;

namespace Nullspace
{
    /// <summary>
    /// 基于时间点为 0 开始处理
    /// </summary>
    public class BehaviourCallback
    {
        internal static BehaviourSort SortInstance = new BehaviourSort();
        internal class BehaviourSort : IComparer<BehaviourCallback>
        {
            public int Compare(BehaviourCallback x, BehaviourCallback y)
            {
                return x.StartTime.CompareTo(y.StartTime);
            }
        }
        // 开始执行时间点
        internal float StartTime;
        // 持续时长
        protected float mDuration;
        // EndTime = StartTime + Duration。结束时间点
        protected float mEndTime;
        // 开始回调
        protected AbstractCallback mBeginCallback;
        // 处理回调，可持续
        protected AbstractCallback mProcessCallback;
        // 结束回调
        protected AbstractCallback mEndCallback;
        // 当前已走过的时长。相对起始时间0
        protected float mTimeElappsed;
        // 当前状态：只有三个状态
        protected ThreeState mState;
        // 只执行一次.起始时间等于结束时间
        protected bool mIsOneShot;
        internal BehaviourCallback(float startTime, float duration, AbstractCallback process = null, AbstractCallback begin = null, AbstractCallback end = null)
        {
            mTimeElappsed = 0;
            mState = ThreeState.Ready;
            mBeginCallback = begin;
            mProcessCallback = process;
            mEndCallback = end;
            SetStartTime(startTime, duration);
        }

        internal BehaviourCallback(AbstractCallback process = null, AbstractCallback begin = null, AbstractCallback end = null)
        {
            mTimeElappsed = 0;
            mState = ThreeState.Ready;
            mBeginCallback = begin;
            mProcessCallback = process;
            mEndCallback = end;
            SetStartTime(0, 0);
        }

        internal BehaviourCallback Begin(AbstractCallback begin)
        {
            mBeginCallback = begin;
            return this;
        }
        internal BehaviourCallback Process(AbstractCallback process)
        {
            mProcessCallback = process;
            return this;
        }
        internal BehaviourCallback End(AbstractCallback end)
        {
            mEndCallback = end;
            return this;
        }
        internal void SetStartTime(float startTime, float duration)
        {
            StartTime = startTime;
            mDuration = duration;
            mEndTime = StartTime + mDuration;
            mIsOneShot = StartTime == mEndTime;
        }
        internal virtual void Reset()
        {
            mTimeElappsed = 0;
            mState = ThreeState.Ready;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeLine">absolute time</param>
        /// <returns>执行结束，返回 false; 否之，返回 true</returns>
        internal bool Update(float timeLine)
        {
            if (mState == ThreeState.Finished)
            {
                return false;
            }
            // 这里是绝对时长
            mTimeElappsed = timeLine;
            if (mTimeElappsed >= StartTime)
            {
                if (mIsOneShot)
                {
                    // 此时 Duration == 0, 调用 Percent 可能会有问题
                    Process();
                    mState = ThreeState.Finished;
                }
                else
                {
                    if (mState == ThreeState.Ready)
                    {
                        Begin();
                        mState = ThreeState.Playing;
                    }
                    if (mTimeElappsed > mEndTime)
                    {
                        if (mState == ThreeState.Playing)
                        {
                            mState = ThreeState.Finished;
                            End();
                        }
                    }
                    else
                    {
                        DebugUtils.Assert(mState == ThreeState.Playing, "wrong");
                        Process();
                    }
                }
            }
            return mState != ThreeState.Finished;
        }
        internal bool IsPlaying { get { return mState == ThreeState.Playing; } }
        internal bool IsFinished { get { return mState == ThreeState.Finished; } }
        internal float Elappsed { get { return MathUtils.Clamp(mTimeElappsed - StartTime, 0, mDuration); } }
        internal float Percent { get { return MathUtils.Clamp((mTimeElappsed - StartTime) / mDuration, 0, 1); } }
        internal virtual void Begin()
        {
            if (mBeginCallback != null)
            {
                mBeginCallback.Run();
            }
        }
        internal virtual void Process()
        {
            if (mProcessCallback != null)
            {
                mProcessCallback.Run();
            }
        }
        internal virtual void End()
        {
            if (mEndCallback != null)
            {
                mEndCallback.Run();
            }
        }
    }

}
