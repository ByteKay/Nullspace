using System;
using System.Collections.Generic;

namespace Nullspace
{

    public class ObjectPool
    {
        private static List<uint> ExpiredKeys = new List<uint>();

        protected int DefaultSize = 2;
        protected Dictionary<uint, ObjectKey> CircleCaches;
        protected int LifeTimeSecond;

        public Type Type { get; set; }
        // 默认5分钟
        public ObjectPool(Type type, int lifeTimeSecond = 20)
        {
            Type = type;
            LifeTimeSecond = lifeTimeSecond;
            CircleCaches = new Dictionary<uint, ObjectKey>(DefaultSize);
            Generator(DefaultSize);
        }

        private void Generator(int size)
        {
            while (size-- > 0)
            {
                ObjectKey circle = (ObjectKey)Activator.CreateInstance(Type);
                if (circle != null)
                {
                    CircleCaches.Add(circle.Key, circle);
                }
            }
        }

        public virtual void Release(ObjectKey t)
        {
            t.Released();
            if (!CircleCaches.ContainsKey(t.Key))
            {
                CircleCaches.Add(t.Key, t);
            }
        }

        public virtual void Acquire(int num, List<ObjectKey> result)
        {
            if (CircleCaches.Count < num)
            {
                Generator(num - CircleCaches.Count);
            }
            var itr = CircleCaches.Values.GetEnumerator();
            while (itr.MoveNext() && result.Count < num)
            {
                ObjectKey circle = itr.Current;
                circle.Acquired();
                result.Add(circle);
            }

            for (int i = 0; i < num; ++i)
            {
                CircleCaches.Remove(result[i].Key);
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
            if (CircleCaches.Count == 0)
            {
                // 生产太多，会多创建空壳。后面从缓存中选取，可能是空壳，进一步需要初始化
                Generator(1);
            }
            var itr = CircleCaches.Values.GetEnumerator();
            itr.MoveNext();
            ObjectKey circle = itr.Current;
            circle.Acquired();
            CircleCaches.Remove(circle.Key);
            return circle;
        }

        public virtual void Clear()
        {
            // 实际上 缓存中的物体 不用调用 Released
            //var itr = CircleCaches.Values.GetEnumerator();
            //while (itr.MoveNext())
            //{
            //    ObjectCacheBase item = itr.Current;
            //    item.Released();
            //}
            CircleCaches.Clear();
        }

        public void RemoveExpired(int lifeSeconds)
        {
            var itr = CircleCaches.Values.GetEnumerator();
            float timeStamp = DateTimeUtils.GetTimeStampSeconds();
            ExpiredKeys.Clear();
            while (itr.MoveNext())
            {
                ObjectKey item = itr.Current;
                if (item.IsExpired(LifeTimeSecond))
                {
                    ExpiredKeys.Add(item.Key);
                }
            }
            foreach (uint key in ExpiredKeys)
            {
                CircleCaches.Remove(key);
            }
        }

        public void RemoveExpired()
        {
            RemoveExpired(LifeTimeSecond);
        }

        public bool IsEmpty()
        {
            return Count == 0;
        }

        public int Count { get { return CircleCaches.Count; } }
    }

}
