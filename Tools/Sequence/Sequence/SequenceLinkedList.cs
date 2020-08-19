
using System;
using System.Collections;
using System.Collections.Generic;

namespace Nullspace
{
    /// <summary>
    /// for decorator
    /// </summary>
    public partial class SequenceLinkedList
    {
        public void Pause(string tag)
        {
            // 是否为当前暂停器
            if (Current != null)
            {
                PauseCallback pc = Current as PauseCallback;
                if (pc != null && pc.Tag == tag)
                {
                    DebugUtils.Log(InfoType.Warning, "duplicated tag: " + tag);
                    return;
                }
            }
            // 队列中查找
            IEnumerator itr = mBehaviours.GetEnumerator();
            while (itr.MoveNext())
            {
                PauseCallback pc = itr.Current as PauseCallback;
                if (pc != null && pc.Tag == tag)
                {
                    DebugUtils.Log(InfoType.Warning, "duplicated tag: " + tag);
                    return;
                }
            }
            // unique tag
            float duration = 100000000f;
            PauseCallback ec = new PauseCallback(tag, mMaxDuration, duration);
            Append(ec, duration);
        }

        /// <summary>
        /// 这里指挥清理一个，添加的时候确保只有一个 Tag
        /// </summary>
        /// <param name="tag"></param>
        public bool Resume(string tag)
        {
            // 是否为当前暂停器
            if (Current != null)
            {
                PauseCallback pc = Current as PauseCallback;
                if (pc != null && pc.Tag == tag)
                {
                    NextCallback();
                    return true;
                }
            }
            // 队列中查找
            IEnumerator itr = mBehaviours.GetEnumerator();
            while (itr.MoveNext())
            {
                PauseCallback pc = itr.Current as PauseCallback;
                if (pc != null && pc.Tag == tag)
                {
                    mBehaviours.Remove((BehaviourCallback)itr.Current);
                    return true;
                }
            }
            return false;
        }

        public void Cooldown(float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            CooldownCallback ec = new CooldownCallback(mMaxDuration, duration);
            Append(ec, duration);
        }
    }

    public partial class SequenceLinkedList : ISequnceUpdate
    {
        protected LinkedList<BehaviourCallback> mBehaviours;
        protected Callback mOnCompleted;
        protected float mMaxDuration;
        protected float mTimeLine;
        protected ISequnceUpdate mSibling;

        internal BehaviourCallback Current;
        internal SequenceLinkedList()
        {
            mBehaviours = new LinkedList<BehaviourCallback>();
            mOnCompleted = null;
            Current = null;
            mMaxDuration = 0;
            mTimeLine = 0;
            mSibling = null;
        }

        public bool IsPlaying
        {
            get
            {
                return Current != null;
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

        internal float MaxDuration { get { return mMaxDuration; } }

        public void Pause(float duration)
        {
            DebugUtils.Assert(duration >= 0, "");
            PauseCallback ec = new PauseCallback(mMaxDuration, duration);
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
            Current = null;
            mTimeLine = 0;
            mBehaviours.Clear();
        }

        void ISequnceUpdate.Next()
        {
            NextCallback();
        }

        internal void NextCallback()
        {
            Current = null;
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
            if (Current != null)
            {
                Current.Update(mTimeLine);
            }
        }

        internal void ConsumeChild()
        {
            if ((Current == null || Current.IsFinished))
            {
                Current = null;
            }
            if (Current == null && mBehaviours.Count > 0)
            {
                Current = mBehaviours.First.Value;
                mBehaviours.RemoveFirst();
            }
            if (Current == null)
            {
                if (mOnCompleted != null)
                {
                    mOnCompleted.Run();
                }
            }

        }
    }

}
