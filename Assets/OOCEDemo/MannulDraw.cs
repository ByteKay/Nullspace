using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Nullspace
{
    public class MannulDraw : MonoBehaviour
    {
        // DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int layer);
        private Mesh mesh;
        private Material material;
        private int layer;

        private void Awake()
        {
            mesh = GetComponent<MeshFilter>().sharedMesh;
            material = GetComponent<MeshRenderer>().material;
        }

        public void DrawMesh()
        {
            Graphics.DrawMesh(mesh, transform.localToWorldMatrix, material, gameObject.layer);
        }

        private void OnEnable()
        {
            MannulDrawManager.Instance.AddObject(this);
        }

        private void OnDisable()
        {
            MannulDrawManager.Instance.RemoveObject(this);
        }

        private void OnDestroy()
        {
            MannulDrawManager.Instance.RemoveObject(this);
        }
    }

}
