using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nullspace
{
    public class ResourceCacheBehaviourParam
    {
        public List<TimerCallback> Callback { get; set; }
        public int DelayShow { get; set; }

        public ResourceCacheBehaviourParam()
        {
            Callback = new List<TimerCallback>();
        }

        public void AddCallback(TimerCallback task)
        {
            if (task != null)
            {
                Callback.Add(task);
            }
        }

        public void InvokeCallback()
        {
            foreach (var callback in Callback)
            {
                callback.Run();
                ObjectPools.Instance.Release(callback);
            }
            Callback.Clear();
        }

    }

    public partial class ResourceCacheBehaviour
    {
        public virtual void StartByDerive()
        {
            // Param
        }

        public virtual void ReleaseEventAndTimers()
        {
            StopDelayTimer();
            StopLifeTimer();
        }

        protected virtual void InterruptWhenUsing()
        {
            // todo by derive
        }

        protected virtual void PreviousDestroy()
        {
            // todo by derive
        }

        protected virtual void PostDestroy()
        {
            // todo by derive
        }

        protected virtual void IntializeBeforeDelay()
        {
            // todo by derive
        }

    }

    public partial class ResourceCacheBehaviour : MonoBehaviour
    {
        public ResourceCachePool Pool { get; set; }
        public ResourceCacheEntity Entity { get; set; }
        public ResourceCacheBehaviourParam Param { get; set; }
        public int LifeTime { get; private set; }
        public int DelayTimerId { get; set; }
        public int LifeTimerId { get; set; }

        public void OnDestroy()
        {
            PreviousDestroy();
            Destroying();
            PostDestroy();
        }

        public void InitializeBase(ResourceCachePool pool, ResourceCacheEntity entity)
        {
            Pool = pool;
            Entity = entity;
            LifeTime = pool.GetLifeTime();
            Param = null;
            DelayTimerId = -1;
            LifeTimerId = -1;
        }

        public void Destroying()
        {
            StopLifeTimer();
            StopDelayTimer();
            if (ResourceCacheBindParent.IsCacheUnusedParent(gameObject))
            {
                DebugUtils.Info("BaseBehaviour:Destroying", "{0}  wrong destroyed ", gameObject.name);
                Pool.OwnedPools.RemoveByDestroy(Entity.InstanceId, gameObject);
            }
            Param = null;
            Pool = null;
            Entity = null;
        }

        public void StartLifeTimer()
        {
            StopLifeTimer();
            LifeTimerId = TimerTaskQueue.Instance.AddTimer(0, LifeTime, LifeEnd);
        }

        public void StopLifeTimer()
        {
            if (LifeTimerId >= 0)
            {
                TimerTaskQueue.Instance.DelTimer(LifeTimerId);
                LifeTimerId = -1;
            }
        }
        private void LifeEnd()
        {
            LifeTimerId = -1;
            if (Pool != null && Entity != null)
            {
                Pool.RemoveEntity(Entity);
            }
        }

        public void Release()
        {
            StopDelayTimer();
            ReleaseEventAndTimers();
            InterruptWhenUsing();
            Pool.OwnedPools.Stop(Entity.InstanceId);
        }

        public virtual void PostCallback()
        {
            Param.InvokeCallback();
        }

        public void Process(ResourceCacheBehaviourParam param)
        {
            Param = param;
            if (!ParseDelay())
            {
                gameObject.SetActive(true);
                StartByDerive();
            }
        }

        protected virtual bool ParseDelay()
        {
            if (Param.DelayShow > 0)
            {
                IntializeBeforeDelay();
                StartDelayTimer();
                return true;
            }
            return false;
        }

        private void StartDelayTimer()
        {
            StopDelayTimer();
            DelayTimerId = TimerTaskQueue.Instance.AddTimer(0, Param.DelayShow, ActiveShow);
        }

        private void StopDelayTimer()
        {
            if (DelayTimerId >= 0)
            {
                TimerTaskQueue.Instance.DelTimer(DelayTimerId);
                DelayTimerId = -1;
            }
        }

        private void ActiveShow()
        {
            DelayTimerId = -1;
            Param.DelayShow = 0;
            Process(Param);
        }
    }
}
