
using UnityEngine;

namespace Nullspace
{
    public class NavCurvePosCurveDir : AbstractNavPath
    {
        public NavCurvePosCurveDir(NavPathData pathData, Vector3 offset, NavPathFlipType flipType, IPathTrigger triggerHandler) : base(pathData, offset, flipType, triggerHandler)
        {

        }

        protected override void Initialize()
        {
            base.Initialize();
#if UNITY_EDITOR
            mMovedTime = 0;
            mMovedLength = 0;
#endif
        }

        /// <summary>
        /// 待优化版本：思路清晰为主
        /// </summary>
        protected override void UpdatePosAndTangent()
        {
            // 计算或缓存所处线段的总长度
            float lenTotal = GetLength(mCurrentWaypointIndex + 1) - GetLength(mCurrentWaypointIndex);
            // 计算所处线段已走过的长度
            float len = mPathLengthMoved - GetLength(mCurrentWaypointIndex);
            // 计算 已走过的线段长度/线段的总长度
            float u = len / lenTotal;

            // 确定当前线段的前一个点
            Vector3 prevStart = mCurrentWaypointIndex == 0 ? mWaypointAppend[0] : GetWaypoint(mCurrentWaypointIndex - 1);
            // 确定当前线段的起点
            Vector3 start = GetWaypoint(mCurrentWaypointIndex);
            // 确定当前线段的终点
            Vector3 end = GetWaypoint(mCurrentWaypointIndex + 1);
            // 确定当前线段的下一个点
            Vector3 endNext = (mCurrentWaypointIndex + 2) >= mPathData.WayPoints.Count ? mWaypointAppend[1] : GetWaypoint(mCurrentWaypointIndex + 2);

            //// 套用插值公式 计算当前时刻所处曲线的点
            //Vector3 inter = .5f * (
            //       (-prevStart + 3f * start - 3f * end + endNext) * (u * u * u)
            //       + (2f * prevStart - 5f * start + 4f * end - endNext) * (u * u)
            //       + (-prevStart + end) * u
            //       + 2f * start);
            //// 套用插值公式 计算当前时刻所处曲线点的切线
            //Vector3 tangent = .5f * (
            //       (-prevStart + 3f * start - 3f * end + endNext) * (3 * u * u)
            //       + (2f * prevStart - 5f * start + 4f * end - endNext) * (2 * u)
            //       + (-prevStart + end) * 1
            //       + 2f * start * 0);

            // 套用插值公式 计算当前时刻所处曲线的点
            float tu = u * u;
            Vector3 inter = .5f * (
                   (-prevStart + 3f * start - 3f * end + endNext) * (u * tu)
                   + (2f * prevStart - 5f * start + 4f * end - endNext) * tu
                   + (-prevStart + end) * u) + start;
            // 套用插值公式 计算当前时刻所处曲线点的切线
            Vector3 tangent = .5f * (
                   (-prevStart + 3f * start - 3f * end + endNext) * (3 * tu)
                   + (-prevStart + end)) + (2f * prevStart - 5f * start + 4f * end - endNext) * u;

#if UNITY_EDITOR
            // 保存 当前时刻曲线的点坐标
            mMovedTime += Time.deltaTime;
            mMovedLength += (inter - CurInfo.curvePos).magnitude;
#endif
            // 线性插值 计算当前时刻所处线段的点
            CurInfo.linePos = (1 - u) * start + u * end;
            // 计算当前时刻所处线段的方向
            CurInfo.lineDir = (end - start).normalized;
            // 保存 当前时刻点
            CurInfo.curvePos = inter;
            // 保存 当前时刻点坐标的切向
            CurInfo.curveDir = tangent.normalized;

#if UNITY_EDITOR
            // 记录曲线点和切向，以及线上点和切向
            if (trackPos != null)
            {
                trackPos.Add(CurInfo.linePos);
                trackPos.Add(CurInfo.lineDir);
                trackPos.Add(CurInfo.curvePos);
                trackPos.Add(CurInfo.curveDir);
            }
#endif
        }

#if UNITY_EDITOR
        private List<Vector3> trackPos = new List<Vector3>();
        private float mMovedTime = 0;
        private float mMovedLength = 0;
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
                UnityEditor.Handles.Label(trackPos[cnt * step + 2] + 10 * trackPos[cnt * step + 3] + mOffset, string.Format("speed: {0}", mMovedLength / mMovedTime));
            }
            for (int i = 1; i <= cnt; ++i)
            {
                Gizmos.color = (i % 2 == 0) ? Color.black : Color.white;
                Gizmos.DrawLine(trackPos[(i - 1) * step + 2] + mOffset, trackPos[i * step + 2] + mOffset);
            }
        }
#endif

    }
}
