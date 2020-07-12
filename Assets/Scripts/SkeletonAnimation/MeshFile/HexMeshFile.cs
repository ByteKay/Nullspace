using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class HexMeshFile : IStream
    {

        private static uint MakeFourCC(string four)
        {
            return BitConverter.ToUInt32(Encoding.UTF8.GetBytes(four), 0);
        }

        private const ushort MESH_FILE_VERSION = 100;
        private static uint StaticMesh = MakeFourCC("HXB0");
        private static uint SkeletonMesh = MakeFourCC("HXBS");
        private static uint SkeletonAnimation = MakeFourCC("HXBA");

        protected ushort m_version;
        protected byte m_workingMode;
        protected uint m_blockSize;
        //base mesh
        protected HexMeshObjects m_meshObjectList;
        protected HexMeshObjects m_skinObjectList;
        protected HexSocketNodes m_socketNodeList;
        protected HexNodeDummy m_nodeDummy;
        //skeleton animation
        protected HexNodeTree m_nodeTree;
        protected HexSkeletonBinding m_skeletonBinding;
        protected HexSkeletonAnimations m_skeletonAnimations;
        //ertex morph animation
        protected HexVertexMorphAnimations m_vertexMorphAnimations;

        public uint SaveToStream(SimpleMemoryStream stream)
        {
            return 0;
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            uint foucc = stream.ReadUInt();
            m_blockSize  = stream.ReadUInt();
            m_version = stream.ReadUShort();
            if (!ValidateFileHeader())
            {
                return false;
            }
            //if (m_version > MESH_FILE_VERSION)
            //{
            //    return false;
            //}
            //switch (m_workingMode)
            //{
            //    case EHXWF_STATIC_MESH:
            //        res &= LoadFromStreamForStaticMesh(stream);
            //        break;
            //    case EHXWF_SKELETON_MESHPIECE:
            //        res &= LoadFromStreamForSkeletonMesh(stream);
            //        break;
            //    case EHXWF_NODE_ANIM:
            //        res &= LoadFromStreamForSkeletonAnimation(stream);
            //        break;
            //    default:
            //        return false;
            //}

            return true;
        }

        private bool ValidateFileHeader()
        {
            return true;
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
                // m_workingMode = EHXWF_AUTO_DETECT;
            }
            return true;
        }

        private void InitializeAsStaticMesh(ushort version)
        {

        }

        private void InitializeAsSkeletonMesh(ushort version)
        {

        }

        private void InitializeAsSkeletonAnimation(ushort version)
        {

        }

    }
}
