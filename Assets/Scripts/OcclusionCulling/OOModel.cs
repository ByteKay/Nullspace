
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
            // Faces = ArrayToList(MeshFilter.sharedMesh.triangles);
            Vertices = new Vector3[] { new Vector3(1.14f, -0.7f, -5.94f), new Vector3(1.14f, 1.96f, -7.81f), new Vector3(7.28f, -0.7f, -5.94f) };
            Faces = new Vector3i[] { new Vector3i(0, 1, 2) };
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
