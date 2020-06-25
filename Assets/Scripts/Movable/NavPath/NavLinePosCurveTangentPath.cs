
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class NavLinePosCurveTangentPath : AbstractNavPath
    {
        public NavLinePosCurveTangentPath(int pathId, Vector3 offset, bool pathFlipOn, IPathTrigger triggerHandler) : base(pathId, offset, pathFlipOn, triggerHandler)
        {
#if UNITY_EDITOR
            mMovedTime = 0;
            mMovedLength = 0;
#endif
        }

        protected override void UpdatePosAndTangent()
        {
            float len = mPathLengthMoved - GetLength(mCurrentWaypointIndex);
            float lenTotal = GetLength(mCurrentWaypointIndex + 1) - GetLength(mCurrentWaypointIndex);
            float u = len / lenTotal;
            Vector3 start = GetWaypoint(mCurrentWaypointIndex);
            Vector3 end = GetWaypoint(mCurrentWaypointIndex + 1);
            Vector3 linePos = (1 - u) * start + u * end;
#if UNITY_EDITOR
            mMovedTime += Time.deltaTime;
            mMovedLength += (linePos - mCurInfo.linePos).magnitude;
#endif
            Vector3 prevStart = mCurrentWaypointIndex == 0 ? mWaypointAppend[0] : GetWaypoint(mCurrentWaypointIndex - 1);
            Vector3 endNext = (mCurrentWaypointIndex + 2) >= mPathData.WayPoints.Count ? mWaypointAppend[1] : GetWaypoint(mCurrentWaypointIndex + 2);
            Vector3 tangent = .5f * (
                   (-prevStart + 3f * start - 3f * end + endNext) * (3 * u * u)
                   + (-prevStart + end)) + (2f * prevStart - 5f * start + 4f * end - endNext) * u;
            mCurInfo.linePos = linePos;
            mCurInfo.lineDir = (end - start).normalized;
            mCurInfo.curvePos = (1 - u) * start + u * end;
            mCurInfo.curveDir = tangent.normalized;

#if UNITY_EDITOR
            // 记录曲线点和切向，以及线上点和切向
            if (trackPos != null)
            {
                trackPos.Add(mCurInfo.linePos);
                trackPos.Add(mCurInfo.lineDir);
                trackPos.Add(mCurInfo.curvePos);
                trackPos.Add(mCurInfo.curveDir);
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
                //Gizmos.color = (i % 2 == 0) ? Color.blue : Color.green;
                //Gizmos.DrawLine(trackPos[(i - 1) * step] + mOffset, trackPos[i * step] + mOffset);
            }
        }
#endif
    }
}
