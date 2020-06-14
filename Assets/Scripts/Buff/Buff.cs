
namespace Nullspace
{
    public abstract class Buff 
    {
        protected IBuffTarget Target { get; set; }

        public int Type { get; set; }
        public bool IsActive { get; set; }

        public Buff(int buffType, IBuffTarget target)
        {
            Type = buffType;
            Target = target;
            IsActive = false;
        }
        
        public void Update(float timeElappsed)
        {
            if (!IsActive)
            {
                IsActive = true;
                Begin();
            }
            else
            {
                UpdateConfition(timeElappsed);
                if (IsEnd())
                {
                    IsActive = false;
                    End();
                }
                else
                {
                    Process();
                }
            }
        }
       
        public abstract bool IsEnd();
        public abstract void Merge(Buff other);
        protected abstract void UpdateConfition(float timeElappsed);

        public abstract void End();
        protected abstract void Begin();
        protected abstract void Process();
        
    }
}
