

namespace Nullspace
{
    public class TimerTask : ObjectCacheBase
    {
        public int TimerId { get; set; }

        public int Interval { get; set; }

        public int NextTick { get; set; }

        public AbstractCallback Callback { get; set; }

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
    }
}
