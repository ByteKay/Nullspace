using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MeshFile
{
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

    public enum PrimitiveType
    {
        MOT_UNKNOWN = 0,
        MOT_TRIANGLES = 1,
        MOT_INDEXED_PRIMITIVES = 2,
        MOT_TRIANGLE_STRIPS = 3,
    }

    public enum HEXFacegroupOpacity
    {
        EHXFO_DEFAULT = 0,
        EHXFO_SOLID,
        EHXFO_OPACITY,
        EHXFO_ALPHAREF
    }

    public enum DataStructType
    {
        DST_DEFAULT = 0,
        DST_SHORT = 1,
        DST_FLOAT = 2,
    }

    /// <summary>
    /// 模型的顶点可以保存为 short
    /// 模型的索引可以强制为 ushort，小于 65536.即顶点数量要小于 65536
    /// </summary>
    public partial class HexMeshObject : IStream
    {
        //enums for mesh_object_options

        public ushort mCurrentVersion;

        // number of piece vertex
        public ushort m_vertexCount;
        // offset value for int vertex 3
        public float[] m_offset;
        // scale value for int vertex
        public double m_vertexScale;
        public List<Vector3> m_vertexArray;
        public List<Vector3Int> m_faceArray;

        public uint m_faceIndexCount;
        public byte m_smoothGroup;
        private PrimitiveType m_meshType;
        private uint triangleCount;
        private bool includingNormal;
        private bool includeTangent;
        private bool includingVertexColor;

        public HexMeshObject(ushort version)
        {
            mCurrentVersion = version;
            m_mask = 0;
            m_meshObjectName = null;
            m_meshObjectHandle = 0;
            m_parentHandle = 0;
            m_vertexCount = 0;
            m_faceIndexCount = 0;
            m_offset = new float[3];
            m_vertexScale = 1.0;
            m_vertexArray = null;
            m_faceArray = null;
            m_uvGroups = null;
            m_normalArray = null;
            m_tangentArray = null;
            m_binormalArray = null;
            m_vertexColorArray = null;
            m_smoothGroup = 0;
            m_materialName = null;
            m_materialId = 0;
            m_opacity = HEXFacegroupOpacity.EHXFO_DEFAULT;
            m_meshType = PrimitiveType.MOT_INDEXED_PRIMITIVES;
            SetMeshObjectType((byte)m_meshType);
            //if (m_faceIndexCount == 0 || m_vertexCount <= 0 || m_vertexCount > 65535)
            //{
            //    // 应该抛出异常，不支持 超过 65536 的模型
            //    return;
            //}
        }

        public void SetMeshObjectLargeMesh(bool isLargeMesh)
        {
            if (IsLargeMeshObject() == isLargeMesh)
            {
                return;
            }
            ushort v = (ushort)MeshObjectOptionEnum.MOO_LARGE_MESH;
            if (isLargeMesh)
            {
                m_mask |= v;
            }
            else
            {
                m_mask &= (ushort)~v;
            }
            ApplyLargeMeshObjectOptionChanging();
        }

        public void Clear()
        {
            m_vertexCount = 0;
            m_faceIndexCount = 0;
            m_vertexArray = null;
            m_faceArray = null;
            m_uvGroups = null;
            m_normalArray = null;
            m_tangentArray = null;
            m_binormalArray = null;
            m_vertexColorArray = null;
        }


        public DataStructType GetVertexDataType()
        {
            DataStructType dataType = (DataStructType)((m_mask >> 8) & 0x0f);
            if (dataType != DataStructType.DST_SHORT)
            {
                dataType = DataStructType.DST_FLOAT;
            }
            return dataType;
        }

        public bool IsLargeMeshObject()
        {
            return (m_mask & (ushort)MeshObjectOptionEnum.MOO_LARGE_MESH) != 0;
        }

        public byte GetMeshObjectType()
        {
            return (byte)((m_mask >> 12) & 0x0f);
        }

        public void ApplyLargeMeshObjectOptionChanging()
        {
            if ((m_faceIndexCount > 0) && (m_faceArray != null))
            {

            }
        }

        public void SetMeshObjectType(byte mot)
        {
            m_mask = (ushort)((((mot << 12) & 0xf000)) | (m_mask & 0x0fff));
        }

        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            bool res = LoadBaseDataFromStream(stream);
            res &= LoadVertexDataFromStream(stream);
            res &= LoadUVGroupFromStream(stream);
            res &= LoadNormalFromStream(stream);
            res &= LoadColorFromStream(stream);
            res &= LoadMaterialFromStream(stream);
            return res;
        }

        public bool LoadBaseDataFromStream(SimpleMemoryStream stream)
        {
            bool res = stream.ReadUShort(ref m_mask);
            res &= stream.ReadString(ref m_meshObjectName);
            res &= stream.ReadUInt(ref m_meshObjectHandle);
            res &= stream.ReadUInt(ref m_parentHandle);
            return res;
        }

        public bool LoadVertexDataFromStream(SimpleMemoryStream stream)
        {
            Clear();
            m_faceArray = new List<Vector3Int>();
            m_vertexArray = new List<Vector3>();
            bool res = stream.ReadVector3Lst(ref m_vertexArray);
            res &= stream.ReadVector3IntLst(ref m_faceArray);
            if (res)
            {
                m_vertexCount = (ushort)m_vertexArray.Count;
                m_faceIndexCount = (uint)m_faceArray.Count;
                SetMeshObjectLargeMesh(m_vertexCount > 65535);
            }
            return res;
        }

        public bool LoadUVGroupFromStream(SimpleMemoryStream stream)
        {
            bool gotUVGroups = false;
            bool res = stream.ReadBool(ref gotUVGroups);
            if (gotUVGroups)
            {
                if (m_uvGroups == null)
                {
                    m_uvGroups = new HexUVGroups(mCurrentVersion, m_vertexCount);
                }
                res &= m_uvGroups.LoadFromStream(stream);
            }
            return res;
        }

        public bool LoadNormalFromStream(SimpleMemoryStream stream)
        {
            //try get normal data
            bool gotNormals = false;
            bool res = stream.ReadBool(ref gotNormals);
            if (gotNormals)
            {
                m_normalArray = new List<Vector3>(m_vertexCount);
                res &= stream.ReadVector3Lst(ref m_normalArray);
            }
            //try get tangent data
            res &= stream.ReadBool(ref gotNormals);
            if (gotNormals)
            {
                m_tangentArray = new List<Vector3>();
                res &= stream.ReadVector3Lst(ref m_tangentArray);
            }
            return res;
        }

        public bool LoadColorFromStream(SimpleMemoryStream stream)
        {
            bool gotColors = false;
            bool res = stream.ReadBool(ref gotColors);
            if (gotColors)
            {
                m_vertexColorArray = new List<Color>();
                res &= stream.ReadColorLst(ref m_vertexColorArray);
            }
            return res;
        }

        public bool LoadMaterialFromStream(SimpleMemoryStream stream)
        {
            bool res = stream.ReadString(ref m_materialName);
            res &= stream.ReadUInt(ref m_materialId);
            byte b = 0;
            res &= stream.ReadByte(ref b);
            if (res)
            {
                m_opacity = (HEXFacegroupOpacity)b;
            }
            return res;
        }
    }

    public partial class HexMeshObject
    {
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
        public HexUVGroups m_uvGroups;
    }

    public partial class HexMeshObject
    {
        public List<Vector3> m_normalArray;
        public List<Vector3> m_tangentArray;
        public List<Vector3> m_binormalArray;
    }

    public partial class HexMeshObject
    {
        public List<Color> m_vertexColorArray;
    }
    
    public partial class HexMeshObject
    {
        //material name
        public string m_materialName;
        //version101 specific -index of material: on exporter, it's the index of sub material. on exporter, we use index as an alternate indexing method to solve the problem of duplicate material name.
        public uint m_materialId;               
        //version102 specific: indicate if the face group is opacity
        public HEXFacegroupOpacity m_opacity;
    }

    public class HexMeshObjects : IStream
    {
        protected List<HexMeshObject> m_meshObjectList;
        protected ushort m_meshObjectCount;
        protected ushort mCurrentVersion;

        public HexMeshObjects(ushort version)
        {
            mCurrentVersion = version;
            m_meshObjectCount = 0;
            m_meshObjectList = null;
        }

        public int SaveToStream(SimpleMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {

        }

        public bool LoadFromStream(SimpleMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(ref count);
            for (int i = 0; i < count; i++)
            {
                HexMeshObject meshObject = AppendMeshObject(PrimitiveType.MOT_INDEXED_PRIMITIVES);
                res &= meshObject.LoadFromStream(stream);
            }
            return res;
        }

        public HexMeshObject AppendMeshObject(PrimitiveType meshType)
        {
            HexMeshObject meshObject = new HexMeshObject(mCurrentVersion);
            m_meshObjectList.Add(meshObject);
            m_meshObjectCount++;
            return meshObject;
        }
    }
}
