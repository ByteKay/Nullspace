namespace Nullspace
{
    public class SequenceTree : IUpdate
    {
        private SequenceSingle Root = null;
        private SequenceSingle Current = null;

        internal SequenceTree()
        {

        }

        public bool IsPlaying { get { return Current != null; } }
        public void Update(float deltaTime)
        {
            if (Current != null)
            {
                Current.Update(deltaTime);
            }
        }
        internal void SetRoot(SequenceSingle root)
        {
            Root = root;
            Current = root;
        }
        internal void MoveNext()
        {
            if (Current != null)
            {
                Current.ConsumeChild();
            }
        }
        internal void ChangeToBrother()
        {
            if (Current != null)
            {
                Current = Current.NextBrother;
            }
        }

    }
}
