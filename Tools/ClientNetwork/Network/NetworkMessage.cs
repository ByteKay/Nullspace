using System;
using System.Collections.Generic;
using System.Text;

namespace Nullspace
{
    public abstract class NetworkMessage
    {
        protected byte[] mCacheBytes;
        private int mReadIndex = 0;
        protected List<byte> mSendBytes;

        public NetworkMessage()
        {
            mCacheBytes = null;
            mSendBytes = new List<byte>();
        }

        public abstract void ReadPacket();
        public abstract void WritePacket();

        public virtual void InitializeBy(byte[] bytes)
        {
            mReadIndex = 0;
            mSendBytes.Clear();
            mCacheBytes = bytes;
            ReadPacket();
        }

        public void WriteLength()
        {
            byte[] size = BitConverter.GetBytes(mSendBytes.Count);
            mSendBytes.InsertRange(0, size);
        }

        public short ReadInt16()
        {
            if (CanRead(sizeof(short)))
            {
                short value = BitConverter.ToInt16(mCacheBytes, mReadIndex);
                mReadIndex += sizeof(short);
                return value;
            }
            return short.MaxValue;
        }

        public ushort ReadUInt16()
        {
            if (CanRead(sizeof(ushort)))
            {
                ushort value = BitConverter.ToUInt16(mCacheBytes, mReadIndex);
                mReadIndex += sizeof(ushort);
                return value;
            }
            return ushort.MaxValue;
        }

        public void WriteUInt16(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            mSendBytes.AddRange(bytes);
        }

        public void WriteInt16(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            mSendBytes.AddRange(bytes);
        }

        public int ReadInt32()
        {
            if (CanRead(sizeof(int)))
            {
                int value = BitConverter.ToInt32(mCacheBytes, mReadIndex);
                mReadIndex += sizeof(int);
                return value;
            }
            return int.MaxValue;
        }

        public void WriteInt32(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            mSendBytes.AddRange(bytes);
        }

        public long ReadInt64()
        {
            if (CanRead(sizeof(long)))
            {
                long value = BitConverter.ToInt64(mCacheBytes, mReadIndex);
                mReadIndex += sizeof(long);
                return value;
            }
            return long.MaxValue;
        }

        public void WriteInt64(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            mSendBytes.AddRange(bytes);
        }

        public void WriteBoolean(bool value)
        {
            byte b = (byte)(value ? 1 : 0);
            mSendBytes.Add(b);
        }

        public bool ReadBoolean()
        {
            if (CanRead(sizeof(bool)))
            {
                bool value = BitConverter.ToBoolean(mCacheBytes, mReadIndex);
                mReadIndex += sizeof(bool);
                return value;
            }
            return false;
        }

        public float ReadFloat()
        {
            if (CanRead(sizeof(float)))
            {
                float value = BitConverter.ToSingle(mCacheBytes, mReadIndex);
                mReadIndex += sizeof(float);
                return value;
            }
            return float.MaxValue;
        }

        public void WriteFloat(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            mSendBytes.AddRange(bytes);
        }

        public double ReadDouble()
        {
            if (CanRead(sizeof(double)))
            {
                double value = BitConverter.ToDouble(mCacheBytes, mReadIndex);
                mReadIndex += sizeof(double);
                return value;
            }
            return double.MaxValue;
        }

        public void WriteDouble(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            mSendBytes.AddRange(bytes);
        }

        public string ReadWString()
        {
            int len = ReadInt32();
            if (len != int.MaxValue)
            {
                byte[] bytes = ReadBytes(len);
                if (bytes != null)
                {
                    return Encoding.Unicode.GetString(bytes);
                }
            }
            return null;
        }

        public string ReadString()
        {
            int len = ReadInt32();
            if (len != int.MaxValue)
            {
                byte[] bytes = ReadBytes(len);
                if (bytes != null)
                {
                    return Encoding.UTF8.GetString(bytes);
                }
            }
            return null;
        }

        public byte[] ReadBytes(int len)
        {
            if (CanRead(len))
            {
                byte[] bytes = new byte[len];
                Array.Copy(mCacheBytes, mReadIndex, bytes, 0, len);
                mReadIndex += len;
                return bytes;
            }
            return null;
        }

        public void WriteBytes(byte[] bytes)
        {
            mSendBytes.AddRange(bytes);
        }

        public void WriteString(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteInt32(bytes.Length);
            foreach (byte b in bytes)
            {
                mSendBytes.Add(b);
            }
        }

        public void WriteWString(string value)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(value);
            WriteInt32(bytes.Length);
            mSendBytes.AddRange(bytes);
        }

        public bool CanRead(int len)
        {
            if (mReadIndex + len <= mCacheBytes.Length)
            {
                return true;
            }
            return false;
        }

    }
}



