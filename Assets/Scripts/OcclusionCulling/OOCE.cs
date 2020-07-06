using UnityEngine;

namespace Nullspace
{
    public class OOCE
    {
        private static int[][] STAB = new int[][]
        {
            new int[]{0,0,0,0,0,0,0},
            new int[]{4,0,2,6,4,0,0},
            new int[]{4,3,1,5,7,0,0},
            new int[]{0,0,0,0,0,0,0},
            new int[]{4,1,0,4,5,0,0},
            new int[]{6,1,0,2,6,4,5},
            new int[]{6,3,1,0,4,5,7},
            new int[]{0,0,0,0,0,0,0},
            new int[]{4,2,3,7,6,0,0},
            new int[]{6,0,2,3,7,6,4},
            new int[]{6,2,3,1,5,7,6},
            new int[]{0,0,0,0,0,0,0},
            new int[]{0,0,0,0,0,0,0},
            new int[]{0,0,0,0,0,0,0},
            new int[]{0,0,0,0,0,0,0},
            new int[]{0,0,0,0,0,0,0},

            new int[]{4,0,1,3,2,0,0},
            new int[]{6,4,0,1,3,2,6},
            new int[]{6,0,1,5,7,3,2},
            new int[]{0,0,0,0,0,0,0},
            new int[]{6,5,1,3,2,0,4},
            new int[]{6,4,5,1,3,2,6},
            new int[]{6,0,4,5,7,3,2},
            new int[]{0,0,0,0,0,0,0},
            new int[]{6,1,3,7,6,2,0},
            new int[]{6,0,1,3,7,6,4},
            new int[]{6,0,1,5,7,6,2},
            new int[]{0,0,0,0,0,0,0},
            new int[]{0,0,0,0,0,0,0},
            new int[]{0,0,0,0,0,0,0},
            new int[]{0,0,0,0,0,0,0},
            new int[]{0,0,0,0,0,0,0},

            new int[]{4,6,7,5,4,0,0},
            new int[]{6,2,6,7,5,4,0},
            new int[]{6,6,7,3,1,5,4},
            new int[]{0,0,0,0,0,0,0},
            new int[]{6,0,4,6,7,5,1},
            new int[]{6,2,6,7,5,1,0},
            new int[]{6,0,4,6,7,3,1},
            new int[]{0,0,0,0,0,0,0},
            new int[]{6,3,7,5,4,6,2},
            new int[]{6,2,3,7,5,4,0},
            new int[]{6,2,3,1,5,4,6},
            new int[]{0,0,0,0,0,0,0},
            new int[]{0,0,0,0,0,0,0},
            new int[]{0,0,0,0,0,0,0},
            new int[]{0,0,0,0,0,0,0},
            new int[]{0,0,0,0,0,0,0}
        };
        public const int OOCE_FRUSTUM_CULLING = 0;
        public const int OOCE_OCCLUSION_CULLING = 1;
        public const int OOCE_OCCLUSION_CULLING_OLD = 2;

        private Camera MainCamera;

        private OOFrustum mFrustum;
        private OOClipper mClip;

        private OOObject mVisible;
        private OOObject mTail;
        private OOObject mTemp;
        private Matrix4x4 mMTR;
        private Matrix4x4 mModelView;
        private Matrix4x4 mProject;
        private Vector3 mPosition;
        private PriorityQueue<float, object, float> mMinQueue;
        private PriorityQueue<float, object, float> mMaxQueue;
        private Vector3 mLook;
        private Vector3 mAbsLook;
        private float mSafeDistance;
        private int mMaxItems;
        private int mMaxLevel;

        public long[] Stat;
        public OOMap Map;
        public OOKDTree Tree;

        public OOCE()
        {
            Map = new OOMap();
            Tree = new OOKDTree();
            mClip = new OOClipper();
            mFrustum = new OOFrustum();
            Stat = new long[2] { 0, 0 };
            mSafeDistance = 1;
            mMaxItems = 8;
            mMaxLevel = 32;
            mMinQueue = new PriorityQueue<float, object, float>();
            mMaxQueue = new PriorityQueue<float, object, float>();
        }

        public void Init(ref Vector3 min, ref Vector3 max)
        {
            Tree.Init(ref min, ref max);
        }

        public void Camera(Camera camera)
        {
            MainCamera = camera;
            UpdateCameraMatrix();
        }

        private void UpdateCameraMatrix()
        {
            mMTR = MainCamera.previousViewProjectionMatrix;
            mModelView = MainCamera.worldToCameraMatrix;
            mProject = MainCamera.projectionMatrix;
            mPosition = MainCamera.transform.position;
            mLook = mModelView.GetColumn(2);
            mAbsLook = mLook.Abs();
            mFrustum.Set(ref mMTR, ref mPosition);
        }

        public void SetResolution(int x, int y)
        {
            Map.SetResolution(x, y);
            mClip.SetResolution(x, y);
        }

        public void Delete()
        {
            DeleteNodes(Tree.Root);
            Tree.Root = new OONode();
            Tree.Root.Level = 0;
        }

        public void Add(OOObject obj)
        {
            Tree.Add(obj);
        }

        public void Remove(OOObject obj)
        {
            Tree.Delete(obj);
        }

        public void FindVisible(int mode)
        {
            mVisible = mTail = null;
            Tree.TouchCounter++;
            switch (mode)
            {
                case OOCE_FRUSTUM_CULLING:
                    FrustumCull();
                    break;
                case OOCE_OCCLUSION_CULLING:
                    OcclusionCull();
                    break;
                case OOCE_OCCLUSION_CULLING_OLD:
                    OcclusionCullOld();
                    break;
            }
        }

        public void InitTree()
        {
            Tree.Root.FullDistribute(mMaxLevel, mMaxItems);
        }

        public void SafeDistance(float dist)
        {
            mSafeDistance = dist;
        }

        public void MaxItems(int n)
        {
            mMaxItems = n;
        }

        public void MaxDepth(int n)
        {
            mMaxLevel = n;
        }

        public int GetObjectID()
        {
            return mTemp.GetObjectId();
        }

        public int GetFirstObject()
        {
            mTemp = mVisible;
            return mTemp != null ? 1 : 0;
        }

        public int GetNextObject()
        {
            mTemp = mTemp.Next;
            return mTemp != null ? 1 : 0;
        }

        public int GetObjectTransform(ref Matrix4x4 m)
        {
            m = mTemp.Transform;
            return 1;
        }

        private void DeleteNodes(OONode nd)
        {
            if (nd == null)
            {
                return;
            }
            nd.DeleteItems();
            DeleteNodes(nd.Left);
            DeleteNodes(nd.Right);
        }

        private void FrustumCull()
        {
            mMinQueue.Clear();
            PushBox(Tree.Root, ref Tree.Root.Box);
            while (mMinQueue.Size > 0)
            {
                OONode nd = (OONode)mMinQueue.Dequeue();
                if (mFrustum.Test(nd.Box) > 0)
                {
                    nd.Distribute(mMaxLevel, mMaxItems);
                    OOItem itm = nd.Head.Next;
                    while (itm != nd.Tail)
                    {
                        OOObject obj = itm.Obj;
                        if (obj.TouchId != Tree.TouchCounter)
                        {
                            obj.TouchId = Tree.TouchCounter;
                            if (mFrustum.Test(obj.Box) > 0)
                            {
                                obj.Next = null;
                                if (mVisible == null)
                                {
                                    mVisible = mTail = obj;
                                }
                                else
                                {
                                    mTail.Next = obj;
                                    mTail = obj;
                                }
                            }
                        }
                        itm = itm.Next;
                    }
                    if (nd.SplitAxis != OONode.LEAF)
                    {
                        PushBox(nd.Left, ref nd.Left.Box);
                        PushBox(nd.Right, ref nd.Right.Box);
                    }
                }
            }
        }
        private void OcclusionCull()
        {
            Stat[0] = Stat[1] = 0;
            Map.Clear();
            mMinQueue.Clear();
            mMaxQueue.Clear();
            PushBox2(Tree.Root, ref Tree.Root.Box);
            while (mMinQueue.Size > 0)
            {
                OONode nd = (OONode)mMinQueue.Dequeue();
                nd.Distribute(mMaxLevel, mMaxItems);
                MinMax(ref nd.Box, ref nd.Box.Zmin, ref nd.Box.Zmax);
                if (nd.SplitAxis != OONode.LEAF) // 非叶节点
                {
                    // 该节点存在两个子节点
                    // 如果 该节点 Visible 为可见 ,则不需要进一步计算
                    // 如果 该结点 Visible 为不可见,则进一步计算判断可见性
                    if ((nd.Visible != 0) || IsVisible(1, ref nd.Box, nd.Box.Zmin) != 0)
                    {
                        // 标记为可见
                        nd.Visible = 1;
                        PushBox2(nd.Left, ref nd.Left.Box);
                        PushBox2(nd.Right, ref nd.Right.Box);
                    }
                    else
                    {
                        // 标记为不可见
                        nd.Visible = 0;
                        if (nd.Parent != null)
                        {
                            nd.Parent.Visible = 0;
                        }
                    }
                }
                else // 叶节点
                {
                    
                    if (IsVisible(1, ref nd.Box, nd.Box.Zmin) != 0)
                    {
                        OOItem itm = nd.Head.Next;
                        while (itm != nd.Tail)
                        {
                            if (itm.Obj.TouchId != Tree.TouchCounter)
						    {
                                itm.Obj.TouchId = Tree.TouchCounter;
                                OOObject obj = itm.Obj;
                                MinMax(ref obj.Box, ref obj.Box.Zmin, ref obj.Box.Zmax);
                                if (IsVisible(0, ref obj.Box, 0) != 0)
                                {
                                    mMaxQueue.Enqueue(obj.Box.Zmax, obj, obj.Box.Zmax);
                                    obj.Next = null;
                                    if (mVisible == null)
                                    {
                                        mVisible = mTail = obj;
                                    }
                                    else
                                    {
                                        mTail.Next = obj;
                                        mTail = obj;
                                    }
                                }
                            }
                            itm = itm.Next;
                        }
                    }
                    if (nd.Parent != null)
                    {
                        nd.Parent.Visible = 0;
                    }
                }
            }
        }

        private void OcclusionCullOld()
        {
            Stat[0] = Stat[1] = 0;
            Map.Clear();
            mMinQueue.Clear();
            mMaxQueue.Clear();
            PushBox(Tree.Root, ref Tree.Root.Box);
            while (mMinQueue.Size > 0)
            {
                OONode nd = (OONode)mMinQueue.Dequeue();
                nd.Distribute(mMaxLevel, mMaxItems);
                MinMax(ref nd.Box, ref nd.Box.Zmin, ref nd.Box.Zmax);
                if (nd.SplitAxis != OONode.LEAF)
                {
                    if (nd.Visible != 0 || IsVisible(1, ref nd.Box, nd.Box.Zmin) != 0)
                    {
                        nd.Visible = 1;
                        PushBox(nd.Left, ref nd.Left.Box);
                        PushBox(nd.Right, ref nd.Right.Box);
                    }
                    else
                    {
                        nd.Visible = 0;
                        if (nd.Parent != null)
                        {
                            nd.Parent.Visible = 0;
                        }
                    }
                }
                else
                {
                    if (IsVisible(1, ref nd.Box, nd.Box.Zmin) != 0)
                    {
                        OOItem itm = nd.Head.Next;
                        while (itm != nd.Tail)
                        {
                            if (itm.Obj.TouchId != Tree.TouchCounter) {
                                itm.Obj.TouchId = Tree.TouchCounter;
                                OOObject obj = itm.Obj;
                                float dis = -Vector3.Dot(obj.Box.Mid, mLook);
                                float d = Vector3.Dot(mAbsLook, obj.Box.Size);
                                obj.Box.Zmin = dis - d;
                                obj.Box.Zmax = dis + d;

                                if (IsVisible(0, ref obj.Box, 0) != 0)
                                {
                                    mMaxQueue.Enqueue(obj.Box.Zmax, obj, obj.Box.Zmax);
                                    obj.Next = null;
                                    if (mVisible == null)
                                    {
                                        mVisible = mTail = obj;
                                    }
                                    else
                                    {
                                        mTail.Next = obj;
                                        mTail = obj;
                                    }
                                }
                            }
                            itm = itm.Next;
                        }
                    }
                    if (nd.Parent != null)
                    {
                        nd.Parent.Visible = 0;
                    }
                }
            }
        }

        private void FlushOccluders(float distance)
        {
            while (mMaxQueue.Size > 0 && ((OOObject)mMaxQueue.Peek()).Box.Zmax <= distance)
            {
                OOObject obj = (OOObject)mMaxQueue.Dequeue();
                if (obj.CanOcclude == 0)
                {
                    continue;
                }
                DrawOccluder(obj);
            }
        }

        private int IsVisible(int flush, ref OOBox b, float dist)
        {
            OOBox q = b;
            q.Size = b.Size + Vector3.one * mSafeDistance;
            int visible = mFrustum.Test(q);
            if (visible == 0)
            {
                return 0;
            }
            if (visible == 2)
            {
                return 1;
            }
            if (flush != 0)
            {
                FlushOccluders(dist);
            }
            return QueryBox(ref b);
        }


        private void MinMax(ref OOBox b, ref float min, ref float max)
        {
            min = 0;
            max = float.MinValue;
            Vector3 diff = (mPosition - b.Mid).Abs();

            for (int i = 0; i < 3; i++)
            {
                float d1 = diff[i] - b.Size[i];
                if (d1 > min)
                {
                    min = d1;
                }
                float d2 = diff[i] + b.Size[i];
                if (d2 > max)
                {
                    max = d2;
                }
            }
            if (min < 0)
            {
                min = 0;
            }
        }

        private void PushBox(object obj, ref OOBox b)
        {
            float dis, d;
            dis = -Vector3.Dot(b.Mid, mLook);
            d = Vector3.Dot(mAbsLook, b.Size);
            b.Zmin = dis - d;
            b.Zmax = dis + d;
            mMinQueue.Enqueue(b.Zmin, obj, b.Zmin);
        }

        private void PushBox2(object obj, ref OOBox b)
        {
            MinMax(ref b, ref b.Zmin, ref b.Zmax);
            mMinQueue.Enqueue(b.Zmin, obj, b.Zmin);
            return;
        }

        private int QueryBox(ref OOBox x)
        {
            Vector3[] vxt = new Vector3[8];
            Vector3 min = x.Mid - x.Size;
            Vector3 max = x.Mid + x.Size;
            int cd = 0;
            if (mPosition[0] < min[0])
            {
                cd |= 1;
            }
            if (mPosition[0] > max[0])
            {
                cd |= 2;
            }
            if (mPosition[1] < min[1])
            {
                cd |= 4;
            }
            if (mPosition[1] > max[1])
            {
                cd |= 8;
            }
            if (mPosition[2] < min[2])
            {
                cd |= 16;
            }
            if (mPosition[2] > max[2])
            {
                cd |= 32;
            }
            vxt[0][0] = vxt[2][0] = vxt[4][0] = vxt[6][0] = min[0];
            vxt[1][0] = vxt[3][0] = vxt[5][0] = vxt[7][0] = max[0];
            vxt[0][1] = vxt[1][1] = vxt[4][1] = vxt[5][1] = min[1];
            vxt[2][1] = vxt[3][1] = vxt[6][1] = vxt[7][1] = max[1];
            vxt[0][2] = vxt[1][2] = vxt[2][2] = vxt[3][2] = min[2];
            vxt[4][2] = vxt[5][2] = vxt[6][2] = vxt[7][2] = max[2];
            int[] stt = STAB[cd];
            int vp = stt[0];
            for (int i = 0; i < vp; i++)
            {
                int j = stt[i + 1];
                mClip.Vi[i] = mMTR * vxt[j];
            }
            vp = mClip.ClipAndProject(vp);
            if (vp < 3)
            {
                return 0;
            }
            int res = Map.QueryOPolygon(mClip.Vs, vp);
            return res;
        }

        private void DrawOccluder(OOObject obj)
        {
            
            int j, nv;
            OOModel mdl = obj.Model;
            Matrix4x4 mcb = mModelView * obj.Transform;
            for (int i = 0; i < mdl.NumVert; i++)
            {
                mdl.CVertices[i] = mcb * mdl.Vertices[i];
                mdl.TVertices[i] = mProject * mdl.CVertices[i];
            }
            int xmin = 100000;
            int xmax = 0;
            int ymin = 100000;
            int ymax = 0;
            for (int i = 0; i < mdl.NumFace; i++)
            {
                int p1 = mdl.Faces[i][0];
                int p2 = mdl.Faces[i][1];
                int p3 = mdl.Faces[i][2];
                Vector3 a = mdl.CVertices[p2] - mdl.CVertices[p1];
                Vector3 b = mdl.CVertices[p3] - mdl.CVertices[p1];
                Vector3 n = Vector3.Cross(a, b);
                if (Vector3.Dot(n, mdl.CVertices[p1]) < 0)
                {
                    mClip.Vi[0] = mdl.TVertices[p1];
                    mClip.Vi[1] = mdl.TVertices[p2];
                    mClip.Vi[2] = mdl.TVertices[p3];
                    nv = mClip.ClipAndProject(3);
                    if (nv > 2)
                    {
                        for (j = 0; j < nv; j++)
                        {
                            if (mClip.Vs[j][0] < xmin)
                            {
                                xmin = mClip.Vs[j][0];
                            }
                            else
                            {
                                if (mClip.Vs[j][0] > xmax) xmax = mClip.Vs[j][0];
                            }
                            if (mClip.Vs[j][1] < ymin)
                            {
                                ymin = mClip.Vs[j][1];
                            }
                            else if (mClip.Vs[j][1] > ymax)
                            {
                                ymax = mClip.Vs[j][1];
                            }
                        }
                        Map.DrawOPolygon(mClip.Vs, nv);
                    }
                }
            }
            Map.SetDirtyRectangle(xmin, ymin, xmax, ymax);
        }
    }
}
