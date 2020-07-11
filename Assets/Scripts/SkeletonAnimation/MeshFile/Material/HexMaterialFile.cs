using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class HexMaterialFile
    {
        private ushort m_version;
        private uint m_blockSize;
        private uint m_reserved;
        private uint m_reserved2;
        private uint m_reserved3;
        private uint m_reserved4;
        private HexMaterials m_materialArray;
    }
}
