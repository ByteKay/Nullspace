
using UnityEngine;

namespace Nullspace
{
    public class OONode
    {
        public const int AXIS_X = 0;
        public const int AXIS_Y = 0;
        public const int AXIS_Z = 0;
        public const int LEAF = 0;
        private static int DoubleCounter = 0;

        public int ItemCount;
        public int Level;
        public OOItem Head;
        public OOItem Tail;
        public OOBox Box;
        public OONode Left;
        public OONode Right;
        public OONode Parent;
        public int SplitAxis;
        public byte Visible;
        private float mSplitValue;

        public OONode()
        {
            Left = null;
            Right = null;
            Parent = null;
            Box = new OOBox();
            SplitAxis = LEAF;
            Visible = 0;
            ItemCount = 0;
            Head = new OOItem();
            Tail = new OOItem();
            Head.Prev = null;
            Tail.Next = null;
            Head.Next = Tail;
            Tail.Prev = Head;
        }

        public void AddObject(OOObject obj)
        {
            OOItem item = new OOItem();
            item.Obj = obj;
            item.Link(obj.Head);
            item.Attach(this);
        }

        public void Distribute(int max_level, int max_items)
        {
            if (Level < max_level && (SplitAxis != LEAF || ItemCount > max_items))
            {
                if (SplitAxis == LEAF)
                {
                    Split();
                }
                while (Head.Next != Tail)
                {
                    OOItem i1 = Head.Next;
                    i1.Detach();
                    float mid = i1.Obj.Box.Mid[SplitAxis];
                    float size = i1.Obj.Box.Size[SplitAxis];
                    if (mid + size < mSplitValue)
                    {
                        i1.Attach(Left);
                    }
                    else if (mid - size > mSplitValue)
                    {
                        i1.Attach(Right);
                    }
                    else
                    {
                        OOItem i2 = i1.Split();
                        i1.Attach(Left);
                        i2.Attach(Right);
                    }
                }
            }
            Merge(max_items);
        }

        public void FullDistribute(int max_level, int max_items)
        {
            if (Level < max_level && (SplitAxis != LEAF || ItemCount > max_items))
            {
                if (SplitAxis == LEAF)
                {
                    Split();
                }
                while (Head.Next != Tail)
                {
                    OOItem i1 = Head.Next;
                    i1.Detach();
                    float mid = i1.Obj.Box.Mid[SplitAxis];
                    float size = i1.Obj.Box.Size[SplitAxis];
                    if (mid + size < mSplitValue)
                    {
                        i1.Attach(Left);
                    }
                    else if (mid - size > mSplitValue)
                    {
                        i1.Attach(Right);
                    }
                    else
                    {
                        OOItem i2 = i1.Split();
                        i1.Attach(Left);
                        i2.Attach(Right);
                    }
                }
                Left.FullDistribute(max_level, max_items);
                Right.FullDistribute(max_level, max_items);
            }
        }

        public void DeleteItems()
        {
            while (Head.Next != Tail)
            {
                OOItem i1 = Head.Next;
                i1.Detach();
                i1.Unlink();
            }
        }

        private void Merge(int max_items)
        {
            if (SplitAxis == LEAF)
            {
                return;
            }
            if (Left.SplitAxis != LEAF || Right.SplitAxis != LEAF)
            {
                return;
            }
            if (Left.ItemCount + Right.ItemCount >= max_items)
            {
                return;
            }
            DoubleCounter++;
            OOItem i1;
            while (Left.Head.Next != Left.Tail)
            {
                i1 = Left.Head.Next;
                i1.Detach();
                i1.Attach(this);
                i1.Obj.DoubleId = DoubleCounter;
            }
            while (Right.Head.Next != Right.Tail)
            {
                i1 = Right.Head.Next;
                i1.Detach();
                if (i1.Obj.DoubleId != DoubleCounter)
                {
                    i1.Attach(this);
                }
                else
                {
                    i1.Unlink();
                }
            }
            Left = Right = null;
            SplitAxis = LEAF;
        }

        private void Split()
        {
            Left = new OONode();
            Right = new OONode();
            Right.Level = Left.Level = Level + 1;
            Left.Parent = Right.Parent = this;
            SplitAxis = GetSplitAxis(ref Box.Size);
            mSplitValue = Box.Mid[SplitAxis];
            Left.Box = Right.Box = Box;
            float half = 0.5f * Box.Size[SplitAxis];
            Left.Box.Size[SplitAxis] = Right.Box.Size[SplitAxis] = half;
            Left.Box.Mid[SplitAxis] = Box.Mid[SplitAxis] - half;
            Right.Box.Mid[SplitAxis] = Box.Mid[SplitAxis] + half;
        }

        private int GetSplitAxis(ref Vector3 size)
        {
            if ((size[0] > size[1]) && (size[0] > size[2]))
            {
                return AXIS_X;
            }
            else if (size[1] > size[2])
            {
                return AXIS_Y;
            }
            else
            {
                return AXIS_Z;
            }
        }
    }
}
