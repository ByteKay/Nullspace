using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public abstract class BuffClock
    {
        public Action OnRemove;
        public Action OnTick;

        public ValueStack StackSize = new ValueStack();

        public abstract float DisplayPercent
        {
            get;
        }

        public abstract void Update(float deltaTime);

        public virtual void RemoveClock()
        {
            if (OnRemove != null)
            {
                OnRemove();
            }
        }
    }
}
