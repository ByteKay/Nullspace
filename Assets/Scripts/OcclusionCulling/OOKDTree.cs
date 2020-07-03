
using UnityEngine;

namespace Nullspace
{
    public class OOKDTree
    {
        public int TouchCounter;
        public OONode Root;

        public OOKDTree()
        {
            Root = new OONode();
            Root.Level = 0;
        }

        public void Add(OOObject obj)
        {
            Root.AddObject(obj);
        }

        public void Delete(OOObject obj)
        {
            obj.Detach();
        }

        public void Refresh(OOObject obj)
        {
            OONode nd;
            nd = obj.Head.CNext.Node;
            Vector3 absV = (obj.Box.Mid - nd.Box.Mid).Abs();
            Vector3 sizeV = nd.Box.Size - obj.Box.Size;
            if (absV.Less(sizeV))
                return;
            while (nd.Parent != null && absV.AnyGreater(sizeV)) 
                nd = nd.Parent;
            obj.Detach();
            nd.AddObject(obj);
        }

        public void Init(ref Vector3 min, ref Vector3 max)
        {
            Root.Box.Min = min;
            Root.Box.Max = max;
            Root.Box.ToMidSize();
        }
    }
}
