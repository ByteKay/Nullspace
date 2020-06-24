
using UnityEngine;

namespace Nullspace
{
    public class ooce_object
    {
        private ooce_object next;
        private ooce_item head;
        private ooce_item tail;
        private Matrix4x4 transform;
        private int touch_id;
        private int double_id;
        private int id;
        private int is_visible;
        private ooce_model model;

        public int can_occlude;
        public bbox b;

        public ooce_object()
        {
            head = new ooce_item();
            tail = new ooce_item();
            tail.cnext = null;
            head.cprev = null;
            head.cnext = tail;
            tail.cprev = head;
            can_occlude = 1;

            model = null;
        }

        public void UseModel(ooce_model md)
        {
            model = md;
        }

        public void SetTransform(ref Matrix4x4 m)
        {
            transform = m;
            // update bbox
        }

        public void Detach()
        {
            ooce_item itm;
            ooce_item itm2;
            itm = head.cnext;
            while (itm != tail)
            {
                itm2 = itm.cnext;
                itm.Detach();
                itm = itm2;
            }
            tail.cnext = null;
            head.cprev = null;
            head.cnext = tail;
            tail.cprev = head;
        }

        public void SetID(int ident)
        {
            id = ident;
        }
        public int GetID()
        {
            return id;
        }
        public int GetModelID()
        {
            return model.id;
        }
        public void GetTransform(ref Matrix4x4 m)
        {
            m = transform;
        }
    }
}
