using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public class HexUVGroup
    {
        public enum UVType
        {
            UVT_DEFAULT = 0,        //default, equal uv for base-texture
            UVT_NORMAL_MAP = 1,     //uv for normal/gloss map
            UVT_LIGHT_MAP = 2,      //uv for light-map
            UVT_RESERVE = 3,        //maxium texture layer 
        };

        protected byte m_UVDataType;
        protected byte m_UVType;
        protected double m_UVScale;
        protected byte[] m_UVArray;
        protected ushort mCurrentVersion;
        private uint m_UVCount;
    }

    public class HexUVGroups
    {
        protected ushort mCurrentVersion;
        protected HexUVGroup[] m_UVGroupList;
        private uint m_UVGroupSize;
        private byte m_internalSize;
    }

}
