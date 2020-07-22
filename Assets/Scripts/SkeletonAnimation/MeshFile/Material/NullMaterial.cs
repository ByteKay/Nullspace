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
            mAmbientColor = 0x0f0f0fff;
            mDiffuseColor = 0xc0c0c0ff;
            mSpecularColor = 0xc0c0c0ff;
            mEmissiveColor = 0x000000ff;
            mShinStrength = 26;
            mShininess = 10;
            mMaterialId = 0;
            mLibraryName = "";
            mMaterialName = "";
            mTextureArray = new NullTextures();
        }

        public bool SetTextureCount(int count)
        {
            Clear();
            for (int i = 0; i < count; i++)
            {
                mTextureArray.AddTexture(i);
            }
            return count > 0;
        }

        public NullTexture this[int index]
        {
            get
            {
                return mTextureArray[index];
            }
        }

        public NullTexture AddTexture(int textureId = 0, NullTextureWrap wrapMode = NullTextureWrap.EHXTW_WRAP_UV, NullTextureMode textureMode = NullTextureMode.EHXTM_MODAL, byte alphaChannel = 0, string fileName = "")
        {
            return mTextureArray.AddTexture(textureId, wrapMode, textureMode, alphaChannel, fileName);
        }

        public void Clear()
        {
            mTextureArray.Clear();
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

        public NullMaterial this[int index]
        {
            get
            {
                return index < mMaterialArray.Count ? mMaterialArray[index] : null;
            }
        }

        public NullMaterial AddMaterial()
        {
            NullMaterial mat = new NullMaterial();
            mMaterialArray.Add(mat);
            return mat;
        }

        public void Clear()
        {
            mMaterialArray.Clear();
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
