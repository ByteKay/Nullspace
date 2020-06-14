
namespace Nullspace
{
    public abstract class TimeBuff : Buff
    {
        protected float TimeElappsed { get; set; }
        protected float Duration { get; set; }

        public TimeBuff(int buffType, IBuffTarget target, float duration) : base(buffType, target)
        {
            TimeElappsed = 0;
            Duration = duration;
        }

        public override bool IsEnd()
        {
            return TimeElappsed >= Duration;
        }

        protected override void UpdateConfition(float timeElappsed)
        {
            TimeElappsed += timeElappsed;
        }

        public override void Merge(Buff other)
        {
            Duration += ((TimeBuff)other).Duration;
        }
    }
}
