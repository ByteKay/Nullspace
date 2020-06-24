using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nullspace
{
    public class ResourceCachePools
    {
        private static List<int> CACHE = new List<int>();
        public static string SUFFIX_FLAG = "19500";
        private Dictionary<int, ResourceCachePool> mPools;
        private Dictionary<int, ResourceCacheEntity> mAcquiredItems;

        public int Level { get; set; }

        public ResourceCachePools()
        {
            mPools = new Dictionary<int, ResourceCachePool>();
            mAcquiredItems = new Dictionary<int, ResourceCacheEntity>();
            Level = DeviceLevel.High;
        }

        public void ChangePoolMaskFromTo(ResourceCacheMask from, ResourceCacheMask to)
        {
            Clear(from);
            Initialize(to);
        }

        public virtual void Initialize(ResourceCacheMask mask, bool cacheOn = true)
        {
            Dictionary<int, ResourceConfig> configs = new Dictionary<int, ResourceConfig>();
            Initialize(mask, configs, cacheOn);
        }

        public virtual void Initialize(ResourceCacheMask mask, Dictionary<int, ResourceConfig> configs, bool cacheOn = true)
        {
            foreach (ResourceConfig config in configs.Values)
            {
                if (CheckLevelShow(config.Level))
                {
                    if (((int)mask & config.Mask) != 0)
                    {
                        ResourceCachePool pool = mPools[config.Id];
                        if (pool == null)
                        {
                            pool = new ResourceCachePool();
                            pool.Initialize(config, ResourceCacheBindParent.CacheUnused, GetLevel(), this, cacheOn);
                            mPools.Add(config.Id, pool);
                        }
                    }
                }
            }
        }

        public virtual void Clear(ResourceCacheMask mask)
        {
            ReleaseActives();
            ClearInternal(mask);
        }

        private void ReleaseActives()
        {
            List<int> keys = mAcquiredItems.Keys.ToList();
            foreach (int key in keys)
            {
                ForceRelease(key);
            }
        }

        public virtual int Play(int poolId, Transform parent, ResourceCacheBehaviourParam param, int delay = 0)
        {
            return Play(poolId,int.MaxValue, parent, param, delay);
        }

        public virtual int Play(int poolId, int duration, Transform parent, ResourceCacheBehaviourParam param, int delay = 0)
        {
            int instanceId = -1;
            ResourceCacheEntity entity;
            ResourceCachePool pool = GetEntityFromManager(poolId, out entity);
            if (entity != null)
            {
                entity.SetParent(parent, false);
                AdjustParam(ref param, ref duration, delay);
                instanceId = GenInstanceId(entity, duration);
                pool.AddToStrategy(instanceId);
                HandleEntity(entity, param);
            }
            return instanceId;
        }

        protected virtual void HandleEntity(ResourceCacheEntity entity, ResourceCacheBehaviourParam param)
        {
            if (entity != null && entity.Behaviour != null)
            {
                entity.Behaviour.Process(param);
            }
        }

        protected virtual int GenInstanceId(ResourceCacheEntity entity, int duration)
        {
            int instanceId = GetInstanceId(entity, duration);
            entity.InstanceId = instanceId;
            mAcquiredItems.Add(instanceId, entity);
            return instanceId;
        }

        protected virtual void TimerCallback(ResourceCacheEntity entity)
        {
            if (entity != null && entity.Behaviour != null)
            {
                entity.Behaviour.PostCallback();
                ForceRelease(entity.InstanceId);
            }
        }

        protected virtual int GetInstanceId(ResourceCacheEntity entity, int duration)
        {
            if (entity.IsTimerOn)
            {
                return TimerTaskQueue.Instance.AddTimer(0, duration, TimerCallback, entity);
            }
            else
            {
                return TimerTaskQueue.Instance.NextTimerId;
            }
        }

        protected virtual void AdjustParam(ref ResourceCacheBehaviourParam param, ref int duration, int delay)
        {
            if (delay > 0)
            {
                duration += delay;
                param.DelayShow = delay;
            }
        }

        protected virtual ResourceCachePool GetEntityFromManager(int poolId, out ResourceCacheEntity entity)
        {
            entity = null;
            ResourceCachePool pool = mPools[poolId];
            if (pool != null)
            {
                entity = pool.Acquire();
            }
            return pool;
        }

        // this.ForceRelease -> Behaviour.Release -> this.Stop -> this.Release -> Pool.Release
        public virtual void ForceRelease(int instanceId)
        {
            if (mAcquiredItems.ContainsKey(instanceId))
            {
                mAcquiredItems[instanceId].Release();
            }
        }

        public virtual void Stop(int instanceId)
        {
            Release(instanceId);
        }
        protected virtual void Release(int instanceId)
        {
            if (mAcquiredItems.ContainsKey(instanceId))
            {
                ResourceCacheEntity entity = mAcquiredItems[instanceId];
                if (entity.IsTimerOn)
                {
                    TimerTaskQueue.Instance.DelTimer(entity.InstanceId);
                }
                mAcquiredItems.Remove(instanceId);
                ResourceCachePool pool = mPools[entity.ManagerId];
                pool.Release(entity);
            }
        }

        protected virtual void ClearInternal(ResourceCacheMask mask)
        {
            CACHE.Clear();
            List<int> poolKeys = mPools.Keys.ToList();
            foreach (int key in poolKeys)
            {
                ResourceCachePool pool = mPools[key];
                if ((pool.GetMask() & (int)mask) != 0)
                {
                    pool.Clear();
                    CACHE.Add(key);
                }
            }
            foreach (int key in CACHE)
            {
                mPools.Remove(key);
            }
        }

        protected virtual int GetLevel()
        {
            return Level;
        }

        protected virtual bool CheckLevelShow(int level)
        {
            return true;
        }

        public virtual void RemoveByDestroy(int instanceId, GameObject obj)
        {
            if (mAcquiredItems.ContainsKey(instanceId))
            {
                mAcquiredItems.Remove(instanceId);
            }
            else
            {
                DebugUtils.Info("ObjectMaster:RemoveByDestroy", "{0} is not right set", obj.name);
            }
        }
    }
}
