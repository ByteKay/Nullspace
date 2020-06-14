/********************************************************************************
** All rights reserved
** Auth： kay.yang
** E-mail: 1025115216@qq.com
** Date： 6/30/2017 11:13:04 AM
** Version:  v1.0.0
*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
