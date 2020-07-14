using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullTexture
    {
        // Texture Clamp Mode
        protected byte m_wrapMode;                          
        protected byte m_textureMode;
        protected byte m_alphaChannel;
        protected uint m_textureId;
        //个数为TextureCount， 不同的TextureName间用一个标记隔开
        protected string m_textureNameArray;
    }

    public class NullTextures
    {
        private byte m_textureCount;
        private List<NullTexture> m_textureArray;
    }

}
