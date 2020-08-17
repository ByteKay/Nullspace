using System;
using System.Collections.Generic;

namespace Nullspace
{
    public class SequenceParallel : ISequnceUpdate
    {
        protected float mTimeLine;
        protected List<BehaviourCallback> mBehaviours;
        protected ThreeState mState;
        protected Callback mOnCompletedCallback;
        protected float mPrependTime;
        protected float mMaxDuration;
        protected ISequnceUpdate mSibling;

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

        protected internal virtual void Update(float time)
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
                bool completion = true;
                for (int i = 0; i < mBehaviours.Count; ++i)
                {
                    // 只要有一个没执行完，就代表没结束
                    if (!mBehaviours[i].IsFinished && mBehaviours[i].Update(timeElappsed))
                    {
                        completion = false;
                    }
                }
                if (completion)
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
        /// 整体后延。确保最开始设置，否则  InsertDelay 有问题
        /// </summary>
        /// <param name="interval">秒</param>        
        public void PrependInterval(float interval)
        {
            mPrependTime = interval;
        }

        /// <summary>
        /// 相对当前 mPrependTime 的时间 delay。先设置好 mPrependTime
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="process"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public BehaviourCallback InsertDelay(float delay, Callback process, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(null, process, null);
            bc.SetStartDurationTime(mPrependTime + delay, duration);
            // 设置所属 sequence
            return Insert(mPrependTime + delay, bc, duration);
        }

        public BehaviourCallback Append(Callback process, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(null, process, null);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            return Append(bc, duration);
        }
        
        public BehaviourCallback Append(Callback begin, Callback end, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(begin, null, end);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            return Append(bc, duration);
        }

        public BehaviourCallback Append(Callback begin, Callback process, Callback end, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(begin, process, end);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            return Append(bc, duration);
        }

        public BehaviourCallback AppendFrame(Callback process, float duration, float interval, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            return AppendFrame(null, process, null, duration, interval, forceProcess);
        }

        public BehaviourCallback AppendFrame(Callback process, float duration, int targetFrameCount, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            return AppendFrame(null, process, null, duration, targetFrameCount, forceProcess);
        }

        // duration / interval
        public BehaviourCallback AppendFrame(Callback begin, Callback process, Callback end, float duration, float interval, bool forceProcess)
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
        public BehaviourCallback AppendFrame(Callback begin, Callback process, Callback end, float duration, int targetFrameCount, bool forceProcess)
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

        public BehaviourCallback Insert(float time, Callback process, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(null, process, null);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(time, duration);
            // 设置所属 sequence
            return Insert(time, bc, duration);
        }

        public BehaviourCallback Insert(float time, Callback begin, Callback end, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(begin, null, end);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(time, duration);
            // 设置所属 sequence
            return Insert(time, bc, duration);
        }

        public BehaviourCallback Insert(float time, Callback begin, Callback process, Callback end, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(begin, process, end);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(time, duration);
            // 设置所属 sequence
            return Insert(time, bc, duration);
        }

        public BehaviourCallback InsertFrame(float time, Callback process, float duration, float interval, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            return InsertFrame(time, null, process, null, duration, interval, forceProcess);
        }

        public BehaviourCallback InsertFrame(float time, Callback process, float duration, int targetFrameCount, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            return InsertFrame(time, null, process, null, duration, targetFrameCount, forceProcess);
        }

        // duration / interval
        public BehaviourCallback InsertFrame(float time, Callback begin, Callback process, Callback end, float duration, float interval, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            DebugUtils.Assert(interval > 0, "");
            FrameCountCallback fc = new FrameCountCallback(begin, process, end);
            // 以当前最大结束时间作为开始时间点
            fc.SetStartDurationTime(time, duration);
            fc.SetIntervalTime(interval);
            fc.SetForceProcess(forceProcess);
            // 设置所属 sequence
            return Insert(time, fc, duration);
        }

        // duration targetFrameCount
        public BehaviourCallback InsertFrame(float time, Callback begin, Callback process, Callback end, float duration, int targetFrameCount, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            DebugUtils.Assert(targetFrameCount >= 0, "");
            FrameCountCallback fc = new FrameCountCallback(begin, process, end);
            // 以当前最大结束时间作为开始时间点
            fc.SetStartDurationTime(time, duration);
            fc.SetFrameCount(targetFrameCount);
            fc.SetForceProcess(forceProcess);
            return Insert(time, fc, duration);
        }

        //////////////////////////////////////////////////////////////////////

        public BehaviourCallback Append(BehaviourCallback callback, float duration)
        {
            // 以当前最大结束时间作为开始时间点
            return Insert(mMaxDuration, callback, duration);
        }

        public BehaviourCallback Insert(float time, BehaviourCallback callback, float duration)
        {
            callback.SetStartDurationTime(time, duration);
            mBehaviours.Add(callback);
            mMaxDuration = Math.Max(mMaxDuration, time + duration);
            mBehaviours.Sort(BehaviourCallback.SortInstance);
            return callback;
        }

    }
}
