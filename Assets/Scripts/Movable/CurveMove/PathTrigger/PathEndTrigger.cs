﻿
using UnityEngine;

namespace Nullspace
{
    /// <summary>
    /// 结束触发器定义
    /// </summary>

    public class PathEndTrigger : PathTrigger
    {
        public PathEndTrigger(float length)
        {
            this.mTriggerLength = length;
        }

        public override void OnDrawGizmos(Vector3 pos)
        {
            Gizmos.DrawSphere(pos, 1);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(pos, "end");
#endif
        }

        public override void OnTrigger(ITriggerHandler handler)
        {
            if (handler != null)
            {
                handler.OnPathEnd();
            }
        }
    }
}
