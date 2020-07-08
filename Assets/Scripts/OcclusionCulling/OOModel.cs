
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class OOModel
    {
        public MeshFilter MeshFilter;
        public Vector3[] Vertices;
        public Vector3i[] Faces;
        public int NumVert;
        public int NumFace;
        public OOBox Box;
        public Vector3[] CameraSpaceVertices;
        public Vector4[] ClipSpaceVertices;

        public OOModel(MeshFilter filter)
        {
            MeshFilter = filter;
            MeshFilter.sharedMesh.RecalculateBounds();
            Vertices = MeshFilter.sharedMesh.vertices;
            Faces = ArrayToList(MeshFilter.sharedMesh.triangles);
            NumVert = Vertices.Length;
            NumFace = Faces.Length;
            CameraSpaceVertices = new Vector3[NumVert];
            ClipSpaceVertices = new Vector4[NumVert];
            Bounds b = MeshFilter.sharedMesh.bounds;
            Box = new OOBox(b.min, b.max);
        }

        private Vector3i[] ArrayToList(int[] triangles)
        {
            int len = triangles.Length;
            Vector3i[] faces = new Vector3i[len / 3];
            int idx = 0;
            for (int i = 0; i < len; i += 3)
            {
                faces[idx++] = new Vector3i(triangles[i], triangles[i + 1], triangles[i + 2]);
            }
            return faces;
        }
    }
}
