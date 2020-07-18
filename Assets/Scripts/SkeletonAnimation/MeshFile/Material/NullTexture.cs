using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullTexture : INullStream
    {
        // Texture Clamp Mode
        protected byte mWrapMode;                          
        protected byte mTextureMode;
        protected byte mAlphaChannel;
        protected int mTextureId;
        //个数为TextureCount， 不同的TextureName间用一个标记隔开
        protected string mTextureName;

        public NullTexture()
        {
            
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteByte(mWrapMode);
            size = stream.WriteByte(mTextureMode);
            size = stream.WriteByte(mAlphaChannel);
            size = stream.WriteInt(mTextureId);
            size = stream.WriteString(mTextureName);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadByte(out mWrapMode);
            res &= stream.ReadByte(out mTextureMode);
            res &= stream.ReadByte(out mAlphaChannel);
            res &= stream.ReadInt(out mTextureId);
            res &= stream.ReadString(out mTextureName);
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
