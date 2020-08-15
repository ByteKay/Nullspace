
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
        private float mPrependTime;
        private bool mIsPlaying;
        // for tree
        internal SequenceLinkedList NextSibling { get; set; }

        internal SequenceLinkedList()
        {
            mBehaviours = new LinkedList<BehaviourCallback>();
            mOnCompleted = null;
            mCurrent = null;
            mMaxDuration = 0;
            mTimeLine = 0;
            mPrependTime = 0;
            NextSibling = null;
            mIsPlaying = false;
        }

        public bool IsPlaying
        {
            get
            {
                return mIsPlaying;
            }
        }


        public void PrependInterval(float interval)
        {
            mPrependTime = interval;
        }

        public void Append(Callback process, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(null, process, null);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            bc.mSequence = this;
            mBehaviours.AddLast(bc);
            mMaxDuration += duration;
        }

        public void Append(Callback begin, Callback end, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(begin, null, end);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            bc.mSequence = this;
            mBehaviours.AddLast(bc);
            mMaxDuration += duration;
        }

        public void Append(Callback begin, Callback process, Callback end, float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            BehaviourCallback bc = new BehaviourCallback(begin, process, end);
            // 以当前最大结束时间作为开始时间点
            bc.SetStartDurationTime(mMaxDuration, duration);
            // 设置所属 sequence
            bc.mSequence = this;
            mBehaviours.AddLast(bc);
            mMaxDuration += duration;
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
            fc.mSequence = this;
            mBehaviours.AddLast(fc);
            mMaxDuration += duration;
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
            // 设置所属 sequence
            fc.mSequence = this;
            mBehaviours.AddLast(fc);
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
            Next();
        }

        internal void Next()
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
            mIsPlaying = true;
            mTimeLine += deltaTime;
            if (mTimeLine < mPrependTime)
            {
                return;
            }
            ConsumeChild();
            if (mCurrent != null)
            {
                mCurrent.Update(mTimeLine - mPrependTime);
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
                mIsPlaying = false;
                if (mOnCompleted != null)
                {
                    mOnCompleted.Run();
                }
            }

        }

    }

}
