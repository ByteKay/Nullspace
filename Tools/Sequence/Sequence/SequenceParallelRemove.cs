
using System;
using System.Collections.Generic;

namespace Nullspace
{
    public class SequenceParallelRemove : SequenceParallel
    {
        private static List<int> RemoveCompetionBehaviourCaches = new List<int>();
        // single thread
        private List<BehaviourCallback> BehaviourCaches = new List<BehaviourCallback>();
        protected bool mInternalLock = false;

        protected override internal void Update(float time)
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
                BehaviourCaches.Clear();
                mInternalLock = true;
                RemoveCompetionBehaviourCaches.Clear();
                for (int i = 0; i < mBehaviours.Count; ++i)
                {
                    // 只要有一个没执行完，就代表没结束
                    if (!mBehaviours[i].IsFinished)
                    {
                        mBehaviours[i].Update(timeElappsed);
                    }
                    if (mBehaviours[i].IsFinished)
                    {
                        RemoveCompetionBehaviourCaches.Add(i);
                    }
                }
                mInternalLock = false;
                for (int i = RemoveCompetionBehaviourCaches.Count - 1; i >= 0; --i)
                {
                    mBehaviours.RemoveAt(RemoveCompetionBehaviourCaches[i]);
                }
                foreach (BehaviourCallback bc in BehaviourCaches)
                {
                    mBehaviours.Remove(bc);
                }
                BehaviourCaches.Clear();
                if (mBehaviours.Count == 0)
                {
                    mState = ThreeState.Finished;
                    if (mOnCompletedCallback != null)
                    {
                        mOnCompletedCallback.Run();
                    }
                }
            }
        }

        public void Remove(BehaviourCallback bc)
        {
            if (mInternalLock)
            {
                BehaviourCaches.Add(bc);
            }
            else
            {
                mBehaviours.Remove(bc);
            }
        }

    }
}
