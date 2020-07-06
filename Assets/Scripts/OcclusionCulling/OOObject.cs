
using UnityEngine;

namespace Nullspace
{
    public class OOObject
    {
        public Matrix4x4 Transform;
        public OOModel Model;
        public OOBox Box;

        public OOObject Next;
        public OOItem Head;
        public OOItem Tail;

        public int TouchId;
        public int DoubleId;
        public int CanOcclude;
        public int IsVisible;

        private MannulDraw Drawer;
        private int Id;

        public OOObject(MannulDraw drawer)
        {
            Drawer = drawer;
            MeshFilter mf = drawer.gameObject.GetComponent<MeshFilter>();
            Model = new OOModel(mf);
            Box = new OOBox(Vector3.one * float.MaxValue, Vector3.one * float.MinValue);
            UpdateTransform();
            Head = new OOItem();
            Tail = new OOItem();
            Tail.CNext = null;
            Head.CPrev = null;
            Head.CNext = Tail;
            Tail.CPrev = Head;
            CanOcclude = 1;
            GeoDebugDrawUtils.DrawAABB(Box.Min, Box.Max);
        }

        public void SetObjectId(int id)
        {
            Id = id;
        }

        public int GetObjectId()
        {
            return Id;
        }

        public void Draw()
        {
            if (Drawer != null)
            {
                Drawer.DrawMesh();
            }
        }

        public void UpdateTransform()
        {
            Transform = Drawer.transform.localToWorldMatrix;
            Box.Mid = Transform * Model.Box.Mid;

            Vector3 va = Vector3.Scale(Transform.GetRow(0), Model.Box.Size);
            Vector3 vb = Vector3.Scale(Transform.GetRow(1), Model.Box.Size);
            Vector3 vc = Vector3.Scale(Transform.GetRow(2), Model.Box.Size);

            Box.Size[0] = Mathf.Abs(va[0]) + Mathf.Abs(vb[0]) + Mathf.Abs(vc[0]);
            Box.Size[1] = Mathf.Abs(va[1]) + Mathf.Abs(vb[1]) + Mathf.Abs(vc[1]);
            Box.Size[2] = Mathf.Abs(va[2]) + Mathf.Abs(vb[2]) + Mathf.Abs(vc[2]);

            Box.ToMinMax();
        }

        public void Detach()
        {
            OOItem itm;
            OOItem itm2;
            itm = Head.CNext;
            while (itm != Tail)
            {
                itm2 = itm.CNext;
                itm.Detach();
                itm = itm2;
            }
            Tail.CNext = null;
            Head.CPrev = null;
            Head.CNext = Tail;
            Tail.CPrev = Head;
        }

    }
}
