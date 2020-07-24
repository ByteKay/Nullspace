using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace NullMesh
{
    public enum NullWorkingFlag
    {
        WF_AUTO_DETECT = 0,
        WF_STATIC_MESH = 1,
        WF_SKELETON_MESHPIECE = 2,
        WF_NODE_ANIM = 3,
    }

    public partial class NullMeshFile
    {
        public NullMeshObject AppendMeshObject(NullPrimitiveType meshType, int triangleCount, int vertexCount, bool includingNormal, bool includeTangent, bool includingVertexColor)
        {
            NullMeshObject mesh = null;
            switch (mWorkingMode)
            {
                case NullWorkingFlag.WF_STATIC_MESH:
                case NullWorkingFlag.WF_SKELETON_MESHPIECE:
                    mesh = mMeshObjectList.AppendMeshObject(meshType, triangleCount, vertexCount, includingNormal, includeTangent, includingVertexColor);
                    mesh.SetMeshObjectHandle(mMeshObjectList.GetMeshObjectIndex(mesh));
                    break;
            }
            return mesh;
        }
        public NullMeshObject AppendSkinObject(NullPrimitiveType meshType, int triangleCount, int vertexCount, bool includingNormal, bool includeTangent, bool includingVertexColor)
        {
            NullMeshObject mesh = null;
            switch (mWorkingMode)
            {
                case NullWorkingFlag.WF_SKELETON_MESHPIECE:
                    mesh = mSkinObjectList.AppendMeshObject(meshType, triangleCount, vertexCount, includingNormal, includeTangent, includingVertexColor);
                    mesh.SetMeshObjectHandle(mMeshObjectList.GetMeshObjectCount() + mSkinObjectList.GetMeshObjectIndex(mesh));
                    break;
            }
            return mesh;
        }
        public bool ExtractToTriangles()
        {
            bool res = false;
            //try extract mesh-objects and vertex-morph animations
            if (mMeshObjectList != null)
            {
                for (int i = 0; i < mMeshObjectList.GetMeshObjectCount(); i++)
                {
                    List<Vector3Int> originalFaceIndices = new List<Vector3Int>();
                    NullMeshObject meshObject = mMeshObjectList[i];
                    res |= meshObject.ExtractToTriangles(originalFaceIndices);
                    if (meshObject.IsVertexAnimationMeshObject() && mVertexMorphAnimations != null)
                    {
                        for (int k = 0; k < mVertexMorphAnimations.GetAnimationCount(); k++)
                        {
                            NullVertexMorphAnimation animation = mVertexMorphAnimations[k];
                            for (int j = 0; j < animation.GetFrameCount(); j++)
                            {
                                NullVertexMorphAnimationFrame animationFrame = animation[j];
                                NullVertexMorphObject morphObject = animationFrame.FindMorphAnimationNodeByIndex(i);
                                Assert.IsTrue(morphObject != null, "");
                                switch (meshObject.GetMeshObjectType())
                                {
                                    case NullPrimitiveType.MOT_INDEXED_PRIMITIVES:
                                        res |= morphObject.ExtractToTrianglesFromIndexedPrimitives(originalFaceIndices);
                                        break;
                                        //case NullPrimitiveType.MOT_TRIANGLE_STRIPS:
                                        //    res |= morphObject.ExtractToTrianglesFromStrips(meshObject.GetTriangleCount());
                                        //    break;
                                }
                            }
                        }
                    }
                    originalFaceIndices.Clear();
                }
            }

            //try extract mesh-objects and skeleton-bindings
            if (mSkinObjectList != null)
            {
                for (int i = 0; i < mSkinObjectList.GetMeshObjectCount(); i++)
                {
                    List<Vector3Int> originalFaceIndices = new List<Vector3Int>();
                    NullMeshObject meshObject = mSkinObjectList[i];
                    res |= meshObject.ExtractToTriangles(originalFaceIndices);
                    if (mSkeletonBinding != null && mSkeletonBinding.GetSkeletonBindingCount() > 0)
                    {
                        NullSkeletonPiece skeletonPiece = mSkeletonBinding[i];
                        switch (meshObject.GetMeshObjectType())
                        {
                            case NullPrimitiveType.MOT_INDEXED_PRIMITIVES:
                                res |= skeletonPiece.ExtractToTrianglesFromIndexedPrimitives(originalFaceIndices);
                                break;
                                //case HexMeshObject::MOT_TRIANGLE_STRIPS:
                                //    res |= skeletonPiece.ExtractToTrianglesFromStrips(meshObject.GetTriangleCount());
                                //    break;
                        }
                    }
                }
            }

            return res;
        }
        public bool BuildIndexedPrimitives()
        {
            bool res = false;
            if (mMeshObjectList != null)
            {
                List<NullMergeIndex> indexMapping = new List<NullMergeIndex>();
                for (int i = 0; i < mMeshObjectList.GetMeshObjectCount(); i++)
                {
                    NullMeshObject meshObject = mMeshObjectList[i];
                    res |= meshObject.BuildIndexedPrimitives(indexMapping);
                    if (meshObject.IsVertexAnimationMeshObject() && mVertexMorphAnimations != null)
                    {
                        for (int k = 0; k < mVertexMorphAnimations.GetAnimationCount(); k++)
                        {
                            NullVertexMorphAnimation animation = mVertexMorphAnimations[k];
                            for (int j = 0; j < animation.GetFrameCount(); j++)
                            {
                                NullVertexMorphAnimationFrame animationFrame = animation[j];
                                NullVertexMorphObject morphObject = animationFrame.FindMorphAnimationNodeByIndex(i);
                                Assert.IsTrue(morphObject != null, "");
                                res |= morphObject.BuildIndexedPrimitives(indexMapping);
                            }
                        }
                    }
                }
            }

            //try extract mesh-objects and skeleton-bindings
            if (mSkinObjectList != null)
            {
                for (int i = 0; i < mSkinObjectList.GetMeshObjectCount(); i++)
                {
                    List<NullMergeIndex> indexMapping = new List<NullMergeIndex>();
                    NullMeshObject meshObject = mSkinObjectList[i];
                    res |= meshObject.BuildIndexedPrimitives(indexMapping);
                    if (mSkeletonBinding != null && indexMapping.Count > 0)
                    {
                        NullSkeletonPiece skeletonPiece = mSkeletonBinding[i];
                        res |= skeletonPiece.BuildIndexedPrimitives(indexMapping);
                    }
                }
            }
            return res;
        }
        public static List<T> Make<T>(int count) where T : new()
        {
            List<T> res = new List<T>();
            for (int i = 0; i < count; ++i)
            {
                res.Add(new T());
            }
            return res;
        }
        public static void Set<T>(List<T> lst, T v) 
        {
            for (int i = 0; i < lst.Count; ++i)
            {
                lst[i] = v;
            }
        }
        protected bool BuildTangentForMeshObjects(NullMeshObjects meshObjects, float thresHold, bool forceSmooth = false)
        {
            if (meshObjects == null || meshObjects.GetMeshObjectCount() == 0)
            {
                return false;
            }

            int triangleCount = meshObjects.GetTriangleCount();
            if (triangleCount > 0)
            {
                //preparing data
                List<Vector3> vertices = new List<Vector3>();
                List<Vector2> uvs = new List<Vector2>();

                List<Vector3> tangents = Make<Vector3>(triangleCount * 3);
                List<Vector3> binormals = Make<Vector3>(triangleCount * 3);
                List<byte> smoothGroups = Make<byte>(triangleCount);
                int realTriangleCount = 0;

                for (int i = 0; i < meshObjects.GetMeshObjectCount(); i++)
                {
                    NullMeshObject meshObject = meshObjects[i];
                    if (meshObject.GetUVGroups().Count == 0 || meshObject.GetNormalData().Count == 0)
                    {
                        continue;
                    }
                    int count = meshObject.GetTriangleCount();
                    vertices.AddRange(meshObject.GetVertexData());
                    byte smoothGroup = meshObject.GetSmoothGroup();

                    if (forceSmooth && smoothGroup == 0)
                    {
                        smoothGroup = 1;
                    }
                    Set(smoothGroups, smoothGroup);
                    NullUVGroup uvGroup = meshObject.GetUVGroup(UVType.UVT_NORMAL_MAP);
                    if (uvGroup == null)
                    {
                        uvGroup = meshObject.GetUVGroup(UVType.UVT_DEFAULT);
                    }
                    uvs.AddRange(uvGroup.GetUVData());
                    realTriangleCount += count;
                }
                //do face-smoothing
                ComputeTengents(thresHold, vertices, realTriangleCount, smoothGroups, uvs, tangents, binormals);

                //update mesh objects normals
                for (int i = 0; i < meshObjects.GetMeshObjectCount(); i++)
                {
                    NullMeshObject meshObject = meshObjects[i];
                    if (meshObject.GetUVGroups().Count == 0 || meshObject.GetNormalData().Count == 0)
                    {
                        continue;
                    }
                    meshObject.BuildTangentArray();
                    int count = meshObject.GetTriangleCount();
                    for (int j = 0; j < count; j++)
                    {
                        //set tangent
                        meshObject.SetTangent(j * 3 + 0, tangents[j * 3]);
                        meshObject.SetTangent(j * 3 + 1, tangents[j * 3 + 1]);
                        meshObject.SetTangent(j * 3 + 2, tangents[j * 3 + 2]);

                        //set binormal
                        meshObject.SetBinormal(j * 3 + 0, binormals[j * 3]);
                        meshObject.SetBinormal(j * 3 + 1, binormals[j * 3 + 1]);
                        meshObject.SetBinormal(j * 3 + 2, binormals[j * 3 + 2]);
                    }
                }
                //delete temp buffer
                vertices.Clear();
                tangents.Clear();
                binormals.Clear();
                uvs.Clear();
                smoothGroups.Clear();
            }
	        return true;
        }
        public static Vector3 CalcPlaneNormal3f(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return Vector3.Cross(p2 - p1, p3 - p1).normalized;
        }
        /// <summary>
        ///                   p3                          uv3
        ///                   -                            -
        ///                 /   \                        /   \
        ///                /     \                      /     \
        ///               /       \                    /       \
        ///              /         \                  /         \
        ///             _ _ _ _ _ _ _                _ _ _ _ _ _ _
        ///           p1             p2            uv1             uv2
        ///           
        /// 设 任意三角面上一点为 p 点，其uv坐标为(u, v)，令 p - p1  =  (u - u1)  * T  +  (v - v1) * B
        /// 确定切空间 T和B
        /// p2 - p1  = (u2 - u1) * T  + (v2 - v1) * B = du21 * T - dv21 * B
        /// p3 - p1  = (u3 - u1) * T  + (v3 - v1) * B = du31 * T - dv31 * B
        /// 则
        /// p2 - p1          du21  -dv21      T
        ///            =  
        /// p3 - p1          du31  -dv31      B
        /// 
        /// 
        ///             -1
        /// du21  -dv21                     1                 -dv31  dv21
        ///                 =   ---------------------------                           
        /// du31  -dv31         (dv21 * du31 - du21 * dv31)   -du31  du21
        /// 
        /// side0 = p2 - p1 
        /// side1 = p3 - p1
        /// 
        /// 
        ///  -dv31  dv21     side0       T                                      side1 * dv21 - side0 * dv31
        ///                          =        (dv21 * du31 - du21 * dv31) =                          
        ///  -du31  du21     side1       B                                      side1 * du21 - side0 * du31
        ///  
        /// 
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="uv1"></param>
        /// <param name="uv2"></param>
        /// <param name="uv3"></param>
        /// <param name="tangent"></param>
        /// <param name="binormal"></param>
        public static void CalculateTangentSpaceVector(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv1, Vector2 uv2, Vector2 uv3, ref Vector3 tangent, ref Vector3 binormal)
        {
            Vector3 side0 = p2 - p1;
            Vector3 side1 = p3 - p1;
            //Calculate face normal
            Vector3 normal = Vector3.Cross(side1, side0).normalized;
            //Now we use a formula to calculate the tangent.
            Vector2 deltaUV21 = uv2 - uv1;
            Vector2 deltaUV31 = uv3 - uv1;
            tangent = (side1 * deltaUV21.y - side0 * deltaUV31.y).normalized;
            binormal = (side1 * deltaUV21.x - side0 * deltaUV31.x).normalized;
            //Now, we take the cross product of the tangents to get a vector which
            //should point in the same direction as our normal calculated above.
            //If it points in the opposite direction (the dot product between the tangents is less than zero),
            //then we need to reverse the s and t tangents.
            //This is because the triangle has been mirrored when going from tangent space to object space.
            //reverse tangents if necessary
            Vector3 tangentCross = Vector3.Cross(tangent, binormal).normalized;
            float dot = Vector3.Dot(tangentCross, normal);
            if (dot < 0)
            {
                tangent = -tangent;
            }
        }
        public static void ComputeTengents(float thresHold, List<Vector3> vertices, int triangleCount, List<byte> smoothGroups,List<Vector2> uvs, List<Vector3> tangents, List<Vector3> binormals)
        {
            //re-culate facet-tangents
            List<Vector3> facetTangents = Make<Vector3>(triangleCount);
            List<Vector3> facetBinormals = Make<Vector3>(triangleCount);
            List<Vector3> facetNormals = Make<Vector3>(triangleCount);
            Vector3 t = Vector3.zero;
            Vector3 b = Vector3.zero;
            for (int i = 0; i < triangleCount; i++)
            {
                facetNormals[i] = Vector3.Cross(vertices[i * 3 + 1] - vertices[i * 3], vertices[i * 3 + 2] - vertices[i * 3]).normalized;
                CalculateTangentSpaceVector(vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2], uvs[i * 3 + 0], uvs[i * 3 + 1], uvs[i * 3 + 2], ref t, ref b);
                facetTangents[i] = t;
                facetBinormals[i] = b;
            }
            for (int i = 0; i < triangleCount; i++)
            {
                tangents[i * 3 + 0] = facetTangents[i];
                tangents[i * 3 + 1] = facetTangents[i];
                tangents[i * 3 + 2] = facetTangents[i];
                binormals[i * 3 + 0] = facetBinormals[i];
                binormals[i * 3 + 1] = facetBinormals[i];
                binormals[i * 3 + 2] = facetBinormals[i];
            }

            //build map of vertices whitch shared normal
            List<bool> usedTangents = Make<bool>(triangleCount * 3);
            List<TangentEquals> tangentMappings = new List<TangentEquals>();
            float _c_threshold = Mathf.Cos(180.0f - thresHold);
            for (int i = 0; i < (triangleCount - 1) * 3; i++)
            {
                if (usedTangents[i])
                {
                    continue;
                }
                TangentEquals mapping = new TangentEquals(i);
                byte currentFaceGroup = smoothGroups[i / 3];
                if (currentFaceGroup == 0)
                {
                    //facegroup id equals to zero, add it directly
                    tangentMappings.Add(mapping);
                    usedTangents[i] = true;
                    continue;
                }
                for (int j = i + (3 - i % 3); j < triangleCount * 3; j++)
                {
                    if (usedTangents[j])
                    {
                        continue;
                    }
                    byte faceGroup = smoothGroups[j / 3];
                    if (faceGroup == 0)
                    {
                        //facegroup id equals to zero, add it directly
                        TangentEquals newMapping = new TangentEquals(j);
                        tangentMappings.Add(newMapping);
                        usedTangents[j] = true;
                        continue;
                    }
                    if (vertices[i] == vertices[j])
                    {
                        float angleNormal = Vector3.Dot(facetNormals[i / 3], facetNormals[j / 3]);
                        if (angleNormal > _c_threshold)
                        {
                            mapping.equalOnes.Add(j);
                            usedTangents[j] = true;
                        }
                    }
                }
                tangentMappings.Add(mapping);
            }

            //copy to output array
            for (int i = 0; i < tangentMappings.Count; i++)
            {
                tangentMappings[i].Normalize(facetTangents, tangents, facetBinormals, binormals);
            }

            //clear temp data
            tangentMappings.Clear();
            facetNormals.Clear();
            facetTangents.Clear();
            facetBinormals.Clear();
            usedTangents.Clear();
        }
        public bool DoRotateCalculation(float angle)
        {
            bool res = false;
            switch (GetWorkingFlag())
            {
                case NullWorkingFlag.WF_STATIC_MESH:
                    res |= RotateMeshObjects(mMeshObjectList, angle);
                    res |= RotateVertexMorphAnimations(mVertexMorphAnimations, angle);
                    break;
                case NullWorkingFlag.WF_SKELETON_MESHPIECE:
                    res |= RotateMeshObjects(mMeshObjectList, angle);
                    res |= RotateMeshObjects(mSkinObjectList, angle);
                    res |= RotateVertexMorphAnimations(mVertexMorphAnimations, angle);
                    res |= RotateSocketNodes(mSocketNodeList, angle);
                    res |= RotateNodeDummy(mNodeDummy, angle);
                    res |= RotateNodeTree(mNodeTree, angle);
                    break;
                case NullWorkingFlag.WF_NODE_ANIM:
                    res |= RotateSocketNodes(mSocketNodeList, angle);
                    res |= RotateNodeDummy(mNodeDummy, angle);
                    res |= RotateNodeTree(mNodeTree, angle);
                    res |= RotateAnimation(mSkeletonAnimations.GetAnimationCount() > 0 ? mSkeletonAnimations[0] : null, angle);
                    break;
                default:
                    return false;
            }
            return res;
        }
        public static void Vector3RotateCalculation(float angle, Vector3 data, ref Vector3 newData, bool preMerge)
        {
            List<Vector3> tmp = new List<Vector3>() { data };
            List<Vector3> result = new List<Vector3>();
            Vector3RotateCalculation(angle, tmp, result, preMerge);
            newData = result[0];
        }
        public static void Vector3RotateCalculation(float angle, List<Vector3> data, List<Vector3> newData, bool preMerge)
        {
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.up);
            newData.Clear();
            int count = data.Count;
            for (int i = 0; i < count; i++)
            {
                Vector3 vec = q * data[i];
                newData.Add(vec);
            }
        }
        public static void QuaternionRotateCalculation(float angle, Quaternion data, ref Quaternion newData)
        {
            List<Quaternion> tmp = new List<Quaternion>() { data };
            List<Quaternion> result = new List<Quaternion>();
            QuaternionRotateCalculation(angle, tmp, result);
            newData = result[0];
        }
        public static void QuaternionRotateCalculation(float angle, List<Quaternion> data, List<Quaternion> newData)
        {
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.up);
            newData.Clear();
            int count = data.Count;
            for (int i = 0; i < count; i++)
            {
                newData.Add(data[i] * q);
            }
        }
        protected bool RotateMeshObjects(NullMeshObjects meshObjects, float angle)
        {
            if (meshObjects == null || meshObjects.GetVertexCount() == 0)
            {
                return false;
            }
            int triangleCount = meshObjects.GetTriangleCount();
            if (triangleCount > 0)
            {
                //preparing data
                List<Vector3> vertices = new List<Vector3>();
                List<Vector3> newVertices = new List<Vector3>();
                for (int i = 0; i < meshObjects.GetMeshObjectCount(); i++)
                {
                    NullMeshObject meshObject = meshObjects[i];
                    int count = meshObject.GetTriangleCount();
                    vertices.AddRange(meshObject.GetVertexData());
                }

                //do rotate calculation
                Vector3RotateCalculation(angle, vertices, newVertices, false);

                //update mesh objects vertices
                int index = 0;
                for (int i = 0; i < meshObjects.GetMeshObjectCount(); i++)
                {
                    NullMeshObject meshObject = meshObjects[i];
                    int count = meshObject.GetTriangleCount();
                    for (int j = 0; j < count; j++)
                    {
                        meshObject.SetVertex(j * 3 + 0, newVertices[index++]);
                        meshObject.SetVertex(j * 3 + 1, newVertices[index++]);
                        meshObject.SetVertex(j * 3 + 2, newVertices[index++]);
                    }
                }
                //delete temp buffer
                vertices.Clear();
                newVertices.Clear();
            }
            return true;
        }
        protected bool RotateVertexMorphAnimations(NullVertexMorphAnimations vertexMorphAnimations, float angle)
        {
            if (vertexMorphAnimations == null || vertexMorphAnimations.GetAnimationCount() == 0)
            {
                return false;
            }
            bool res = true;
            for (int i = 0; i < vertexMorphAnimations.GetAnimationCount(); i++)
            {
                NullVertexMorphAnimation vertexMorphAnimation = vertexMorphAnimations[i];
                res &= RotateVertexMorphAnimation(vertexMorphAnimation, angle);
            }
            return res;
        }
        protected bool RotateVertexMorphAnimation(NullVertexMorphAnimation vertexMorphAnimation, float angle)
        {
            if (vertexMorphAnimation == null || vertexMorphAnimation.GetFrameCount() == 0)
            {
                return false;
            }
            bool res = true;
            for (int i = 0; i < vertexMorphAnimation.GetFrameCount(); i++)
            {
                NullVertexMorphAnimationFrame animFrame = vertexMorphAnimation[i];
                res &= RotateVertexMorphAnimationFrame(animFrame, angle);
            }
            return res;
        }
        protected bool RotateVertexMorphAnimationFrame(NullVertexMorphAnimationFrame vertexMorphAnimationFrame, float angle)
        {
            if (vertexMorphAnimationFrame == null || vertexMorphAnimationFrame.GetMorphObjectCount() == 0)
            {
                return false;
            }
            bool res = true;
            for (int i = 0; i < vertexMorphAnimationFrame.GetMorphObjectCount(); i++)
            {
                NullVertexMorphObject vertexMorphObject = vertexMorphAnimationFrame[i];
                res &= RotateVertexMorphObject(vertexMorphObject, angle);
            }
            return res;
        }
        protected bool RotateVertexMorphObject(NullVertexMorphObject vertexMorphObject, float angle)
        {
            if (vertexMorphObject == null || vertexMorphObject.GetVertexCount() == 0)
            {
                return false;
            }
            List<Vector3> posDataArray = vertexMorphObject.GetVertexData();
            int vertexCount = GetTriangleCount() * 3;
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> nVertices = new List<Vector3>();
            vertices.AddRange(posDataArray);
            Vector3RotateCalculation(angle, vertices, nVertices, false);
            for (int i = 0; i < vertexCount; i += 3)
            {
                vertexMorphObject.SetVertex(i + 0, nVertices[i]);
                vertexMorphObject.SetVertex(i + 1, nVertices[i + 1]);
                vertexMorphObject.SetVertex(i + 2, nVertices[i + 2]);

                Vector3 normal = CalcPlaneNormal3f(nVertices[i], nVertices[i + 1], nVertices[i + 2]);
                vertexMorphObject.SetNormal(i + 0, normal);
                vertexMorphObject.SetNormal(i + 1, normal);
                vertexMorphObject.SetNormal(i + 2, normal);
            }
            vertices.Clear();
            nVertices.Clear();
            return true;
        }
        protected bool RotateSocketNodes(NullSocketNodes socketNodes, float angle)
        {
            if (socketNodes == null || socketNodes.GetSocketCount() == 0)
            {
                return false;
            }
            NullSocketNode rootSocketNode = socketNodes[0];
            if (rootSocketNode != null)
            {
                Vector3 pos = rootSocketNode.GetPosition();
                Quaternion quat = rootSocketNode.GetQuaternion();
                Vector3 newPos = Vector3.zero;
                Quaternion newQuat = Quaternion.identity;
                Vector3RotateCalculation(angle, pos, ref newPos, false);
                QuaternionRotateCalculation(angle, quat, ref newQuat);
                rootSocketNode.SetPosition(newPos);
                rootSocketNode.SetQuaternion(newQuat);
            }
            return true;
        }
        protected bool RotateNodeDummy(NullNodeDummy nodeDummy, float angle)
        {
            if (nodeDummy == null || nodeDummy.GetNodeCount() == 0)
            {
                return false;
            }
            for (int i = 0; i < nodeDummy.GetNodeCount(); i++)
            {
                NullNodeDummyObject nodeDummyObject = nodeDummy[i];
                Vector3 pos = nodeDummyObject.GetPosition();
                Vector3 newPos = Vector3.zero;
                Quaternion quat = nodeDummyObject.GetQuaternion();
                Quaternion newQuat = Quaternion.identity;
                Vector3RotateCalculation(angle, pos, ref newPos, false);
                QuaternionRotateCalculation(angle, quat, ref newQuat);
                nodeDummyObject.SetTransform(newPos.x, newPos.y, newPos.z, newQuat.x, newQuat.y, newQuat.z, newQuat.w);
            }

            return true;
        }
        protected bool RotateNodeTree(NullNodeTree nodeTree, float angle)
        {
            if (nodeTree == null || nodeTree.GetNodeCount() == 0)
            {
                return false;
            }
            NullNodeTree rootNode = nodeTree;
            if (rootNode != null)
            {
                Vector3 pos = rootNode.GetPosition();
                Vector3 newPos = Vector3.zero;
                Quaternion quat = rootNode.GetQuaternion();
                Quaternion newQuat = Quaternion.identity;
                Vector3RotateCalculation(angle, pos, ref newPos, false);
                QuaternionRotateCalculation(angle, quat, ref newQuat);
                rootNode.SetTransform(newPos.x, newPos.y, newPos.z, newQuat.x, newQuat.y, newQuat.z, newQuat.w);
            }
            return true;
        }
        protected bool RotateAnimation(NullSkeletonAnimation animation, float angle)
        {
            if (animation == null || animation.GetFrameCount() == 0 || angle == 0.0f)
            {
                return false;
            }
            //rotate root node only
            NullSkeletonNodeAnimation node = animation[0];
            List<Vector3> poses = node.GetPosition();
            List<Quaternion> quates = node.GetQuat();
            for (int i = 0; i < animation.GetFrameCount(); i++)
            {
                Vector3 newPos = Vector3.zero;
                Quaternion newQuat = Quaternion.identity;
                Vector3RotateCalculation(angle, poses[i], ref newPos, false);
                QuaternionRotateCalculation(angle, quates[i], ref newQuat);
                node.SetPosition(i, newPos);
                node.SetQuaternion(i, newQuat);
            }
            return true;
        }

    }
    public partial class NullMeshFile
    {
        public void SetWorkingFlag(NullWorkingFlag workingFlag)
        {
            InitializeWorkingFlag(workingFlag);
        }
        public NullWorkingFlag GetWorkingFlag()
        {
            return mWorkingMode;
        }
        public int GetMeshObjectCount()
        {
            return mMeshObjectList.GetMeshObjectCount();
        }
        public int GetSkinObjectCount()
        {
            return mSkinObjectList.GetMeshObjectCount();
        }
        public int GetStaticTriangleCount()
        {
            return mMeshObjectList.GetTriangleCount();
        }
        public int GetSkinTriangleCount()
        {
            return mSkinObjectList.GetTriangleCount();
        }
        public int GetVertexCount()
        {
            int count = 0;
            count += mMeshObjectList.GetVertexCount();
            count += mSkinObjectList.GetVertexCount();
            return count;
        }
        public int GetTriangleCount()
        {
            return GetStaticTriangleCount() + GetSkinTriangleCount();
        }
        public NullMeshObjects GetMeshObjects() { return mMeshObjectList; }
        public NullMeshObjects GetSkinObjects() { return mSkinObjectList; }
        public NullNodeTree GetNodeTree() { return mNodeTree; }
        public NullNodeDummy GetNodeDummy() { return mNodeDummy; }
        public NullSocketNodes GetSocketNodes() { return mSocketNodeList; }
        public NullSkeletonBinding GetSkeletonBinding() { return mSkeletonBinding; }
        public NullVertexMorphAnimations GetVertexMorphAnimations() { return mVertexMorphAnimations; }
        public NullSkeletonAnimations GetSkeletonAnimations() { return mSkeletonAnimations; }
        public int GetVersion() { return CurrentVersion; }
        public int GetSkeletonNodeCount() { return mNodeTree.GetNodeCount(); }
        public int GetDummyNodeCount() { return mNodeDummy.GetNodeCount(); }
        public bool StandarizeDefaultUVs(float errorLimit)
        {
            switch (GetWorkingFlag())
            {
                case NullWorkingFlag.WF_STATIC_MESH:
                case NullWorkingFlag.WF_SKELETON_MESHPIECE:
                    break;
                default:
                    return false;
            }
            bool res = true;
            for (int i = 0; i < GetMeshObjectCount(); i++)
            {
                res &= GetMeshObjects().GetMeshObjectByIndex(i).StandarizeDefaultUVs(errorLimit);
            }
            for (int i = 0; i < GetSkinObjectCount(); i++)
            {
                res &= GetSkinObjects().GetMeshObjectByIndex(i).StandarizeDefaultUVs(errorLimit);
            }
            return res;
        }
        public void RemoveUVGroup(UVType uvType)
        {
            mMeshObjectList.RemoveUVGroup(uvType);
            mSkinObjectList.RemoveUVGroup(uvType);
        }
    }

    public partial class NullMeshFile : INullStream
    {
        public const int MESH_FILE_VERSION = 100;
        public static uint MaterialCC = MakeFourCC("HXBM");
        private static uint StaticMesh = MakeFourCC("HXBO");
        private static uint SkeletonMesh = MakeFourCC("HXBS");
        private static uint SkeletonAnimation = MakeFourCC("HXBA");
        public static uint MakeFourCC(string four)
        {
            return BitConverter.ToUInt32(Encoding.UTF8.GetBytes(four), 0);
        }
        public int CurrentVersion;
        protected NullWorkingFlag mWorkingMode;
        protected int mBlockSize;
        //base mesh
        protected NullMeshObjects mMeshObjectList;
        protected NullMeshObjects mSkinObjectList;
        protected NullSocketNodes mSocketNodeList;
        protected NullNodeDummy mNodeDummy;
        //skeleton animation
        protected NullNodeTree mNodeTree;
        protected NullSkeletonBinding mSkeletonBinding;
        protected NullSkeletonAnimations mSkeletonAnimations;
        //ertex morph animation
        protected NullVertexMorphAnimations mVertexMorphAnimations;
        public NullMeshFile()
        {
            mWorkingMode = NullWorkingFlag.WF_AUTO_DETECT;
            CurrentVersion = MESH_FILE_VERSION;
            mBlockSize = 0;
            //base mesh
            mMeshObjectList = new NullMeshObjects();
            mSkinObjectList = new NullMeshObjects();
            //skeleton animation
            mNodeTree = new NullNodeTree();
            mSocketNodeList = new NullSocketNodes();
            mNodeDummy = new NullNodeDummy();
            mSkeletonBinding = new NullSkeletonBinding();
            mSkeletonAnimations = new NullSkeletonAnimations();
            //ertex morph animation
            mVertexMorphAnimations = new NullVertexMorphAnimations();

        }
        public NullMeshFile(NullWorkingFlag workingFlag) : this()
        {
            InitializeWorkingFlag(workingFlag);
        }
        protected void InitializeWorkingFlag(NullWorkingFlag workingFlag)
        {
            Clear();
            mWorkingMode = workingFlag;
            switch (mWorkingMode)
            {
                case NullWorkingFlag.WF_STATIC_MESH:
                    InitializeAsStaticMesh(MESH_FILE_VERSION);
                    break;
                case NullWorkingFlag.WF_SKELETON_MESHPIECE:
                    InitializeAsSkeletonMesh(MESH_FILE_VERSION);
                    break;
                case NullWorkingFlag.WF_NODE_ANIM:
                    InitializeAsSkeletonAnimation(MESH_FILE_VERSION);
                    break;
                default:
                    return;
            }
        }
        public int SaveToStream(NullMemoryStream stream)
        {
            uint foucc = GenerateFouCC();
            if (foucc == 0)
            {
                return 0;
            }
            CurrentVersion = MESH_FILE_VERSION;
            stream.WriteUInt(foucc);
            stream.WriteInt(mBlockSize);
            stream.WriteInt(CurrentVersion);
            int size = 0;
            switch (mWorkingMode)
            {
                case NullWorkingFlag.WF_STATIC_MESH:
                    size = SaveToStreamForStaticMesh(stream);
                    break;
                case NullWorkingFlag.WF_SKELETON_MESHPIECE:
                    size = SaveToStreamForSkeletonMesh(stream);
                    break;
                case NullWorkingFlag.WF_NODE_ANIM:
                    size = SaveToStreamForSkeletonAnimation(stream);
                    break;
                default:
                    return 0;
            }
            return size;
        }
        public int SaveToStreamForStaticMesh(NullMemoryStream stream)
        {
            int size = mMeshObjectList.SaveToStream(stream);
            size += mVertexMorphAnimations.SaveToStream(stream);
            return size;
        }
        public int SaveToStreamForSkeletonMesh(NullMemoryStream stream)
        {
            int size = mMeshObjectList.SaveToStream(stream);
            size += mSkinObjectList.SaveToStream(stream);
            size += mVertexMorphAnimations.SaveToStream(stream);
            size += mSocketNodeList.SaveToStream(stream);
            size += mNodeDummy.SaveToStream(stream);
            size += mNodeTree.SaveToStream(stream, false);
            size += mSkeletonBinding.SaveToStream(stream);
            return size;
        }
        public int SaveToStreamForSkeletonAnimation(NullMemoryStream stream)
        {
            int size = mNodeTree.SaveToStream(stream, true);
            size += mSocketNodeList.SaveToStream(stream);
            size += mNodeDummy.SaveToStream(stream);
            size += mSkeletonAnimations.SaveToStream(stream);
            return size;
        }
        public bool LoadFromStream(NullMemoryStream stream)
        {
            uint foucc = 0;
            bool res = stream.ReadUInt(out foucc);
            res &= stream.ReadInt(out mBlockSize);
            res &= stream.ReadInt(out CurrentVersion);
            if (!res || CurrentVersion > MESH_FILE_VERSION || !ValidateFileHeader(foucc, CurrentVersion))
            {
                return false;
            }
            switch (mWorkingMode)
            {
                case NullWorkingFlag.WF_STATIC_MESH:
                    res = LoadFromStreamForStaticMesh(stream);
                    break;
                case NullWorkingFlag.WF_SKELETON_MESHPIECE:
                    res = LoadFromStreamForSkeletonMesh(stream);
                    break;
                case NullWorkingFlag.WF_NODE_ANIM:
                    res = LoadFromStreamForSkeletonAnimation(stream);
                    break;
                default:
                    return false;
            }
            return true;
        }
        public uint GenerateFouCC()
        {
            uint fouCC = 0;
            switch (mWorkingMode)
            {
                case NullWorkingFlag.WF_STATIC_MESH:
                    fouCC = StaticMesh;
                    break;
                case NullWorkingFlag.WF_SKELETON_MESHPIECE:
                    fouCC = SkeletonMesh;
                    break;
                case NullWorkingFlag.WF_NODE_ANIM:
                    fouCC = SkeletonAnimation;
                    break;
            }
            return fouCC;
        }
        private bool LoadFromStreamForStaticMesh(NullMemoryStream stream)
        {
            bool res = mMeshObjectList.LoadFromStream(stream);
            res &= mVertexMorphAnimations.LoadFromStream(stream);
            return res;
        }
        private bool LoadFromStreamForSkeletonMesh(NullMemoryStream stream)
        {
            bool res = mMeshObjectList.LoadFromStream(stream);
            res &= mSkinObjectList.LoadFromStream(stream);
            res &= mVertexMorphAnimations.LoadFromStream(stream);
            res &= mSocketNodeList.LoadFromStream(stream);
            res &= mNodeDummy.LoadFromStream(stream);
            res &= mNodeTree.LoadFromStream(stream);
            res &= mSkeletonBinding.LoadFromStream(stream);
            return res;
        }
        private bool LoadFromStreamForSkeletonAnimation(NullMemoryStream stream)
        {
            bool res = mNodeTree.LoadFromStream(stream);
            res &= mSocketNodeList.LoadFromStream(stream);
            res &= mNodeDummy.LoadFromStream(stream);
            res &= mSkeletonAnimations.LoadFromStream(stream);
            if (res)
            {
                ResolveBoneNames();
            }
            return res;
        }
        private void ResolveBoneNames()
        {
            if (mNodeTree == null || mNodeTree.GetNodeCount() == 0 || mSkeletonAnimations == null)
            {
                return;
            }
            for (int i = 0; i < mSkeletonAnimations.GetAnimationCount(); i++)
            {
                NullSkeletonAnimation animation = mSkeletonAnimations[i];
                for (int j = 0; j < animation.GetNodeCount(); j++)
                {
                    NullSkeletonNodeAnimation node = animation[j];
                    {
                        int id = node.GetParent();
                        NullNodeTree bone = mNodeTree.FindNode(id);
                        if (bone != null)
                        {
                            node.SetBoneName(bone.GetNodeName());
                        }
                    }
                }
            }
        }
        private bool ValidateFileHeader(uint aType, int version)
        {
            if (aType == StaticMesh)
            {
                InitializeAsStaticMesh(version);
            }
            else if (aType == SkeletonMesh)
            {
                InitializeAsSkeletonMesh(version);
            }
            else if (aType == SkeletonAnimation)
            {
                InitializeAsSkeletonAnimation(version);
            }
            else
            {
                mWorkingMode = NullWorkingFlag.WF_AUTO_DETECT;
            }
            return true;
        }
        private void Clear()
        {
            CurrentVersion = MESH_FILE_VERSION;
            mBlockSize = 0;
            mMeshObjectList.Clear();
            mSkinObjectList.Clear();
            mSocketNodeList.Clear();
            mNodeDummy.Clear();
            mNodeTree.Clear();
            mSkeletonBinding.Clear();
            mSkeletonAnimations.Clear();
            mVertexMorphAnimations.Clear();
        }
        private void InitializeAsStaticMesh(int version)
        {
            Clear();
            mWorkingMode = NullWorkingFlag.WF_STATIC_MESH;
            CurrentVersion = version;
            mBlockSize = 0;
            //base mesh
            mMeshObjectList = new NullMeshObjects(CurrentVersion);
            mVertexMorphAnimations = new NullVertexMorphAnimations();
        }
        private void InitializeAsSkeletonMesh(int version)
        {
            Clear();
            mWorkingMode = NullWorkingFlag.WF_SKELETON_MESHPIECE;
            CurrentVersion = version;
            mBlockSize = 0;
            //base mesh
            mMeshObjectList = new NullMeshObjects(CurrentVersion);
            mSkinObjectList = new NullMeshObjects(CurrentVersion);
            mSocketNodeList = new NullSocketNodes();
            mNodeDummy = new NullNodeDummy();
            mNodeTree = new NullNodeTree(CurrentVersion);
            mSkeletonBinding = new NullSkeletonBinding(CurrentVersion);
            mVertexMorphAnimations = new NullVertexMorphAnimations();
        }
        private void InitializeAsSkeletonAnimation(int version)
        {
            Clear();
            mWorkingMode = NullWorkingFlag.WF_NODE_ANIM;
            CurrentVersion = version;
            mBlockSize = 0;
            mNodeTree = new NullNodeTree(CurrentVersion);
            mSocketNodeList = new NullSocketNodes();
            mNodeDummy = new NullNodeDummy();
            mSkeletonAnimations = new NullSkeletonAnimations(CurrentVersion);
        }
    }
}
