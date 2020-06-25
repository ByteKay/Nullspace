using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Internal;

namespace Nullspace
{
    public class GameObjectPathTween : MonoBehaviour, IPathTrigger
    {
        private AbstractNavPath NavPath;
        private float Duration;
        private float Speed;
        protected bool EnableMove;
        protected bool EnableRotate;
        protected Transform MoveTransform;
        protected Transform RotateTransform;

        public void SetPath(AbstractNavPath navPath, bool enableMove = false, Transform moveTransform = null, bool enableRotate = false, Transform rotateTransform = null)
        {
            NavPath = navPath;
            MoveTransform = moveTransform;
            RotateTransform = rotateTransform;
            EnableMove = enableMove;
            EnableRotate = enableRotate;
        }
        
        // 根据总长度和总时长 求出 移动速度
        public void SetDuration(float duration)
        {
            Debug.Assert(duration > 0, "wrong");
            Duration = duration;
            Speed = NavPath.PathLength / duration;
        }

        // 直接设置速度
        public void SetSpeed(float speed)
        {
            Debug.Assert(speed > 0, "wrong");
            Speed = speed;
            Duration = NavPath.PathLength / speed;
        }

        // 指定时间插入
        public void RegisterTrigger(float time, AbstractCallback callback)
        {
            float length = Speed * time;
            NavPath.InsertTriggerByLength(false, length, callback);
        }

        public void Update()
        {
            Step(Speed * Time.deltaTime);
        }

        public void Step(float moved)
        {
            if (EnableMove)
            {
                NavPath.UpdatePath(moved);
                if (MoveTransform != null)
                {
                    MoveTransform.position = NavPath.CurInfo.curvePos;
                }
                if (EnableRotate && RotateTransform != null)
                {
                    RotateTransform.forward = NavPath.CurInfo.curveDir;
                }
            }
        }

        public void OnPathStart()
        {
            DebugUtils.Info("GameObjectPathTween", "OnPathStart");
        }

        public void OnPathEnd()
        {
            DebugUtils.Info("GameObjectPathTween", "OnPathEnd");
        }

        public void OnPathTrigger(int triggerId)
        {
            DebugUtils.Info("GameObjectPathTween", "OnPathTrigger ", triggerId);
        }

        public void OnPathTrigger(AbstractCallback callback)
        {
            callback.Run();
            DebugUtils.Info("GameObjectPathTween", "OnPathTrigger AbstractCallback");
        }
    }
}
