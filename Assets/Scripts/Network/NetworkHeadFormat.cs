
using System;

namespace Nullspace
{
    public class NetworkHeadFormat : NetworkMessage
    {
        public int mType = 0;
        public int mLength = 0;
        public int mResult = 0;
        public int mSession = 0;
        public Int64 mFrom = 0;
        public Int64 mTo = 0;
        public Int64 mMask = 0;
        public Int64 mAddition = 0;


        public bool mIsInitialized = false;

        public static int Size()
        {
            int size = 0;
            size += sizeof(int);
            size += sizeof(int);
            size += sizeof(int);
            size += sizeof(int);
            size += sizeof(Int64);
            size += sizeof(Int64);
            size += sizeof(Int64);
            size += sizeof(Int64);
            return size;
        }

        public NetworkHeadFormat Clone()
        {
            NetworkHeadFormat head = new NetworkHeadFormat();
            head.mType = mType;
            head.mLength = mLength;
            head.mResult = mResult;
            head.mFrom = mFrom;
            head.mTo = mTo;
            head.mSession = mSession;
            head.mAddition = mAddition;
            head.mMask = mMask;
            mIsInitialized = true;
            mSendBytes.Clear();
            return head;
        }

        public void CopyFrom(NetworkHeadFormat other)
        {
            mSendBytes.Clear();
            this.mType = other.mType;
            this.mLength = other.mLength;
            this.mResult = other.mResult;
            this.mFrom = other.mFrom;
            this.mTo = other.mTo;
            this.mSession = other.mSession;
            this.mAddition = other.mAddition;
            this.mMask = other.mMask;
            mIsInitialized = true;
        }

        public virtual byte[] ToBytes()
        {
            mSendBytes.Clear();
            WritePacket();
            return mSendBytes.ToArray();
        }

        public byte[] Merge(byte[] other)
        {
            if (other != null && other.Length != 0)
            {
                mLength = other.Length;
                byte[] me = ToBytes();
                byte[] bytes = new byte[me.Length + other.Length];
                Array.Copy(me, 0, bytes, 0, me.Length);
                Array.Copy(other, 0, bytes, me.Length, other.Length);
                return bytes;
            }
            else
            {
                mLength = 0;
                return ToBytes();
            }
        }

        public override void ReadPacket()
        {
            mType = ReadInt32();
            mLength = ReadInt32();
            mResult = ReadInt32();
            mSession = ReadInt32();

            mFrom = ReadInt64();
            mTo = ReadInt64();
            mMask = ReadInt64();
            mAddition = ReadInt64();

            mType = System.Net.IPAddress.NetworkToHostOrder(mType);
            mLength = System.Net.IPAddress.NetworkToHostOrder(mLength);
            mResult = System.Net.IPAddress.NetworkToHostOrder(mResult);
            mFrom = System.Net.IPAddress.NetworkToHostOrder(mFrom);
            mTo = System.Net.IPAddress.NetworkToHostOrder(mTo);
            mSession = System.Net.IPAddress.NetworkToHostOrder(mSession);
            mAddition = System.Net.IPAddress.NetworkToHostOrder(mAddition);
            mMask = System.Net.IPAddress.NetworkToHostOrder(mMask);
            mIsInitialized = true;
        }

        public override void WritePacket()
        {
            WriteInt32(System.Net.IPAddress.HostToNetworkOrder(mType));
            WriteInt32(System.Net.IPAddress.HostToNetworkOrder(mLength));
            WriteInt32(System.Net.IPAddress.HostToNetworkOrder(mResult));
            WriteInt32(System.Net.IPAddress.HostToNetworkOrder(mSession));

            WriteInt64(System.Net.IPAddress.HostToNetworkOrder(mFrom));
            WriteInt64(System.Net.IPAddress.HostToNetworkOrder(mTo));
            WriteInt64(System.Net.IPAddress.HostToNetworkOrder(mMask));
            WriteInt64(System.Net.IPAddress.HostToNetworkOrder(mAddition));
        }

        public override void Initialize()
        {
            mIsInitialized = false;
        }

    }
}


