
using System.Collections.Generic;
using UnityEngine;
namespace Nullspace
{

    public class RayTracerTest : MonoBehaviour
    {
        private List<BVHObject> mTriangleObjects;
        private GameObject mMeshObject;
        private MeshFilter mMeshFilter;
        private MeshRenderer mMeshRenderer;

        private BVH mBVH;
        private void Awake()
        {
            mTriangleObjects = new List<BVHObject>();
            mMeshObject = new GameObject("mesh");
            mMeshFilter = mMeshObject.AddComponent<MeshFilter>();
            mMeshRenderer = mMeshObject.AddComponent<MeshRenderer>();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("MeshCreate"))
            {
                mTriangleObjects.Clear();
                List<Vector3> drawTriangles = new List<Vector3>();
                BuildTriangles(ref mTriangleObjects, ref drawTriangles);
                List<Vector3> vertices = new List<Vector3>();
                List<int> indices = new List<int>();
                GeoUtils.MeshVertexPrimitiveType(drawTriangles, ref vertices, ref indices);
                Mesh mesh = new Mesh();
                mesh.SetVertices(vertices);
                mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
                mMeshFilter.mesh = mesh;
                float start = Time.realtimeSinceStartup;
                mBVH = new BVH(mTriangleObjects);
                float end1 = Time.realtimeSinceStartup;
                Debug.Log(string.Format("time initialized: {0}", end1 - start));

            }
            if (GUILayout.Button("RayTest"))
            {
                List<BVHRay> rayList = new List<BVHRay>();
                BuildRay(ref rayList, ref mTriangleObjects);
                float end1 = Time.realtimeSinceStartup;
                BVHIntersectionInfo insect = new BVHIntersectionInfo();
                int insectC = 0;
                int missC = 0;
                for (int i = 0; i < 200; ++i)
                {
                    foreach (BVHRay ray in rayList)
                    {
                        bool isInsect = mBVH.GetIntersection(ray, ref insect, false);
                        int test = isInsect ? insectC++ : missC++;
                        if (isInsect)
                        {
                            Debug.DrawLine(ray.mOrigin, insect.mHitPoint, Color.blue, 2000);
                        }
                    }
                }
                float end2 = Time.realtimeSinceStartup;
                Debug.Log(string.Format("time slapped: {0}, insect: {1}, miss: {2}", end2 - end1, insectC, missC));
            }

        }


        public static void BuildTriangles(ref List<BVHObject> triObjects, ref List<Vector3> drawTriangles)
        {
            int w = 100;
            int h = 100;
            float startW = 0;
            float startH = 0;
            float wStep = 5f;
            float hStep = 5f;
            float y = 4.0f;
            Vector3 v1;
            Vector3 v2;
            Vector3 v3;
            Vector3 v4;
            Vector3 v5;
            Vector3 v6;
            while (startW < w)
            {
                while (startH < h)
                {
                    v1 = new Vector3(startW, y, startH);
                    v2 = new Vector3(startW, y, startH + hStep);
                    v3 = new Vector3(startW + wStep, y, startH + hStep);

                    v4 = new Vector3(startW + wStep, y, startH + hStep);
                    v5 = new Vector3(startW + wStep, y, startH);
                    v6 = new Vector3(startW, y, startH);
                    drawTriangles.Add(v1);
                    drawTriangles.Add(v2);
                    drawTriangles.Add(v3);
                    drawTriangles.Add(v4);
                    drawTriangles.Add(v5);
                    drawTriangles.Add(v6);
                    BVHTriangleObject tri1 = new BVHTriangleObject(v1, v2, v3);
                    BVHTriangleObject tri2 = new BVHTriangleObject(v4, v5, v6);
                    triObjects.Add(tri1);
                    triObjects.Add(tri2);
                    
                    startH += hStep;
                }
                startW += wStep;
                startH = 0;
            }
        }

        public static void BuildRay(ref List<BVHRay> rayList, ref List<BVHObject> triObjects)
        {
            BVHIntersectionInfo info = new BVHIntersectionInfo();
            for (int i = 0; i < triObjects.Count; ++i)
            {
                Vector3 pos = triObjects[i].GetCentroid();
                BVHRay ray = new BVHRay(new Vector3(pos.x, 0.0f, pos.z), Vector3.up);
                rayList.Add(ray);
            }
        }
    }
}
