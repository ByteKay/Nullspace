
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
        protected float Duration;
        // EndTime = StartTime + Duration。结束时间点
        protected float EndTime;
        // 开始回调
        protected AbstractCallback BeginCallback;
        // 处理回调，可持续
        protected AbstractCallback ProcessCallback;
        // 结束回调
        protected AbstractCallback EndCallback;
        // 当前已走过的时长。相对起始时间0
        protected float TimeElappsed;
        // 当前状态：只有三个状态
        protected ThreeState State;
        // 只执行一次.起始时间等于结束时间
        protected bool IsOneShot;
        internal BehaviourCallback(float startTime, float duration, AbstractCallback process = null, AbstractCallback begin = null, AbstractCallback end = null)
        {
            TimeElappsed = 0;
            State = ThreeState.Ready;
            BeginCallback = begin;
            ProcessCallback = process;
            EndCallback = end;
            SetStartTime(startTime, duration);
        }

        internal BehaviourCallback(AbstractCallback process = null, AbstractCallback begin = null, AbstractCallback end = null)
        {
            TimeElappsed = 0;
            State = ThreeState.Ready;
            BeginCallback = begin;
            ProcessCallback = process;
            EndCallback = end;
            SetStartTime(0, 0);
        }

        internal BehaviourCallback Begin(AbstractCallback begin)
        {
            BeginCallback = begin;
            return this;
        }
        internal BehaviourCallback Process(AbstractCallback process)
        {
            ProcessCallback = process;
            return this;
        }
        internal BehaviourCallback End(AbstractCallback end)
        {
            EndCallback = end;
            return this;
        }
        internal void SetStartTime(float startTime, float duration)
        {
            StartTime = startTime;
            Duration = duration;
            EndTime = StartTime + Duration;
            IsOneShot = StartTime == EndTime;
        }
        internal virtual void Reset()
        {
            TimeElappsed = 0;
            State = ThreeState.Ready;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeLine">absolute time</param>
        /// <returns>执行结束，返回 false; 否之，返回 true</returns>
        internal bool Update(float timeLine)
        {
            if (State == ThreeState.Finished)
            {
                return false;
            }
            // 这里是绝对时长
            TimeElappsed = timeLine;
            if (TimeElappsed >= StartTime)
            {
                if (IsOneShot)
                {
                    // 此时 Duration == 0, 调用 Percent 可能会有问题
                    Process();
                    State = ThreeState.Finished;
                }
                else
                {
                    if (State == ThreeState.Ready)
                    {
                        Begin();
                        State = ThreeState.Playing;
                    }
                    if (TimeElappsed > EndTime)
                    {
                        if (State == ThreeState.Playing)
                        {
                            State = ThreeState.Finished;
                            End();
                        }
                    }
                    else
                    {
                        DebugUtils.Assert(State == ThreeState.Playing, "wrong");
                        Process();
                    }
                }
            }
            return State != ThreeState.Finished;
        }
        internal bool IsPlaying { get { return State == ThreeState.Playing; } }
        internal bool IsFinished { get { return State == ThreeState.Finished; } }
        internal float Elappsed { get { return MathUtils.Clamp(TimeElappsed - StartTime, 0, Duration); } }
        internal float Percent { get { return MathUtils.Clamp((TimeElappsed - StartTime) / Duration, 0, 1); } }
        internal virtual void Begin()
        {
            if (BeginCallback != null)
            {
                BeginCallback.Run();
            }
        }
        internal virtual void Process()
        {
            if (ProcessCallback != null)
            {
                ProcessCallback.Run();
            }
        }
        internal virtual void End()
        {
            if (EndCallback != null)
            {
                EndCallback.Run();
            }
        }
    }

}
