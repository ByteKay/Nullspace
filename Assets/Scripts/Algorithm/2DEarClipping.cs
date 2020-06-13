
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class EarPoint
    {
        public int mIndex;
        public Vector2 mPoint;
		public EarPoint(float pX, float pY) 
        {
            mIndex = -1;
            mPoint = new Vector2(pX, pY);
        }
        public EarPoint(int index, float pX, float pY)
        {
            mIndex = index;
            mPoint = new Vector2(pX, pY);
        }
		public EarPoint()
        {
            mIndex = -1;
            mPoint = new Vector3(0.0f, 0.0f, 0.0f);
        }
        public float this[int idx]
        {
            get
            {
                return mPoint[idx];
            }
        }

        public override bool Equals(object o)
        {
            return this == (EarPoint)o;
        }

        public override int GetHashCode()
        {
            return string.Format("{0}_{1}", mPoint[0], mPoint[1]).GetHashCode();
        }

		public float Cross(EarPoint rhs)
        { 
            return (mPoint[0] * rhs.mPoint[1]) - (mPoint[1] * rhs.mPoint[0]);
        }
		public float Dot(EarPoint rhs)
        { 
            return Vector2.Dot(mPoint, rhs.mPoint); 
        }
        public float SqrDot(EarPoint rhs)
        { 
            return mPoint[0] * rhs.mPoint[0] + mPoint[1] * rhs.mPoint[1]; 
        }
        public static bool operator == (EarPoint e1, EarPoint e2)
        {
            return e1.mIndex == e2.mIndex;
        }
        public static bool operator !=(EarPoint e1, EarPoint e2)
        {
            return e1.mIndex != e2.mIndex;
        }
    }

    public class EarPolygon
    {
        private LinkedList<EarPoint> mHead;
        private int mNumberOfPoints;
        private List<EarPolygon> mChildren;
        private float mArea; // 带符号
        public List<List<Vector2>> mResults;
        public EarPolygon()
        {
            mChildren = new List<EarPolygon>();
            mResults = new List<List<Vector2>>();
            mHead = new LinkedList<EarPoint>();
            mNumberOfPoints = 0;
        }
        public EarPolygon(EarPolygon parent)
        {
            mChildren = new List<EarPolygon>();
            mResults = new List<List<Vector2>>();
            mHead = new LinkedList<EarPoint>();
            parent.AddChild(this);
            mNumberOfPoints = 0;
        }

        public LinkedListNode<EarPoint> Get() { return mHead.First; }
        public int NumPoints() { return mHead.Count; }
        public int NumChildren() { return mChildren.Count; }
        public void AddChild(EarPolygon child) { mChildren.Add(child); }
        public List<EarPolygon> GetChildren() { return mChildren; }

        public bool AddPoint(float x, float y)
	    {
            if (mNumberOfPoints == 0)
            {
                mHead.AddFirst(new EarPoint(mNumberOfPoints++, x, y));
                return true;
            }
            mHead.AddLast(new EarPoint(mNumberOfPoints++, x, y));
            return true;
	    }

        public void Reverse(int pos)
        {
            if (pos < 0)
            {
                LinkedList<EarPoint> head = new LinkedList<EarPoint>();
                LinkedListNode<EarPoint> last = mHead.Last;
                do
                {
                    head.AddLast(last.Value);
                    last = last.Previous;
                } while (last != null);
                mHead = head;
            }
            else
            {
                mChildren[pos].Reverse(-1);
            }
        }

        public EarPolygon this[int idx]
        {
            get
            {
                return mChildren[idx];
            }
        }
        public LinkedListNode<EarPoint> InsertPoint(float x, float y, LinkedListNode<EarPoint> cur)
	    {
            EarPoint newPoint = new EarPoint(mNumberOfPoints++, x, y);
            return mHead.AddAfter(cur, newPoint);
	    }
	    public void AddResult(float x, float y, float x1, float y1, float x2, float y2)
	    {
		    mResults.Add(new List<Vector2>());
		    mResults[mResults.Count - 1].Add(new Vector2(x, y));
		    mResults[mResults.Count - 1].Add(new Vector2(x1, y1));
		    mResults[mResults.Count - 1].Add(new Vector2(x2, y2));
	    }
        public void CalculateArea()
        {
            mArea = 0.0f;
            LinkedListNode<EarPoint> active = Get();
            LinkedListNode<EarPoint> next;
            for (int i = 0; i < NumPoints(); ++i)
            {
                next = Next(active);
                mArea += (active.Value[0] * next.Value[1] - next.Value[0] * active.Value[1]);
                active = next;
            }
            mArea *= 0.5f;
        }
        public LinkedListNode<EarPoint> Previous(LinkedListNode<EarPoint> cur)
        {
            if (cur.Previous == null)
            {
                return mHead.Last;
            }
            return cur.Previous;
        }
        public LinkedListNode<EarPoint> Next(LinkedListNode<EarPoint> cur)
        {
            if (cur.Next == null)
            {
                return mHead.First;
            }
            return cur.Next;
        }

        public bool IsCCW()
        {
            return mArea > 0;
        }
        public bool Remove(LinkedListNode<EarPoint> point)
	    {
            mHead.Remove(point);
		    return true;
	    }
    }

    public class EarClipping
    {
        public static void Clip(EarPolygon poly)
        {
            Merge(poly);
            RecordEars(poly);
        }

        public static List<Vector4> Merge(EarPolygon poly)
        {
            OrientatePolygon(poly);
            return MergePolygon(poly);
        }

        private static void OrientatePolygon(EarPolygon poly)
        {
            poly.CalculateArea();
            if (!poly.IsCCW())
            {
                poly.Reverse(-1);
            }
            for (int i = 0; i < poly.NumChildren(); i++)
            {
                poly[i].CalculateArea();
                if (poly[i].IsCCW())
                {
                    poly[i].Reverse(-1);
                }
            }
        }
        private static List<Vector4> MergePolygon(EarPolygon poly)
        {
            List<Vector4> connects = new List<Vector4>();
            if (poly.NumChildren() > 0)
            {
                List<EarPolygon> children = poly.GetChildren();
                List<KeyValuePair<int, float>> order = ChildOrder(children);
                KeyValuePair<LinkedListNode<EarPoint>, LinkedListNode<EarPoint>> connection;
                LinkedListNode<EarPoint> temp;
                for (int i = 0; i < order.Count; i++)
                {
                    connection = GetSplit(poly, children[order[i].Key], order[i].Value);
                    connects.Add(new Vector4(connection.Key.Value[0], connection.Key.Value[1], connection.Value.Value[0], connection.Value.Value[1]));
                    LinkedListNode<EarPoint> newP = poly.InsertPoint(connection.Key.Value[0], connection.Key.Value[1], connection.Value);
                    temp = connection.Key;
                    do
                    {
                        temp = children[order[i].Key].Next(temp);
                        newP = poly.InsertPoint(temp.Value[0], temp.Value[1], newP);
                    } while (temp.Value != connection.Key.Value);
                    newP = poly.InsertPoint(connection.Value.Value[0], connection.Value.Value[1], newP);
                }
            }
            return connects;
        }

        private static bool RecordEars(EarPolygon poly)
	    {
            LinkedListNode<EarPoint> active = poly.Get();
		    while (poly.NumPoints() >= 3)
		    {
                int idx = active.Value.mIndex;
                do
			    {
                    if (IsConvex(active, poly))
				    {
                        if (IsEar(active, poly))
					    {
                            break;
					    }
				    }
                    active = poly.Next(active);
                } while (idx != active.Value.mIndex);

                poly.AddResult(poly.Previous(active).Value[0], poly.Previous(active).Value[1], active.Value[0], active.Value[1], poly.Next(active).Value[0], poly.Next(active).Value[1]);
                active = poly.Next(active);
                poly.Remove(poly.Previous(active));
			    continue;
		    }
		    return true;
	    }

        private static bool IsEar(LinkedListNode<EarPoint> ele, EarPolygon poly)
        {
            LinkedListNode<EarPoint> checkerN1 = poly.Next(ele);
            LinkedListNode<EarPoint> checker = poly.Next(checkerN1);
            while (checker.Value.mIndex != poly.Previous(ele).Value.mIndex)
            {
                if (InTriangle(checker.Value, ele.Value, poly.Next(ele).Value, poly.Previous(ele).Value))
                {
                    return false;
                }
                checker = poly.Next(checker);
            }
            return true;
        }

        private static bool InTriangle(EarPoint pointToCheck, EarPoint earTip, EarPoint earTipPlusOne, EarPoint earTipMinusOne)
	    {
            bool isIntriangle = GeoTriangleUtils.IsPointInTriangle2(earTip.mPoint, earTipPlusOne.mPoint, earTipMinusOne.mPoint, ref pointToCheck.mPoint);
            if (isIntriangle)
            {
                if (pointToCheck.mPoint == earTip.mPoint || pointToCheck.mPoint == earTipPlusOne.mPoint || pointToCheck.mPoint == earTipMinusOne.mPoint) // 端点
                {
                    return false;
                }
            }
            return isIntriangle;
	    }

        private static bool IsConvex(LinkedListNode<EarPoint> ele, EarPolygon poly)
        {
            LinkedListNode<EarPoint> a = poly.Previous(ele);
            LinkedListNode<EarPoint> b = ele;
            LinkedListNode<EarPoint> c = poly.Next(ele);
            return GeoPolygonUtils.IsConvex(a.Value.mPoint, b.Value.mPoint, c.Value.mPoint);
        }
        private static KeyValuePair<LinkedListNode<EarPoint>, LinkedListNode<EarPoint>> GetSplit(EarPolygon outer, EarPolygon inner, float smallestX)
	    {
            LinkedListNode<EarPoint> smallest = inner.Get();
		    do
		    {
                if (smallest.Value[0] == smallestX)
                {
                    break;
                }
                smallest = smallest.Next;
            } while (smallest != null);

            LinkedListNode<EarPoint> closest = GetClosest(OrderPoints(outer, smallest), 0, outer, smallest, inner);
            KeyValuePair<LinkedListNode<EarPoint>, LinkedListNode<EarPoint>> split = new KeyValuePair<LinkedListNode<EarPoint>, LinkedListNode<EarPoint>>(smallest, closest);
		    return split;
	    }
        private static List<LinkedListNode<EarPoint>> OrderPoints(EarPolygon poly, LinkedListNode<EarPoint> point)
	    {
            LinkedListNode<EarPoint> head = poly.Get();
            List<LinkedListNode<EarPoint>> pointContainer = new List<LinkedListNode<EarPoint>>();
		    activePoint = point;
            while (head != null)
		    {
			    pointContainer.Add(head);
			    head = head.Next;
		    }
            pointContainer.Sort((i, j) =>
            {		
                Vector2 t1 = i.Value.mPoint - activePoint.Value.mPoint;
                Vector2 t2 = j.Value.mPoint - activePoint.Value.mPoint;
                return t1.sqrMagnitude.CompareTo(t2.sqrMagnitude);
            });
		    return pointContainer;
	    }
        private static List< KeyValuePair< int, float > > ChildOrder(List<EarPolygon> children)
	    {
            List<KeyValuePair<int, float>> toSort = new List<KeyValuePair<int, float>>();
            LinkedListNode<EarPoint> head;
            int size = children.Count;
		    for (int i = 0; i < size; i++)
		    {
			    head = children[i].Get();
                float smallestX = head.Value[0];
                do
                {
                    smallestX = head.Next.Value[0] > smallestX ? smallestX : head.Next.Value[0];
                    head = head.Next;
                } while (head.Next != null);
                toSort.Add(new KeyValuePair<int, float>(i, smallestX));
		    }
            toSort.Sort((x, y) => x.Value.CompareTo(y.Value));
		    return toSort;
	    }
        private static LinkedListNode<EarPoint> GetClosest(List<LinkedListNode<EarPoint>> pointsOrdered, int index, EarPolygon poly, LinkedListNode<EarPoint> innerPoint, EarPolygon polychild)
	    {
            LinkedListNode<EarPoint> a = innerPoint;
            LinkedListNode<EarPoint> b = pointsOrdered[index];
            LinkedListNode<EarPoint> c = poly.Get();
		    bool intersection = false;
		    do
		    {
                intersection = DoIntersect(a.Value, b.Value, c.Value, poly.Next(c).Value);
                c = c.Next;
            } while ((!intersection) && (c != null));
		    if (!intersection)
		    {
			    c = a;
			    do
			    {
                    intersection = DoIntersect(a.Value, b.Value, c.Value, polychild.Next(c).Value);
                    c = polychild.Next(c);
                } while ((!intersection) && (c.Value != a.Value));
                if (!intersection)
                {
                    return b;
                }
		    }
            return GetClosest(pointsOrdered, index + 1, poly, innerPoint, polychild);
	    }
        private static bool DoIntersect(EarPoint a, EarPoint b, EarPoint c, EarPoint d)
	    {
            GeoInsectPointInfo insect = new GeoInsectPointInfo();
            bool isInsect = GeoSegmentUtils.IsSegmentInsectSegment2(a.mPoint, b.mPoint, c.mPoint, d.mPoint, ref insect);
            if (isInsect)
            {
                float x = insect.mHitGlobalPoint[0];
                float y = insect.mHitGlobalPoint[1];
                if ((x == c[0] && y == c[1]) || (x == d[0] && y == d[1])) // 端点检测
                {
                    return false;
                }
            }
            return isInsect;
	    }
        private static LinkedListNode<EarPoint> activePoint; // 多线程 会出问题
    }
}
