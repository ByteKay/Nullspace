using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{

    public class SequenceBehaviour
    {
        public static SequenceBehaviour Create()
        {
            SequenceBehaviour sb = new SequenceBehaviour();
            SequenceBehaviourManager.Instance.AddSequence(sb);
            return sb;
        }

        private float TimeLine;
        private List<BehaviourTimeCallback> Behaviours;
        private ThreeState State;
        private AbstractCallback OnCompletedCallback;
        private float PrependTime;
        private float MaxDuration;

        private SequenceBehaviour()
        {
            TimeLine = 0;
            State = ThreeState.Ready;
            Behaviours = new List<BehaviourTimeCallback>();
            OnCompletedCallback = null;
            PrependTime = 0.0f;
            MaxDuration = 0.0f;
        }

        public void Insert(float time, BehaviourTimeCallback callback, float duration)
        {
            if (State == ThreeState.Ready)
            {
                callback.SetStartTime(time, duration);
                Behaviours.Add(callback);
                float target = time + duration;
                MaxDuration = Math.Max(MaxDuration, target);
            }
        }

        public void InsertCallback(float time, BehaviourTimeCallback callback)
        {
            if (State == ThreeState.Ready)
            {
                // 以当前最大结束时间作为开始时间点
                Insert(time, callback, 0);
            }
        }

        public void Append(BehaviourTimeCallback callback, float duration)
        {
            if (State == ThreeState.Ready)
            {
                // 以当前最大结束时间作为开始时间点
                Insert(MaxDuration, callback, duration);
            }
        }

        public void OnComplete(AbstractCallback callback)
        {
            OnCompletedCallback = callback;
        }

        public void PrependInterval(float interval)
        {
            PrependTime = interval;
        }

        public bool IsPlaying { get { return State == ThreeState.Playing; } }

        public void Update(float time)
        {
            if (State == ThreeState.Ready)
            {
                BehaviourSort sort = new BehaviourSort();
                Behaviours.Sort(sort);
                State = ThreeState.Playing;
            }
            if (IsPlaying)
            {
                TimeLine += time;
                if (TimeLine < PrependTime)
                {
                    return;
                }
                bool completed = true;
                foreach (BehaviourTimeCallback beh in Behaviours)
                {
                    // 只要有一个没执行完，就代表没结束
                    if (beh.Update(TimeLine))
                    {
                        completed = false;
                    }
                }
                if (completed && OnCompletedCallback != null)
                {
                    OnCompletedCallback.Run();
                }
            }
        }

        public void Replay()
        {
            if (State == ThreeState.Finished)
            {
                TimeLine = 0;
                State = ThreeState.Playing;
            }
        }

        public void Kill()
        {
            State = ThreeState.Finished;
        }
    }
}
