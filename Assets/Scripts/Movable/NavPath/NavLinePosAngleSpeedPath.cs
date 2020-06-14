
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class NavLinePosAngleSpeedPath : AbstractNavPath
    {
        private float mFrameCountInv;   // 插值帧数的倒数
        private int mCurFrame;          // 当前执行的帧数
        private Vector3 mLast;          // 插值的起始方向
        private Vector3 mNext;          // 插值的结束方向

        private List<Vector3> trackPos = new List<Vector3>();
        private float mMovedTime = 0;
        private float mMovedLength = 0;

        public NavLinePosAngleSpeedPath(int pathId, Vector3 offset, bool pathFlipOn, IPathTrigger triggerHandler, float frameCount) : base(pathId, offset, pathFlipOn, triggerHandler)
        {
            // 会先执行 Initialize()
            mFrameCountInv = 1.0f / frameCount;
            mCurFrame = 0;
            CurInfo.isDirChanged = true;
            UpdatePosAndTangent();
            CurInfo.curveDir = mNext;
            mLast = mNext;
            mMovedTime = 0;
            mMovedLength = 0;
        }

        protected override void Initialize()
        {
            InitializeAppendWaypoint();
            RegisterAllTriggers();
        }
        public override void OnDrawGizmos()
        {
            if (trackPos == null)
            {
                return;
            }
            int step = 4;
            int cnt = trackPos.Count / step - 1;
            if (cnt < 0)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(trackPos[cnt * step], 0.2f);
            Gizmos.DrawSphere(trackPos[cnt * step + 2], 0.2f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(trackPos[cnt * step] + mOffset, trackPos[cnt * step + 2] + mOffset);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(trackPos[cnt * step] + mOffset, trackPos[cnt * step] + 2 * trackPos[cnt * step + 1] + mOffset);
            Gizmos.DrawLine(trackPos[cnt * step + 2] + mOffset, trackPos[cnt * step + 2] + 2 * trackPos[cnt * step + 3] + mOffset);

            if (mMovedTime != 0)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(trackPos[cnt * step + 2] + 10 * trackPos[cnt * step + 3] + mOffset, string.Format("speed: {0}", mMovedLength / mMovedTime));
#endif
            }

            for (int i = 1; i <= cnt; ++i)
            {
                Gizmos.color = (i % 2 == 0) ? Color.black : Color.white;
                Gizmos.DrawLine(trackPos[(i - 1) * step + 2] + mOffset, trackPos[i * step + 2] + mOffset);
                //Gizmos.color = (i % 2 == 0) ? Color.blue : Color.green;
                //Gizmos.DrawLine(trackPos[(i - 1) * step] + mOffset, trackPos[i * step] + mOffset);
            }
        }

        protected override void UpdatePosAndTangent()
        {
            mCurFrame++;
            float len = mPathLengthMoved - GetLength(mCurrentWaypointIndex);
            float lenTotal = GetLength(mCurrentWaypointIndex + 1) - GetLength(mCurrentWaypointIndex);
            float u = len / lenTotal;
            Vector3 start = GetWaypoint(mCurrentWaypointIndex);
            Vector3 end = GetWaypoint(mCurrentWaypointIndex + 1);

            Vector3 linePos = (1 - u) * start + u * end;
            // mMovedTime += Time.deltaTime;
            // mMovedLength += (linePos - mCurInfo.linePos).magnitude;
            // CurInfo.linePos = linePos;
            CurInfo.curvePos = linePos;
            if (CurInfo.isDirChanged)
            {
                mLast = CurInfo.curveDir;
                // 测试
                mNext = Vector3.forward;// (end - start).normalized;
                mCurFrame = 0;
            }

            float pro = mCurFrame * mFrameCountInv;
            if (pro < 1)
            {
                CurInfo.curveDir = GeoUtils.Interpolation(mLast, mNext, pro).normalized;
                // CurInfo.lineDir = CurInfo.curveDir;
            }
            else
            {
                CurInfo.curveDir = mNext;
                // CurInfo.lineDir = mNext;
            }

            // 记录曲线点和切向，以及线上点和切向
            if (trackPos != null)
            {
                trackPos.Add(mCurInfo.linePos);
                trackPos.Add(mCurInfo.lineDir);
                trackPos.Add(mCurInfo.curvePos);
                trackPos.Add(mCurInfo.curveDir);
            }
        }
    }
}
