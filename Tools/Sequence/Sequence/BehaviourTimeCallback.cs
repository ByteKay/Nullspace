
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{

    public class BehaviourSort : IComparer<BehaviourTimeCallback>
    {
        public int Compare(BehaviourTimeCallback x, BehaviourTimeCallback y)
        {
            return x.StartTime.CompareTo(y.StartTime);
        }
    }

    public class BehaviourTimeCallback
    {
        public float StartTime;
        protected float Duration;
        protected float EndTime;
        protected AbstractCallback BeginCallback;
        protected AbstractCallback ProcessCallback;
        protected AbstractCallback EndCallback;
        protected float TimeElappsed;
        protected ThreeState State;
        protected bool IsOneShot; // 只执行一次.起始时间等于结束时间
        public object Obj { get; set; }
        public BehaviourTimeCallback(AbstractCallback process = null, AbstractCallback begin = null, AbstractCallback end = null)
        {
            TimeElappsed = 0;
            State = ThreeState.Ready;
            BeginCallback = begin;
            ProcessCallback = process;
            EndCallback = end;
            SetStartTime(0, 0);
        }

        public BehaviourTimeCallback Begin(AbstractCallback begin)
        {
            BeginCallback = begin;
            return this;
        }

        public BehaviourTimeCallback Process(AbstractCallback process)
        {
            ProcessCallback = process;
            return this;
        }
        public BehaviourTimeCallback End(AbstractCallback end)
        {
            EndCallback = end;
            return this;
        }
        public void SetStartTime(float startTime, float duration)
        {
            StartTime = startTime;
            Duration = duration;
            EndTime = StartTime + Duration;
            IsOneShot = StartTime == EndTime;
        }

        public virtual void Reset()
        {
            TimeElappsed = 0;
            State = ThreeState.Ready;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeLine">absolute time</param>
        /// <returns>执行结束，返回 false; 否之，返回 true</returns>
        public bool Update(float timeLine)
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
                        Debug.Assert(State == ThreeState.Playing, "wrong");
                        Process();
                    }
                }
            }
            return State != ThreeState.Finished;
        }

        public bool IsPlaying { get { return State == ThreeState.Playing; } }
        public bool IsFinished { get { return State == ThreeState.Finished; } }
        public float Elappsed { get { return Mathf.Clamp(TimeElappsed - StartTime, 0, Duration); } }

        public float Percent { get { return Mathf.Clamp((TimeElappsed - StartTime) / Duration, 0, 1); } }

        public virtual void Begin()
        {
            if (BeginCallback != null)
            {
                BeginCallback.Run();
            }
        }

        public virtual void Process()
        {
            if (ProcessCallback != null)
            {
                ProcessCallback.Run();
            }
        }
        public virtual void End()
        {
            if (EndCallback != null)
            {
                EndCallback.Run();
            }
        }
    }
}
