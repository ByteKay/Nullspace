using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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
        // MOT_TRIANGLE_STRIPS = 3,
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

    public partial class NullMeshObject
    {

        public bool ExtractToTriangles(List<Vector3Int> originalFaceIndices)
        {
            bool done = false;
            originalFaceIndices.Clear();
            originalFaceIndices.AddRange(FaceArray);
            switch (GetMeshObjectType())
            {
                case NullPrimitiveType.MOT_TRIANGLES:
                    done = true;
                    break;
                //case NullPrimitiveType.MOT_TRIANGLE_STRIPS:
                //    done = ExtractToTrianglesFromStrips();
                //    break;
                case NullPrimitiveType.MOT_INDEXED_PRIMITIVES:
                    done = ExtractToTrianglesFromIndexedPrimitives(originalFaceIndices);
                    break;
            }
            return done;
        }

        public bool ExtractToTrianglesFromIndexedPrimitives(List<Vector3Int> originalFaceIndices)
        {
            List<Vector3> trianglePos = new List<Vector3>();
            int triangleCount = originalFaceIndices.Count;
            for (int i = 0; i < triangleCount; ++i)
            {
                Vector3Int face = originalFaceIndices[i];
                trianglePos.Add(VertexPosArray[face.x]);
                trianglePos.Add(VertexPosArray[face.y]);
                trianglePos.Add(VertexPosArray[face.z]);
            }
            VertexPosArray = trianglePos;

            if (HadNormalData())
            {
                List<Vector3> normalLst = new List<Vector3>();
                for (int i = 0; i < triangleCount; ++i)
                {
                    Vector3Int face = originalFaceIndices[i];
                    normalLst.Add(NormalArray[face.x]);
                    normalLst.Add(NormalArray[face.y]);
                    normalLst.Add(NormalArray[face.z]);
                }
                NormalArray = normalLst;
            }
            if (HadTangentData())
            {
                List<Vector3> tangantLst = new List<Vector3>();
                List<Vector3> binormalLst = new List<Vector3>();
                for (int i = 0; i < triangleCount; ++i)
                {
                    Vector3Int face = originalFaceIndices[i];
                    tangantLst.Add(TangentArray[face.x]);
                    tangantLst.Add(TangentArray[face.y]);
                    tangantLst.Add(TangentArray[face.z]);
                    binormalLst.Add(BinormalArray[face.x]);
                    binormalLst.Add(BinormalArray[face.y]);
                    binormalLst.Add(BinormalArray[face.z]);
                }
                TangentArray = tangantLst;
                BinormalArray = binormalLst;
            }
            if (HadVertexColorData())
            {
                List<uint> vertextColorLst = new List<uint>();
                for (int i = 0; i < triangleCount; ++i)
                {
                    Vector3Int face = originalFaceIndices[i];
                    vertextColorLst.Add(VertexColorArray[face.x]);
                    vertextColorLst.Add(VertexColorArray[face.y]);
                    vertextColorLst.Add(VertexColorArray[face.z]);
                }
                VertexColorArray = vertextColorLst;
            }

            if (HadUVData())
            {
                UVGroups.ExtractToTrianglesFromIndexedPrimitives(originalFaceIndices);
            }
            SetMeshObjectType(NullPrimitiveType.MOT_TRIANGLES);
            FaceArray.Clear();
            return true;
        }

        public bool BuildIndexedPrimitives(List<NullMergeIndex> indexMapping)
        {
            if ((GetMeshObjectType() != NullPrimitiveType.MOT_TRIANGLES) || (GetTriangleCount() < 2))
            {
                return false;
            }
            List<NullVertexStruct> floatData = PrepareFloatDataForVertexMerging();
            if (floatData == null)
            {
                return false;
            }
            MergeVertices(floatData, indexMapping);

            //update mesh data
            List<Vector3> newVertices = ReCreateCompactData(VertexPosArray, indexMapping);
            VertexPosArray.Clear();
            VertexPosArray = newVertices;

            List<uint> newColors = ReCreateCompactData(VertexColorArray, indexMapping);
            if (VertexColorArray != null && VertexColorArray.Count > 0)
            {
                VertexColorArray.Clear();
                VertexColorArray = newColors;
            }

            List<Vector3> newNormals = ReCreateCompactData(NormalArray, indexMapping);
            if (NormalArray != null && NormalArray.Count > 0)
            {
                NormalArray.Clear();
                NormalArray = newNormals;
            }

            List<Vector3> newTangents = ReCreateCompactData(TangentArray, indexMapping);
            if (TangentArray != null && TangentArray.Count > 0)
            {
                TangentArray.Clear();
                TangentArray = newTangents;
            }

            List<Vector3> newBinormals = ReCreateCompactData(BinormalArray, indexMapping);
            if (BinormalArray != null && BinormalArray.Count > 0)
            {
                BinormalArray.Clear();
                BinormalArray = newBinormals;
            }
            //update uv groups
            if (UVGroups != null)
            {
                UVGroups.BuildIndexedPrimitives(indexMapping);
            }
           
            List<int> faceIndexes = new List<int>();
            for (int i = 0; i < GetTriangleCount(); i++)
            {
                faceIndexes.Add(i * 3 + 0);
                faceIndexes.Add(i * 3 + 1);
                faceIndexes.Add(i * 3 + 2);
            }
            for (int i = 0; i < indexMapping.Count; i++)
            {
                NullMergeIndex index = indexMapping[i];
                faceIndexes[index.index] = i;
                for (int j = 0; j < index.equalOnes.Count; j++)
                {
                    faceIndexes[index.equalOnes[j]] = i;
                }
            }
            FaceArray.Clear();
            for (int i = 0; i < GetTriangleCount(); i++)
            {
                FaceArray.Add(new Vector3Int(faceIndexes[i * 3 + 0], faceIndexes[i * 3 + 1], faceIndexes[i * 3 + 2]));
            }
            SetMeshObjectType(NullPrimitiveType.MOT_INDEXED_PRIMITIVES);
            return true;
        }

        public static List<T> ReCreateCompactData<T>(List<T> sourceData, List<NullMergeIndex> indexMapping)
        {
            if (sourceData == null && sourceData.Count > 0)
            {
                return null;
            }
            List<T> dst = new List<T>();
            for (int i = 0; i < indexMapping.Count; i++)
            {
                NullMergeIndex index = indexMapping[i];
                dst.Add(sourceData[index.index]);
            }
            return dst;
        }

        public void MergeVertices(List<NullVertexStruct> vertices, List<NullMergeIndex> indexMapping)
        {
            indexMapping.Clear();
            if (vertices == null || vertices.Count < 3)
            {
                return;
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                NullVertexStruct vertex = vertices[i];
                bool merged = false;
                foreach (NullMergeIndex index in indexMapping)
                {
                    if (vertices[index.index].IsEquals(vertex))
                    {
                        merged = true;
                        index.equalOnes.Add(i);
                        break;
                    }
                }
                if (!merged)
                {
                    NullMergeIndex index = new NullMergeIndex(i);
                    indexMapping.Add(index);
                }
            }
        }

        public NullUVGroups GetUVGroups()
        {
            return UVGroups;
        }

        protected List<NullVertexStruct>  PrepareFloatDataForVertexMerging()
        {
            if ((GetMeshObjectType() != NullPrimitiveType.MOT_TRIANGLES) || (GetTriangleCount() < 2))
            {
                return null;
            }
            List<NullVertexStruct> floatData = new List<NullVertexStruct>();
            for (int i = 0; i < GetVertexCount(); ++i)
            {
                floatData.Add(new NullVertexStruct());
            }

            bool hadColor = HadVertexColorData();
            bool hadTangent = HadTangentData();
            int uvCount = 0;
            if (UVGroups != null)
            {
                uvCount = UVGroups.GetUVGroupCount();
            }
            NullVertexStruct buffer = null;
            for (int i = 0; i < GetVertexCount(); i++)
            {
                buffer = floatData[i];
                GetVertex(i, ref buffer.vertex);
                GetNormal(i, ref buffer.normal);
                buffer.hadColor = hadColor;
                buffer.hadTangent = hadTangent;
                if (hadColor)
                {
                    GetVertexColor(i, ref buffer.color);
                }
                else
                {
                    buffer.color = 0;
                }
                if (hadTangent)
                {
                    GetTangent(i, ref buffer.tangent);
                    GetBinormal(i, ref buffer.binormal);
                }
                for (int j = 0; j < uvCount; j++)
                {
                    NullUVGroup uvGroup = UVGroups.GetUVGroupByIndex(j);
                    Vector2 uv = Vector2.zero;
                    uvGroup.GetUV(i, ref uv);
                    buffer.uvLst.Add(uv);
                }
            }
            return floatData;
        }

        public byte GetSmoothGroup()
        {
            return SmoothGroup;
        }

        public void SetSmoothGroup(byte smoothGroup)
        {
            SmoothGroup = smoothGroup;
        }
    }

    /// <summary>
    /// 模型的顶点可以保存为 short
    /// 模型的索引可以强制为 ushort，小于 65536.即顶点数量要小于 65536
    /// </summary>
    public partial class NullMeshObject : INullStream
    {
        public int CurrentVersion;
        public List<Vector3> VertexPosArray;
        public List<Vector3Int> FaceArray;

        public List<Vector3> NormalArray;
        public List<Vector3> TangentArray;
        public List<Vector3> BinormalArray;
        public List<uint> VertexColorArray;
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

        public NullMeshObject()
        {
            mMeshObjectName = "";
            mMeshObjectHandle = 0;
            mMask = 0;
            mParentHandle = 0;
            SmoothGroup = 0;
            MaterialName = "";
            MaterialId = 0;
            VertexPosArray = new List<Vector3>();
            FaceArray = new List<Vector3Int>();
            NormalArray = new List<Vector3>();
            TangentArray = new List<Vector3>();
            BinormalArray = new List<Vector3>();
            VertexColorArray = new List<uint>();
            UVGroups = new NullUVGroups();
            Opacity = NullFacegroupOpacity.EHXFO_DEFAULT;
        }

        public NullMeshObject(int version, NullPrimitiveType meshType, int triangleCount, int vertexCount, bool includingNormal, bool includeTangent, bool includingVertexColor) : this()
        {
            CurrentVersion = version;
            SetMeshObjectType(meshType);
            if (triangleCount == 0)
            {
                return;
            }
            switch (GetMeshObjectType())
            {
                case NullPrimitiveType.MOT_TRIANGLES:
                    vertexCount = triangleCount * 3;
                    break;
                case NullPrimitiveType.MOT_INDEXED_PRIMITIVES:
                    break;
                //case NullPrimitiveType.MOT_TRIANGLE_STRIPS:
                //    vertexCount = triangleCount + 2;
                //    break;
                default:
                    return;
            }
            if (vertexCount > 65535)
            {
                SetMeshObjectLargeMesh(true);
            }
            else
            {
                SetMeshObjectLargeMesh(false);
            }
            SetVertexAndIndexCount(vertexCount, triangleCount);
            if (includingNormal)
            {
                BuildNormalArray();
            }
            if (includeTangent)
            {
                BuildTangentArray();
            }
            if (includingVertexColor)
            {
                BuildVertexColorArray();
            }
        }

        /// <summary>
        /// 返回 平均值 和 总和
        /// </summary>
        /// <param name="sum"></param>
        /// <returns></returns>
        public Vector3 GetCenter(ref Vector3 sum)
        {
            sum = Vector3.zero;
            int count = GetVertexCount();
            for (int i = 0; i < count; ++i)
            {
                sum += VertexPosArray[i];
            }
            if (count == 0)
            {
                return sum;
            }
            return sum / count;
        }

        public void Transform(Matrix4x4 m)
        {
            Assert.IsTrue(GetMeshObjectType() == NullPrimitiveType.MOT_TRIANGLES, "");
            int vt = GetVertexCount();
            //do transform
            for (int i = 0; i < vt; i++)
            {
                Vector3 v = Vector3.zero;
                GetVertex(i, ref v);
                v = m * v;
                SetVertex(i, v);
            }
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

        public NullPrimitiveType GetMeshObjectType()
        {
            return (NullPrimitiveType)((mMask >> 12) & 0x0f);
        }

        public void SetMeshObjectType(NullPrimitiveType mot)
        {
            byte t = (byte)mot;
            mMask = (ushort)((((t << 12) & 0xf000)) | (mMask & 0x0fff));
        }

        public int GetTriangleCount()
        {
            int count = 0;
            switch (GetMeshObjectType())
            {
                case NullPrimitiveType.MOT_TRIANGLES:
                    count = GetVertexCount() / 3;
                    break;
                case NullPrimitiveType.MOT_INDEXED_PRIMITIVES:
                    count = FaceArray.Count;
                    break;
                //case NullPrimitiveType.MOT_TRIANGLE_STRIPS:
                //    count = GetVertexCount() - 2;
                //    break;
            }
            return count;
        }

        public int GetVertexCount()
        {
            return VertexPosArray.Count;
        }

        public int GetMeshObjectHandle()
        {
            return mMeshObjectHandle;
        }

        public void SetMeshObjectHandle(int handle)
        {
            mMeshObjectHandle = handle;
        }

        void SetVertexAndIndexCount(int vertexCount, int faceCount = 0)
        {
            Clear();
            SetMeshObjectLargeMesh(vertexCount > 65535);
            for (int i = 0; i < vertexCount; ++i)
            {
                VertexPosArray.Add(Vector3.zero);
            }
            if (faceCount > 0)
            {
                for (int i = 0; i < faceCount; ++i)
                {
                    FaceArray.Add(Vector3Int.zero);
                }
            }
        }
        public void SetMeshObjectName(string name)
        {
            mMeshObjectName = name;
        }

        public string GetMeshObjectName()
        {
            return mMeshObjectName;
        }

        public void SetParentHandle(int handle)
        {
            mParentHandle = handle;
        }

        public int GetParentHandle()
        {
            return mParentHandle;
        }

        public List<Vector3Int> GetFaceIndices()
        {
            return FaceArray;
        }

        public List<Vector3> GetVertexData()
        {
            return VertexPosArray;
        }

        public List<Vector3> GetNormalData()
        {
            return NormalArray;
        }

        public List<Vector3> GetTangentData()
        {
            return TangentArray;
        }

        public List<uint> GetVertexColorData()
        {
            return VertexColorArray;
        }

        public void SetMask(int mask)
        {
            mMask = (mMask & 0xff00) | (mask & 0x00ff);
        }

        public void SetMeshObjectLargeMesh(bool isLargeMesh)
        {
            if (IsLargeMeshObject() == isLargeMesh)
            {
                return;
            }
            int mask = (int)NullMeshObjectOptionEnum.MOO_LARGE_MESH;
            if (isLargeMesh)
            {
                mMask |= mask;
            }
            else
            {
                mMask &= ~mask;
            }
        }

        public void SetMeshObjectNoCollision(bool noCollision)
        {
            int mask = (int)NullMeshObjectOptionEnum.MOO_NO_COLLISION;
            if (noCollision)
            {
                mMask |= mask;
            }
            else
            {
                mMask &= ~mask;
            }
        }

        public void SetMeshObjectInvisible(bool invisible)
        {
            int mask = (int)NullMeshObjectOptionEnum.MOO_INVISIBLE;
            if (invisible)
            {
                mMask |= mask;
            }
            else
            {
                mMask &= ~mask;
            }
        }

        public void SetMeshObjectControllable(bool controllable)
        {
            int mask = (int)NullMeshObjectOptionEnum.MOO_CONTROLLABLE;
            if (controllable)
            {
                mMask |= mask;
            }
            else
            {
                mMask &= ~mask;
            }
        }

        public void SetMeshObjectUVDivided(bool uvDivided)
        {
            int mask = (int)NullMeshObjectOptionEnum.MOO_UVDIVIDED;
            if (uvDivided)
            {
                mMask |= mask;
            }
            else
            {
                mMask &= ~mask;
            }
        }

        public void SetMeshObjectVertexAnimation(bool isVertexAnimation)
        {
            int mask = (int)NullMeshObjectOptionEnum.MOO_VERTEX_ANIMATION;
            if (isVertexAnimation)
            {
                mMask |= mask;
            }
            else
            {
                mMask &= ~mask;
            }
        }

        public void SetMeshObjectCollider(bool collider)
        {
            int mask = (int)NullMeshObjectOptionEnum.MOO_COLLIDER;
            if (collider)
            {
                mMask |= mask;
            }
            else
            {
                mMask &= ~mask;
            }
        }

        public bool IsColliderMeshObject() { return (mMask & (int)NullMeshObjectOptionEnum.MOO_COLLIDER) != 0; }
        public bool IsLargeMeshObject() { return (mMask & (int)NullMeshObjectOptionEnum.MOO_LARGE_MESH) != 0; }
        public bool IsNoCollisionMeshObject() { return (mMask & (int)NullMeshObjectOptionEnum.MOO_NO_COLLISION) != 0; }
        public bool IsInvisibleMeshObject() { return (mMask & (int)NullMeshObjectOptionEnum.MOO_INVISIBLE) != 0; }
        public bool IsControllableMeshObject() { return (mMask & (int)NullMeshObjectOptionEnum.MOO_CONTROLLABLE) != 0; }
        public bool IsUVDividedMeshObject() { return (mMask & (int)NullMeshObjectOptionEnum.MOO_UVDIVIDED) != 0; }
        public bool IsVertexAnimationMeshObject() { return (mMask & (int)NullMeshObjectOptionEnum.MOO_VERTEX_ANIMATION) != 0; }

        public void SetOpacity(NullFacegroupOpacity opacity)
        {
            Opacity = opacity;
        }

        public void SetMaterialName(string materialName)
        {
            MaterialName = materialName;
        }

        public string GetMaterialName()
        {
            return MaterialName;
        }

        public void SetMaterialId(int id)
        {
            MaterialId = id;
        }

        public int GetMaterialId()
        {
            return MaterialId;
        }
        public bool GetTriangle(int index, ref Vector3 p1, ref Vector3 p2, ref Vector3 p3)
        {
            if (index >= GetTriangleCount())
            {
                return false;
            }
            Vector3Int faceIndex = Vector3Int.zero;

            switch (GetMeshObjectType())
            {
                case NullPrimitiveType.MOT_TRIANGLES:
                    GetVertex(index * 3 + 0, ref p1);
                    GetVertex(index * 3 + 1, ref p2);
                    GetVertex(index * 3 + 2, ref p3);
                    break;
                //case NullPrimitiveType.MOT_TRIANGLE_STRIPS:
                //    if ((index & 0x00000001) == 0)
                //    {
                //        GetVertex(index + 0, ref p1);
                //        GetVertex(index + 1, ref p2);
                //        GetVertex(index + 2, ref p3);
                //    }
                //    else
                //    {
                //        GetVertex(index + 0, ref p1);
                //        GetVertex(index + 2, ref p2);
                //        GetVertex(index + 1, ref p3);
                //    }
                //    break;
                case NullPrimitiveType.MOT_INDEXED_PRIMITIVES:
                    GetTriangleIndex(index, ref faceIndex);
                    GetVertex(faceIndex.x, ref p1);
                    GetVertex(faceIndex.y, ref p2);
                    GetVertex(faceIndex.z, ref p3);
                    break;
            }
            return true;
        }

        public void SetVertex(int index, Vector3 v)
        {
            Assert.IsTrue(index < GetVertexCount(), "");
            VertexPosArray[index] = v;
        }

        public void SetVertexColor(int index, uint color)
        {
            Assert.IsTrue(index < GetVertexCount(), "");
            VertexColorArray[index] = color;
        }

        public void SetNormal(int index, Vector3 normal)
        {
            Assert.IsTrue(index < GetVertexCount(), "");
            NormalArray[index] = normal;
        }

        public void SetTangent(int index, Vector3 tangent)
        {
            Assert.IsTrue(index < GetVertexCount(), "");
            TangentArray[index] = tangent;
        }

        public void SetBinormal(int index, Vector3 binormal)
        {
            Assert.IsTrue(index < GetVertexCount(), "");
            BinormalArray[index] = binormal;
        }

        public void SetTriangleIndex(int index, Vector3Int face)
        {
            Assert.IsTrue(index < GetTriangleCount(), "");
            FaceArray[index] = face;
        }

        public void GetTriangleIndex(int index, ref Vector3Int face)
        {
            Assert.IsTrue(index < GetTriangleCount(), "");
            face = FaceArray[index];
        }

        public void GetVertex(int index, ref Vector3 v)
        {
            Assert.IsTrue(index < GetVertexCount(), "");
            v = VertexPosArray[index];
        }

        public void GetNormal(int index, ref Vector3 normal)
        {
            Assert.IsTrue(index < GetVertexCount(), "");
            normal = NormalArray[index];
        }

        public void GetTangent(int index, ref Vector3 tangent)
        {
            Assert.IsTrue(index < GetVertexCount(), "");
            tangent = TangentArray[index];
        }

        public void GetBinormal(int index, ref Vector3 binormal)
        {
            Assert.IsTrue(index < GetVertexCount(), "");
            binormal = BinormalArray[index];
        }

        public void GetVertexColor(int index, ref uint color)
        {
            Assert.IsTrue(index < GetVertexCount(), "");
            color = VertexColorArray[index];
        }
        
        public bool HadUVData()
        {
            return UVGroups.Count > 0;
        }

        public bool HadNormalData()
        {
            return NormalArray.Count > 0;
        }

        public bool HadTangentData()
        {
            return TangentArray.Count > 0;
        }

        public bool HadVertexColorData()
        {
            return VertexColorArray.Count > 0;
        }

        public void BuildVertexColorArray()
        {
            int vertexCount = GetVertexCount();
            VertexColorArray.Clear();
            for (int i = 0; i < vertexCount; ++i)
            {
                VertexColorArray.Add(0);
            }
        }

        public void BuildNormalArray()
        {
            int vertexCount = GetVertexCount();
            NormalArray.Clear();
            for (int i = 0; i < vertexCount; ++i)
            {
                NormalArray.Add(Vector3.zero);
            }
        }

        public void BuildTangentArray()
        {
            int vertexCount = GetVertexCount();
            TangentArray.Clear();
            for (int i = 0; i < vertexCount; ++i)
            {
                TangentArray.Add(Vector3.zero);
            }
        }
        public bool StandarizeDefaultUVs(float errorLimit)
        {
            if ((GetMeshObjectType() != NullPrimitiveType.MOT_TRIANGLES) || (GetTriangleCount() < 2))
            {
                return false;
            }
            NullUVGroup uvGroup = GetUVGroup(UVType.UVT_DEFAULT);
            if (uvGroup == null)
            {
                return false;
            }
            return uvGroup.StandarizeUVs(errorLimit);
        }

        public void RemoveUVGroup(UVType uvType)
        {
            if (UVGroups != null)
            {
                UVGroups.RemoveUVGroup(uvType);
            }
        }

        public NullUVGroup GetOrCreateUVGroup(UVType uvType)
        {
            if (GetVertexCount() == 0)
            {
                return null;
            }
            NullUVGroup group = GetUVGroup(uvType);
            if (group == null)
            {
                group = UVGroups.AppendUV(uvType);
            }
            return group;
        }

        public NullUVGroup GetOrCreateUVGroup(UVType uvType, int uvSize)
        {
            if (GetVertexCount() == 0)
            {
                return null;
            }
            NullUVGroup group = GetUVGroup(uvType);
            if (group == null)
            {
                group = UVGroups.AppendUV(uvType, uvSize);
            }
            return group;
        }

        public NullUVGroup GetUVGroup(UVType uvType)
        {
            return UVGroups[uvType];
        }
    }

    public partial class NullMeshObject
    {
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

        public void Clear()
        {
            VertexPosArray.Clear();
            FaceArray.Clear();
            UVGroups.Clear();
            NormalArray.Clear();
            TangentArray.Clear();
            BinormalArray.Clear();
            VertexColorArray.Clear();
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
                    UVGroups = new NullUVGroups(CurrentVersion, GetVertexCount());
                }
                size += UVGroups.SaveToStream(stream);
            }
            return size;
        }

        private int SaveVertexDataToStream(NullMemoryStream stream)
        {
            int size = stream.WriteList(VertexPosArray, false);
            size += stream.WriteList(FaceArray, false);
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
            res &= stream.ReadList(out FaceArray);
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
                    UVGroups = new NullUVGroups(CurrentVersion, GetVertexCount());
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
                res &= stream.ReadList(out NormalArray, GetVertexCount());
            }
            //try get tangent data
            res &= stream.ReadBool(out gotNormals);
            if (gotNormals)
            {
                TangentArray = new List<Vector3>();
                res &= stream.ReadList(out TangentArray, GetVertexCount());
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

        public void RemoveUVGroup(UVType uvType)
        {
            for (int i = 0; i < MeshObjectList.Count; ++i)
            {
                MeshObjectList[i].RemoveUVGroup(uvType);
            }
        }

        public bool RemoveMeshObject(NullMeshObject deletingOne, bool destroySource = true)
        {
            bool res = MeshObjectList.Remove(deletingOne);
            if (res && destroySource)
            {
                deletingOne.Clear();
            }
            return true;
        }

        public void DoAutoCenter()
        {
            Vector3 center = Vector3.zero;
            GetCenter(ref center);
            DoAutoCenter(center);
        }

        public void DoAutoCenter(Vector3 center)
        {
            for (int i = 0; i < MeshObjectList.Count; i++)
            {
                NullMeshObject meshObject = MeshObjectList[i];
                for (int j = 0; j < meshObject.GetVertexCount(); j++)
                {
                    Vector3 v = Vector3.zero;
                    meshObject.GetVertex(j, ref v);
                    v -= center;
                    meshObject.SetVertex(j, v);
                }
            }
        }

        public void GetCenter(ref Vector3 result)
        {
            result = Vector3.zero;
            int totalCount = 0;
            for (int i = 0; i < MeshObjectList.Count; i++)
            {
                NullMeshObject meshObject = MeshObjectList[i];
                meshObject.GetCenter(ref result);
                totalCount += meshObject.GetVertexCount();
            }
            if (totalCount == 0)
            {
                return;
            }
            result = result * (1.0f / totalCount);
        }

        public int GetMeshObjectCount()
        {
            return MeshObjectList.Count;
        }


        public bool CombineWithObjects(NullMeshObjects otherOne, Matrix4x4 m1, Matrix4x4 m2, bool newCenter, ref Vector3 center)
        {
            if (otherOne == null)
            {
                return false;
            }
            for (int i = 0; i < MeshObjectList.Count; i++)
            {
                if (this[i].IsVertexAnimationMeshObject())
                {
                    return false;
                }
            }
            for (int i = 0; i < otherOne.GetMeshObjectCount(); i++)
            {
                if (otherOne[i].IsVertexAnimationMeshObject())
                {
                    return false;
                }
            }
            for (int i = 0; i < MeshObjectList.Count; i++)
            {
                this[i].Transform(m1);
            }
            for (int i = 0; i < otherOne.GetMeshObjectCount(); i++)
            {
                otherOne[i].Transform(m2);
            }
            for (int i = 0; i < otherOne.GetMeshObjectCount(); i++)
            {
                MeshObjectList.Add(otherOne[i]);
            }
            otherOne.ClearWithOutDeletingData();
            if (newCenter)
            {
                GetCenter(ref center);
                DoAutoCenter(center);
            }
            else
            {
                DoAutoCenter();
            }
            return false;
        }

        // 不销毁集合里面的物体
        public void ClearWithOutDeletingData()
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

        public NullMeshObject AppendMeshObject(NullPrimitiveType meshType, int triangleCount, int vertexCount, bool includingNormal, bool includeTangent, bool includingVertexColor)
        {
            NullMeshObject meshObject = new NullMeshObject(CurrentVersion, meshType, triangleCount, vertexCount, includingNormal, includeTangent, includingVertexColor);
            MeshObjectList.Add(meshObject);
            return meshObject;
        }

        public int GetMeshObjectIndex(NullMeshObject mesh)
        {
            int index = 0xffff;
            for (int i = 0; i < MeshObjectList.Count; i++)
            {
                if (mesh == MeshObjectList[i])
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        public int GetTriangleCount()
        {
            int count = 0;
            for (int i = 0; i < GetMeshObjectCount(); i++)
            {
                count += MeshObjectList[i].GetTriangleCount();
            }
            return count;
        }

        public int GetVertexCount()
        {
            int count = 0;
            for (int i = 0; i < GetMeshObjectCount(); i++)
            {
                count += MeshObjectList[i].GetVertexCount();
            }
            return count;
        }

        public NullMeshObject GetMeshObjectByIndex(int index)
        {
            return this[index];
        }
    }
}
