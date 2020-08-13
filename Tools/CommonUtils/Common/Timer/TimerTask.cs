

using System;

namespace Nullspace
{
    public class TimerTask : ObjectKey
    {
        public int TimerId { get; set; }

        public int Interval { get; set; }

        public int NextTick { get; set; }

        public Callback Callback { get; set; }

        public void DoAction()
        {
            if (Callback != null)
            {
                Callback.Run();
            }
        }

        public override void Initialize()
        {
            Clear();
        }

        public override void Clear()
        {
            if (Callback != null)
            {
                ObjectPools.Instance.Release(Callback);
                Callback = null;
            }
        }

        public override void Destroy()
        {
            Clear();
        }
    }
}
