
using System.Collections.Generic;

using UnityEngine;

namespace Nullspace
{
    public class RectPartionNode
    {
        private Vector2 mStart;
        private Vector2 mSize;
        private bool mIsChildren;
        private RectPartionNode mLeftChild;
        private RectPartionNode mRightChild;

        public int mIndex;
        public float this[int idx]
        {
            get
            {
                return mStart[idx];
            }
        }

        public RectPartionNode()
        {
            mStart = new Vector2();
            mSize = new Vector2();
            mLeftChild = null;
            mRightChild = null;
            mIsChildren = false;
            mIndex = 0;
        }
        public RectPartionNode(float x, float y, float w, float h)
        {
            mStart = new Vector2(x, y);
            mSize = new Vector2(w, h);
            mLeftChild = null;
            mRightChild = null;
            mIsChildren = false;
            mIndex = 0;
        }

        public RectPartionNode Insert(float w, float h, float intervalW = 0.0f, float intervalH = 0.0f)
	    {
            RectPartionNode newNode = null;
		    float dw, dh;
            if (!mIsChildren && 0 == mIndex)
		    {
			    dw = mSize[0] - w;
			    dh = mSize[1] - h;
                if (dw >= 0 && dh >= 0)
                {
                    if (dw < 0.01f && dh < 0.01f)
                    {
                        return this;
                    }
                    else
                    {
                        float t = dh - dw;
                        if (t < 0)
                        {
                            mLeftChild = new RectPartionNode(mStart[0], mStart[1], w, mSize[1]);
                            mRightChild = new RectPartionNode(mStart[0] + w + intervalW, mStart[1], dw, mSize[1] - intervalW);
                            mIsChildren = true;
                        }
                        else
                        {
                            mLeftChild = new RectPartionNode(mStart[0], mStart[1], mSize[0], h);
                            mRightChild = new RectPartionNode(mStart[0], mStart[1] + h + intervalH, mSize[0], dh - intervalH);
                            mIsChildren = true;
                        }
                        return mLeftChild.Insert(w, h);
                    }
                }
                else
                {
                    return null;
                }
		    }
            else if (mIsChildren)
            {
                newNode = mLeftChild.Insert(w, h);
                if (newNode == null)
                {
                    newNode = mRightChild.Insert(w, h);
                }
            }
            else
            {
                return null;
            }
		    return newNode;
	    }
    }

    public class RectPartion
    {
        private List<RectPartionNode> mRectList;
        public RectPartion()
        {
            mRectList = new List<RectPartionNode>();
        }
        public void AddRect(float x, float y, float w, float h)
        {
            mRectList.Add(new RectPartionNode(x, y, w, h));
        }
        public RectPartionNode this[int index]
        {
            get
            {
                return mRectList[index];
            }
            set
            {
                mRectList[index] = value;
            }
        }

        public bool GetNextImage(Vector2 size, int texIndex, out float lmX, out float lmY, out int mLInd)
        {
            lmX = lmY = mLInd = 0;
            int lIndex = 0;
            RectPartionNode node = null;
            foreach (RectPartionNode rect in mRectList)
            {
                if ((node = rect.Insert(size[0], size[1])) != null)
                {
                    break;
                }
                lIndex++;
            }
            if (node != null)
            {
                lmX = node[0];
                lmY = node[1];
                mLInd = lIndex;
                node.mIndex = texIndex;
                return true;
            }
            else
                return false;
        }

    };
}
