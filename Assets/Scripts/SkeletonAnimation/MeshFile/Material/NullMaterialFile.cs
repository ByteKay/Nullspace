using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullMaterialFile
    {
        private ushort m_version;
        private uint m_blockSize;
        private uint m_reserved;
        private uint m_reserved2;
        private uint m_reserved3;
        private uint m_reserved4;
        private NullMaterials m_materialArray;
    }
}
