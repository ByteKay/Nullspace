
using System;
using System.Collections.Generic;

namespace Nullspace
{
    public class SequenceParallelDynamic : SequenceParallelFull
    {
        protected List<BehaviourCallback> RemoveBehaviourCaches = new List<BehaviourCallback>();

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
                AddBehaviourCaches.Clear();
                LockUpdate();
                for (int i = 0; i < mBehaviours.Count; ++i)
                {
                    if (!mBehaviours[i].IsFinished)
                    {
                        mBehaviours[i].Update(timeElappsed);
                    }
                    if (mBehaviours[i].IsFinished)
                    {
                        Remove(mBehaviours[i]);
                    }
                }
                UnLockUpdate();
                ResolveRemove();
                ResolveAdd();
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
            if (IsLockUpdate())
            {
                RemoveBehaviourCaches.Add(bc);
            }
            else
            {
                mBehaviours.Remove(bc);
            }
        }

        protected bool ResolveRemove()
        {
            bool res = AddBehaviourCaches.Count > 0;
            foreach (BehaviourCallback bc in AddBehaviourCaches)
            {
                mBehaviours.Add(bc);
            }
            AddBehaviourCaches.Clear();
            return res;
        }
    }
}
