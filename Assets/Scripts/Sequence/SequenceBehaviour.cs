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

        private SequenceBehaviour()
        {
            TimeLine = 0;
            State = ThreeState.Ready;
            Behaviours = new List<BehaviourTimeCallback>();
            OnCompletedCallback = null;
        }

        public void Insert(float time, BehaviourTimeCallback callback, float duration)
        {
            if (State == ThreeState.Ready)
            {
                callback.SetStartTime(time, duration);
                Behaviours.Add(callback);
            }
        }

        public void OnComplete(AbstractCallback callback)
        {
            OnCompletedCallback = callback;
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
                bool completed = true;
                foreach (BehaviourTimeCallback beh in Behaviours)
                {
                    // 只要有一个没执行完，就代表没结束
                    if (beh.Update(time))
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
