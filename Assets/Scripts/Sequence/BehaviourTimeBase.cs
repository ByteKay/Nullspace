
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
        private float Duration;
        private float EndTime;
        private AbstractCallback Callback;
        private float TimeElappsed;
        private ThreeState State;

        public BehaviourTimeCallback(AbstractCallback behaviour)
        {
            TimeElappsed = 0;
            State = ThreeState.Ready;
            Callback = behaviour;
            SetStartTime(0, 0);
        }

        public void SetStartTime(float startTime, float duration)
        {
            StartTime = startTime;
            Duration = duration;
            EndTime = StartTime + Duration;
        }

        public void Reset()
        {
            TimeElappsed = 0;
            State = ThreeState.Ready;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time">step time</param>
        /// <returns>执行结束，返回 false; 否之，返回 true</returns>
        public bool Update(float time)
        {
            if (State == ThreeState.Finished)
            {
                return false;
            }
            TimeElappsed += time;
            if (TimeElappsed >= StartTime)
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
            return State != ThreeState.Finished;
        }

        public bool IsPlaying { get { return State == ThreeState.Playing; } }

        public float Elappsed { get { return Mathf.Clamp(TimeElappsed - StartTime, 0, Duration); } }

        public float Percent { get { return Mathf.Clamp((TimeElappsed - StartTime) / Duration, 0, 1); } }

        public virtual void Begin()
        {
            if (Callback != null)
            {
                Callback.Run();
            }
        }

        public virtual void Process()
        {
            if (Callback != null)
            {
                Callback.Run();
            }
        }
        public virtual void End()
        {
            if (Callback != null)
            {
                Callback.Run();
            }
        }
    }
}
