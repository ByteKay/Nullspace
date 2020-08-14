
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
        protected Callback mBeginCallback;
        // 处理回调，可持续
        protected Callback mProcessCallback;
        // 结束回调
        protected Callback mEndCallback;
        // 当前已走过的时长。相对起始时间0
        protected float mTimeElappsed;
        // 当前状态：只有三个状态
        protected ThreeState mState;
        // 此行为所属序列实例
        internal ISequnceUpdate mSequence;

        internal BehaviourCallback(float startTime, float duration, Callback begin = null, Callback process = null, Callback end = null)
        {
            mTimeElappsed = 0;
            mState = ThreeState.Ready;
            mBeginCallback = begin;
            mProcessCallback = process;
            mEndCallback = end;
            SetStartTime(startTime, duration);
        }

        internal BehaviourCallback(Callback begin = null, Callback process = null, Callback end = null)
        {
            mTimeElappsed = 0;
            mState = ThreeState.Ready;
            mBeginCallback = begin;
            mProcessCallback = process;
            mEndCallback = end;
            SetStartTime(0, 0);
        }

        internal BehaviourCallback Begin(Callback begin)
        {
            mBeginCallback = begin;
            return this;
        }

        internal BehaviourCallback Process(Callback process)
        {
            mProcessCallback = process;
            return this;
        }

        internal BehaviourCallback End(Callback end)
        {
            mEndCallback = end;
            return this;
        }

        protected internal virtual void SetStartTime(float startTime, float duration)
        {
            StartTime = startTime;
            mDuration = duration;
            mEndTime = StartTime + mDuration;
        }

        internal virtual void Reset()
        {
            mTimeElappsed = 0;
            mState = ThreeState.Ready;
        }

        /// <summary>
        /// 每帧都会执行
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
                if (mState == ThreeState.Ready)
                {
                    Begin();
                    mState = ThreeState.Playing;
                }
                if (mTimeElappsed >= mEndTime)
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
            if (mSequence != null)
            {
                // 执行下一个
                mSequence.Next();
            }
        }
    }

}
