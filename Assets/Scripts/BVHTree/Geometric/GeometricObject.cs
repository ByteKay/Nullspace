/********************************************************************************
** All rights reserved
** Auth： kay.yang
** E-mail: 1025115216@qq.com
** Date： 6/30/2017 11:13:04 AM
** Version:  v1.0.0
*********************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public enum GeoShape
    {
        // 3d的圆 由 2d的圆 + 3d Plane
        GeoCircle3,
        GeoCircle2,
        // 3d的椭圆 由 2d的椭圆 + 3d Plane
        GeoEllipse2,
        GeoEllipse3,
        // 3d的多边形 由 2d的多边形 + 3d Plane
        GeoPolygon2, // include concave and convex
        GeoPolygon3,
        GeoPolygonConvex2, // 凸多边形
        // 散点 数组
        PointsArray2,
        PointsArray3,
        // 直线
        GeoLine2,
        GeoLine3,
        // 线段
        GeoSegment2,
        GeoSegment3,
        // 射线
        GeoRay2,
        GeoRay3,
        // 三角面
        GeoTriangle2,
        GeoTriangle3,
        // 2d为box或者矩形 3d 包围盒
        GeoAABB2,
        GeoAABB3,

        // next only in 3d
        GeoPlane,     // 平面
        GeoSphere,    // 球
        GeoEllipsoid, // 椭球
        GeoCylinder,  // 圆柱体
        GeoCapsule,   // 胶囊体
        GeoMesh,      // 三角面网格模型
        GeoMeshConvex,//3d 凸包

        // 以下为曲线和曲面，暂不实现
        Bezier2, // 贝塞尔
        Bezier3, // 贝塞尔
        BSpline2, // b样条
        BSpline3, // b样条
        Nurbs2,  // nurbs  非均匀有理B样条
        Nurbs3,  // nurbs  非均匀有理B样条
        NONE
    }

    // 异面直线 最近 两点
    public class GeoInsectEachInfo
    {
        public bool mIsIntersect;
        public float mLength;
        public GeometricObject mHitObject1;
        public Vector3 mHitGlobalPoint1; // if Vector2, then add 0 to z
        public GeometricObject mHitObject2;
        public Vector3 mHitGlobalPoint2; // if Vector2, then add 0 to z
        public GeoInsectEachInfo()
        {
            mIsIntersect = false;
            mLength = 0.0f;
        }
    }

    // 相交一个点
    public class GeoInsectPointInfo
    {
        public bool mIsIntersect;
        public float mLength;
        public GeometricObject mHitObject1;
        public GeometricObject mHitObject2;
        public Vector3 mHitGlobalPoint; // if Vector2, then add 0 to z
        public GeoInsectPointInfo()
        {
            mIsIntersect = false;
            mLength = 0.0f;
        }
    }

    // 相交有多个点  直线与三角形相交两个点
    public class GeoInsectPointArrayInfo
    {
        public bool mIsIntersect;
        public float mLength;
        public GeometricObject mHitObject1;
        public GeometricObject mHitObject2;
        public GeoPointsArray3 mHitGlobalPoint; // if Vector2, then add 0 to z
        public GeoInsectPointArrayInfo()
        {
            mIsIntersect = false;
            mLength = 0.0f;
            mHitGlobalPoint = new GeoPointsArray3();
        }
        public void Clear()
        {
            mIsIntersect = false;
            mLength = 0.0f;
            mHitGlobalPoint.Clear();
        }

        public void UniquePoint()
        {
            mHitGlobalPoint.Distinct();
        }

    }

    // 相交成几何形状 面与面相交为直线等
    public class GeoInsectObjectArrayInfo
    {
        public bool mIsIntersect;
        public float mLength;
        public GeometricObject mHitObject1;
        public GeometricObject mHitObject2;
        public List<GeometricObject> mHitGlobalPoint; // if Vector2, then add 0 to z
        public GeoInsectObjectArrayInfo()
        {
            mIsIntersect = false;
            mLength = 0.0f;
            mHitGlobalPoint = new List<GeometricObject>();
        }
        public void Clear()
        {
            mIsIntersect = false;
            mLength = 0.0f;
            mHitGlobalPoint.Clear();
        }
    }

    public class GeometricObject
    {
        public static Vector2 MAX_VECTOR2 = new Vector2(float.MaxValue, float.MaxValue);
        public static Vector2 MIN_VECTOR2 = new Vector2(float.MinValue, float.MinValue);
        public static Vector3 MAX_VECTOR3 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        public static Vector3 MIN_VECTOR3 = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        public GeoShape mShapeType;
        private Dictionary<GeoShape, GeoShape> _type_insect = new Dictionary<GeoShape, GeoShape>();
        static GeometricObject()
        {
            RegisterInsect();
        }

        public GeometricObject(GeoShape shape)
        {
            mShapeType = shape;
        }

        public GeometricObject()
        {
            mShapeType = GeoShape.NONE;
        }

        public static void RegisterInsect()
        {
            // 添加 碰撞类型之间的限制
        }

        public virtual bool IsIntersect(ref GeoRay2 dist, ref GeoInsectPointArrayInfo insect)
        {
            return false;
        }

        public virtual bool IsIntersect(ref GeoRay3 dist, ref GeoInsectPointArrayInfo insect)
        {
            return false;
        }
        // 点在obj里面
        public virtual bool IsPointIn(ref Vector2 p)
        {
            // to do
            return false;
        }
        public virtual bool IsPointIn(ref Vector3 p)
        {
            // to do
            return false;
        }

        // 点在obj上面
        public virtual bool IsPointOn(ref Vector3 p)
        {
            // to do
            return false;
        }
        public virtual bool IsPointOn(ref Vector2 p)
        {
            // to do
            return false;
        }

        // 点距离obj最短距离
        public virtual float PointClosestDistance(ref Vector2 p)
        {
            // to do
            return 0.0f;
        }
        public virtual float PointClosestDistance(ref Vector3 p)
        {
            // to do
            return 0.0f;
        }

        // 点距离obj垂直距离
        public virtual float PointToDistance(ref Vector2 p)
        {
            // to do
            return 0.0f;
        }
        public virtual float PointToDistance(ref Vector3 p)
        {
            // to do
            return 0.0f;
        }

        // 点的法向
        public virtual Vector2 GetNormal(ref Vector2 p)
        {
            // to do
            return MAX_VECTOR2;
        }

        public virtual Vector2 GetNormal(ref Vector3 p)
        {
            // to do
            return MAX_VECTOR3;
        }
    }

}
