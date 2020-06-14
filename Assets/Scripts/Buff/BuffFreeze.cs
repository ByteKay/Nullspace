using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fishing
{
    public class BuffFreeze : TimeBuff
    {
        public BuffFreeze(int buffType, IBuffTarget target, float duration) : base(buffType, target, duration)
        {
            
        }

        public override void End()
        {
            Target.EnableMove(true);
            Target.Freeze(false);
        }

        protected override void Begin()
        {
            Target.EnableMove(false);
            Target.Freeze(true);
        }

        protected override void Process()
        {
            
        }
    }
}
