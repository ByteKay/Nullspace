
using System.Collections.Generic;

namespace Nullspace
{
    public class GeoMesh
    {
        public List<GeoTriangle3> mTriangles;
        public GeoMesh(List<GeoTriangle3> triangles)
        {
            mTriangles = triangles;
        }
    }

    public class GeoMeshConvex : GeoMesh
    {
        public GeoMeshConvex(List<GeoTriangle3> triangles)
            : base(triangles)
         
        {
        }
    }
}
