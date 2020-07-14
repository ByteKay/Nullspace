using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public enum NullMeshObjectOptionEnum
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

    public enum NullPrimitiveType
    {
        MOT_UNKNOWN = 0,
        MOT_TRIANGLES = 1,
        MOT_INDEXED_PRIMITIVES = 2,
        MOT_TRIANGLE_STRIPS = 3,
    }

    public enum NullFacegroupOpacity
    {
        EHXFO_DEFAULT = 0,
        EHXFO_SOLID,
        EHXFO_OPACITY,
        EHXFO_ALPHAREF
    }

    public enum NullDataStructType
    {
        DST_DEFAULT = 0,
        DST_SHORT = 1,
        DST_FLOAT = 2,
    }

    /// <summary>
    /// 模型的顶点可以保存为 short
    /// 模型的索引可以强制为 ushort，小于 65536.即顶点数量要小于 65536
    /// </summary>
    public partial class NullMeshObject : INullStream
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
        private NullPrimitiveType m_meshType;
        private uint triangleCount;
        private bool includingNormal;
        private bool includeTangent;
        private bool includingVertexColor;

        public NullMeshObject(ushort version)
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
            m_opacity = NullFacegroupOpacity.EHXFO_DEFAULT;
            m_meshType = NullPrimitiveType.MOT_INDEXED_PRIMITIVES;
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
            ushort v = (ushort)NullMeshObjectOptionEnum.MOO_LARGE_MESH;
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


        public NullDataStructType GetVertexDataType()
        {
            NullDataStructType dataType = (NullDataStructType)((m_mask >> 8) & 0x0f);
            if (dataType != NullDataStructType.DST_SHORT)
            {
                dataType = NullDataStructType.DST_FLOAT;
            }
            return dataType;
        }

        public bool IsLargeMeshObject()
        {
            return (m_mask & (ushort)NullMeshObjectOptionEnum.MOO_LARGE_MESH) != 0;
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

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = LoadBaseDataFromStream(stream);
            res &= LoadVertexDataFromStream(stream);
            res &= LoadUVGroupFromStream(stream);
            res &= LoadNormalFromStream(stream);
            res &= LoadColorFromStream(stream);
            res &= LoadMaterialFromStream(stream);
            return res;
        }

        public bool LoadBaseDataFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadUShort(out m_mask);
            res &= stream.ReadString(out m_meshObjectName);
            res &= stream.ReadUInt(out m_meshObjectHandle);
            res &= stream.ReadUInt(out m_parentHandle);
            return res;
        }

        public bool LoadVertexDataFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadList(out m_vertexArray);
            res &= stream.ReadList(out m_faceArray);
            if (res)
            {
                m_vertexCount = (ushort)m_vertexArray.Count;
                m_faceIndexCount = (uint)m_faceArray.Count;
                SetMeshObjectLargeMesh(m_vertexCount > 65535);
            }
            return res;
        }

        public bool LoadUVGroupFromStream(NullMemoryStream stream)
        {
            bool gotUVGroups = false;
            bool res = stream.ReadBool(out gotUVGroups);
            if (gotUVGroups)
            {
                if (m_uvGroups == null)
                {
                    m_uvGroups = new NullUVGroups(mCurrentVersion, m_vertexCount);
                }
                res &= m_uvGroups.LoadFromStream(stream);
            }
            return res;
        }

        public bool LoadNormalFromStream(NullMemoryStream stream)
        {
            //try get normal data
            bool gotNormals = false;
            bool res = stream.ReadBool(out gotNormals);
            if (gotNormals)
            {
                res &= stream.ReadList(out m_normalArray);
            }
            //try get tangent data
            res &= stream.ReadBool(out gotNormals);
            if (gotNormals)
            {
                m_tangentArray = new List<Vector3>();
                res &= stream.ReadList(out m_tangentArray);
            }
            return res;
        }

        public bool LoadColorFromStream(NullMemoryStream stream)
        {
            bool gotColors = false;
            bool res = stream.ReadBool(out gotColors);
            if (gotColors)
            {
                res &= stream.ReadList(out m_vertexColorArray);
            }
            return res;
        }

        public bool LoadMaterialFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadString(out m_materialName);
            res &= stream.ReadUInt(out m_materialId);
            byte b = 0;
            res &= stream.ReadByte(out b);
            if (res)
            {
                m_opacity = (NullFacegroupOpacity)b;
            }
            return res;
        }
    }

    public partial class NullMeshObject
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

    public partial class NullMeshObject
    {
        public NullUVGroups m_uvGroups;
    }

    public partial class NullMeshObject
    {
        public List<Vector3> m_normalArray;
        public List<Vector3> m_tangentArray;
        public List<Vector3> m_binormalArray;
    }

    public partial class NullMeshObject
    {
        public List<Color> m_vertexColorArray;
    }
    
    public partial class NullMeshObject
    {
        //material name
        public string m_materialName;
        //version101 specific -index of material: on exporter, it's the index of sub material. on exporter, we use index as an alternate indexing method to solve the problem of duplicate material name.
        public uint m_materialId;               
        //version102 specific: indicate if the face group is opacity
        public NullFacegroupOpacity m_opacity;
    }

    public class NullMeshObjects : INullStream
    {
        protected List<NullMeshObject> m_meshObjectList;
        protected ushort m_meshObjectCount;
        protected ushort mCurrentVersion;

        public NullMeshObjects(ushort version)
        {
            mCurrentVersion = version;
            m_meshObjectCount = 0;
            m_meshObjectList = null;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {

        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(out count);
            for (int i = 0; i < count; i++)
            {
                NullMeshObject meshObject = AppendMeshObject(NullPrimitiveType.MOT_INDEXED_PRIMITIVES);
                res &= meshObject.LoadFromStream(stream);
            }
            return res;
        }

        public NullMeshObject AppendMeshObject(NullPrimitiveType meshType)
        {
            NullMeshObject meshObject = new NullMeshObject(mCurrentVersion);
            m_meshObjectList.Add(meshObject);
            m_meshObjectCount++;
            return meshObject;
        }
    }
}
