using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public enum NullTextureWrap : byte
    {
        EHXTW_WRAP_NONE,
        EHXTW_WRAP_U,
        EHXTW_WRAP_V,
        EHXTW_WRAP_UV,
    }

    public enum NullTextureMode : byte
    {
        EHXTM_MODAL,
        EHXTM_DECAL,
        EHXTM_REPLACE,
        EHXTM_CUBEMAP,
    }

    public enum NullTextureID : byte
    {
        EHXTI_DIFFUSE,
        EHXTI_SPECULAR,
        EHXTI_OPTICAL,
        EHXTI_REFLECTION,
        EHXTI_BUMP
    }

    public class NullTexture : INullStream
    {
        //个数为TextureCount， 不同的TextureName间用一个标记隔开
        protected string mTextureNameArray;
        // Texture Clamp Mode
        protected NullTextureWrap mWrapMode;                          
        protected NullTextureMode mTextureMode;
        protected int mTextureId;
        protected byte mAlphaChannel;

        public NullTexture()
        {
            mTextureNameArray = "";
        }

        public NullTexture(NullTextureWrap wrapMode, NullTextureMode textureMode, byte alphaChannel, int textureId) : this()
        {
            mWrapMode = wrapMode;
            mTextureMode = textureMode;
            mAlphaChannel = alphaChannel;
            mTextureId = textureId;
        }

        public int GetTextureId()
        {
            return mTextureId;
        }

        public bool AddTextureFile(string fileName)
        {
            fileName = fileName.ToLower();
            if (mTextureNameArray.IndexOf(fileName) >= 0)
            {
                return false;
            }
            if (mTextureNameArray.Equals(""))
            {
                mTextureNameArray = fileName;
            }
            else
            {
                mTextureNameArray = ";" + fileName;
            }
            return true;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteByte((byte)mWrapMode);
            size = stream.WriteByte((byte)mTextureMode);
            size = stream.WriteByte(mAlphaChannel);
            size = stream.WriteInt(mTextureId);
            size = stream.WriteString(mTextureNameArray);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            byte b;
            bool res = stream.ReadByte(out b);
            mWrapMode = (NullTextureWrap)b;
            res &= stream.ReadByte(out b);
            mTextureMode = (NullTextureMode)b;
            res &= stream.ReadByte(out mAlphaChannel);
            res &= stream.ReadInt(out mTextureId);
            res &= stream.ReadString(out mTextureNameArray);
            return res;
        }
    }

    public class NullTextures : INullStream
    {
        private List<NullTexture> mTextureArray;

        public NullTextures()
        {
            mTextureArray = new List<NullTexture>();
        }

        public NullTexture this[int index]
        {
            get
            {
                return index < mTextureArray.Count ? mTextureArray[index] : null;
            }
        }

        public NullTexture AddTexture(int textureId = 0, NullTextureWrap wrapMode = NullTextureWrap.EHXTW_WRAP_UV, NullTextureMode textureMode = NullTextureMode.EHXTM_MODAL, byte alphaChannel = 0, string fileName = "")
        {
            NullTexture texture = null;
            for (int i = 0; i < mTextureArray.Count; i++)
            {
                if (mTextureArray[i].GetTextureId() == textureId)
                {
                    texture = mTextureArray[i];
                    break;
                }
            }
            if (texture != null)
            {
                if (texture.AddTextureFile(fileName))
                {
                    return texture;
                }
                else
                {
                    return null;
                }
            }
            texture = new NullTexture(wrapMode, textureMode, alphaChannel, textureId);
            if (texture.AddTextureFile(fileName))
            {
                mTextureArray.Add(texture);
                return texture;
            }
            else
            {
                return null;
            }
        }

        public void Clear()
        {
            mTextureArray.Clear();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            return stream.ReadList(out mTextureArray);
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            return stream.WriteList(mTextureArray, false);
        }
    }

}
