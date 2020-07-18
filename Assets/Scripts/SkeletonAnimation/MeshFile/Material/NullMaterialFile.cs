using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullMaterialFile : INullStream
    {
        public int CurrentVersion;
        private uint mBlockSize;
        private uint mReserved;
        private uint mReserved2;
        private uint mReserved3;
        private uint mReserved4;
        private NullMaterials mMaterialArray;

        public NullMaterialFile()
        {
            mMaterialArray = new NullMaterials();
        }

        public NullMaterialFile(int version) : this()
        {
            CurrentVersion = version;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteUInt(mBlockSize);
            size += stream.WriteUInt(mReserved);
            size += stream.WriteUInt(mReserved2);
            size += stream.WriteUInt(mReserved3);
            size += stream.WriteUInt(mReserved4);
            size += mMaterialArray.SaveToStream(stream);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadUInt(out mBlockSize);
            res &= stream.ReadUInt(out mReserved);
            res &= stream.ReadUInt(out mReserved2);
            res &= stream.ReadUInt(out mReserved3);
            res &= stream.ReadUInt(out mReserved4);
            res &= mMaterialArray.LoadFromStream(stream);
            return res;
        }
    }
}
