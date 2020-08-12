namespace Nullspace
{
    public class SequenceTree : IUpdate
    {
        private SequenceSingle mRoot = null;
        private SequenceSingle mCurrent = null;

        internal SequenceTree()
        {

        }

        public bool IsPlaying { get { return mCurrent != null; } }
        public void Update(float deltaTime)
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
        internal void MoveNext()
        {
            if (mCurrent != null)
            {
                mCurrent.ConsumeChild();
            }
        }
        internal void ChangeToBrother()
        {
            if (mCurrent != null)
            {
                mCurrent = mCurrent.NextBrother;
            }
        }

    }
}
