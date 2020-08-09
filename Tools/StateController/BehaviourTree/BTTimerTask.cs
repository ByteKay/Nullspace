using System;

namespace Nullspace
{
    /// <summary>
    /// 时间单位为 秒
    /// </summary>
    public class BTTimerTask
    {
        private float mCurrentSeconds;
        private float mInterval;
        private float mNextSeconds;
        private AbstractCallback mCallback = null;
        private bool isStop;
        public BTTimerTask(float interval, AbstractCallback callback)
        {
            mCallback = callback;
            mCurrentSeconds = DateTimeUtils.GetTimeStampSeconds();
            mInterval = interval;
            mNextSeconds = mCurrentSeconds + mInterval;
            isStop = true;
        }

        public bool Process<T>(T obj)
        {
            Start();
            if (DateTimeUtils.GetTimeStampSeconds() > mNextSeconds)
            {
                if (mCallback != null)
                {
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
                mCurrentSeconds = DateTimeUtils.GetTimeStampSeconds();
                mNextSeconds = mCurrentSeconds + mInterval;
            }
        }

        public void Stop()
        {
            isStop = true;
            mCurrentSeconds = 0;
        }
    }

}
