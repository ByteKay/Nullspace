﻿
namespace Nullspace
{
    public class BuffAccelerate : TimeBuff
    {
        private float Accelerate { get; set; }

        public BuffAccelerate(int buffType, IBuffTarget target, float duration, float accelerate) : base(buffType, target, duration)
        {
            Accelerate = accelerate;
        }

        public override void End()
        {
            Target.SpeedTimes(-Accelerate);
        }

        protected override void Begin()
        {
            Target.SpeedTimes(Accelerate);
        }

        protected override void Process()
        {

        }
    }
}
