using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public class FlyBehaviour : ResourceCacheBehaviour
    {
        public override void StartByDerive()
        {
            DebugUtils.Info("FlyBehaviour", "StartByDerive");
        }


        protected override void InterruptWhenUsing()
        {
            DebugUtils.Info("FlyBehaviour", "InterruptWhenUsing");
        }
    }
}
