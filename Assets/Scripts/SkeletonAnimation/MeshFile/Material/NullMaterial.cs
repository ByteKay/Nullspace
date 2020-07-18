using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullMaterial : INullStream
    {
        protected string mMaterialName;
        protected int mMaterialId;
        protected string mLibraryName;
        protected uint mAmbientColor;
        protected uint mDiffuseColor;
        protected uint mSpecularColor;
        protected uint mEmissiveColor;
        protected byte mShinStrength;
        protected byte mShininess;              
        protected NullTextures mTextureArray;

        public NullMaterial()
        {
            mTextureArray = new NullTextures();
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteString(mMaterialName);
            size += stream.WriteInt(mMaterialId);
            size += stream.WriteString(mLibraryName);
            size += stream.WriteUInt(mAmbientColor);
            size += stream.WriteUInt(mDiffuseColor);
            size += stream.WriteUInt(mSpecularColor);
            size += stream.WriteUInt(mEmissiveColor);
            size += stream.WriteByte(mShinStrength);
            size += stream.WriteByte(mShininess);
            size += mTextureArray.SaveToStream(stream);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadString(out mMaterialName);
            res &= stream.ReadInt(out mMaterialId);
            res &= stream.ReadString(out mLibraryName);
            res &= stream.ReadUInt(out mAmbientColor);
            res &= stream.ReadUInt(out mDiffuseColor);
            res &= stream.ReadUInt(out mSpecularColor);
            res &= stream.ReadUInt(out mEmissiveColor);
            res &= stream.ReadByte(out mShinStrength);
            res &= stream.ReadByte(out mShininess);
            res &= mTextureArray.LoadFromStream(stream);
            return res;
        }
    }

    public class NullMaterials : INullStream
    {
        protected List<NullMaterial> mMaterialArray;

        public NullMaterials()
        {
            mMaterialArray = new List<NullMaterial>();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            return stream.ReadList(out mMaterialArray);
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            return stream.WriteList(mMaterialArray, false);
        }
    }

}
