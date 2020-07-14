using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullMeshFileLoader
    {
        private NullMeshFile mMeshFile;
        private NullSkeletonOffsetFile mOffsetFile;
        private bool mLoadMaterial;
        private bool mLoadAnimation;
        private bool mForceUsingVertexColor;
        private bool mForceSoftwareSkin;
        private bool mIgnoreNormal;
        private bool mIgnoreVertexColor;
        private bool mIgnoreLightmap;
        private bool mIgnoreTangent;
        private bool mAutoBindSkeleton;
        private bool mLoadCollider;

    }
}
