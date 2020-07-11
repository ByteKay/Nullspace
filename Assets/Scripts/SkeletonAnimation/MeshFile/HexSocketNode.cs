using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class HexSocketNode
    {
		protected uint m_handle;
        protected uint m_parent;
        protected float[] m_position; // 3
        protected float[] m_quat; // 4
    }

    public class HexSocketNodes
    {
		protected ushort m_socketNodeCount;
        protected HexSocketNode[] m_socketNodeArray;
    }
}
