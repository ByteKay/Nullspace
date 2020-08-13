
using System.Collections.Generic;

namespace Nullspace
{


    public class SequenceSingle : IUpdate
    {
        private LinkedList<SingleCallback> mBehaviours;
        private Callback mOnCompleted;
        private SingleCallback mCurrent;
        private float mMaxDuration;
        private float mTimeLine;
        internal SequenceSingle NextBrother { get; set; }
        internal void Next()
        {
            mCurrent = null;
            ConsumeChild();
        }

        internal SequenceSingle()
        {
            mBehaviours = new LinkedList<SingleCallback>();
            mOnCompleted = null;
            mCurrent = null;
            mMaxDuration = 0;
            mTimeLine = 0;
            NextBrother = null;
        }

        public bool IsPlaying
        {
            get
            {
                return mCurrent != null;
            }
        }

        public void Append(SingleCallback callback, float duration, bool playImmediate = false)
        {
            // 以当前最大结束时间作为开始时间点
            callback.SetStartTime(mMaxDuration, duration);
            // 设置所属 sequence
            callback.Single = this;
            mBehaviours.AddLast(callback);
            mMaxDuration += duration;
            if (playImmediate)
            {
                ConsumeChild();
            }
        }

        public void PrependInterval(float duration)
        {
            Append(new SingleCallback(), duration);
        }

        public void OnCompletion(Callback onCompletion)
        {
            mOnCompleted = onCompletion;
        }

        public void Stop()
        {
            mCurrent = null;
            mBehaviours.Clear();
        }

        /// <summary>
        /// 一次只会有一个行为执行
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
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
