
using System.Collections.Generic;
using System.Diagnostics;

namespace Nullspace
{

    public partial class SequenceManager
    {
        public static SequenceManager Instance = new SequenceManager();

        public static SequenceTree CreateTree()
        {
            SequenceTree seqTree = new SequenceTree();
            Instance.AddSequence(seqTree);
            return seqTree;
        }

        public static SequenceParallel CreateParallel()
        {
            SequenceParallel sb = new SequenceParallel();
            Instance.AddSequence(sb);
            return sb;
        }

        public static SequenceSingle CreateSingle()
        {
            SequenceSingle sb = new SequenceSingle();
            Instance.AddSequence(sb);
            return sb;
        }

        private List<IUpdate> mBehaviours;
        private List<int> mFinishedList = new List<int>();
        private Stopwatch mStopWatch;

        private SequenceManager()
        {
            mBehaviours = new List<IUpdate>();
            mStopWatch = new Stopwatch();
        }

        public void Update()
        {
            float seconds = mStopWatch.ElapsedMilliseconds * 0.001f;
            mStopWatch.Reset();
            mStopWatch.Start();

            int count = mBehaviours.Count;
            mFinishedList.Clear();
            for (int i = 0; i < count; ++i)
            {
                IUpdate sb = mBehaviours[i];
                sb.Update(seconds);
                if (!sb.IsPlaying)
                {
                    mFinishedList.Add(i);
                }
            }
            count = mFinishedList.Count;
            if (count > 0)
            {
                for (int i = count - 1; i >= 0; --i)
                {
                    mBehaviours.RemoveAt(i);
                }
            }
        }

        public void Clear()
        {
            mBehaviours.Clear();
        }

        private void AddSequence(IUpdate seq)
        {
            // 这里即使 Update 调用到这里，不会打断 循环
            mBehaviours.Add(seq);
        }
    }
}
