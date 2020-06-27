using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nullspace
{
    public class SingleSequenceBehaviourTree : IUpdate
    {
        private SingleSequenceBehaviour Root;
        private SingleSequenceBehaviour Current;

        public bool IsPlaying { get { return Current != null; } }

        public SingleSequenceBehaviourTree(SingleSequenceBehaviour root = null)
        {
            Root = root;
            Current = root;
        }

        public void Update(float deltaTime)
        {
            Current.Update(deltaTime);
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
