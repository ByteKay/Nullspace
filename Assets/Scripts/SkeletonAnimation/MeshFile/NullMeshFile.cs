using System;

using System.Text;

namespace NullMesh
{
    public enum E_HXBWorkingFlag
    {
        EHXWF_AUTO_DETECT = 0,
        EHXWF_STATIC_MESH = 1,
        EHXWF_SKELETON_MESHPIECE = 2,
        EHXWF_NODE_ANIM = 3,
    }

    public class NullMeshFile : INullStream
    {

        public static uint MakeFourCC(string four)
        {
            return BitConverter.ToUInt32(Encoding.UTF8.GetBytes(four), 0);
        }

        private const ushort MESH_FILE_VERSION = 100;

        private static uint StaticMesh = MakeFourCC("HXBO");
        private static uint SkeletonMesh = MakeFourCC("HXBS");
        private static uint SkeletonAnimation = MakeFourCC("HXBA");

        protected ushort m_version;
        protected E_HXBWorkingFlag m_workingMode;
        protected uint m_blockSize;
        //base mesh
        protected NullMeshObjects m_meshObjectList;
        protected NullMeshObjects m_skinObjectList;
        protected NullSocketNodes m_socketNodeList;
        protected NullNodeDummy m_nodeDummy;
        //skeleton animation
        protected NullNodeTree m_nodeTree;
        protected NullSkeletonBinding m_skeletonBinding;
        protected NullSkeletonAnimations m_skeletonAnimations;
        //ertex morph animation
        protected NullVertexMorphAnimations m_vertexMorphAnimations;

        public int SaveToStream(NullMemoryStream stream)
        {
            return 0;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            uint foucc = 0;
            bool res = stream.ReadUInt(out foucc);
            res &= stream.ReadUInt(out m_blockSize);
            res &= stream.ReadUShort(out m_version);
            if (!res || m_version > MESH_FILE_VERSION || !ValidateFileHeader(foucc, m_version))
            {
                return false;
            }
            switch (m_workingMode)
            {
                case E_HXBWorkingFlag.EHXWF_STATIC_MESH:
                    res = LoadFromStreamForStaticMesh(stream);
                    break;
                case E_HXBWorkingFlag.EHXWF_SKELETON_MESHPIECE:
                    res = LoadFromStreamForSkeletonMesh(stream);
                    break;
                case E_HXBWorkingFlag.EHXWF_NODE_ANIM:
                    res = LoadFromStreamForSkeletonAnimation(stream);
                    break;
                default:
                    return false;
            }
            return true;
        }

        private bool LoadFromStreamForSkeletonMesh(NullMemoryStream stream)
        {
            bool res = m_meshObjectList.LoadFromStream(stream);
            res &= m_skinObjectList.LoadFromStream(stream);
            res &= m_vertexMorphAnimations.LoadFromStream(stream);
            res &= m_socketNodeList.LoadFromStream(stream);
            res &= m_nodeDummy.LoadFromStream(stream);
            res &= m_nodeTree.LoadFromStream(stream);
            res &= m_skeletonBinding.LoadFromStream(stream);
            return res;
        }

        private bool LoadFromStreamForSkeletonAnimation(NullMemoryStream stream)
        {
            bool res = m_nodeTree.LoadFromStream(stream);
            res &= m_socketNodeList.LoadFromStream(stream);
            res &= m_nodeDummy.LoadFromStream(stream);
            res &= m_skeletonAnimations.LoadFromStream(stream);
            if (res)
            {
                ResolveBoneNames();
            }
            return res;
        }

        private void ResolveBoneNames()
        {
            if (m_nodeTree == null || m_nodeTree.GetNodeCount() == 0 || m_skeletonAnimations == null)
            {
                return;
            }
            for (int i = 0; i < m_skeletonAnimations.GetAnimationCount(); i++)
            {
                NullSkeletonAnimation animation = m_skeletonAnimations[i];
                for (int j = 0; j < animation.GetNodeCount(); j++)
                {
                    NullSkeletonNodeAnimation node = animation[j];
                    {
                        uint id = node.GetParent();
                        NullNodeTree bone = m_nodeTree.FindNode(id);
                        if (bone != null)
                        {
                            node.SetBoneName(bone.GetNodeName());
                        } 
                    }
                }
            }
        }

        private bool LoadFromStreamForStaticMesh(NullMemoryStream stream)
        {
            bool res = m_meshObjectList.LoadFromStream(stream);
            res &= m_vertexMorphAnimations.LoadFromStream(stream);
            return res;
        }

        private bool ValidateFileHeader(uint aType, ushort version)
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
                m_workingMode = E_HXBWorkingFlag.EHXWF_AUTO_DETECT;
            }
            return true;
        }

        private void Clear()
        {
            m_version = MESH_FILE_VERSION;
            m_blockSize = 0;
            m_meshObjectList = null;
            m_skinObjectList = null;
            m_socketNodeList = null;
            m_nodeDummy = null;
            m_nodeTree = null;
            m_skeletonBinding = null;
            m_skeletonAnimations = null;
            m_vertexMorphAnimations = null;
        }

        private void InitializeAsStaticMesh(ushort version)
        {
            Clear();
            m_workingMode = E_HXBWorkingFlag.EHXWF_STATIC_MESH;
            m_version = version;
            m_blockSize = 0;
            //base mesh
            m_meshObjectList = new NullMeshObjects(m_version);
            m_vertexMorphAnimations = new NullVertexMorphAnimations();
        }

        private void InitializeAsSkeletonMesh(ushort version)
        {
            Clear();
            m_workingMode = E_HXBWorkingFlag.EHXWF_SKELETON_MESHPIECE;
            m_version = version;
            m_blockSize = 0;
            //base mesh
            m_meshObjectList = new NullMeshObjects(m_version);
            m_skinObjectList = new NullMeshObjects(m_version);
            m_socketNodeList = new NullSocketNodes();
            m_nodeDummy = new NullNodeDummy();
            m_nodeTree = new NullNodeTree(m_version);
            m_skeletonBinding = new NullSkeletonBinding(m_version);
            m_vertexMorphAnimations = new NullVertexMorphAnimations();
        }

        private void InitializeAsSkeletonAnimation(ushort version)
        {
            Clear();
            m_workingMode = E_HXBWorkingFlag.EHXWF_NODE_ANIM;
            m_version = version;
            m_blockSize = 0;
            //base mesh
            m_nodeTree = new NullNodeTree(m_version);
            m_socketNodeList = new NullSocketNodes();
            m_nodeDummy = new NullNodeDummy();
            m_skeletonAnimations = new NullSkeletonAnimations(m_version);
        }

    }
}
