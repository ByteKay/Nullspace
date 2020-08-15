using System;
using System.Collections.Generic;

namespace Nullspace
{
    public class SequenceParallel : ISequnceUpdate
    {
        protected float mTimeLine;
        private List<BehaviourCallback> mBehaviours;
        private ThreeState mState;
        private Callback mOnCompletedCallback;
        private float mPrependTime;
        private float mMaxDuration;

        internal SequenceParallel()
        {
            mTimeLine = 0;
            mState = ThreeState.Ready;
            mBehaviours = new List<BehaviourCallback>();
            mOnCompletedCallback = null;
            mPrependTime = 0.0f;
            mMaxDuration = 0.0f;
        }

        void ISequnceUpdate.Next()
        {
            // todo
        }

        /// <summary>
        /// 整体后延
        /// </summary>
        /// <param name="interval">秒</param>
        public void PrependInterval(float interval)
        {
            mPrependTime = interval;
        }

        public void Append(BehaviourCallback callback, float duration)
        {
            if (mState == ThreeState.Ready)
            {
                // 以当前最大结束时间作为开始时间点
                Insert(mMaxDuration, callback, duration);
            }
        }

        public void Insert(float time, BehaviourCallback callback, float duration)
        {
            if (mState == ThreeState.Ready)
            {
                callback.SetStartDurationTime(time, duration);
                mBehaviours.Add(callback);
                mMaxDuration = Math.Max(mMaxDuration, time + duration);
            }
        }

        public void OnComplete(Callback callback)
        {
            mOnCompletedCallback = callback;
        }

        public bool IsPlaying { get { return mState == ThreeState.Playing; } }

        /// <summary>
        /// 一次只会有一个行为执行
        /// </summary>
        /// <param name="deltaTime"></param>
        void ISequnceUpdate.Update(float deltaTime)
        {
            Update(deltaTime);
        }
        
        internal void Update(float time)
        {
            if (mState == ThreeState.Ready)
            {
                mBehaviours.Sort(BehaviourCallback.SortInstance);
                mState = ThreeState.Playing;
            }
            if (IsPlaying)
            {
                mTimeLine += time;
                if (mTimeLine < mPrependTime)
                {
                    return;
                }
                float timeElappsed = mTimeLine - mPrependTime;
                bool completed = true;
                foreach (BehaviourCallback beh in mBehaviours)
                {
                    // 只要有一个没执行完，就代表没结束
                    if (!beh.IsFinished && beh.Update(timeElappsed))
                    {
                        completed = false;
                    }
                }
                if (completed)
                {
                    mState = ThreeState.Finished;
                    if (mOnCompletedCallback != null)
                    {
                        mOnCompletedCallback.Run();
                    }
                }
            }
        }

        public void Replay()
        {
            if (mState == ThreeState.Finished)
            {
                mTimeLine = 0;
                mState = ThreeState.Playing;
            }
        }

        public void Kill()
        {
            mState = ThreeState.Finished;
            mBehaviours.Clear();
        }

    }
}
