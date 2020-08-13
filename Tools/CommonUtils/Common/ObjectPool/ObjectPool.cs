using System;
using System.Collections.Generic;

namespace Nullspace
{

    public class ObjectPool
    {
        private static List<uint> mExpiredKeys = new List<uint>();

        protected int mDefaultSize = 2;
        protected Dictionary<uint, ObjectKey> mCircleCaches;
        protected int mLifeTimeSecond;

        public Type Type { get; set; }
        // 默认5分钟
        public ObjectPool(Type type, int lifeTimeSecond = 20)
        {
            Type = type;
            mLifeTimeSecond = lifeTimeSecond;
            mCircleCaches = new Dictionary<uint, ObjectKey>(mDefaultSize);
            Generator(mDefaultSize);
        }

        private void Generator(int size)
        {
            while (size-- > 0)
            {
                ObjectKey circle = (ObjectKey)Activator.CreateInstance(Type);
                if (circle != null)
                {
                    mCircleCaches.Add(circle.Key, circle);
                }
            }
        }

        public virtual void Release(ObjectKey t)
        {
            t.Released();
            if (!mCircleCaches.ContainsKey(t.Key))
            {
                mCircleCaches.Add(t.Key, t);
            }
        }

        public virtual void Acquire(int num, List<ObjectKey> result)
        {
            if (mCircleCaches.Count < num)
            {
                Generator(num - mCircleCaches.Count);
            }
            var itr = mCircleCaches.Values.GetEnumerator();
            while (itr.MoveNext() && result.Count < num)
            {
                ObjectKey circle = itr.Current;
                circle.Acquired();
                result.Add(circle);
            }

            for (int i = 0; i < num; ++i)
            {
                mCircleCaches.Remove(result[i].Key);
            }
        }

        public virtual List<ObjectKey> Acquire(int num)
        {
            List<ObjectKey> result = new List<ObjectKey>();
            Acquire(num, result);
            return result;
        }

        public virtual ObjectKey Acquire()
        {
            if (mCircleCaches.Count == 0)
            {
                // 生产太多，会多创建空壳。后面从缓存中选取，可能是空壳，进一步需要初始化
                Generator(1);
            }
            var itr = mCircleCaches.Values.GetEnumerator();
            itr.MoveNext();
            ObjectKey circle = itr.Current;
            circle.Acquired();
            mCircleCaches.Remove(circle.Key);
            return circle;
        }

        public virtual void Clear()
        {
            // 实际上 缓存中的物体 不用调用 Released
            var itr = mCircleCaches.Values.GetEnumerator();
            while (itr.MoveNext())
            {
                ObjectKey item = itr.Current;
                item.Destroy();
            }
            mCircleCaches.Clear();
        }

        public void RemoveExpired(int lifeSeconds)
        {
            var itr = mCircleCaches.Values.GetEnumerator();
            float timeStamp = DateTimeUtils.GetTimeStampSeconds();
            mExpiredKeys.Clear();
            while (itr.MoveNext())
            {
                ObjectKey item = itr.Current;
                if (item.IsExpired(mLifeTimeSecond))
                {
                    mExpiredKeys.Add(item.Key);
                }
            }
            foreach (uint key in mExpiredKeys)
            {
                mCircleCaches.Remove(key);
            }
        }

        public void RemoveExpired()
        {
            RemoveExpired(mLifeTimeSecond);
        }

        public bool IsEmpty()
        {
            return Count == 0;
        }

        public int Count { get { return mCircleCaches.Count; } }
    }

}
