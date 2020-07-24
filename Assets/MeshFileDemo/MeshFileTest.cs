using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace NullMesh
{
    public class MeshFileTest : MonoBehaviour
    {
        private string OutPutDir;

        private GameObject Target;
        private Mesh mMesh;
        private SkinnedMeshRenderer mSkinMeshRender;
        private Animation mAnim;

        private Transform mRootBone;
        private Transform[] mBones;
        private Matrix4x4[] mBindposes;
        private BoneWeight[] mBoneweights;

        private Vector3[] mVertexPoses;
        private int[] mVertexTriangles;
        private Color32[] mVertexColors;
        private Vector3[] mVertexNormals;
        private Vector4[] mVertexTangents;
        private Vector2[] mVertexuvs;

        // Use this for initialization
        void Start()
        {
            OutPutDir = Application.dataPath + "/MeshFileDemo";
            Target = gameObject;

            mSkinMeshRender = Target.GetComponentInChildren<SkinnedMeshRenderer>();
            mMesh = mSkinMeshRender.sharedMesh;

            mBones = mSkinMeshRender.bones;
            mRootBone = mSkinMeshRender.rootBone;

            mBindposes = mMesh.bindposes;
            mBoneweights = mMesh.boneWeights;
            mVertexColors = mMesh.colors32;
            mVertexNormals = mMesh.normals;
            mVertexTangents = mMesh.tangents;
            mVertexuvs = mMesh.uv;
            mVertexPoses = mMesh.vertices;
            mVertexTriangles = mMesh.triangles;

            mAnim = Target.GetComponent<Animation>();
            
            CreateSkeletonMesh();
            CreateSkeletonAnimation();
        }

        private void CreateSkeletonAnimation()
        {
            if (mAnim != null)
            {
                foreach (AnimationState anim in mAnim)
                {
                    AnimationClip clip = anim.clip;
                    EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
                    foreach (EditorCurveBinding binding in bindings)
                    {
                        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                        Keyframe[] keys =  curve.keys;
                        //binding.path;
                        //binding.propertyName;
                    }
                }
            }
        }

        private void CreateSkeletonMesh()
        {
            NullMeshFile meshFile = new NullMeshFile(NullWorkingFlag.WF_SKELETON_MESHPIECE);

            bool hadNormals = mVertexNormals.Length > 0;
            bool hadTangents = mVertexTangents.Length > 0;
            bool hadColors = mVertexColors.Length > 0;
            bool hadUv = mVertexuvs.Length > 0;

            NullMeshObject meshObject = meshFile.AppendSkinObject(NullPrimitiveType.MOT_INDEXED_PRIMITIVES, mVertexTriangles.Length, mVertexPoses.Length, true, true, true);
            NullUVGroup uvGroup = hadUv ? meshObject.GetOrCreateUVGroup(UVType.UVT_DEFAULT, mVertexPoses.Length) : null;
            int vertexCount = mVertexPoses.Length;
            for (int i = 0; i < vertexCount; ++i)
            {
                meshObject.SetVertex(i, mVertexPoses[i]);
                if (hadColors)
                {
                    meshObject.SetVertexColor(i, Convert(mVertexColors[i]));
                }
                if (hadTangents)
                {
                    meshObject.SetTangent(i, mVertexTangents[i]);
                }
                if (hadNormals)
                {
                    meshObject.SetNormal(i, mVertexNormals[i]);
                }
                if (hadUv)
                {
                    uvGroup.SetUV(i, mVertexuvs[i]);
                }
            }

            for (int i = 0; i < mVertexTriangles.Length; i += 3)
            {
                int i1 = mVertexTriangles[i];
                int i2 = mVertexTriangles[i + 1];
                int i3 = mVertexTriangles[i + 2];
                meshObject.SetTriangleIndex(i, new Vector3Int(i1, i2, i3));
            }

            // 骨骼数
            NullNodeTree rootNode = meshFile.GetNodeTree();
            ConvertNodeTree(mRootBone, rootNode, mBindposes, mBones);

            // 权重绑定
            NullSkeletonBinding skeletonBinding = meshFile.GetSkeletonBinding();
            int pieceCount = 1; // 对应子模型，现在只限制一个子模型
            skeletonBinding.SetSkeletonName(mMesh.name);
            skeletonBinding.SetSkeletonBindingCount(pieceCount);
            for (int i = 0; i < pieceCount; ++i)
            {
                NullSkeletonPiece skeletonPiece = skeletonBinding[i];
                skeletonPiece.SetPieceHandle(i);
                ConvertSkeletonPiece(skeletonPiece, mBoneweights);
            }

            string fileName = OutPutDir + "/" + Target.name + ".hxs";
            using (NullMemoryStream stream = NullMemoryStream.WriteToFile(fileName))
            {
                meshFile.SaveToStream(stream);
            }
        }

        private void ConvertSkeletonPiece(NullSkeletonPiece skeletonBinding, BoneWeight[] boneweights)
        {
            skeletonBinding.SetSkeletonBindingNodeCount(boneweights.Length);
            for (int i = 0; i < boneweights.Length; ++i)
            {
                skeletonBinding[i].AppendWeightNode(boneweights[i].boneIndex0, boneweights[i].weight0);
                skeletonBinding[i].AppendWeightNode(boneweights[i].boneIndex1, boneweights[i].weight1);
                skeletonBinding[i].AppendWeightNode(boneweights[i].boneIndex2, boneweights[i].weight2);
                skeletonBinding[i].AppendWeightNode(boneweights[i].boneIndex3, boneweights[i].weight3);
            }
        }

        private void ConvertNodeTree(Transform bone, NullNodeTree nodeTree, Matrix4x4[] bindposes, Transform[] bones)
        {
            int index = GetBoneIndex(bones, bone);
            if (index == -1)
            {
                return ;
            }
            nodeTree.SetNodeHandle(index);
            nodeTree.SetNodeName(bone.name);
            nodeTree.SetPosition(bindposes[index].GetColumn(3));
            nodeTree.SetQuaternion(bindposes[index].rotation);
            int childCount = bone.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                Transform child = bone.GetChild(i);
                NullNodeTree childNode = new NullNodeTree();
                childNode.SetNodeHandle(-1);
                ConvertNodeTree(child, childNode, bindposes, bones);
                if (childNode.GetNodeHandle() != -1)
                {
                    nodeTree.Children.Add(childNode);
                }
            }
        }

        private int GetBoneIndex(Transform[] bones, Transform bone)
        {
            int index = -1;
            for (int i = 0; i < bones.Length; ++i)
            {
                if (bones[i].GetInstanceID() == bone.GetInstanceID())
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private uint Convert(Color32 clr)
        {
            return (uint)(clr.r << 24 | clr.g << 16 | clr.b << 8 | clr.a);
        }
    }
    
}

