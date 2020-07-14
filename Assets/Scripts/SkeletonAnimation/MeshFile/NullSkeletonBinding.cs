using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public class NullSkeletonBindingNode : INullStream
    {
        public class NullNodeWeight : INullStream
        {
            public uint Target;
            public float Weight;

            public bool LoadFromStream(NullMemoryStream stream)
            {
                bool res = stream.ReadUInt(out Target);
                res &= stream.ReadFloat(out Weight);
                return res;
            }

            public int SaveToStream(NullMemoryStream stream)
            {
                int size = stream.WriteUInt(Target);
                size += stream.WriteFloat(Weight);
                return size;
            }
        }

        protected ushort CurrentVersion;
        protected byte WeightCount;       
        protected List<NullNodeWeight> NodeWeightArray;

        public NullSkeletonBindingNode(ushort version)
        {
            CurrentVersion = version;
            WeightCount = 0;
            NodeWeightArray = null;
        }

        public void StandarizeWeights()
        {
            float totalWeight = 0.0f;
            for (int i = 0; i < WeightCount; i++)
            {
                totalWeight += NodeWeightArray[i].Weight;
            }
            if (totalWeight < 1e-4f)
            {
                totalWeight = 1e-4f;
            }
            for (int i = 0; i < WeightCount; i++)
            {
                NodeWeightArray[i].Weight = NodeWeightArray[i].Weight / totalWeight;
            }
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteByte(WeightCount);
            for (int i = 0; i < WeightCount; i++)
            {
                size += NodeWeightArray[i].SaveToStream(stream);
            }
            return size;
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            byte count = 0;
            bool res = stream.ReadByte(out count);
            SetWeightCount(count);
            for (int i = 0; i < WeightCount; i++)
            {
                res &= NodeWeightArray[i].LoadFromStream(stream);
            }
            return res;
        }

        public bool SetWeightCount(byte cnt)
        {
            WeightCount = cnt;
            if (WeightCount == 0)
            {
                return false;
            }
            if (NodeWeightArray == null)
            {
                NodeWeightArray = new List<NullNodeWeight>();
            }
            for (int i = 0; i < WeightCount; ++i)
            {
                NodeWeightArray.Add(new NullNodeWeight());
            }
            return true;
        }

        public void Clear()
        {
            NodeWeightArray = null;
            WeightCount = 0;
        }
    }

    public class NullSkeletonPiece : INullStream
    {
        protected ushort CurrentVersion;
		protected uint PieceHandle;
        protected ushort BindingNodeCount;
        protected List<NullSkeletonBindingNode> BindingNodeArray;

        public NullSkeletonPiece(ushort version)
        {
            CurrentVersion = version;
            PieceHandle = 0;
            BindingNodeCount = 0;
            BindingNodeArray = null;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            int size = stream.WriteUShort(BindingNodeCount);
            size += stream.WriteUInt(PieceHandle);
            for (int i = 0; i < BindingNodeCount; i++)
            {
                size += BindingNodeArray[i].SaveToStream(stream);
            }
            return size;
        }

        public void StandarizeWeights()
        {
            for (int i = 0; i < BindingNodeCount; i++)
            {
                BindingNodeArray[i].StandarizeWeights();
            }
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(out count);
            res &= stream.ReadUInt(out PieceHandle);
            SetSkeletonBindingNodeCount(count);
            for (int i = 0; i < BindingNodeCount; i++)
            {
                res &= BindingNodeArray[i].LoadFromStream(stream);
            }
            return res;
        }

        public bool SetSkeletonBindingNodeCount(ushort count)
        {
            Clear();
            BindingNodeCount = count;
            if (BindingNodeCount == 0)
            {
                return false;
            }
            BindingNodeArray = new List<NullSkeletonBindingNode>(BindingNodeCount);
            for (int i = 0; i < BindingNodeCount; i++)
            {
                BindingNodeArray[i] = new NullSkeletonBindingNode(CurrentVersion);
            }
            return true;

        }

        public void Clear()
        {
            for (int i = 0; i < BindingNodeCount; i++)
            {
                BindingNodeArray[i].Clear();
            }

            BindingNodeArray = null;
            BindingNodeCount = 0;
            PieceHandle = 0;
        }
    }
    
    public class NullSkeletonBinding : INullStream
    {
        protected ushort CurrentVersion;
        protected string SkeletonName;
        protected ushort BindingPieceCount;
        protected List<NullSkeletonPiece> BindingPieceNodeArray;

        public NullSkeletonBinding(ushort version)
        {
            BindingPieceCount = 0;
            BindingPieceNodeArray = null;
            SkeletonName = null;
            CurrentVersion = version;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            CurrentVersion = NullMeshFile.MESH_FILE_VERSION;
            StandarizeWeights();
            int size = stream.WriteUShort(BindingPieceCount);
            size += stream.WriteString(SkeletonName);
            for (int i = 0; i < BindingPieceCount; i++)
            {
                size += BindingPieceNodeArray[i].SaveToStream(stream);
            }
            return size;
        }

        public void StandarizeWeights()
        {
            for (int i = 0; i < BindingPieceCount; i++)
            {
                BindingPieceNodeArray[i].StandarizeWeights();
            }
        }

        public bool LoadFromStream(NullMemoryStream stream)
        {
            Clear();
            ushort count = 0;
            bool res = stream.ReadUShort(out count);
            res &= stream.ReadString(out SkeletonName);
            SetSkeletonBindingCount(count);
            for (int i = 0; i < BindingPieceCount; i++)
            {
                res &= BindingPieceNodeArray[i].LoadFromStream(stream);
            }
            return res;
        }

        public bool SetSkeletonBindingCount(ushort count)
        {
            Clear();
            BindingPieceCount = count;
            if (BindingPieceCount == 0)
            {
                return false;
            }
            BindingPieceNodeArray = new List<NullSkeletonPiece>(BindingPieceCount);
            for (int i = 0; i < BindingPieceCount; i++)
            {
                BindingPieceNodeArray[i] = new NullSkeletonPiece(CurrentVersion);
            }
            return true;
        }

        public void Clear()
        {
            BindingPieceNodeArray = null;
            BindingPieceCount = 0;
        }
    }
}
