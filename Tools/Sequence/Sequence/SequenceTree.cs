using System;

namespace Nullspace
{
    public class SequenceTree : ISequnceUpdate
    {
        private ISequnceUpdate mRoot;
        private ISequnceUpdate mCurrent;
        private ISequnceUpdate mSibling;
        internal SequenceTree()
        {
            mRoot = null;
            mCurrent = null;
        }

        public bool IsPlaying { get { return mCurrent != null; } }

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

        public void Kill()
        {
            mCurrent = null;
        }

        void ISequnceUpdate.Update(float deltaTime)
        {
            Update(deltaTime);
        }

        internal void Update(float deltaTime)
        {
            if (mCurrent != null)
            {
                mCurrent.Update(deltaTime);
                if (!mCurrent.IsPlaying)
                {
                    mCurrent = mCurrent.Sibling;
                }
            }
        }

        internal void SetRoot(ISequnceUpdate root)
        {
            mRoot = root;
            mCurrent = root;
        }

        void ISequnceUpdate.Next()
        {
             // todo
        }

    }
}
