
using System.Collections.Generic;

namespace Nullspace
{
    public class SequenceSingle : ISequnceUpdate
    {
        private LinkedList<UpdateCallback> mBehaviours;
        private Callback mOnCompleted;
        private UpdateCallback mCurrent;
        private float mMaxDuration;
        private float mTimeLine;
        // for tree
        internal SequenceSingle NextSibling { get; set; }

        internal SequenceSingle()
        {
            mBehaviours = new LinkedList<UpdateCallback>();
            mOnCompleted = null;
            mCurrent = null;
            mMaxDuration = 0;
            mTimeLine = 0;
            NextSibling = null;
        }

        public bool IsPlaying
        {
            get
            {
                return mCurrent != null;
            }
        }

        public void PrependInterval(float interval)
        {
            Append(new UpdateCallback(), interval);
        }

        public void Append(UpdateCallback callback, float duration)
        {
            // 以当前最大结束时间作为开始时间点
            callback.SetStartTime(mMaxDuration, duration);
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
            ConsumeChild();
            if (mCurrent != null)
            {
                mTimeLine += deltaTime;
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
            if (mCurrent == null && mOnCompleted != null)
            {
                mOnCompleted.Run();
            }
        }

    }

}
