
using UnityEngine;

namespace Nullspace
{
    public class FixedPathController
    {
        // 在路径上移动：位置改变和事件触发
        public NavPathType mPathType;
        private AbstractNavPath mNavPath;
        // 改变位置的gameObject
        public Transform mCtlPosition;
        // 改变旋转的gameObject
        public Transform mCtlRotate;
        // 响应触发器的实例
        public IPathTrigger mTriggerHandler;
        // 控制速度
        public float mLineSpeed;
        // 控制速度加成，默认值为1
        public float mLineSpeedTimes;
        // 控制游动
        public bool canMove;
        // 正在执行中处理
        private bool bLocked;

        public FixedPathController()
        {
            mNavPath = null;
            bLocked = false;
            canMove = false;
            mLineSpeed = 0;
            mLineSpeedTimes = 1;
        }

        // Update is called once per frame
        public void Update(float time)
        {
            if (!canMove)
            {
                return;
            }
            MoveTo(time);
        }

        /// <summary>
        /// 开始移动
        /// </summary>
        /// <param name="lineSpeed">线速度设置</param>
        /// <param name="offset">路径偏移设置。编辑器定义</param>
        /// <param name="pathFlip">路径是否做 中心对称旋转(x, y, z) -> (-x, y, -z)</param>
        public void StartMove(AbstractNavPath navPath, float lineSpeed, Vector3 offset, bool pathFlip)
        {
            mNavPath = navPath;
            if (mNavPath != null)
            {
                mCtlPosition.position = mNavPath.CurInfo.curvePos;
                RotateTo(mNavPath.CurInfo.curveDir);
                SetLineSpeed(lineSpeed);
                EnableMove(true);
            }
        }
        /// <summary>
        /// 控制是否移动
        /// </summary>
        /// <param name="enableMove">是否移动</param>
        public void EnableMove(bool enableMove)
        {
            if (bLocked)
            {
                Debug.Log("DragTo 正在执行中");
            }
            canMove = enableMove;
        }

        public virtual void MoveTo(float time)
        {
            float moved = mLineSpeed * mLineSpeedTimes * time;
            if (moved > 0)
            {
                DragTo(moved);
            }
        }

        /// <summary>
        /// 将物体直接拉到某一位置
        /// </summary>
        /// <param name="moved">移动的位置长度</param>
        public void DragTo(float moved)
        {
            bLocked = true;
            float start = Time.realtimeSinceStartup;
            NavPathPoint track = mNavPath.UpdatePath(moved);
            float end = Time.realtimeSinceStartup;
            if (!track.isFinished)
            {
                mCtlPosition.position = track.curvePos;
                RotateTo(track.curveDir);
            }
            bLocked = false;
        }

        /// <summary>
        /// 旋转处理
        /// </summary>
        /// <param name="target">目标朝向</param>
        public virtual void RotateTo(Vector3 target)
        {
            mCtlRotate.forward = target;
        }

        /// <summary>
        /// 设置线速度
        /// </summary>
        /// <param name="speed">线速度</param>
        public void SetLineSpeed(float speed)
        {
            mLineSpeed = speed;
        }

        public void OnDrawGizmosSelected()
        {
            if (mNavPath != null)
            {
                mNavPath.OnDrawGizmos();
            }
        }

        // 指定时间插入
        public void RegisterTrigger(float time, AbstractCallback callback)
        {
            float length = mLineSpeed * time;
            mNavPath.InsertTriggerByLength(false, length, callback);
        }
    }
}
