using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class ObjectKey
    {
        private static uint CurrentSize = 0;
        private static uint NextKey { get { return CurrentSize++; } }

        public uint Key;
        private float ReleasedTimePoint;
        public ObjectKey()
        {
            Key = NextKey;
            ReleasedTimePoint = Time.realtimeSinceStartup;
            Initialize();
        }

        protected virtual void Initialize()
        {

        }

        public virtual void Release()
        {
            ReleasedTimePoint = Time.realtimeSinceStartup;
        }

        public virtual bool IsExpired(float life)
        {
            return Time.realtimeSinceStartup - ReleasedTimePoint >= life;
        }
    }

    public class ObjectPools
    {
        private static Dictionary<Type, ObjectPool> Pools;
        private static List<Type> ClearEmptyPools;
        static ObjectPools()
        {
            Pools = new Dictionary<Type, ObjectPool>();
            ClearEmptyPools = new List<Type>();
            TimerTaskQueue.Instance.AddTimer(2000, 2000, CheckLifeExpired);
        }

        private class ObjectPool
        {
            private static List<uint> ExpiredKeys = new List<uint>();

            protected int DefaultSize = 5;
            protected Dictionary<uint, ObjectKey> CircleCaches;
            protected int LifeTimeSecond;

            public Type Type { get; set; }
            // 默认5分钟
            public ObjectPool(Type type, int lifeTimeSecond = 300)
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
                t.Release();
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
                CircleCaches.Remove(circle.Key);
                return circle;
            }

            public virtual void Clear()
            {
                var itr = CircleCaches.Values.GetEnumerator();
                while (itr.MoveNext())
                {
                    ObjectKey item = itr.Current;
                    item.Release();
                }
                CircleCaches.Clear();
            }

            public void RemoveExpired()
            {
                var itr = CircleCaches.Values.GetEnumerator();
                float timeStamp = Time.realtimeSinceStartup;
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

            public bool IsEmpty()
            {
                return CircleCaches.Count == 0;
            }
        }

        public static ObjectKey Acquire(Type type)
        {
            Debug.Assert(type.IsSubclassOf(typeof(ObjectKey)), "wrong type");
            if (!Pools.ContainsKey(type))
            {
                Pools.Add(type, new ObjectPool(type));
            }
            return Pools[type].Acquire();
        }

        public static void Release(ObjectKey obj)
        {
            Type type = obj.GetType();
            Debug.Assert(type.IsSubclassOf(typeof(ObjectKey)), "wrong type");
            if (Pools.ContainsKey(type))
            {
                Pools[type].Release(obj);
            }
            else
            {
                obj.Release();
            }
        }

        public static void Clear()
        {
            foreach (ObjectPool pool in Pools.Values)
            {
                pool.Clear();
            }
        }

        private static void CheckLifeExpired()
        {
            ClearEmptyPools.Clear();
            foreach (ObjectPool pool in Pools.Values)
            {
                pool.RemoveExpired();
                if (pool.IsEmpty())
                {
                    ClearEmptyPools.Add(pool.Type);
                }
            }
            if (ClearEmptyPools.Count > 0)
            {
                foreach (Type type in ClearEmptyPools)
                {
                    Pools.Remove(type);
                }
            }
        }

    }

}
