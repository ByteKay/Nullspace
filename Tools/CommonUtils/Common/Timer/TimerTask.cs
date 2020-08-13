

using System;

namespace Nullspace
{
    public class TimerTask : ObjectKey
    {
        internal int TimerId { get; set; }

        internal int Interval { get; set; }

        internal int NextTick { get; set; }

        internal Callback Callback { get; set; }

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
