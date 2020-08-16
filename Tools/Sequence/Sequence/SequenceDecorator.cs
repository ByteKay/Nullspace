
namespace Nullspace
{
    /// <summary>
    /// 装饰一个 Sequence
    /// 暂停 和 冷却
    /// </summary>
    public class SequenceDecorator : ISequnceUpdate
    {
        protected ISequnceUpdate mDecoratorSequence;
        protected SequenceLinkedList mCheckSequence;

        internal SequenceDecorator(ISequnceUpdate sequnce)
        {
            mDecoratorSequence = sequnce;
            mCheckSequence = null;
        }

        public bool IsPlaying
        {
            get
            {
                return mDecoratorSequence.IsPlaying;
            }
        }

        public ISequnceUpdate Sibling
        {
            get
            {
                return mDecoratorSequence.Sibling;
            }

            set
            {
                mDecoratorSequence.Sibling = value;
            }
        }

        public void Kill()
        {
            if (mCheckSequence != null)
            {
                mCheckSequence.Kill();
                mCheckSequence = null;
            }
            
            mDecoratorSequence.Kill();
        }

        public void Next()
        {
            mDecoratorSequence.Next();
        }

        void ISequnceUpdate.Update(float deltaTime)
        {
            Update(deltaTime);
        }

        internal void Update(float deltaTime)
        {
            if (CheckSequencePlaying(deltaTime))
            {
                return;
            }
            mDecoratorSequence.Update(deltaTime);
        }

        public void Cooldown(float duration)
        {
            CheckSequence.Cooldown(duration);
        }

        public void Pause(float duration)
        {
            CheckSequence.Pause(duration);
        }

        public void Pause(string tag)
        {
            CheckSequence.Pause(tag);
        }

        // 去除当前的暂停
        public bool Resume(string tag)
        {
            return CheckSequence.Resume(tag);
        }

        protected SequenceLinkedList CheckSequence
        {
            get
            {
                if (mCheckSequence == null)
                {
                    mCheckSequence = SequenceManager.CreateSingle();
                    mCheckSequence.Kill();
                }
                return mCheckSequence;
            }
        }

        internal bool CheckSequencePlaying(float deltaTime)
        {
            if (mCheckSequence != null)
            {
                mCheckSequence.Update(deltaTime);
                return mCheckSequence.IsPlaying;
            }
            return true;
        }
    }
}
