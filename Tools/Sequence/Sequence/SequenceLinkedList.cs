
using System;
using System.Collections.Generic;

namespace Nullspace
{
    public class SequenceLinkedList : ISequnceUpdate
    {
        private LinkedList<BehaviourCallback> mBehaviours;
        private Callback mOnCompleted;
        private BehaviourCallback mCurrent;
        private float mMaxDuration;
        private float mTimeLine;
        private ISequnceUpdate mSibling;
        internal SequenceLinkedList()
        {
            mBehaviours = new LinkedList<BehaviourCallback>();
            mOnCompleted = null;
            mCurrent = null;
            mMaxDuration = 0;
            mTimeLine = 0;
            mSibling = null;
        }

        public bool IsPlaying
        {
            get
            {
                return mCurrent != null;
            }
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

        public void AppendInterval(float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            DelayCallback ec = new DelayCallback(mMaxDuration, duration);
            Append(ec, duration);
        }

        public void Append(Callback process, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(null, process, null);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            Append(bc, duration);
        }

        public void Append(Callback begin, Callback end, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(begin, null, end);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            Append(bc, duration);
        }

        public void Append(Callback begin, Callback process, Callback end, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(begin, process, end);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            Append(bc, duration);
        }

        public void AppendFrame(Callback process, float duration, float interval, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            AppendFrame(null, process, null, duration, interval, forceProcess);
        }

        public void AppendFrame(Callback process, float duration, int targetFrameCount, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            AppendFrame(null, process, null, duration, targetFrameCount, forceProcess);
        }

        // duration / interval
        public void AppendFrame(Callback begin, Callback process, Callback end, float duration, float interval, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            DebugUtils.Assert(interval > 0, "");
            FrameCountCallback fc = new FrameCountCallback(begin, process, end);
            // 以当前最大结束时间作为开始时间点
            fc.SetStartDurationTime(mMaxDuration, duration);
            fc.SetIntervalTime(interval);
            fc.SetForceProcess(forceProcess);
            // 设置所属 sequence
            Append(fc, duration);
        }

        // duration targetFrameCount
        public void AppendFrame(Callback begin, Callback process, Callback end, float duration, int targetFrameCount, bool forceProcess)
        {
            DebugUtils.Assert(duration > 0, "");
            DebugUtils.Assert(targetFrameCount >= 0, "");
            FrameCountCallback fc = new FrameCountCallback(begin, process, end);
            // 以当前最大结束时间作为开始时间点
            fc.SetStartDurationTime(mMaxDuration, duration);
            fc.SetFrameCount(targetFrameCount);
            fc.SetForceProcess(forceProcess);
            Append(fc, duration);
        }

        public void Append(BehaviourCallback callback, float duration)
        {
            // 设置所属 sequence
            callback.mSequence = this;
            mBehaviours.AddLast(callback);
            mMaxDuration += duration;
        }

        public void OnCompletion(Callback onCompletion)
        {
            mOnCompleted = onCompletion;
        }

        public void Kill()
        {
            mCurrent = null;
            mTimeLine = 0;
            mBehaviours.Clear();
        }

        void ISequnceUpdate.Next()
        {
            NextCallback();
        }

        internal void NextCallback()
        {
            mCurrent = null;
            ConsumeChild();
        }

        /// <summary>
        /// 一次只会有一个行为执行
        /// </summary>
        /// <param name="deltaTime"></param>
        void ISequnceUpdate.Update(float deltaTime)
        {
            Update(deltaTime);
        }

        /// <summary>
        /// 一次只会有一个行为执行
        /// </summary>
        /// <param name="deltaTime"></param>
        internal void Update(float deltaTime)
        {
            mTimeLine += deltaTime;
            ConsumeChild();
            if (mCurrent != null)
            {
                mCurrent.Update(mTimeLine);
            }
        }

        internal void ConsumeChild()
        {
            if ((mCurrent == null || mCurrent.IsFinished))
            {
                mCurrent = null;
            }
            if (mCurrent == null && mBehaviours.Count > 0)
            {
                mCurrent = mBehaviours.First.Value;
                mBehaviours.RemoveFirst();
            }
            if (mCurrent == null)
            {
                if (mOnCompleted != null)
                {
                    mOnCompleted.Run();
                }
            }

        }
    }

}
