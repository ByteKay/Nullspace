
using UnityEngine;

namespace Nullspace
{
    public class OOObject
    {

        public OOItem Tail;
        public Matrix4x4 Transform;
        public int IsVisible;
        public OOModel Model;
        public int Id;
        public OOObject Next;
        public int TouchId;
        public int DoubleId;
        public OOItem Head;
        public int CanOcclude;
        public OOBox Box;

        public OOObject()
        {
            Head = new OOItem();
            Tail = new OOItem();
            Tail.CNext = null;
            Head.CPrev = null;
            Head.CNext = Tail;
            Tail.CPrev = Head;
            CanOcclude = 1;
            Model = null;
        }

        public void UseModel(OOModel md)
        {
            Model = md;
        }

        public void SetTransform(ref Matrix4x4 m)
        {
            Transform = m;
            Box.Mid = m * Model.Box.Mid;

            Vector3 va = Vector3.Scale(m.GetColumn(0), Model.Box.Size);
            Vector3 vb = Vector3.Scale(m.GetColumn(1), Model.Box.Size);
            Vector3 vc = Vector3.Scale(m.GetColumn(2), Model.Box.Size);

            Box.Size[0] = Mathf.Abs(va[0]) + Mathf.Abs(vb[0]) + Mathf.Abs(vc[0]);
            Box.Size[1] = Mathf.Abs(va[1]) + Mathf.Abs(vb[1]) + Mathf.Abs(vc[1]);
            Box.Size[2] = Mathf.Abs(va[2]) + Mathf.Abs(vb[2]) + Mathf.Abs(vc[2]);
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

        public void SetID(int ident)
        {
            Id = ident;
        }
        public int GetID()
        {
            return Id;
        }
        public int GetModelID()
        {
            return Model.Id;
        }
        public void GetTransform(ref Matrix4x4 m)
        {
            m = Transform;
        }
    }
}
