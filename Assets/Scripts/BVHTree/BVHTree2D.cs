
using System.Collections.Generic;

namespace Nullspace
{
    public class BVHTree2D
    {
        static BVHBuildEntry[] PREALLOC;
        static BVHTree2D()
        {
            PREALLOC = new BVHBuildEntry[128];
            for (int i = 0; i < 128; ++i)
            {
                PREALLOC[i] = new BVHBuildEntry();
            }
        }

        private int mNumNodes, mNumLeafs, mNodeMaxLeafSize;
        private List<BVHObject2> mBuildPrims;
        private List<BVHFlatNode2> mFlatTreeList = null;
        public BVHTree2D(List<BVHObject2> objects, int _leafSize = 4)
        {
            mBuildPrims = objects;
            mNodeMaxLeafSize = _leafSize;
            mNumNodes = mNumLeafs = 0;
            Build();
        }
        public BVHTree2D(int _leafSize = 4)
        {
            mBuildPrims = new List<BVHObject2>();
            mNodeMaxLeafSize = _leafSize;
            mNumNodes = mNumLeafs = 0;
        }
        public bool TestIntersection(GeoAABB2 aabb)
        {
            if (mFlatTreeList == null || mFlatTreeList.Count == 0)
            {
                return false;
            }
            int closer, other;
            BVHTraversal[] todo = new BVHTraversal[64];
            todo[0] = new BVHTraversal();
            int stackptr = 0;
            todo[stackptr].mIndex = 0;
            todo[stackptr].mLength = -9999999.0f;
            while (stackptr >= 0)
            {
                int ni = todo[stackptr].mIndex;
                float near = todo[stackptr].mLength;
                stackptr--;
                BVHFlatNode2 node = mFlatTreeList[ni];
                // 对叶节点做相交测试
                if (node.mRightOffset == 0)
                {
                    bool hit = false;
                    for (int o = 0; o < node.mLeafCount; ++o)
                    {
                        BVHObject2 obj = mBuildPrims[(int)node.mStartIndex + o];
                        hit = obj.TestAABBIntersect(aabb);
                        if (hit)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    closer = ni + 1;
                    other = ni + (int)node.mRightOffset;
                    // 对父结点做测试
                    bool hitc0 = GeoAABBUtils.IsAABBInsectAABB2(aabb.mMin, aabb.mMax, mFlatTreeList[closer].mBox.mMin, mFlatTreeList[closer].mBox.mMax);
                    bool hitc1 = GeoAABBUtils.IsAABBInsectAABB2(aabb.mMin, aabb.mMax, mFlatTreeList[other].mBox.mMin, mFlatTreeList[other].mBox.mMax);
                    if (hitc0 && hitc1)
                    {
                        todo[++stackptr] = new BVHTraversal(other, -9999);
                        todo[++stackptr] = new BVHTraversal(closer, -9999);
                    }
                    else if (hitc0)
                    {
                        todo[++stackptr] = new BVHTraversal(closer, -9999);
                    }
                    else if (hitc1)
                    {
                        todo[++stackptr] = new BVHTraversal(other, -9999);
                    }
                }
            }
            return false;
        }

        public bool GetIntersection(GeoRay2 ray, ref GeoInsectPointArrayInfo intersection, bool occlusion)
        {
            if (mFlatTreeList.Count == 0)
            {
                return false;
            }
            intersection.mIsIntersect = false;
            intersection.mLength = 999999999.0f;
            intersection.mHitObject2 = null;
            int closer, other;
            BVHTraversal[] todo = new BVHTraversal[64];
            todo[0] = new BVHTraversal();
            int stackptr = 0;
            todo[stackptr].mIndex = 0;
            todo[stackptr].mLength = -9999999.0f;
            while (stackptr >= 0)
            {
                int ni = todo[stackptr].mIndex;
                float near = todo[stackptr].mLength;
                stackptr--;
                BVHFlatNode2 node = mFlatTreeList[ni];
                if (near > intersection.mLength)
                    continue;
                // 对叶节点做相交测试
                if (node.mRightOffset == 0)
                {
                    bool hit = false;
                    for (int o = 0; o < node.mLeafCount; ++o)
                    {
                        GeoInsectPointArrayInfo current = new GeoInsectPointArrayInfo();
                        BVHObject2 obj = mBuildPrims[(int)node.mStartIndex + o];
                        hit = obj.IsIntersect(ref ray, ref current);
                        if (hit)
                        {
                            if (occlusion)
                            {
                                intersection = current;
                                return true;
                            }
                            if (current.mLength < intersection.mLength)
                            {
                                intersection = current;
                            }
                        }
                    }
                }
                else
                {
                    closer = ni + 1;
                    other = ni + (int)node.mRightOffset;
                    // 对父结点做测试
                    GeoInsectPointArrayInfo in1 = new GeoInsectPointArrayInfo();
                    GeoInsectPointArrayInfo in2 = new GeoInsectPointArrayInfo();
                    bool hitc0 = GeoRayUtils.IsRayInsectAABB2(ray.mOrigin, ray.mDirection, mFlatTreeList[closer].mBox.mMin, mFlatTreeList[closer].mBox.mMax, ref in1);
                    bool hitc1 = GeoRayUtils.IsRayInsectAABB2(ray.mOrigin, ray.mDirection, mFlatTreeList[other].mBox.mMin, mFlatTreeList[other].mBox.mMax, ref in2);

                    if (hitc0 && hitc1)
                    {
                        float l0 = (GeoUtils.ToVector2(in1.mHitGlobalPoint[0]) - ray.mOrigin).magnitude;
                        float l2 = (GeoUtils.ToVector2(in2.mHitGlobalPoint[0]) - ray.mOrigin).magnitude;
                        if (l2 < l0)
                        {
                            float temp = l0;
                            l0 = l2;
                            l2 = temp;
                            int itemp = closer;
                            closer = other;
                            other = itemp;
                        }
                        todo[++stackptr] = new BVHTraversal(other, l2);
                        todo[++stackptr] = new BVHTraversal(closer, l0);
                    }
                    else if (hitc0)
                    {
                        float l0 = (GeoUtils.ToVector2(in1.mHitGlobalPoint[0]) - ray.mOrigin).magnitude;
                        todo[++stackptr] = new BVHTraversal(closer, l0);
                    }
                    else if (hitc1)
                    {
                        float l2 = (GeoUtils.ToVector2(in2.mHitGlobalPoint[0]) - ray.mOrigin).magnitude;
                        todo[++stackptr] = new BVHTraversal(other, l2);
                    }
                }
            }
            if (intersection.mHitObject2 != null)
            {
                intersection.mHitGlobalPoint.Clear();
                intersection.mHitGlobalPoint.Add(ray.mOrigin + ray.mDirection * intersection.mLength);
            }
            return intersection.mHitObject2 != null;
        }

        // this is not property.but just support dynamic add operator
        public void AddObject(BVHObject2 obj, bool imme = false)
        {
            mBuildPrims.Add(obj);
            if (imme)
            {
                Build();
            }
        }
        // this is not property.but just support dynamic delete operator
        public void DeleteObject(BVHObject2 obj, bool imme = false)
        {
            bool success = mBuildPrims.Remove(obj);
            if (success && imme)
            {
                Build();
            }
        }

        private void Build()
        {
            mNumNodes = mNumLeafs = 0;
            int stackptr = 0;
            uint Untouched = 0xffffffff;
            uint TouchedTwice = 0xfffffffd;
            PREALLOC[stackptr].mStart = 0;
            PREALLOC[stackptr].mEnd = (uint)mBuildPrims.Count;
            PREALLOC[stackptr].mParent = 0xfffffffc;
            stackptr++;
            List<BVHFlatNode2> buildnodes = new List<BVHFlatNode2>(mBuildPrims.Count * 2);
            while (stackptr > 0)
            {
                BVHBuildEntry bnode = PREALLOC[--stackptr];
                uint start = bnode.mStart;
                uint end = bnode.mEnd;
                uint nPrims = end - start;
                mNumNodes++;
                BVHFlatNode2 node = new BVHFlatNode2();
                node.mStartIndex = start;
                node.mLeafCount = nPrims;
                node.mRightOffset = Untouched;
                BVHAABB2 bb = new BVHAABB2(mBuildPrims[(int)start].GetAABB().mMin, mBuildPrims[(int)start].GetAABB().mMax);
                BVHAABB2 bc = new BVHAABB2(mBuildPrims[(int)start].GetCenter(), mBuildPrims[(int)start].GetCenter());
                for (uint p = start + 1; p < end; ++p)
                {
                    bb.ExpandToInclude(mBuildPrims[(int)p].GetAABB());
                    bc.ExpandToInclude(mBuildPrims[(int)p].GetCenter());
                }
                node.mBox = bb;
                if (nPrims <= mNodeMaxLeafSize)
                {
                    node.mRightOffset = 0;
                    mNumLeafs++;
                }

                buildnodes.Add(node);
                // 记录父节点关于右孩子结点相对父结点的偏移值mRightOffset
                // 第一次为左孩子，相对父结点的偏移值为1
                // 每个父节点最多被两次 hit
                if (bnode.mParent != 0xfffffffc)
                {
                    buildnodes[(int)bnode.mParent].mRightOffset--;
                    if (buildnodes[(int)bnode.mParent].mRightOffset == TouchedTwice)
                    {
                        buildnodes[(int)bnode.mParent].mRightOffset = (uint)mNumNodes - 1 - bnode.mParent;
                    }
                }
                if (node.mRightOffset == 0)
                    continue;
                // 选择合适的分割维度
                uint split_dim = (uint)bc.MaxDimension();
                float split_coord = 0.5f * (bc.mMin[(int)split_dim] + bc.mMax[(int)split_dim]);
                uint mid = start;
                // 交换 start 和 end 之间 的数据
                for (uint i = start; i < end; ++i)
                {
                    if (mBuildPrims[(int)i].GetCenter()[(int)split_dim] < split_coord)
                    {
                        BVHObject2 temp = mBuildPrims[(int)i];
                        mBuildPrims[(int)i] = mBuildPrims[(int)mid];
                        mBuildPrims[(int)mid] = temp;
                        ++mid;
                    }
                }
                if (mid == start || mid == end)
                {
                    mid = start + (end - start) / 2;
                }
                // 右孩子
                PREALLOC[stackptr].mStart = mid;
                PREALLOC[stackptr].mEnd = end;
                PREALLOC[stackptr].mParent = (uint)mNumNodes - 1;
                stackptr++;
                // 左孩子
                PREALLOC[stackptr].mStart = start;
                PREALLOC[stackptr].mEnd = mid;
                PREALLOC[stackptr].mParent = (uint)mNumNodes - 1;
                stackptr++;
            }
            if (mFlatTreeList != null)
                mFlatTreeList.Clear();
            mFlatTreeList = new List<BVHFlatNode2>(mNumNodes);
            for (uint n = 0; n < mNumNodes; ++n)
            {
                mFlatTreeList.Add(buildnodes[(int)n]);
            }
        }
    }
}
