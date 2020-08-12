
using System.Collections.Generic;

namespace Nullspace
{


    public class SequenceSingle : IUpdate
    {
        private LinkedList<SingleCallback> Behaviours;
        private AbstractCallback OnCompleted;
        private SingleCallback Current;
        private float MaxDuration;
        private float TimeElappsed;
        internal SequenceSingle()
        {
            Behaviours = new LinkedList<SingleCallback>();
            OnCompleted = null;
            Current = null;
            MaxDuration = 0;
            TimeElappsed = 0;
        }

        public bool IsPlaying { get { return Current != null; } }

        public void Append(SingleCallback callback, float duration, bool playImmediate = false)
        {
            // 以当前最大结束时间作为开始时间点
            callback.SetStartTime(MaxDuration, duration);
            // 设置所属 sequence
            callback.Single = this;
            Behaviours.AddLast(callback);
            MaxDuration += duration;
            if (playImmediate)
            {
                ConsumeChild();
            }
        }

        public void PrependInterval(float duration)
        {
            Append(new SingleCallback(), duration);
        }

        public void OnCompletion(AbstractCallback onCompletion)
        {
            OnCompleted = onCompletion;
        }

        public void Stop()
        {
            Current = null;
            Behaviours.Clear();
        }

        /// <summary>
        /// 一次只会有一个行为执行
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            ConsumeChild();
            if (Current != null)
            {
                TimeElappsed += deltaTime;
                Current.Update(TimeElappsed);
            }
            else
            {
                if (OnCompleted != null)
                {
                    OnCompleted.Run();
                }
            }
        }

        internal SequenceSingle NextBrother { get; set; }

        internal void ConsumeChild()
        {
            if ((Current == null || Current.IsFinished))
            {
                Current = null;
            }
            if (Current == null && Behaviours.Count > 0)
            {
                Current = Behaviours.First.Value;
                Behaviours.RemoveFirst();
            }
        }

        internal void Next()
        {
            Current = null;
            ConsumeChild();
        }

    }

}
