
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

        public static SequenceParallelFull CreateParallel()
        {
            SequenceParallelFull sb = new SequenceParallelFull();
            Instance.AddSequence(sb);
            return sb;
        }

        public static SequenceLinkedList CreateSingle()
        {
            SequenceLinkedList sb = new SequenceLinkedList();
            Instance.AddSequence(sb);
            return sb;
        }

        private List<ISequnceUpdate> mBehaviours;
        private List<int> mFinishedList = new List<int>();
        private Stopwatch mStopWatch;

        private SequenceManager()
        {
            mBehaviours = new List<ISequnceUpdate>();
            mStopWatch = new Stopwatch();
            mStopWatch.Start();
        }

        public void Tick()
        {
            float seconds = mStopWatch.ElapsedMilliseconds * 0.001f;
            mStopWatch.Reset();
            mStopWatch.Start();
            Update(seconds);
        }

        public void Update(float seconds)
        {
            int count = mBehaviours.Count;
            mFinishedList.Clear();
            for (int i = 0; i < count; ++i)
            {
                ISequnceUpdate sb = mBehaviours[i];
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

        private void AddSequence(ISequnceUpdate seq)
        {
            // 这里即使 Update 调用到这里，不会打断 循环
            mBehaviours.Add(seq);
        }
    }
}
