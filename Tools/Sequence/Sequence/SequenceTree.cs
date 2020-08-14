namespace Nullspace
{
    public class SequenceTree : ISequnceUpdate
    {
        private SequenceSingle mRoot;
        private SequenceSingle mCurrent;

        internal SequenceTree()
        {
            mRoot = null;
            mCurrent = null;
        }

        public bool IsPlaying { get { return mCurrent != null; } }

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
            }
        }

        internal void SetRoot(SequenceSingle root)
        {
            mRoot = root;
            mCurrent = root;
        }

        void ISequnceUpdate.Next()
        {
            MoveNext();
        }

        internal void MoveNext()
        {
            if (mCurrent != null)
            {
                mCurrent.ConsumeChild();
            }
        }

        internal void ToSibling()
        {
            if (mCurrent != null)
            {
                mCurrent = mCurrent.NextSibling;
            }
        }

    }
}
