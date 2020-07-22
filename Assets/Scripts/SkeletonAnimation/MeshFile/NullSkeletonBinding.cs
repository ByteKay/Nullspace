using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Assertions;

namespace NullMesh
{
    public class NullSkeletonBindingNode : INullStream
    {
        public class NullNodeWeight : INullStream
        {
            public int Target;
            public float Weight;

            public NullNodeWeight()
            {
                Target = 0;
                Weight = 0;
            }

            public NullNodeWeight(int target, float weight)
            {
                Target = target;
                Weight = weight;
            }

            public bool LoadFromStream(NullMemoryStream stream)
            {
                bool res = stream.ReadInt(out Target);
                res &= stream.ReadFloat(out Weight);
                return res;
            }

            public int SaveToStream(NullMemoryStream stream)
            {
                int size = stream.WriteInt(Target);
                size += stream.WriteFloat(Weight);
                return size;
            }
        }

        public int CurrentVersion;     
        protected List<NullNodeWeight> mNodeWeightArray;

        public NullSkeletonBindingNode()
        {
            mNodeWeightArray = new List<NullNodeWeight>();
        }

        public NullSkeletonBindingNode(int version) : this()
        {
            CurrentVersion = version;
        }

        public bool SetWeightCount(int weightCount)
        {
            Clear();
            if (weightCount == 0)
            {
                return false;
            }
            for (int i = 0; i < weightCount; i++)
            {
                mNodeWeightArray.Add(new NullNodeWeight());
            }
            return true;
        }

        public NullNodeWeight AppendWeightNode(int target, float weight)
        {
            for (int i = 0; i < mNodeWeightArray.Count; i++)
            {
                if (mNodeWeightArray[i].Target == target)
                {
                    mNodeWeightArray[i].Weight += weight;
                    return mNodeWeightArray[i];
                }
            }

            NullNodeWeight nodeWeight = new NullNodeWeight(target, weight);
            mNodeWeightArray.Add(nodeWeight);
            return nodeWeight;
        }

        public void StandarizeWeights()
        {
            float totalWeight = 0.0f;
            for (int i = 0; i < mNodeWeightArray.Count; i++)
            {
                totalWeight += mNodeWeightArray[i].Weight;
            }
            if (totalWeight < 1e-4f)
            {
                totalWeight = 1e-4f;
            }
            for (int i = 0; i < mNodeWeightArray.Count; i++)
            {
                mNodeWeightArray[i].Weight = mNodeWeightArray[i].Weight / totalWeight;
            }
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteList(mNodeWeightArray, false);
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            return stream.ReadList(out mNodeWeightArray);
        }

        public void Clear()
        {
            mNodeWeightArray.Clear();
        }
    }

    public class NullSkeletonPiece : INullStream
    {
        public int CurrentVersion;
		protected int mPieceHandle;
        protected List<NullSkeletonBindingNode> mBindingNodeArray;

        public NullSkeletonPiece()
        {
            mPieceHandle = 0;
            mBindingNodeArray = new List<NullSkeletonBindingNode>();
        }

        public NullSkeletonPiece(int version) : this()
        {
            CurrentVersion = version;
        }

        public NullSkeletonBindingNode this[int idx]
        {
            get
            {
                Assert.IsTrue(idx < mBindingNodeArray.Count, "");
                return mBindingNodeArray[idx];
            }
        }


        public void SetPieceHandle(int handle)
        {
            mPieceHandle = handle;
        }

        public int GetPieceHandle()
        {
            return mPieceHandle;
        }

        public void SetNodeZero(int index)
        {
            if (index < mBindingNodeArray.Count)
            {
                mBindingNodeArray[index] = null;
            }
        }

        public bool SetSkeletonBindingNodeCount(int count)
        {
            Clear();
            for (int i = 0; i < count; i++)
            {
                mBindingNodeArray.Add(new NullSkeletonBindingNode(CurrentVersion));
            }
            return true;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteInt(mPieceHandle);
            size += stream.WriteList(mBindingNodeArray, false);
            return size;
        }

        public void StandarizeWeights()
        {
            for (int i = 0; i < mBindingNodeArray.Count; i++)
            {
                mBindingNodeArray[i].StandarizeWeights();
            }
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadInt(out mPieceHandle);
            res &= stream.ReadList(out mBindingNodeArray);
            return res;
        }

        public void Clear()
        {
            for (int i = 0; i < mBindingNodeArray.Count; i++)
            {
                if (mBindingNodeArray[i] != null)
                {
                    mBindingNodeArray[i].Clear();
                }
            }
            mBindingNodeArray.Clear();
            mPieceHandle = 0;
        }
    }
    
    public class NullSkeletonBinding : INullStream
    {
        public int CurrentVersion;
        protected string mSkeletonName;
        protected List<NullSkeletonPiece> mBindingPieceNodeArray;

        public NullSkeletonBinding()
        {
            mBindingPieceNodeArray =  new List<NullSkeletonPiece>();
            mSkeletonName = "";
        }

        public NullSkeletonBinding(int version) : this()
        {
            CurrentVersion = version;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            StandarizeWeights();
            int size = stream.WriteString(mSkeletonName);
            size += stream.WriteList(mBindingPieceNodeArray, false);
            return size;
        }

        public void StandarizeWeights()
        {
            for (int i = 0; i < mBindingPieceNodeArray.Count; i++)
            {
                mBindingPieceNodeArray[i].StandarizeWeights();
            }
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            bool res = stream.ReadString(out mSkeletonName);
            res &= stream.ReadList(out mBindingPieceNodeArray);
            return res;
        }

        public void SetPieceZero(int index)
        {
            if (mBindingPieceNodeArray[index] != null)
            {
                mBindingPieceNodeArray[index].Clear();
                mBindingPieceNodeArray[index] = null;
            }
        }

        public void RefreshSkeletonPiece()
        {
            mBindingPieceNodeArray.RemoveAll((piece) => { return piece == null; });
        }

        public string GetSkeletonName()
        {
            return mSkeletonName;
        }

        public void SetSkeletonName(string name)
        {
            mSkeletonName = name;
        }

        public NullSkeletonPiece AppendSkeletonPiece()
        {
            NullSkeletonPiece piece = new NullSkeletonPiece(CurrentVersion);
            mBindingPieceNodeArray.Add(piece);
            return piece;
        }

        public bool ExchangeSkeletonPiece(int first, int second)
        {
            if ((first >= GetSkeletonBindingCount()) || (second >= GetSkeletonBindingCount()))
            {
                return false;
            }
            NullSkeletonPiece tmp = mBindingPieceNodeArray[first];
            mBindingPieceNodeArray[first] = mBindingPieceNodeArray[second];
            mBindingPieceNodeArray[second] = tmp;
            return true;
        }

        public bool SetSkeletonBindingCount(int count)
        {
            Clear();
            for (int i = 0; i < count; i++)
            {
                mBindingPieceNodeArray.Add(new NullSkeletonPiece(CurrentVersion));
            }
            return true;
        }

        public int GetSkeletonBindingCount()
        {
            return mBindingPieceNodeArray.Count;
        }

        public void Clear()
        {
            mBindingPieceNodeArray.Clear();
        }
    }
}
