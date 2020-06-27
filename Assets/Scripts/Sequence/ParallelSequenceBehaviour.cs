using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{

    public class ParallelSequenceBehaviour : IUpdate
    {
        public static ParallelSequenceBehaviour Create()
        {
            ParallelSequenceBehaviour sb = new ParallelSequenceBehaviour();
            SequenceBehaviourManager.Instance.AddSequence(sb);
            return sb;
        }

        public float TimeLine{ get; set; }
        private List<BehaviourTimeCallback> Behaviours;
        private ThreeState State;
        private AbstractCallback OnCompletedCallback;
        private float PrependTime;
        private float MaxDuration;

        private ParallelSequenceBehaviour()
        {
            TimeLine = 0;
            State = ThreeState.Ready;
            Behaviours = new List<BehaviourTimeCallback>();
            OnCompletedCallback = null;
            PrependTime = 0.0f;
            MaxDuration = 0.0f;
        }

        public void Append(BehaviourTimeCallback callback, float duration)
        {
            if (State == ThreeState.Ready)
            {
                // 以当前最大结束时间作为开始时间点
                Insert(MaxDuration, callback, duration);
            }
        }

        public void InsertCallback(float time, Action callback)
        {
            Callback inst = new Callback();
            inst.Handler = callback;
            InsertCallback(time, inst);
        }

        public void InsertCallback<T>(float time, Action<T> callback, T arg1)
        {
            Callback<T> inst = new Callback<T>();
            inst.Handler = callback;
            inst.Arg1 = arg1;
            InsertCallback(time, inst);
        }

        public void InsertCallback<T, U>(float time, Action<T, U> callback, T arg1, U arg2)
        {
            Callback<T, U> inst = new Callback<T, U>();
            inst.Handler = callback;
            inst.Arg1 = arg1;
            inst.Arg2 = arg2;
            InsertCallback(time, inst);
        }

        public void InsertCallback<T, U, V>(float time, Action<T, U, V> callback, T arg1, U arg2, V arg3)
        {
            Callback<T, U, V> inst = new Callback<T, U, V>();
            inst.Handler = callback;
            inst.Arg1 = arg1;
            inst.Arg2 = arg2;
            inst.Arg3 = arg3;
            InsertCallback(time, inst);
        }

        public void InsertCallback<T, U, V, W>(float time, Action<T, U, V, W> callback, T arg1, U arg2, V arg3, W arg4)
        {
            Callback<T, U, V, W> inst = new Callback<T, U, V, W>();
            inst.Handler = callback;
            inst.Arg1 = arg1;
            inst.Arg2 = arg2;
            inst.Arg3 = arg3;
            inst.Arg4 = arg4;
            InsertCallback(time, inst);
        }

        private void InsertCallback(float time, AbstractCallback callback)
        {
            if (State == ThreeState.Ready)
            {
                // 以当前最大结束时间作为开始时间点
                Insert(time, new BehaviourTimeCallback(callback), 0);
            }
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


        public void OnComplete(Action callback)
        {
            Callback inst = new Callback();
            inst.Handler = callback;
            OnComplete(inst);
        }

        public void OnComplete<T>(Action<T> callback, T arg1)
        {
            Callback<T> inst = new Callback<T>();
            inst.Handler = callback;
            inst.Arg1 = arg1;
            OnComplete(inst);
        }

        public void OnComplete<T, U>(Action<T, U> callback, T arg1, U arg2)
        {
            Callback<T, U> inst = new Callback<T, U>();
            inst.Handler = callback;
            inst.Arg1 = arg1;
            inst.Arg2 = arg2;
            OnComplete(inst);
        }

        public void OnComplete<T, U, V>(Action<T, U, V> callback, T arg1, U arg2, V arg3)
        {
            Callback<T, U, V> inst = new Callback<T, U, V>();
            inst.Handler = callback;
            inst.Arg1 = arg1;
            inst.Arg2 = arg2;
            inst.Arg3 = arg3;
            OnComplete(inst);
        }

        public void OnComplete<T, U, V, W>(Action<T, U, V, W> callback, T arg1, U arg2, V arg3, W arg4)
        {
            Callback<T, U, V, W> inst = new Callback<T, U, V, W>();
            inst.Handler = callback;
            inst.Arg1 = arg1;
            inst.Arg2 = arg2;
            inst.Arg3 = arg3;
            inst.Arg4 = arg4;
            OnComplete(inst);
        }
        private void OnComplete(AbstractCallback callback)
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
                float timeElappsed = TimeLine - PrependTime;
                bool completed = true;
                foreach (BehaviourTimeCallback beh in Behaviours)
                {
                    // 只要有一个没执行完，就代表没结束
                    if (!beh.IsFinished && beh.Update(timeElappsed))
                    {
                        completed = false;
                    }
                }
                if (completed && OnCompletedCallback != null)
                {
                    State = ThreeState.Finished;
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
