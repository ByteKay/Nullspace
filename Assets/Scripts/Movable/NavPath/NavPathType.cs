﻿
namespace Nullspace
{
    /// <summary>
    /// 确定坐标和方向的计算方式
    /// </summary>
    public enum NavPathType
    {
        RealCurve,  // 曲线长度确定u值，曲线插值计算坐标和方向
        Curve,      // 线段长度确定u值，曲线插值计算坐标和方向
        Line,       // 线段长度确定u值，线性插值计算坐标，使用线段方向
        LineCurve,  // 线段长度确定u值，线性插值计算坐标，曲线插值计算方向
        LineAngle   // 线段长度确定u值，线性插值计算坐标，利用角速度线性插值计算方向
    }
}