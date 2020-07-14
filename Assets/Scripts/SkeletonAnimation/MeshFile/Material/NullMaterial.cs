using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullMaterial
    {
        protected uint AmbientColor;
        protected uint DiffuseColor;
        protected uint SpecularColor;
        protected uint EmissiveColor;
        protected byte ShinStrength;
        protected byte Shininess;                             

        protected NullTextures TextureArray;

        protected string MaterialName;
        protected ushort MaterialId;
        protected string LibraryName;
    }

    public class NullMaterials
    {
        protected byte MaterialCount;
        protected List<NullMaterial> MaterialArray;
    }

}
