using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public class EffectBehaviour : ResourceCacheBehaviour
    {
        public override void StartByDerive()
        {
            DebugUtils.Info("EffectBehaviour", "StartByDerive");
        }


        protected override void InterruptWhenUsing()
        {
            DebugUtils.Info("EffectBehaviour", "InterruptWhenUsing");
        }
    }
}
