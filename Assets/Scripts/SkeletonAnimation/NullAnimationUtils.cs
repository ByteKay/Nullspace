using NullMesh;
using System;
using System.Collections;
using UnityEngine;

namespace NullAnimation
{
    public class NullAnimationUtils
    {
        //public static void ConvertSkinMeshRender(GameObject go)
        //{
            
        //    SkinnedMeshRenderer skinMesh = go.GetComponentInChildren<SkinnedMeshRenderer>();
        //    Animation animation = go.GetComponentInChildren<Animation>();
        //    if (skinMesh != null)
        //    {
        //        NullMeshFile meshFile = new NullMeshFile(NullWorkingFlag.WF_SKELETON_MESHPIECE);
        //        Mesh mesh = skinMesh.sharedMesh;
        //        int count = mesh.subMeshCount;

        //        Vector3[] vertices = mesh.vertices;
        //        for (int i = 0; i < count; ++i)
        //        {
        //        }
        //        Transform[] bones = skinMesh.bones;
        //        Transform rootBone = skinMesh.rootBone;
        //    }
        //    if (animation != null)
        //    {
        //        NullMeshFile meshFile = new NullMeshFile(NullWorkingFlag.WF_NODE_ANIM);
        //        IEnumerator enumerator = animation.GetEnumerator();
        //        while (enumerator.MoveNext())
        //        {
        //            object obj = enumerator.Current;
        //        }
        //    }
        //}
    }
}
