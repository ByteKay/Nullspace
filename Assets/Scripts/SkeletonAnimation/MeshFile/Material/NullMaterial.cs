using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullMaterial
    {
        protected uint m_ambientColor;
        protected uint m_diffuseColor;
        protected uint m_specularColor;
        protected uint m_emissiveColor;
        protected byte m_shinStrength;
        protected byte m_shininess;                              // Value affecting the size of specular highlights

        protected NullTextures m_textureArray;

        protected string m_materialName;
        protected ushort m_materialId;                         //unique id to identify the material.
        protected string m_libraryName;
    }

    public class NullMaterials
    {
        protected byte m_materialCount;
        protected List<NullMaterial> m_materialArray;
    }

}
