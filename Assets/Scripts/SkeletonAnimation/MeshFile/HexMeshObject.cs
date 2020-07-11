using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public partial class HexMeshObject
    {
        //enums for mesh_object_options
        public enum MeshObjectOptionEnum
        {
            MOO_LARGE_MESH = 0x0080,
            MOO_COLLIDER = 0x0040,
            MOO_CONTROLLABLE = 0x0020,
            MOO_VERTEX_ANIMATION = 0x0010,
            MOO_NO_COLLISION = 0x0008,
            MOO_UVDIVIDED = 0x0004,
            MOO_DENSITY = 0x0002,
            MOO_INVISIBLE = 0x0001,
        }
        public ushort mCurrentVersion;

        // number of piece vertex
        public uint m_vertexCount;
        // offset value for int vertex 3
        public float[] m_offset;
        // scale value for int vertex
        public double m_vertexScale;
        // m_vertexCount * sizeof(data_saving_type) * 3, an array store vertex coordinates
        public byte[] m_vertexPosArray;
        // vertex indeices for indexed-primitives based mesh-object
        public ushort[] m_faceIndexArray;
        public uint m_faceIndexCount;
        public byte m_smoothGroup;

    }

    public partial class HexMeshObject
    {
        public enum PrimitiveTYpe
        {
            MOT_UNKNOWN = 0,
            MOT_TRIANGLES = 1,
            MOT_INDEXED_PRIMITIVES = 2,
            MOT_TRIANGLE_STRIPS = 3,
        }
        
        protected ushort m_mask;
        // 4
        protected uint[] m_parameters; 
        // name of mesh object node
        protected string m_meshObjectName;		
        // unique handle for mesh object
        protected uint m_meshObjectHandle;				
        // parent object
        protected uint m_parentHandle;
        
    }

    public partial class HexMeshObject
    {
        public HexUVGroups[] m_uvGroups;
    }

    public partial class HexMeshObject
    {
        public sbyte[] m_normalArray;
        public sbyte[] m_tangentArray;
        public sbyte[] m_binormalArray;
    }

    public partial class HexMeshObject
    {
        public uint[] m_vertexColorArray;
    }
    
    public partial class HexMeshObject
    {
        //material name
        public string m_materialName;
        //version101 specific -index of material: on exporter, it's the index of sub material. on exporter, we use index as an alternate indexing method to solve the problem of duplicate material name.
        public uint m_materialId;               
        //version102 specific: indicate if the face group is opacity
        public byte m_opacity;
    }

    public class HexMeshObjects
    {
        protected HexMeshObject[] m_meshObjectList;
        protected ushort m_meshObjectCount;
        protected ushort mCurrentVersion;
    }
}
