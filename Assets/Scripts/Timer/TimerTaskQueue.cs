using System;
using System.Diagnostics;

namespace Nullspace
{
    public class TimerTask
    {
        public TimerTask(TimerCallback callback)
        {
            Callback = callback;
        }

        public int TimerId { get; set; }

        public int Interval { get; set; }

        public int NextTick { get; set; }

        public TimerCallback Callback { get; set; }

        public void DoAction()
        {
            Callback.Run();
        }
    }

    public class TimerTaskQueue : Singleton<TimerTaskQueue>
    {
        private int mNextTimerId;
        private int mCurrentTick;
        private TimerPriorityQueue<int, TimerTask, int> mPriorityQueue;
        private Stopwatch mStopWatch;
        private readonly object mQueueLock = new object();

        private void Awake()
        {
            mPriorityQueue = new TimerPriorityQueue<int, TimerTask, int>();
            mStopWatch = new Stopwatch();
            mCurrentTick = 0;
            mNextTimerId = 0;
        }

        public int AddTimer(int start, int interval, Action handler)
        {
            Callback callback = new Callback();
            callback.Handler = handler;
            var p = GetTimerData(callback, start, interval);
            return AddTimer(p);
        }

        public int AddTimer<T>(int start, int interval, Action<T> handler, T arg1)
        {
            Callback<T> callback = new Callback<T>();
            callback.Arg1 = arg1;
            callback.Handler = handler;
            TimerTask p = GetTimerData(callback, start, interval);
            return AddTimer(p);
        }

        public int AddTimer<T, U>(int start, int interval, Action<T, U> handler, T arg1, U arg2)
        {
            Callback<T, U> callback = new Callback<T, U>();
            callback.Arg1 = arg1;
            callback.Arg2 = arg2;
            callback.Handler = handler;
            TimerTask p = GetTimerData(callback, start, interval);
            return AddTimer(p);
        }

        public int AddTimer<T, U, V>(int start, int interval, Action<T, U, V> handler, T arg1, U arg2, V arg3)
        {
            Callback<T, U, V> callback = new Callback<T, U, V>();
            callback.Arg1 = arg1;
            callback.Arg2 = arg2;
            callback.Arg3 = arg3;
            callback.Handler = handler;
            TimerTask p = GetTimerData(callback, start, interval);
            return AddTimer(p);
        }
        public int AddTimer<T, U, V, W>(int start, int interval, Action<T, U, V> handler, T arg1, U arg2, V arg3, W arg4)
        {
            Callback<T, U, V, W> callback = new Callback<T, U, V, W>();
            callback.Arg1 = arg1;
            callback.Arg2 = arg2;
            callback.Arg3 = arg3;
            callback.Arg4 = arg4;
            callback.Handler = handler;
            TimerTask p = GetTimerData(callback, start, interval);
            return AddTimer(p);
        }
        public void DelTimer(int timerId)
        {
            lock (mQueueLock)
            {
                mPriorityQueue.Remove(timerId);
            }
        }

        public void Tick()
        {
            mCurrentTick += (int)mStopWatch.ElapsedMilliseconds;
            mStopWatch.Reset();
            mStopWatch.Start();
            while (mPriorityQueue.Size != 0)
            {
                TimerTask p;
                lock (mQueueLock)
                {
                    p = mPriorityQueue.Peek();
                }
                if (mCurrentTick < p.NextTick)
                {
                    break;
                }
                lock (mQueueLock)
                {
                    mPriorityQueue.Dequeue();
                }
                if (p.Interval > 0)
                {
                    p.NextTick += p.Interval;
                    lock (mQueueLock)
                    {
                        mPriorityQueue.Enqueue(p.TimerId, p, p.NextTick);
                    }
                    p.DoAction();
                }
                else
                {
                    p.DoAction();
                }
            }
        }

        public void Reset()
        {
            mCurrentTick = 0;
            mNextTimerId = 0;
            mStopWatch.Stop();
            lock (mQueueLock)
            {
                while (mPriorityQueue.Size != 0)
                {
                    mPriorityQueue.Dequeue();
                }
            }
        }

        private int AddTimer(TimerTask p)
        {
            lock (mQueueLock)
            {
                mPriorityQueue.Enqueue(p.TimerId, p, p.NextTick);
            }
            return p.TimerId;
        }

        private TimerTask GetTimerData(TimerCallback callback, int start, int interval)
        {
            TimerTask task = new TimerTask(callback);
            task.Interval = interval;
            task.TimerId = NextTimerId;
            task.NextTick = mCurrentTick + 1 + start;
            return task;
        }

        public int NextTimerId { get { return ++mNextTimerId; } }

    }

}

