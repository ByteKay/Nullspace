using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullTexture
    {
        // Texture Clamp Mode
        protected byte WrapMode;                          
        protected byte TextureMode;
        protected byte AlphaChannel;
        protected uint TextureId;
        //个数为TextureCount， 不同的TextureName间用一个标记隔开
        protected string TextureNameArray;
    }

    public class NullTextures
    {
        private byte TextureCount;
        private List<NullTexture> TextureArray;
    }

}
