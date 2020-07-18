using System;

using System.Text;

namespace NullMesh
{
    public enum NullWorkingFlag
    {
        WF_AUTO_DETECT = 0,
        WF_STATIC_MESH = 1,
        WF_SKELETON_MESHPIECE = 2,
        WF_NODE_ANIM = 3,
    }

    public class NullMeshFile : INullStream
    {
        public const int MESH_FILE_VERSION = 100;
        private static uint StaticMesh = MakeFourCC("HXBO");
        private static uint SkeletonMesh = MakeFourCC("HXBS");
        private static uint SkeletonAnimation = MakeFourCC("HXBA");
        public static uint MakeFourCC(string four)
        {
            return BitConverter.ToUInt32(Encoding.UTF8.GetBytes(four), 0);
        }

        public int CurrentVersion;
        protected NullWorkingFlag mWorkingMode;
        protected int mBlockSize;
        //base mesh
        protected NullMeshObjects mMeshObjectList;
        protected NullMeshObjects mSkinObjectList;
        protected NullSocketNodes mSocketNodeList;
        protected NullNodeDummy mNodeDummy;
        //skeleton animation
        protected NullNodeTree mNodeTree;
        protected NullSkeletonBinding mSkeletonBinding;
        protected NullSkeletonAnimations mSkeletonAnimations;
        //ertex morph animation
        protected NullVertexMorphAnimations mVertexMorphAnimations;

        public int SaveToStream(NullMemoryStream stream)
        {
            uint foucc = GenerateFouCC();
            if (foucc == 0)
            {
                return 0;
            }
            CurrentVersion = MESH_FILE_VERSION;
            stream.WriteUInt(foucc);
            stream.WriteInt(mBlockSize);
            stream.WriteInt(CurrentVersion);
            int size = 0;
            switch (mWorkingMode)
            {
                case NullWorkingFlag.WF_STATIC_MESH:
                    size = SaveToStreamForStaticMesh(stream);
                    break;
                case NullWorkingFlag.WF_SKELETON_MESHPIECE:
                    size = SaveToStreamForSkeletonMesh(stream);
                    break;
                case NullWorkingFlag.WF_NODE_ANIM:
                    size = SaveToStreamForSkeletonAnimation(stream);
                    break;
                default:
                    return 0;
            }

            return 0;
        }

        public int SaveToStreamForStaticMesh(NullMemoryStream stream)
        {
            int size = mMeshObjectList.SaveToStream(stream);
            size += mVertexMorphAnimations.SaveToStream(stream);
            return size;
        }

        public int SaveToStreamForSkeletonMesh(NullMemoryStream stream)
        {
            int size = mMeshObjectList.SaveToStream(stream);
            size += mSkinObjectList.SaveToStream(stream);
            size += mVertexMorphAnimations.SaveToStream(stream);
            size += mSocketNodeList.SaveToStream(stream);
            size += mNodeDummy.SaveToStream(stream);
            size += mNodeTree.SaveToStream(stream, false);
            size += mSkeletonBinding.SaveToStream(stream);
            return size;
        }

        public int SaveToStreamForSkeletonAnimation(NullMemoryStream stream)
        {
            int size = mNodeTree.SaveToStream(stream, true);
            size += mSocketNodeList.SaveToStream(stream);
            size += mNodeDummy.SaveToStream(stream);
            size += mSkeletonAnimations.SaveToStream(stream);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            uint foucc = 0;
            bool res = stream.ReadUInt(out foucc);
            res &= stream.ReadInt(out mBlockSize);
            res &= stream.ReadInt(out CurrentVersion);
            if (!res || CurrentVersion > MESH_FILE_VERSION || !ValidateFileHeader(foucc, CurrentVersion))
            {
                return false;
            }
            switch (mWorkingMode)
            {
                case NullWorkingFlag.WF_STATIC_MESH:
                    res = LoadFromStreamForStaticMesh(stream);
                    break;
                case NullWorkingFlag.WF_SKELETON_MESHPIECE:
                    res = LoadFromStreamForSkeletonMesh(stream);
                    break;
                case NullWorkingFlag.WF_NODE_ANIM:
                    res = LoadFromStreamForSkeletonAnimation(stream);
                    break;
                default:
                    return false;
            }
            return true;
        }

        public uint GenerateFouCC()
        {
            uint fouCC = 0;
            switch (mWorkingMode)
            {
                case NullWorkingFlag.WF_STATIC_MESH:
                    fouCC = StaticMesh;
                    break;
                case NullWorkingFlag.WF_SKELETON_MESHPIECE:
                    fouCC = SkeletonMesh;
                    break;
                case NullWorkingFlag.WF_NODE_ANIM:
                    fouCC = SkeletonAnimation;
                    break;
            }
            return fouCC;
        }

        private bool LoadFromStreamForStaticMesh(NullMemoryStream stream)
        {
            bool res = mMeshObjectList.LoadFromStream(stream);
            res &= mVertexMorphAnimations.LoadFromStream(stream);
            return res;
        }

        private bool LoadFromStreamForSkeletonMesh(NullMemoryStream stream)
        {
            bool res = mMeshObjectList.LoadFromStream(stream);
            res &= mSkinObjectList.LoadFromStream(stream);
            res &= mVertexMorphAnimations.LoadFromStream(stream);
            res &= mSocketNodeList.LoadFromStream(stream);
            res &= mNodeDummy.LoadFromStream(stream);
            res &= mNodeTree.LoadFromStream(stream);
            res &= mSkeletonBinding.LoadFromStream(stream);
            return res;
        }

        private bool LoadFromStreamForSkeletonAnimation(NullMemoryStream stream)
        {
            bool res = mNodeTree.LoadFromStream(stream);
            res &= mSocketNodeList.LoadFromStream(stream);
            res &= mNodeDummy.LoadFromStream(stream);
            res &= mSkeletonAnimations.LoadFromStream(stream);
            if (res)
            {
                ResolveBoneNames();
            }
            return res;
        }

        private void ResolveBoneNames()
        {
            if (mNodeTree == null || mNodeTree.GetNodeCount() == 0 || mSkeletonAnimations == null)
            {
                return;
            }
            for (int i = 0; i < mSkeletonAnimations.GetAnimationCount(); i++)
            {
                NullSkeletonAnimation animation = mSkeletonAnimations[i];
                for (int j = 0; j < animation.GetNodeCount(); j++)
                {
                    NullSkeletonNodeAnimation node = animation[j];
                    {
                        int id = node.GetParent();
                        NullNodeTree bone = mNodeTree.FindNode(id);
                        if (bone != null)
                        {
                            node.SetBoneName(bone.GetNodeName());
                        }
                    }
                }
            }
        }

        private bool ValidateFileHeader(uint aType, int version)
        {
            if (aType == StaticMesh)
            {
                InitializeAsStaticMesh(version);
            }
            else if (aType == SkeletonMesh)
            {
                InitializeAsSkeletonMesh(version);
            }
            else if (aType == SkeletonAnimation)
            {
                InitializeAsSkeletonAnimation(version);
            }
            else
            {
                mWorkingMode = NullWorkingFlag.WF_AUTO_DETECT;
            }
            return true;
        }

        private void Clear()
        {
            CurrentVersion = MESH_FILE_VERSION;
            mBlockSize = 0;
            mMeshObjectList = null;
            mSkinObjectList = null;
            mSocketNodeList = null;
            mNodeDummy = null;
            mNodeTree = null;
            mSkeletonBinding = null;
            mSkeletonAnimations = null;
            mVertexMorphAnimations = null;
        }

        private void InitializeAsStaticMesh(int version)
        {
            Clear();
            mWorkingMode = NullWorkingFlag.WF_STATIC_MESH;
            CurrentVersion = version;
            mBlockSize = 0;
            //base mesh
            mMeshObjectList = new NullMeshObjects(CurrentVersion);
            mVertexMorphAnimations = new NullVertexMorphAnimations();
        }

        private void InitializeAsSkeletonMesh(int version)
        {
            Clear();
            mWorkingMode = NullWorkingFlag.WF_SKELETON_MESHPIECE;
            CurrentVersion = version;
            mBlockSize = 0;
            //base mesh
            mMeshObjectList = new NullMeshObjects(CurrentVersion);
            mSkinObjectList = new NullMeshObjects(CurrentVersion);
            mSocketNodeList = new NullSocketNodes();
            mNodeDummy = new NullNodeDummy();
            mNodeTree = new NullNodeTree(CurrentVersion);
            mSkeletonBinding = new NullSkeletonBinding(CurrentVersion);
            mVertexMorphAnimations = new NullVertexMorphAnimations();
        }

        private void InitializeAsSkeletonAnimation(int version)
        {
            Clear();
            mWorkingMode = NullWorkingFlag.WF_NODE_ANIM;
            CurrentVersion = version;
            mBlockSize = 0;
            //base mesh
            mNodeTree = new NullNodeTree(CurrentVersion);
            mSocketNodeList = new NullSocketNodes();
            mNodeDummy = new NullNodeDummy();
            mSkeletonAnimations = new NullSkeletonAnimations(CurrentVersion);
        }

    }
}
