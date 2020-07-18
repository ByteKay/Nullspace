using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public enum NullMeshObjectOptionEnum : ushort
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

    public enum NullPrimitiveType : byte
    {
        MOT_UNKNOWN = 0,
        MOT_TRIANGLES = 1,
        MOT_INDEXED_PRIMITIVES = 2,
        MOT_TRIANGLE_STRIPS = 3,
    }

    public enum NullFacegroupOpacity : byte
    {
        EHXFO_DEFAULT = 0,
        EHXFO_SOLID,
        EHXFO_OPACITY,
        EHXFO_ALPHAREF
    }

    public enum NullDataStructType : byte
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
        public int CurrentVersion;
        public List<Vector3> VertexPosArray;
        public List<Vector3Int> FaceIndexArray;

        public int VertexCount { get { return VertexPosArray.Count; } }
        public int FaceIndexCount { get { return FaceIndexArray.Count; } }

        public List<Vector3> NormalArray;
        public List<Vector3> TangentArray;
        public List<Vector3> BinormalArray;
        public List<Color> VertexColorArray;
        public string MaterialName;
        public int MaterialId;
        public NullFacegroupOpacity Opacity;

        public byte SmoothGroup;
        public NullUVGroups UVGroups;

        protected int mMask;
        protected string mMeshObjectName;
        protected int mMeshObjectHandle;
        protected int mParentHandle;

        private NullPrimitiveType mMeshType;
        private bool mIncludingNormal;
        private bool mIncludeTangent;
        private bool mIncludingVertexColor;

        public NullMeshObject()
        {
            mMeshObjectName = "";
            mMeshObjectHandle = 0;
            mMask = 0;
            mParentHandle = 0;

            Opacity = NullFacegroupOpacity.EHXFO_DEFAULT;
            VertexPosArray = new List<Vector3>();
            FaceIndexArray = new List<Vector3Int>();
            NormalArray = new List<Vector3>();
            TangentArray = new List<Vector3>();
            BinormalArray = new List<Vector3>();
            VertexColorArray = new List<Color>();
            SmoothGroup = 0;
            MaterialName = "";
            MaterialId = 0;
            UVGroups = new NullUVGroups();
        }

        public NullMeshObject(int version) : this()
        {
            CurrentVersion = version;

            SetMeshObjectType((byte)mMeshType);
            if (VertexCount > 65535)
            {
                // 应该抛出异常，不支持 超过 65536 的模型
                throw new Exception("不支持 超过 65536 的模型");
            }
            switch (mMeshType)
            {
                case NullPrimitiveType.MOT_TRIANGLES:
                    break;
                case NullPrimitiveType.MOT_INDEXED_PRIMITIVES:
                    break;
                case NullPrimitiveType.MOT_TRIANGLE_STRIPS:
                    break;
                case NullPrimitiveType.MOT_UNKNOWN:
                    break;
            }
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
                mMask |= v;
            }
            else
            {
                mMask &= (ushort)~v;
            }
        }

        public void Clear()
        {
            VertexPosArray.Clear();
            FaceIndexArray.Clear();
            UVGroups.Clear();
            NormalArray.Clear();
            TangentArray.Clear();
            BinormalArray.Clear();
            VertexColorArray.Clear();
        }

        public NullDataStructType GetVertexDataType()
        {
            NullDataStructType dataType = (NullDataStructType)((mMask >> 8) & 0x0f);
            if (dataType != NullDataStructType.DST_SHORT)
            {
                dataType = NullDataStructType.DST_FLOAT;
            }
            return dataType;
        }

        public bool IsLargeMeshObject()
        {
            return (mMask & (ushort)NullMeshObjectOptionEnum.MOO_LARGE_MESH) != 0;
        }

        public byte GetMeshObjectType()
        {
            return (byte)((mMask >> 12) & 0x0f);
        }

        public void SetMeshObjectType(byte mot)
        {
            mMask = (ushort)((((mot << 12) & 0xf000)) | (mMask & 0x0fff));
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
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
            size += stream.WriteInt(MaterialId);
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
                    UVGroups = new NullUVGroups(CurrentVersion, VertexCount);
                }
                size += UVGroups.SaveToStream(stream);
            }
            return size;
        }

        private int SaveVertexDataToStream(NullMemoryStream stream)
        {
            int size = stream.WriteList(VertexPosArray, false);
            size += stream.WriteList(FaceIndexArray, false);
            return size;
        }

        private int SaveBaseDataToStream(NullMemoryStream stream)
        {
            int size = stream.WriteInt(mMask);
            size += stream.WriteString(mMeshObjectName);
            size += stream.WriteInt(mMeshObjectHandle);
            size += stream.WriteInt(mParentHandle);
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
            bool res = stream.ReadInt(out mMask);
            res &= stream.ReadString(out mMeshObjectName);
            res &= stream.ReadInt(out mMeshObjectHandle);
            res &= stream.ReadInt(out mParentHandle);
            return res;
        }

        public bool LoadVertexDataFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadList(out VertexPosArray);
            res &= stream.ReadList(out FaceIndexArray);
            if (res)
            {

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
                    UVGroups = new NullUVGroups(CurrentVersion, VertexCount);
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
            res &= stream.ReadInt(out MaterialId);
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

    public class NullMeshObjects : INullStream
    {
        protected List<NullMeshObject> MeshObjectList;
        protected int CurrentVersion;

        public NullMeshObjects()
        {
            MeshObjectList = new List<NullMeshObject>();
        }

        public NullMeshObjects(int version) : this()
        {
            CurrentVersion = version;
        }
        public void Clear()
        {
            MeshObjectList.Clear();
        }

        public NullMeshObject this[int idx]
        {
            get
            {
                return idx < MeshObjectList.Count ? MeshObjectList[idx] : null;
            }
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteList(MeshObjectList, false);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            return stream.ReadList(out MeshObjectList);
        }

        public NullMeshObject AppendMeshObject(NullPrimitiveType meshType)
        {
            NullMeshObject meshObject = new NullMeshObject(CurrentVersion);
            MeshObjectList.Add(meshObject);
            return meshObject;
        }
    }
}
