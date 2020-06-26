using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nullspace
{
    public class SequenceBehaviourManager : Singleton<SequenceBehaviourManager>
    {
        private List<SequenceBehaviour> Behaviours;
        private List<int> FinishedList = new List<int>();
        private void Awake()
        {
            Behaviours = new List<SequenceBehaviour>();
        }

        public void Update()
        {
            int count = Behaviours.Count;
            FinishedList.Clear();
            for (int i = 0; i < count; ++i)
            {
                SequenceBehaviour sb = Behaviours[i];
                sb.Update(Time.deltaTime);
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

        public void AddSequence(SequenceBehaviour seq)
        {
            // 这里即使 Update 调用到这里，不会打断 循环
            Behaviours.Add(seq);
        }

        protected override void OnDestroy()
        {
            
        }
    }
}
