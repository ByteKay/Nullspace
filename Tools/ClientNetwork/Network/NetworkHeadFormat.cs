
using System;

namespace Nullspace
{
    public class NetworkHead : NetworkMessage
    {
        public static int Size()
        {
            int size = 0;
            size += sizeof(int);
            size += sizeof(int);
            size += sizeof(int);
            size += sizeof(int);
            size += sizeof(long);
            size += sizeof(long);
            size += sizeof(long);
            size += sizeof(long);
            return size;
        }

        public int mType = 0;
        public int mLength = 0;
        public int mResult = 0;
        public int mSession = 0;
        public long mFrom = 0;
        public long mTo = 0;
        public long mMask = 0;
        public long mAddition = 0;

        public NetworkHead Clone()
        {
            NetworkHead head = new NetworkHead();
            head.mType = mType;
            head.mLength = mLength;
            head.mResult = mResult;
            head.mFrom = mFrom;
            head.mTo = mTo;
            head.mSession = mSession;
            head.mAddition = mAddition;
            head.mMask = mMask;
            mSendBytes.Clear();
            return head;
        }

        public void CopyFrom(NetworkHead other)
        {
            mSendBytes.Clear();
            mType = other.mType;
            mLength = other.mLength;
            mResult = other.mResult;
            mFrom = other.mFrom;
            mTo = other.mTo;
            mSession = other.mSession;
            mAddition = other.mAddition;
            mMask = other.mMask;
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

    }
}


