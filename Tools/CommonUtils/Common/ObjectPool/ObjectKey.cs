
namespace Nullspace
{
    public abstract class ObjectKey
    {
        private static uint CurrentSize = 0;
        private static uint NextKey { get { return CurrentSize++; } }

        private float ReleasedTimePoint;
        private bool IsReleased;
        public ObjectKey()
        {
            Key = NextKey;
            // 构造的时候，会放进缓存先。所以，构造的时刻就是 释放的时刻
            IsReleased = true;
            ReleasedTimePoint = DateTimeUtils.GetTimeStampSeconds();
        }

        public uint Key { get; set; }

        public void Acquired()
        {
            if (IsReleased)
            {
                IsReleased = false;
                Initialize();
            }
        }

        // 这个只能通过 ObjectPools.Instance.Release --> ObjectPool.Release -> Release 过来
        public void Released()
        {
            if (!IsReleased)
            {
                Clear();
                ReleasedTimePoint = DateTimeUtils.GetTimeStampSeconds();// Time.realtimeSinceStartup;
                IsReleased = true;
            }
        }

        public bool IsExpired(float life)
        {
            return DateTimeUtils.GetTimeStampSeconds() - ReleasedTimePoint >= life;
        }

        public abstract void Initialize();
        public abstract void Clear();

    }
}
