using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public class TangentEquals
    {
        public int index;
        public List<int> equalOnes;

        public TangentEquals()
        {
            index = 0;
            equalOnes = new List<int>();
        }
        public TangentEquals(int idx) : this()
        {
            index = idx;
        }

        public void Normalize(List<Vector3> facetTangents, List<Vector3> tangents, List<Vector3> facetBinormals, List<Vector3> binormals)
        {
            Vector3 tangent = facetTangents[index / 3];
            //do normalize
            for (int i = 0; i < equalOnes.Count; i++)
            {
                int idx = equalOnes[i] / 3;
                tangent += facetTangents[idx];
            }
            tangent.Normalize();
            tangents[index] = tangent;

            for (int i = 0; i < equalOnes.Count; i++)
            {
                int idx = equalOnes[i];
                tangents[idx] = tangent;
            }

            Vector3 binormal = facetBinormals[index / 3];
            //do normalize
            for (int i = 0; i < equalOnes.Count; i++)
            {
                int idx = equalOnes[i] / 3;
                binormal += facetBinormals[idx];
            }
            binormal.Normalize();
            binormals[index] = binormal;
            for (int i = 0; i < equalOnes.Count; i++)
            {
                int idx = equalOnes[i];
                binormals[idx] = binormal;
            }
        }
    }


    public class NullVertexStruct
    {
        public Vector3 vertex;
        public Vector3 normal;
        public Vector3 tangent;
        public Vector3 binormal;
        public List<Vector2> uvLst;
        public uint color;

        public bool hadColor;
        public bool hadTangent;

        public NullVertexStruct()
        {
            uvLst = new List<Vector2>();
        }

        public bool IsEquals(NullVertexStruct source)
        {
            if (normal != source.normal)
            {
                return false;
            }
            if (hadTangent && (tangent != source.tangent || binormal != source.binormal))
            {
                return false;
            }
            if (hadColor && (color != source.color))
            {
                return false;
            }
            if (vertex != source.vertex)
            {
                return false;
            }
            if (uvLst.Count != source.uvLst.Count)
            {
                return false;
            }
            for (int i = 0; i < uvLst.Count; i++)
            {
                if (uvLst[i] != source.uvLst[i])
                {
                    return false;
                }
            }
            return true;
        }

    }

    public class NullMergeIndex
    {
        public int index;
        public List<int> equalOnes;

        public NullMergeIndex()
        {
            index = -1;
            equalOnes = new List<int>();
        }

        public NullMergeIndex(int idx) : this()
        {
            index = idx;
        }
    }
}
