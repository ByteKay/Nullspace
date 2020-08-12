
using System.Collections.Generic;
using System.Diagnostics;

namespace Nullspace
{

    public partial class SequenceManager
    {
        public static SequenceManager Instance = new SequenceManager();

        private Stopwatch mStopWatch;
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

        private List<IUpdate> Behaviours;
        private List<int> FinishedList = new List<int>();
        
        private SequenceManager()
        {
            Behaviours = new List<IUpdate>();
            mStopWatch = new Stopwatch();
        }

        public void Update()
        {
            float seconds = mStopWatch.ElapsedMilliseconds * 0.001f;
            mStopWatch.Reset();
            mStopWatch.Start();

            int count = Behaviours.Count;
            FinishedList.Clear();
            for (int i = 0; i < count; ++i)
            {
                IUpdate sb = Behaviours[i];
                sb.Update(seconds);
                if (!sb.IsPlaying)
                {
                    FinishedList.Add(i);
                }
            }
            count = FinishedList.Count;
            if (count > 0)
            {
                for (int i = count - 1; i >= 0; --i)
                {
                    Behaviours.RemoveAt(i);
                }
            }
        }

        public void Clear()
        {
            Behaviours.Clear();
        }

        private void AddSequence(IUpdate seq)
        {
            // 这里即使 Update 调用到这里，不会打断 循环
            Behaviours.Add(seq);
        }
    }
}
