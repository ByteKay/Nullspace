using System;

namespace Nullspace
{
    public class Buff
    {
        public event Action OnRemove;
        public BuffTemplate buffTemplate;
        public SequenceMultipleDynamic mBehaviours;

        public float DisplayPercent
        {
            get
            {
                return mBehaviours.MaxPercent;
            }
        }

        public void AddBehaviour(BehaviourCallback behaviour)
        {
            mBehaviours.Add(behaviour);
        }

        public void RemoveBehaviour(BehaviourCallback behaviour)
        {
            mBehaviours.Remove(behaviour);
        }

        public void Update(float deltaTime)
        {
            mBehaviours.Update(deltaTime);
        }

        public void Remove()
        {
            mBehaviours.Clear();
            OnRemove.Invoke();
        }
    }
}
