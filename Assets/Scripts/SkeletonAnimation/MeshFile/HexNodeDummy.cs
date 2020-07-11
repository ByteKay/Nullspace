using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class HexNodeDummyObject
    {
        protected string m_nodeName;
        protected uint m_nodeHandle;
        protected float[] m_transform; // 7
    }

    public class HexNodeDummy
    {
        protected ushort m_dummyCount;
        protected HexNodeDummyObject[] m_dummyArray;
    }
}
