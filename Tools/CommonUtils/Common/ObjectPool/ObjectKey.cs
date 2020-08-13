
namespace Nullspace
{
    public abstract class ObjectKey
    {
        private static uint mCurrentSize = 0;
        private static uint mNextKey { get { return mCurrentSize++; } }

        private float mReleasedTimePoint;
        private bool mIsReleased;
        public ObjectKey()
        {
            Key = mNextKey;
            // 构造的时候，会放进缓存先。所以，构造的时刻就是 释放的时刻
            mIsReleased = true;
            mReleasedTimePoint = DateTimeUtils.GetTimeStampSeconds();
        }

        public uint Key { get; set; }

        public void Acquired()
        {
            if (mIsReleased)
            {
                mIsReleased = false;
                Initialize();
            }
        }

        // 这个只能通过 ObjectPools.Instance.Release --> ObjectPool.Release -> Release 过来
        public void Released()
        {
            if (!mIsReleased)
            {
                Clear();
                mReleasedTimePoint = DateTimeUtils.GetTimeStampSeconds();// Time.realtimeSinceStartup;
                mIsReleased = true;
            }
        }

        public bool IsExpired(float life)
        {
            return DateTimeUtils.GetTimeStampSeconds() - mReleasedTimePoint >= life;
        }

        public abstract void Initialize();
        public abstract void Clear();
        public abstract void Destroy();
    }
}
