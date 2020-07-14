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
        public ushort mCurrentVersion;
        public ushort VertexCount;
        public float[] Offset;
        public double VertexScale;
        public List<Vector3> VertexArray;
        public List<Vector3Int> FaceArray;
        public uint FaceIndexCount;
        public byte SmoothGroup;
        private NullPrimitiveType MeshType;
        private uint TriangleCount;
        private bool IncludingNormal;
        private bool IncludeTangent;
        private bool IncludingVertexColor;

        public NullMeshObject(ushort version)
        {
            mCurrentVersion = version;
            Mask = 0;
            MeshObjectName = null;
            MeshObjectHandle = 0;
            ParentHandle = 0;
            VertexCount = 0;
            FaceIndexCount = 0;
            Offset = new float[3];
            VertexScale = 1.0;
            VertexArray = null;
            FaceArray = null;
            UVGroups = null;
            NormalArray = null;
            TangentArray = null;
            BinormalArray = null;
            VertexColorArray = null;
            SmoothGroup = 0;
            MaterialName = null;
            MaterialId = 0;
            Opacity = NullFacegroupOpacity.EHXFO_DEFAULT;
            MeshType = NullPrimitiveType.MOT_INDEXED_PRIMITIVES;
            SetMeshObjectType((byte)MeshType);
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
                Mask |= v;
            }
            else
            {
                Mask &= (ushort)~v;
            }
        }

        public void Clear()
        {
            VertexCount = 0;
            FaceIndexCount = 0;
            VertexArray = null;
            FaceArray = null;
            UVGroups = null;
            NormalArray = null;
            TangentArray = null;
            BinormalArray = null;
            VertexColorArray = null;
        }

        public NullDataStructType GetVertexDataType()
        {
            NullDataStructType dataType = (NullDataStructType)((Mask >> 8) & 0x0f);
            if (dataType != NullDataStructType.DST_SHORT)
            {
                dataType = NullDataStructType.DST_FLOAT;
            }
            return dataType;
        }

        public bool IsLargeMeshObject()
        {
            return (Mask & (ushort)NullMeshObjectOptionEnum.MOO_LARGE_MESH) != 0;
        }

        public byte GetMeshObjectType()
        {
            return (byte)((Mask >> 12) & 0x0f);
        }

        public void SetMeshObjectType(byte mot)
        {
            Mask = (ushort)((((mot << 12) & 0xf000)) | (Mask & 0x0fff));
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            mCurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = SaveBaseDataToStream(stream);
            size += SaveVertexDataToStream(stream);
            size += SaveUVGroupToStream(stream);
            size += SaveNormalToStream(stream);
            size += SaveColorToStream(stream);
            size += SaveMaterialToStream(stream);
            return size;
        }

        private int SaveColorToStream(NullMemoryStream stream)
        {
            bool gotColors = HadVertexColorData();
            int size = stream.WriteBool(gotColors);
            if (gotColors)
            {
                size += stream.WriteList(VertexColorArray, true);
            }
            return size;
        }

        private int SaveMaterialToStream(NullMemoryStream stream)
        {
            int size = stream.WriteString(MaterialName);
            size += stream.WriteUInt(MaterialId);
            size += stream.WriteByte((byte)Opacity);
            return size;
        }

        private int SaveNormalToStream(NullMemoryStream stream)
        {
            bool gotNormals = HadNormalData();
            int size = stream.WriteBool(gotNormals);
            if (gotNormals)
            {
                size += stream.WriteList(NormalArray, true);
            }
            gotNormals = HadTangentData();
            size += stream.WriteBool(gotNormals);
            if (gotNormals)
            {
                size += stream.WriteList(TangentArray, true);
            }
            return size;
        }

        private int SaveUVGroupToStream(NullMemoryStream stream)
        {
            bool gotUVGroups = HadUVData();
            int size = stream.WriteBool(gotUVGroups);
            if (gotUVGroups)
            {
                if (UVGroups == null)
                {
                    UVGroups = new NullUVGroups(mCurrentVersion, VertexCount);
                }
                size += UVGroups.SaveToStream(stream);
            }
            return size;
        }

        private int SaveVertexDataToStream(NullMemoryStream stream)
        {
            int size = stream.WriteList(VertexArray, false);
            size += stream.WriteList(FaceArray, false);
            return size;
        }

        private int SaveBaseDataToStream(NullMemoryStream stream)
        {
            int size = stream.WriteUShort(Mask);
            size += stream.WriteString(MeshObjectName);
            size += stream.WriteUInt(MeshObjectHandle);
            size += stream.WriteUInt(ParentHandle);
            return size;
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
            bool res = stream.ReadUShort(out Mask);
            res &= stream.ReadString(out MeshObjectName);
            res &= stream.ReadUInt(out MeshObjectHandle);
            res &= stream.ReadUInt(out ParentHandle);
            return res;
        }

        public bool LoadVertexDataFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadList(out VertexArray);
            res &= stream.ReadList(out FaceArray);
            if (res)
            {
                VertexCount = (ushort)VertexArray.Count;
                FaceIndexCount = (uint)FaceArray.Count;
                SetMeshObjectLargeMesh(VertexCount > 65535);
            }
            return res;
        }

        public bool LoadUVGroupFromStream(NullMemoryStream stream)
        {
            bool gotUVGroups = false;
            bool res = stream.ReadBool(out gotUVGroups);
            if (gotUVGroups)
            {
                if (UVGroups == null)
                {
                    UVGroups = new NullUVGroups(mCurrentVersion, VertexCount);
                }
                res &= UVGroups.LoadFromStream(stream);
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
                res &= stream.ReadList(out NormalArray, VertexCount);
            }
            //try get tangent data
            res &= stream.ReadBool(out gotNormals);
            if (gotNormals)
            {
                TangentArray = new List<Vector3>();
                res &= stream.ReadList(out TangentArray, VertexCount);
            }
            return res;
        }

        public bool LoadColorFromStream(NullMemoryStream stream)
        {
            bool gotColors = false;
            bool res = stream.ReadBool(out gotColors);
            if (gotColors)
            {
                res &= stream.ReadList(out VertexColorArray);
            }
            return res;
        }

        public bool LoadMaterialFromStream(NullMemoryStream stream)
        {
            byte b;
            bool res = stream.ReadString(out MaterialName);
            res &= stream.ReadUInt(out MaterialId);
            res &= stream.ReadByte(out b);
            if (res)
            {
                Opacity = (NullFacegroupOpacity)b;
            }
            return res;
        }

        public bool HadUVData()
        {
            return UVGroups != null;
        }

        public bool HadNormalData()
        {
            return NormalArray != null;
        }

        public bool HadTangentData()
        {
            return TangentArray != null;
        }

        public bool HadVertexColorData()
        {
            return VertexColorArray != null;
        }
    }

    public partial class NullMeshObject
    {
        protected ushort Mask;
        protected string MeshObjectName;		
        protected uint MeshObjectHandle;				
        protected uint ParentHandle;
    }

    public partial class NullMeshObject
    {
        public NullUVGroups UVGroups;
        public List<Vector3> NormalArray;
        public List<Vector3> TangentArray;
        public List<Vector3> BinormalArray;
        public List<Color> VertexColorArray;
    }

    public partial class NullMeshObject
    {
        public string MaterialName;
        public uint MaterialId;               
        public NullFacegroupOpacity Opacity;
    }

    public class NullMeshObjects : INullStream
    {
        protected List<NullMeshObject> MeshObjectList;
        protected ushort MeshObjectCount;
        protected ushort CurrentVersion;

        public NullMeshObjects(ushort version)
        {
            CurrentVersion = version;
            MeshObjectCount = 0;
            MeshObjectList = null;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteUShort(MeshObjectCount);
            for (byte i = 0; i < MeshObjectCount; i++)
            {
                 NullMeshObject meshObject = this[i];
                if (meshObject != null)
                {
                    size += meshObject.SaveToStream(stream);
                }
            }
            return size;
        }

        public NullMeshObject this[int idx]
        {
            get
            {
                return idx < MeshObjectList.Count ? MeshObjectList[idx] : null;
            }
        }


        public void Clear()
        {
            MeshObjectList = null;
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
            NullMeshObject meshObject = new NullMeshObject(CurrentVersion);
            MeshObjectList.Add(meshObject);
            MeshObjectCount++;
            return meshObject;
        }
    }
}
