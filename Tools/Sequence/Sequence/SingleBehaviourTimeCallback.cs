using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public class SingleBehaviourTimeCallback : BehaviourTimeCallback
    {
        public SingleSequenceBehaviour Single { get; set; }

        public override void End()
        {
            base.End();
            if (Single != null)
            {
                Single.Next();
            }
        }
    }
}
