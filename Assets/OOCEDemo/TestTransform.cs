
using UnityEngine;

namespace Nullspace
{
    public class TestTransform : MonoBehaviour
    {
        private Vector3[] Vertices;
        private Vector3i[] Faces;
        private int NumVert;
        private int NumFace;

        private Vector4[] ClipSpaceVertices;

        private Camera CameraMain;
        private void Awake()
        {
            Vertices = new Vector3[] { new Vector3(1.14f, -0.7f, -5.94f), new Vector3(1.14f, 1.96f, -7.81f), new Vector3(7.28f, -0.7f, -5.94f) };
            Faces = new Vector3i[] { new Vector3i(0, 1, 2) };
            NumVert = Vertices.Length;
            NumFace = Faces.Length;
            ClipSpaceVertices = new Vector4[NumVert];
            CameraMain = Camera.main;
            Test();
        }
        
        private void Test()
        {
            Matrix4x4 m = transform.localToWorldMatrix;
            Matrix4x4 view = CameraMain.worldToCameraMatrix;
            Matrix4x4 project = CameraMain.projectionMatrix;
            Matrix4x4 mvp = project * view * m;

            // 变换mesh的顶点到 相机空间 和 裁剪空间
            for (int i = 0; i < NumVert; i++)
            {
                Vector4 v = Vertices[i];
                v.w = 1;
                ClipSpaceVertices[i] = mvp * v;
            }
        }
    }
}
