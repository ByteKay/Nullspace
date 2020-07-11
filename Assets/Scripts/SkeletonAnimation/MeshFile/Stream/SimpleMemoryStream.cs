using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class SimpleMemoryStream
    {

        private sbyte[] m_data;
        private uint m_currentPosition;
        private int m_length;
        private int m_allocated;
        private bool m_imported;
    }
}
