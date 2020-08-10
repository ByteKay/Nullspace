using System;
using System.Collections.Generic;

namespace Nullspace
{
    public class ObjectPools
    {
        public static ObjectPools Instance = null;

        static ObjectPools()
        {
            Instance = new ObjectPools();
            Instance.Initialize();
        }

        private Dictionary<Type, ObjectPool> Pools;
        private List<Type> ClearEmptyPools;
        private int CheckTimerId;

        private ObjectPools()
        {
            Pools = new Dictionary<Type, ObjectPool>();
            ClearEmptyPools = new List<Type>();
        }

        // 一定要将 TimerTaskQueue.Instance 放在这里，不能放在构造函数
        private void Initialize()
        {
            CheckTimerId = TimerTaskQueue.Instance.AddTimer(2000, 1000 * 60 * 3, CheckLifeExpired);
        }

        public T Acquire<T>() where T : ObjectKey
        {
            Type type = typeof(T);
            DebugUtils.Assert(type.GetConstructor(Type.EmptyTypes) != null, "no default constructor");
            if (!Pools.ContainsKey(type))
            {
                Pools.Add(type, new ObjectPool(type));
            }
            return Pools[type].Acquire() as T;
        }

        public void Release(ObjectKey obj)
        {
            if (obj == null)
            {
                return;
            }
            Type type = obj.GetType();
            DebugUtils.Assert(type.IsSubclassOf(typeof(ObjectKey)), "wrong type");
            if (Pools.ContainsKey(type))
            {
                Pools[type].Release(obj);
            }
            else
            {
                obj.Released();
            }
        }

        public void Clear()
        {
            foreach (ObjectPool pool in Pools.Values)
            {
                pool.Clear();
            }
            TimerTaskQueue.Instance.DelTimer(CheckTimerId);
        }

        private void CheckLifeExpired()
        {
            DebugUtils.Log(InfoType.Info, "ObjectPools.CheckLifeExpired");
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
                // 这个可以屏蔽
                // GC.Collect();
            }
        }
    }

}
