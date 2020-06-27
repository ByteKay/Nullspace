using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nullspace
{
    public class SingleSequenceBehaviourTree : IUpdate
    {
        public static SingleSequenceBehaviourTree Create()
        {
            SingleSequenceBehaviourTree seqTree = new SingleSequenceBehaviourTree();
            SequenceBehaviourManager.Instance.AddSequence(seqTree);
            return seqTree;
        }

        private SingleSequenceBehaviour Root;
        private SingleSequenceBehaviour Current;

        public bool IsPlaying { get { return Current != null; } }

        public SingleSequenceBehaviourTree()
        {
            Root = null;
            Current = null;
        }

        public void SetRoot(SingleSequenceBehaviour root)
        {
            Root = root;
            Current = root;
        }

        public void Update(float deltaTime)
        {
            if (Current != null)
            {
                Current.Update(deltaTime);
            }
        }

        public void MoveNext()
        {
            if (Current != null)
            {
                Current.ConsumeChild();
            }
        }

        public void ChangeToBrother()
        {
            if (Current != null)
            {
                Current = Current.NextBrother;
            }
        }

    }
}
