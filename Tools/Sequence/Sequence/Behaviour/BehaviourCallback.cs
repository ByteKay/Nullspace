
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
        internal float EndTime;
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
        internal BehaviourCallback(Callback begin = null, Callback process = null, Callback end = null)
        {
            mTimeElappsed = 0;
            mState = ThreeState.Ready;
            mBeginCallback = begin;
            mProcessCallback = process;
            mEndCallback = end;
            SetStartDurationTime(0, 0);
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

        protected internal virtual void SetStartDurationTime(float startTime, float duration)
        {
            StartTime = startTime;
            mDuration = duration;
            EndTime = StartTime + mDuration;
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

                }
                if (mTimeElappsed >= EndTime)
                {
                    if (mState == ThreeState.Playing)
                    {
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
        public float ElappsedTime { get { return Percent * mDuration; } }
        public float RemainTime { get { return RemainPercent * mDuration; } }
        public virtual float Percent
        {
            get
            {
                if (IsFinished)
                {
                    return 1;
                }
                if (!IsPlaying)
                {
                    return 0;
                }
                return MathUtils.Clamp((mTimeElappsed - StartTime) / mDuration, 0, 1);
            }
        }
        public float RemainPercent
        {
            get
            {
                return 1 - Percent;
            }
        }

        internal virtual void Begin()
        {
            if (mBeginCallback != null)
            {
                // 先执行 Begin ，后设置状态
                mBeginCallback.Run();
            }
            mState = ThreeState.Playing;
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
            // 先设置状态，后执行 End 
            mState = ThreeState.Finished;
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
