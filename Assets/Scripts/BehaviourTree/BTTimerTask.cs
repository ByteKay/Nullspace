﻿using System;
using Object = System.Object;

namespace Nullspace
{
    public class BTTimerTask
    {
        private long mCurrentTicks;
        private long mInterval;
        private long mNextTicks;
        private Callback<Object> mCallback = null;
        private bool isStop;
        public BTTimerTask(float interval, Action<Object> action)
        {
            if (action != null)
            {
                mCallback = new Callback<object>();
                mCallback.Handler = action;
            }
            mCurrentTicks = System.DateTime.Now.Ticks;
            mInterval = (long)(interval * 10000000);
            mNextTicks = mCurrentTicks + mInterval;
            isStop = true;
        }

        public bool Process(Object obj)
        {
            Start();
            if (System.DateTime.Now.Ticks > mNextTicks)
            {
                if (mCallback != null)
                {
                    mCallback.Arg1 = obj;
                    mCallback.Run();
                }
                Stop();
                return true;
            }
            return false;
        }

        private void Start()
        {
            if (isStop)
            {
                isStop = false;
                mCurrentTicks = System.DateTime.Now.Ticks;
                mNextTicks = mCurrentTicks + mInterval;
            }
        }

        public void Stop()
        {
            isStop = true;
            mCurrentTicks = 0;
        }
    }

}