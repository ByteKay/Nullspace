
using System.Collections.Generic;
using System.Diagnostics;

namespace Nullspace
{
    public interface GestureListener
    {
        void GestureEvent(BaseGestureEvent gestureEvent);

    }
    public class TouchManager
    {
        private Stopwatch mTimer;
        private int mMaxTouchQueueCount;
        private LinkedList<TouchQueue> mActivedTouchQueue;
        private List<GestureListener> mGestureListeners;
        private List<TouchQueue> mTouchQueues;
        private BaseGestureRecognizer mGestureRecognizer;
        public TouchManager(int maxCount = 10)
        {
            mActivedTouchQueue = new LinkedList<TouchQueue>();
            mGestureListeners = new List<GestureListener>();
            mGestureRecognizer = BaseGestureRecognizer.Create(BaseGestureRecognizer.NAME_ID);
            mTimer = new Stopwatch();
            mTimer.Reset();
            mMaxTouchQueueCount = maxCount;
            mTouchQueues = new List<TouchQueue>();
            for (int i = 0; i < mMaxTouchQueueCount; i++)
            {
                mTouchQueues.Add(new TouchQueue(i));
            }
            mTimer.Start();
        }


        public void Clear()
        {
            mTimer.Stop();
            mTouchQueues.Clear();
            mActivedTouchQueue.Clear();
            mGestureListeners.Clear();
        }

        public virtual void AddTouch(int x, int y, int touchIndex)
        {
            if (touchIndex >= mMaxTouchQueueCount)
            {
                return;
            }
            mTouchQueues[touchIndex].AddTouch(x, y, mTimer.ElapsedMilliseconds);
            if (mGestureRecognizer != null)
            {
                mGestureRecognizer.TryAddTouchQueueChanging(mTouchQueues[touchIndex], TouchQueueChangingMode.TQC_PRESS, mTimer.ElapsedMilliseconds);
            }
        }

        public virtual void TouchMove(int x, int y, int touchIndex)
        {
            if (touchIndex >= mMaxTouchQueueCount)
            {
                return;
            }
            mTouchQueues[touchIndex].TouchMove(x, y, mTimer.ElapsedMilliseconds);
            if (mGestureRecognizer != null)
            {
                mGestureRecognizer.TryAddTouchQueueChanging(mTouchQueues[touchIndex], TouchQueueChangingMode.TQC_MOVE, mTimer.ElapsedMilliseconds);
            }
        }

        public virtual void ReleaseTouch(int x, int y, int touchIndex)
        {
            if (touchIndex >= mMaxTouchQueueCount)
            {
                return;
            }
            mTouchQueues[touchIndex].ReleaseTouch(x, y, mTimer.ElapsedMilliseconds);
            if (mGestureRecognizer != null)
            {
                mGestureRecognizer.TryAddTouchQueueChanging(mTouchQueues[touchIndex], TouchQueueChangingMode.TQC_RELEASE, mTimer.ElapsedMilliseconds);
            }
        }

        public virtual void Update()
        {
            if (!mTimer.IsRunning)
            {
                return;
            }
            mGestureRecognizer.Update(mTimer.ElapsedMilliseconds);
            BaseGestureEvent gestureEvent = mGestureRecognizer.GetCurrentGestureEvent();
            if (gestureEvent == null)
            {
                return;
            }
            for (int i = 0; i < mGestureListeners.Count; i++)
            {
                mGestureListeners[i].GestureEvent(gestureEvent);
            }
            mGestureRecognizer.ResetCurrentGesture();
        }

        public bool IsEnabled() { return mGestureRecognizer != null && (mGestureListeners.Count > 0); }

        public void RegisterGestureListener(GestureListener listener)
        {
            for (int i = 0; i < mGestureListeners.Count; i++)
            {
                if (listener == mGestureListeners[i])
                {
                    return;
                }
            }
            mGestureListeners.Add(listener);
            TryActiveTouchManager();
        }

        public void UnRegisterGestureListener(GestureListener listener)
        {
            int cnt = mGestureListeners.Count;
            for (int i = 0; i < cnt; ++i)
            {
                if (mGestureListeners[i] == listener)
                {
                    mGestureListeners.RemoveAt(i);
                    break;
                }
            }
            TryActiveTouchManager();
        }

        public void RegisterGestureRecognizer(string recognizerName)
        {
            if (mGestureRecognizer != null)
            {
                if (recognizerName.Equals(mGestureRecognizer.GetId()))
                {
                    return;
                }
            }
            //try create a new one
            mGestureRecognizer = BaseGestureRecognizer.Create(recognizerName);
            mGestureRecognizer.Initialize();
            TryActiveTouchManager();
        }

        private void TryActiveTouchManager()
        {
            bool lastEnabled = mTimer.IsRunning;
            bool currentEnabled = IsEnabled();
            if (lastEnabled == currentEnabled)
            {
                return;
            }
            if (currentEnabled)
            {
                mTimer.Start();
            }
            else
            {
                mTimer.Stop();
            }
        }
    }
}
