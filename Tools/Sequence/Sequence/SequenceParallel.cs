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
        private ISequnceUpdate mSibling;
        internal SequenceParallel()
        {
            mTimeLine = 0;
            mState = ThreeState.Ready;
            mBehaviours = new List<BehaviourCallback>();
            mOnCompletedCallback = null;
            mPrependTime = 0.0f;
            mMaxDuration = 0.0f;
            mSibling = null;
        }
        public ISequnceUpdate Sibling
        {
            get
            {
                return mSibling;
            }

            set
            {
                mSibling = value;
            }
        }

        public bool IsPlaying { get { return mState == ThreeState.Playing; } }

        void ISequnceUpdate.Next()
        {
            // todo
        }
        //////////////////////////////////////////////////////////////////////

        public void OnCompletion(Callback callback)
        {
            mOnCompletedCallback = callback;
        }

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
            mTimeLine = 0;
            mState = ThreeState.Playing;
        }

        public void Kill()
        {
            mState = ThreeState.Finished;
            mBehaviours.Clear();
        }

        /// <summary>
        /// 整体后延
        /// </summary>
        /// <param name="interval">秒</param>        
        public void PrependInterval(float interval)
        {
            mPrependTime = interval;
        }

        public float Append(Callback process, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(null, process, null);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            return Append(bc, duration);
        }

        public float Append(Callback begin, Callback end, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(begin, null, end);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            return Append(bc, duration);
        }

        public float Append(Callback begin, Callback process, Callback end, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(begin, process, end);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            return Append(bc, duration);
        }

        public float AppendFrame(Callback process, float duration, float interval, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            return AppendFrame(null, process, null, duration, interval, forceProcess);
        }

        public float AppendFrame(Callback process, float duration, int targetFrameCount, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            return AppendFrame(null, process, null, duration, targetFrameCount, forceProcess);
        }

        // duration / interval
        public float AppendFrame(Callback begin, Callback process, Callback end, float duration, float interval, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            DebugUtils.Assert(interval > 0, "");
            FrameCountCallback fc = new FrameCountCallback(begin, process, end);
            // 以当前最大结束时间作为开始时间点
            fc.SetStartDurationTime(mMaxDuration, duration);
            fc.SetIntervalTime(interval);
            fc.SetForceProcess(forceProcess);
            // 设置所属 sequence
            return Append(fc, duration);
        }

        // duration targetFrameCount
        public float AppendFrame(Callback begin, Callback process, Callback end, float duration, int targetFrameCount, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            DebugUtils.Assert(targetFrameCount >= 0, "");
            FrameCountCallback fc = new FrameCountCallback(begin, process, end);
            // 以当前最大结束时间作为开始时间点
            fc.SetStartDurationTime(mMaxDuration, duration);
            fc.SetFrameCount(targetFrameCount);
            fc.SetForceProcess(forceProcess);
            return Append(fc, duration);
        }

        //////////////////////////////////////////////////////////////////////////////

        public float Insert(float time, Callback process, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(null, process, null);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            return Insert(time, bc, duration);
        }

        public float Insert(float time, Callback begin, Callback end, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(begin, null, end);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            return Insert(time, bc, duration);
        }

        public float Insert(float time, Callback begin, Callback process, Callback end, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(begin, process, end);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            return Insert(time, bc, duration);
        }

        public float InsertFrame(float time, Callback process, float duration, float interval, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            return InsertFrame(time, null, process, null, duration, interval, forceProcess);
        }

        public float InsertFrame(float time, Callback process, float duration, int targetFrameCount, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            return InsertFrame(time, null, process, null, duration, targetFrameCount, forceProcess);
        }

        // duration / interval
        public float InsertFrame(float time, Callback begin, Callback process, Callback end, float duration, float interval, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            DebugUtils.Assert(interval > 0, "");
            FrameCountCallback fc = new FrameCountCallback(begin, process, end);
            // 以当前最大结束时间作为开始时间点
            fc.SetStartDurationTime(mMaxDuration, duration);
            fc.SetIntervalTime(interval);
            fc.SetForceProcess(forceProcess);
            // 设置所属 sequence
            return Insert(time, fc, duration);
        }

        // duration targetFrameCount
        public float InsertFrame(float time, Callback begin, Callback process, Callback end, float duration, int targetFrameCount, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            DebugUtils.Assert(targetFrameCount >= 0, "");
            FrameCountCallback fc = new FrameCountCallback(begin, process, end);
            // 以当前最大结束时间作为开始时间点
            fc.SetStartDurationTime(mMaxDuration, duration);
            fc.SetFrameCount(targetFrameCount);
            fc.SetForceProcess(forceProcess);
            return Insert(time, fc, duration);
        }

        //////////////////////////////////////////////////////////////////////

        public float Append(BehaviourCallback callback, float duration)
        {
            // 以当前最大结束时间作为开始时间点
            Insert(mMaxDuration, callback, duration);
            return mMaxDuration;
        }

        public float Insert(float time, BehaviourCallback callback, float duration)
        {
            callback.SetStartDurationTime(time, duration);
            mBehaviours.Add(callback);
            mMaxDuration = Math.Max(mMaxDuration, time + duration);
            mBehaviours.Sort(BehaviourCallback.SortInstance);
            return callback.EndTime;
        }



    }
}
