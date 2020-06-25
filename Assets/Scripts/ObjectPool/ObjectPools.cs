using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Nullspace
{
    public abstract class ObjectCacheBase
    {
        private static uint CurrentSize = 0;
        private static uint NextKey { get { return CurrentSize++; } }

        private float ReleasedTimePoint;
        private bool IsReleased;
        public ObjectCacheBase()
        {
            Key = NextKey;
            // 构造的时候，会放进缓存先。所以，构造的时刻就是 释放的时刻
            IsReleased = true;
            ReleasedTimePoint = TimeUtils.GetTimeStampSeconds();
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
                ReleasedTimePoint = TimeUtils.GetTimeStampSeconds();// Time.realtimeSinceStartup;
                IsReleased = true;
            }
        }

        public bool IsExpired(float life)
        {
            return TimeUtils.GetTimeStampSeconds() - ReleasedTimePoint >= life;
        }
        
        public abstract void Initialize();
        public abstract void Clear();

    }

    public class ObjectPool
    {
        private static List<uint> ExpiredKeys = new List<uint>();

        protected int DefaultSize = 2;
        protected Dictionary<uint, ObjectCacheBase> CircleCaches;
        protected int LifeTimeSecond;

        public Type Type { get; set; }
        // 默认5分钟
        public ObjectPool(Type type, int lifeTimeSecond = 20)
        {
            Type = type;
            LifeTimeSecond = lifeTimeSecond;
            CircleCaches = new Dictionary<uint, ObjectCacheBase>(DefaultSize);
            Generator(DefaultSize);
        }

        private void Generator(int size)
        {
            while (size-- > 0)
            {
                ObjectCacheBase circle = (ObjectCacheBase)Activator.CreateInstance(Type);
                if (circle != null)
                {
                    CircleCaches.Add(circle.Key, circle);
                }
            }
        }

        public virtual void Release(ObjectCacheBase t)
        {
            t.Released();
            if (!CircleCaches.ContainsKey(t.Key))
            {
                CircleCaches.Add(t.Key, t);
            }
        }

        public virtual void Acquire(int num, List<ObjectCacheBase> result)
        {
            if (CircleCaches.Count < num)
            {
                Generator(num - CircleCaches.Count);
            }
            var itr = CircleCaches.Values.GetEnumerator();
            while (itr.MoveNext() && result.Count < num)
            {
                ObjectCacheBase circle = itr.Current;
                circle.Acquired();
                result.Add(circle);
            }

            for (int i = 0; i < num; ++i)
            {
                CircleCaches.Remove(result[i].Key);
            }
        }

        public virtual List<ObjectCacheBase> Acquire(int num)
        {
            List<ObjectCacheBase> result = new List<ObjectCacheBase>();
            Acquire(num, result);
            return result;
        }

        public virtual ObjectCacheBase Acquire()
        {
            if (CircleCaches.Count == 0)
            {
                // 生产太多，会多创建空壳。后面从缓存中选取，可能是空壳，进一步需要初始化
                Generator(1);
            }
            var itr = CircleCaches.Values.GetEnumerator();
            itr.MoveNext();
            ObjectCacheBase circle = itr.Current;
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
            float timeStamp = Time.realtimeSinceStartup;
            ExpiredKeys.Clear();
            while (itr.MoveNext())
            {
                ObjectCacheBase item = itr.Current;
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

    [Serializable]
    public class ObjectPools : Singleton<ObjectPools>
    {
        private Dictionary<Type, ObjectPool> Pools;
        private List<Type> ClearEmptyPools;
        private int CheckTimerId;
        private void Awake()
        {
            Pools = new Dictionary<Type, ObjectPool>();
            ClearEmptyPools = new List<Type>();
            CheckTimerId = TimerTaskQueue.Instance.AddTimer(2000, 2000, CheckLifeExpired);
        }

        public T Acquire<T>() where T : ObjectCacheBase
        {
            Type type = typeof(T);
            Debug.Assert(type.GetConstructor(Type.EmptyTypes) != null, "no default constructor");
            if (!Pools.ContainsKey(type))
            {
                Pools.Add(type, new ObjectPool(type));
            }
            return Pools[type].Acquire() as T;
        }

        public void Release(ObjectCacheBase obj)
        {
            if (obj == null)
            {
                return; 
            }
            Type type = obj.GetType();
            Debug.Assert(type.IsSubclassOf(typeof(ObjectCacheBase)), "wrong type");
            if (Pools.ContainsKey(type))
            {
                Pools[type].Release(obj);
            }
            else
            {
                obj.Released();
            }
        }

        protected override void OnDestroy()
        {
            foreach (ObjectPool pool in Pools.Values)
            {
                pool.Clear();
            }
            TimerTaskQueue.Instance.DelTimer(CheckTimerId);
            base.OnDestroy();
        }

        private void CheckLifeExpired()
        {
            DebugUtils.Info("ObjectPools", "CheckLifeExpired");
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

#if UNITY_EDITOR
        public void InspectorShow()
        {
            foreach (ObjectPool pool in Pools.Values)
            {
                EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Window"));
                GUILayout.Label(pool.Type.Name + " " + pool.Count);
                EditorGUILayout.EndVertical();
                GUILayout.Space(16);
            }
        }
#endif

    }

}
