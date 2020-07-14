using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullMaterialFile
    {
        private ushort Version;
        private uint BlockSize;
        private uint Reserved;
        private uint Reserved2;
        private uint Reserved3;
        private uint Reserved4;
        private NullMaterials MaterialArray;
    }
}
