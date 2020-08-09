
using System.Collections.Generic;
using System.Linq;

namespace Nullspace
{
    public class SingleSequenceBehaviour : IUpdate
    {
        public static SingleSequenceBehaviour Create()
        {
            SingleSequenceBehaviour sb = new SingleSequenceBehaviour();
            SequenceBehaviourManager.Instance.AddSequence(sb);
            return sb;
        }

        private LinkedList<SingleBehaviourTimeCallback> Behaviours;
        private AbstractCallback OnCompletedCallback;
        private SingleBehaviourTimeCallback Current;
        private float MaxDuration;
        private float TimeElappsed;

        public SingleSequenceBehaviour()
        {
            Behaviours = new LinkedList<SingleBehaviourTimeCallback>();
            OnCompletedCallback = null;
            Current = null;
            MaxDuration = 0;
            TimeElappsed = 0;
        }

        public SingleSequenceBehaviour NextBrother { get; set; }

        public bool IsPlaying { get { return Current != null; } }

        public void Append(SingleBehaviourTimeCallback callback, float duration, bool playImmediate = false)
        {
            // 以当前最大结束时间作为开始时间点
            callback.SetStartTime(MaxDuration, duration);
            callback.Single = this;
            Behaviours.AddLast(callback);
            MaxDuration += duration;
            if (playImmediate)
            {
                ConsumeChild();
            }
        }

        public void ConsumeChild()
        {
            if (Current == null && Behaviours.Count > 0)
            {
                Current = Behaviours.ElementAt(0);
                Behaviours.RemoveFirst();
                TimeElappsed = Current.StartTime;
            }
        }

        public void Next()
        {
            Current = null;
            ConsumeChild();
        }

        public void Stop()
        {
            Current = null;
            Behaviours.Clear();
        }

        public void Update(float deltaTime)
        {
            ConsumeChild();
            if (Current != null)
            {
                TimeElappsed += deltaTime;
                Current.Update(TimeElappsed);
            }
        }
    }
}
