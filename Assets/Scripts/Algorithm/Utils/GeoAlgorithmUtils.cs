
using UnityEngine;
namespace Nullspace
{
    public class GeoAlgorithmUtils
    {
        public static bool IsOnSamePlane(GeoPointsArray3 points, ref GeoPlane plane)
        {
            if (points.Size() < 4)
            {
                return true;
            }
            int count = points.Size();
            int i = 2;
            Vector3 p = points[0];
            for (; i < count; ++i)
            {
                p = points[i];
                if (!GeoLineUtils.IsPointInLine3(points[0], points[1], ref p))
                {
                    break;
                }
            }
            if (i == count)
            {
                return true;
            }
            plane = GeoPlaneUtils.CreateFromTriangle(points[0], points[1], p);
            i = 2;
            int c = 2;
            for (; i < count; ++i)
            {
                if (GeoPlaneUtils.IsPointOnPlane(plane.mNormal, plane.mD, points[i]))
                {
                    c++;
                }
            }
            return c == count;
        }

        public static GeoPointsArray2 BuildConvexHull(GeoPointsArray2 points)
        {
            points.Distinct();
            return new GeoPointsArray2(JarvisConvex.BuildHull(points.mPointArray));
        }

        public static GeoPointsArray3 BuildConvexHull(GeoPointsArray3 points)
        {
            points.Distinct();
            GeoPlane plane = new GeoPlane(Vector3.zero, 0);
            if (IsOnSamePlane(points, ref plane))
            {
                GeoPointsArray2 point2 = new GeoPointsArray2(GeoPlaneUtils.PlaneTransformLocal(points.mPointArray, plane));
                point2 = BuildConvexHull(point2);
                return new GeoPointsArray3(GeoPlaneUtils.PlaneTransformGlobal(point2.mPointArray, plane));
            }
            else
            {
                return new GeoPointsArray3(QuickHull.BuildHull(points));
            }
        }

    }
}
