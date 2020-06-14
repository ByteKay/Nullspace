
using UnityEngine;

namespace Nullspace
{
    /// <summary>
    /// 开始触发器定义
    /// </summary>
    public class PathStartTrigger : PathTrigger
    {
        public PathStartTrigger(float length)
        {
            this.mTriggerLength = length;
        }

        public override void OnDrawGizmos(Vector3 pos)
        {
            Gizmos.DrawSphere(pos, 1);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(pos, "start");
#endif
        }

        public override void OnTrigger(ITriggerHandler handler)
        {
            if (handler != null)
            {
                handler.OnPathStart();
            }
        }
    }

}
