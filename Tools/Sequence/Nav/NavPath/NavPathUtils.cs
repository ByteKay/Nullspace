﻿
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class NavPathUtils
    {
        public static bool Flip(NavPathFlipType type, ref Vector2 value)
        {
            int idx = 0;
            int t = (int)type;
            while ((t & (1 << idx)) != 0 && idx < 2)
            {
                value[idx] = Flip((NavPathFlipType)(1 << idx), value[idx]);
                idx++;
            }
            return true;
        }
        public static bool Flip(NavPathFlipType type, ref Vector3 value)
        {
            int idx = 0;
            int t = (int)type;
            while ((t & (1 << idx)) != 0 && idx < 3)
            {
                value[idx] = Flip((NavPathFlipType)(1 << idx), value[idx]);
                idx++;
            }
            return true;

        }
        public static bool Flip(NavPathFlipType type, ref Vector4 value)
        {
            int idx = 0;
            int t = (int)type;
            while ((t & (1 << idx)) != 0 && idx < 4)
            {
                value[idx] = Flip((NavPathFlipType)(1 << idx), value[idx]);
                idx++;
            }
            return true;
        }

        public static bool Flip(NavPathFlipType type, Vector2 value, out Vector2 outV)
        {
            outV = value;
            int idx = 0;
            int t = (int)type;
            while ((t & (1 << idx)) != 0 && idx < 2)
            {
                outV[idx] = Flip((NavPathFlipType)(1 << idx), value[idx]);
                idx++;
            }
            return true;
        }
        public static bool Flip(NavPathFlipType type, Vector3 value, out Vector3 outV)
        {
            outV = value;
            int idx = 0;
            int t = (int)type;
            while ((t & (1 << idx)) != 0 && idx < 3)
            {
                outV[idx] = Flip((NavPathFlipType)(1 << idx), value[idx]);
                idx++;
            }
            return true;

        }
        public static bool Flip(NavPathFlipType type, Vector4 value, out Vector4 outV)
        {
            outV = value;
            int idx = 0;
            int t = (int)type;
            while ((t & (1 << idx)) != 0 && idx < 4)
            {
                outV[idx] = Flip((NavPathFlipType)(1 << idx), value[idx]);
                idx++;
            }
            return true;
        }

        private static float Flip(NavPathFlipType type, float v)
        {
            switch (type)
            {
                case NavPathFlipType.None:
                    return v;
                case NavPathFlipType.X:
                case NavPathFlipType.Y:
                case NavPathFlipType.Z:
                case NavPathFlipType.W:
                    return -v;
            }
            return v;
        }

        public static AbstractNavPath Create(NavPathType pathType, Vector3 offset, NavPathFlipType flipType, IPathTrigger triggerHandler, List<Vector3> waypoints, int subdivisions = 5)
        {
            NavPathData pathData = CreatePathData(waypoints, subdivisions);
            return Create(pathType, pathData, offset, flipType, triggerHandler);
        }

        public static AbstractNavPath Create(NavPathType pathType, NavPathData pathData, Vector3 offset, NavPathFlipType flipType, IPathTrigger triggerHandler)
        {
            AbstractNavPath navPath = null;
            if (pathData.WayPoints.Count == 2 && pathType != NavPathType.LinePosLineDir)
            {
                pathType = NavPathType.LinePosLineDir;
                DebugUtils.Log(InfoType.Warning, "AbstractNavPath Create LineType, Not " + EnumUtils.EnumToString(pathType));
            }
            switch (pathType)
            {
                case NavPathType.CurvePosCurveDir:
                    navPath = new NavCurvePosCurveDir(pathData, offset, flipType, triggerHandler);
                    break;
                case NavPathType.LinePosLineDir:
                    navPath = new NavLinePosLineDir(pathData, offset, flipType, triggerHandler);
                    break;
                case NavPathType.LinePosLineAngle:
                    navPath = new NavLinePosLineAngle(pathData, offset, flipType, triggerHandler, 10);
                    break;
                case NavPathType.LinePosCurveDir:
                    navPath = new NavLinePosCurveDir(pathData, offset, flipType, triggerHandler);
                    break;
                default:
                    DebugUtils.Log(InfoType.Error, "AbstractNavPath Create Not Supported " + EnumUtils.EnumToString(pathType));
                    break;
            }
            return navPath;
        }

        /// <summary>
        /// 根据路点生成 NavPath
        /// </summary>
        /// <param name="waypoints">路点</param>
        /// <param name="subdivisions">两个路点之间的分段数量。若为0，直接为原始路径</param>
        /// <returns></returns>
        public static NavPathData CreatePathData(List<Vector3> waypoints, int subdivisions = 5)
        {
            int cnt = waypoints.Count;
            DebugUtils.Assert(cnt >= 2, "路点数据量不够 < 2");

            NavPathData pathData = new NavPathData();
            List<Vector3> wayPoints = new List<Vector3>();
            // 拷贝一份 waypoints
            wayPoints.AddRange(waypoints);
            pathData.OriginWayPoints = new List<Vector3>();
            pathData.OriginWayPoints.AddRange(wayPoints);
            if (cnt == 2)
            {
                // 直接用直线, 直接加
                // 后面可通过 判断多点共线，直接使用支线
                pathData.WayPoints.AddRange(wayPoints);
            }
            else
            {
                if (subdivisions > 0)
                {
                    float div = 1.0f / subdivisions;
                    float half = 0.5f * div;
                    List<Vector3> temp = new List<Vector3>();
                    Vector3 diff = (2.0f * wayPoints[1] - wayPoints[0] - wayPoints[2]) * 0.5f;
                    temp.Add(wayPoints[0] + diff);
                    temp.AddRange(wayPoints);
                    diff = (2.0f * wayPoints[wayPoints.Count - 2] - wayPoints[wayPoints.Count - 3] - wayPoints[wayPoints.Count - 1]) * 0.5f;
                    temp.Add(wayPoints[wayPoints.Count - 1] + diff);
                    cnt = temp.Count - 2;
                    // path.KeyPoints 的第0号，现在是第1号；最后一个，现在是倒数第2个
                    for (int i = 1; i < cnt; ++i)
                    {
                        for (float u = 0; u < 1 - div * half; u += div)
                        {
                            Vector3 inter = .5f * (
                               (-temp[i - 1] + 3f * temp[i] - 3f * temp[i + 1] + temp[i + 2]) * (u * u * u)
                               + (2f * temp[i - 1] - 5f * temp[i] + 4f * temp[i + 1] - temp[i + 2]) * (u * u)
                               + (-temp[i - 1] + temp[i + 1]) * u
                               + 2f * temp[i]);
                            pathData.WayPoints.Add(inter);
                        }
                    }
                    pathData.WayPoints.Add(wayPoints[wayPoints.Count - 1]);
                }
                else
                {
                    // 直接走折线段
                    pathData.WayPoints.AddRange(wayPoints);
                }
            }

            cnt = pathData.WayPoints.Count;
            pathData.PathLength = 0.0f;
            pathData.RangeLengths.Add(0.0f);
            for (int i = 1; i < cnt; ++i)
            {
                Vector3 diff = pathData.WayPoints[i] - pathData.WayPoints[i - 1];
                float length = diff.magnitude;
                pathData.PathLength += length;
                pathData.RangeLengths.Add(pathData.PathLength);
            }
            return pathData;
        }
    }
}