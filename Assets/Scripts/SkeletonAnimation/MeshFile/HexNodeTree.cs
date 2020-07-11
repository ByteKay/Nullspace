using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class HexNodeTree
    {
        public string m_nodeName;    // node name
        public uint m_nodeHandle;         // unique handle to the node
        public float[] m_transform; // 7
        public ushort m_numChildren;            // number of children
        public HexNodeTree[] m_children;               // sub trees
        public ushort m_version;
        public ushort m_groupId;
		protected ushort mCurrentVersion;
        public uint m_typeName;           // non-streamed data

    }
}
