using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class HexVertexMorphObject
    {
        protected byte m_vertexDataType;
        protected ushort m_vertexCount;
        protected byte[] m_vertexPosArray;
        protected sbyte[] m_normalArray;
        protected float[] m_offset; // 3
        protected double m_scale;
        protected uint m_meshObjectIndex;
    }

    public class HexVertexMorphAnimationFrame
    {
        protected HexVertexMorphObject[] m_vertexMorphObjectList;
        protected ushort m_vertexMorphObjectCount;
    }
}
