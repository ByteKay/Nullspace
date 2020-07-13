using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MeshFile
{
    /// <summary>
    /// 待完善
    /// 
    /// float -> ushort (65536)
    /// 1. normal 压缩
    /// 2. uv 压缩
    /// 3. quaternion 压缩
    /// 
    /// int -> ushort
    /// 1. < 65536
    /// 
    /// bool list -> byte array
    /// 1. bool -> 1bit
    /// 
    /// </summary>
    public class SimpleMemoryStream : IDisposable
    {
        private const int CacheLen = 1024 * 1024;
        private static byte[] CacheBytes = new byte[CacheLen];

        public static SimpleMemoryStream ReadFromFile(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            SimpleMemoryStream stream = new SimpleMemoryStream(fileStream);
            return stream;
        }

        public static SimpleMemoryStream ReadFromBytes(byte[] bytes)
        {
            MemoryStream memoryStream = new MemoryStream(bytes);
            SimpleMemoryStream stream = new SimpleMemoryStream(memoryStream);
            return stream;
        }

        public static SimpleMemoryStream WriteToFile(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            SimpleMemoryStream stream = new SimpleMemoryStream(fileStream);
            return stream;
        }

        private Stream mStream;

        private SimpleMemoryStream(Stream stream)
        {
            mStream = stream;
            mStream.Seek(0, SeekOrigin.Begin);
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (mStream != null)
            {
                mStream.Close();
                mStream = null;
            }
        }

        private bool CanRead(int size)
        {
            return mStream.Length - mStream.Position <= size;
        }

        public byte[] ReadBytes(int count)
        {
            System.Diagnostics.Debug.Assert(CanRead(count));
            byte[] bytes = CacheBytes;
            if (CacheLen > count)
            {
                bytes = new byte[count];
            }
            int size = mStream.Read(bytes, 0, count);
            System.Diagnostics.Debug.Assert(size == count);
            return bytes;
        }

        public byte ReadByte()
        {
            byte[] bytes = ReadBytes(sizeof(byte));
            return bytes[0];
        }

        public bool ReadBool()
        {
            byte[] bytes = ReadBytes(sizeof(bool));
            return BitConverter.ToBoolean(bytes, 0);
        }

        public float ReadFloat()
        {
            byte[] bytes = ReadBytes(sizeof(float));
            return BitConverter.ToSingle(bytes, 0);
        }

        public short ReadShort()
        {
            byte[] bytes = ReadBytes(sizeof(short));
            return BitConverter.ToInt16(bytes, 0);
        }

        public int ReadInt()
        {
            byte[] bytes = ReadBytes(sizeof(int));
            return BitConverter.ToInt32(bytes, 0);
        }

        public long ReadInt64()
        {
            byte[] bytes = ReadBytes(sizeof(long));
            return BitConverter.ToInt64(bytes, 0);
        }
        public ushort ReadUShort()
        {
            byte[] bytes = ReadBytes(sizeof(ushort));
            return BitConverter.ToUInt16(bytes, 0);
        }

        public uint ReadUInt()
        {
            byte[] bytes = ReadBytes(sizeof(uint));
            return BitConverter.ToUInt32(bytes, 0);
        }

        public ulong ReadUInt64()
        {
            byte[] bytes = ReadBytes(sizeof(ulong));
            return BitConverter.ToUInt64(bytes, 0);
        }

        public string ReadString()
        {
            ushort len = ReadUShort();
            byte[] bytes = ReadBytes(len);
            return Encoding.UTF8.GetString(bytes, 0, len);
        }

        public void WriteString(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            ushort len = (ushort)bytes.Length;
            WriteUShort(len);
            WriteBytes(bytes, len);
        }

        public void WriteBytes(byte[] bytes, int count)
        {
            mStream.Write(bytes, 0, count);
        }

        public void WriteByte(byte value)
        {
            mStream.WriteByte(value);
        }

        public void WriteBool(bool value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes, bytes.Length);
        }

        public void WriteFloat(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes, bytes.Length);
        }

        public void WriteShort(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes, bytes.Length);
        }

        public void WriteInt(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes, bytes.Length);
        }

        public void WriteInt64(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes, bytes.Length);
        }

        public void WriteUShort(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes, bytes.Length);
        }

        public void WriteUInt(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes, bytes.Length);
        }

        public void WriteUInt64(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes, bytes.Length);
        }


        public List<bool> ReadBoolLst()
        {
            int count = ReadInt();
            List<bool> values = new List<bool>(count);
            for (int i = 0; i < count; ++i)
            {
                values.Add(ReadBool());
            }
            return values;
        }

        public void WriteBoolLst(List<bool> values)
        {
            WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                WriteBool(values[i]);
            }
        }

        public List<byte> ReadByteLst()
        {
            int count = ReadInt();
            List<byte> values = new List<byte>(count);
            for (int i = 0; i < count; ++i)
            {
                values.Add(ReadByte());
            }
            return values;
        }

        public void WriteByteLst(List<byte> values)
        {
            WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                WriteByte(values[i]);
            }
        }

        public List<short> ReadShortLst()
        {
            int count = ReadInt();
            List<short> values = new List<short>(count);
            for (int i = 0; i < count; ++i)
            {
                values.Add(ReadShort());
            }
            return values;
        }

        public void WriteShortLst(List<short> values)
        {
            WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                WriteShort(values[i]);
            }
        }

        public List<ushort> ReadUShortLst()
        {
            int count = ReadInt();
            List<ushort> values = new List<ushort>(count);
            for (int i = 0; i < count; ++i)
            {
                values.Add(ReadUShort());
            }
            return values;
        }

        public void WriteShortLst(List<ushort> values)
        {
            WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                WriteUShort(values[i]);
            }
        }

        public List<int> ReadIntLst()
        {
            int count = ReadInt();
            List<int> values = new List<int>(count);
            for (int i = 0; i < count; ++i)
            {
                values.Add(ReadInt());
            }
            return values;
        }

        public void WriteIntLst(List<int> values)
        {
            WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                WriteInt(values[i]);
            }
        }

        public List<uint> ReadUIntLst()
        {
            int count = ReadInt();
            List<uint> values = new List<uint>(count);
            for (int i = 0; i < count; ++i)
            {
                values.Add(ReadUInt());
            }
            return values;
        }

        public void WriteUIntLst(List<uint> values)
        {
            WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                WriteUInt(values[i]);
            }
        }

        public List<ulong> ReadUInt64Lst()
        {
            int count = ReadInt();
            List<ulong> values = new List<ulong>(count);
            for (int i = 0; i < count; ++i)
            {
                values.Add(ReadUInt64());
            }
            return values;
        }

        public void WriteUInt64Lst(List<ulong> values)
        {
            WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                WriteUInt64(values[i]);
            }
        }

        public List<float> ReadFloatLst()
        {
            int count = ReadInt();
            List<float> values = new List<float>(count);
            for (int i = 0; i < count; ++i)
            {
                values.Add(ReadFloat());
            }
            return values;
        }

        public void WriteFloatLst(List<float> values)
        {
            WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                WriteFloat(values[i]);
            }
        }
        public List<string> ReadStringLst()
        {
            int count = ReadInt();
            List<string> values = new List<string>(count);
            for (int i = 0; i < count; ++i)
            {
                values.Add(ReadString());
            }
            return values;
        }

        public void WriteStringLst(List<string> values)
        {
            WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                WriteString(values[i]);
            }
        }

        public void WriteVector2(Vector2 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
        }

        public Vector2 ReadVector2()
        {
            return new Vector2(ReadFloat(), ReadFloat());
        }

        public void WriteVector3(Vector3 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        public void WriteVector4(Vector4 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
            WriteFloat(value.w);
        }

        public Vector4 ReadVector4()
        {
            return new Vector4(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public void WriteQuaternion(Quaternion value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
            WriteFloat(value.w);
        }

        public Quaternion ReadQuaternion()
        {
            return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public void WriteColor(Color value)
        {
            WriteFloat(value.r);
            WriteFloat(value.g);
            WriteFloat(value.b);
            WriteFloat(value.a);
        }

        public Color ReadColor()
        {
            return new Color(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public void WriteMatrix4x4(Matrix4x4 value)
        {
            WriteVector4(value.GetColumn(0));
            WriteVector4(value.GetColumn(1));
            WriteVector4(value.GetColumn(2));
            WriteVector4(value.GetColumn(3));
        }

        public Matrix4x4 ReadMatrix4x4()
        {
            Vector4 col0 = ReadVector4();
            Vector4 col1 = ReadVector4();
            Vector4 col2 = ReadVector4();
            Vector4 col3 = ReadVector4();
            Matrix4x4 m = new Matrix4x4();
            m.SetColumn(0, col0);
            m.SetColumn(1, col1);
            m.SetColumn(2, col2);
            m.SetColumn(3, col3);
            return m;
        }

        public void WriteColorLst(List<Color> values)
        {
            WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                WriteColor(values[i]);
            }
        }


        public void WriteQuaternionLst(List<Quaternion> values)
        {
            WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                WriteQuaternion(values[i]);
            }
        }

        public void WriteVector4Lst(List<Vector4> values)
        {
            WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                WriteVector4(values[i]);
            }
        }

        public void WriteVector3Lst(List<Vector3> values)
        {
            WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                WriteVector3(values[i]);
            }
        }

        public void WriteVector2Lst(List<Vector2> values)
        {
            WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                WriteVector2(values[i]);
            }
        }

        public List<Color> ReadColorLst()
        {
            int count = ReadInt();
            List<Color> colors = new List<Color>(count);
            for (int i = 0; i < count; ++i)
            {
                colors.Add(ReadColor());
            }
            return colors;
        }

        public List<Quaternion> ReadQuaternionLst()
        {
            int count = ReadInt();
            List<Quaternion> values = new List<Quaternion>(count);
            for (int i = 0; i < count; ++i)
            {
                values.Add(ReadQuaternion());
            }
            return values;
        }

        public List<Vector4> ReadVector4Lst()
        {
            int count = ReadInt();
            List<Vector4> values = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
                values.Add(ReadVector4());
            }
            return values;
        }
        public List<Vector3> ReadVector3Lst()
        {
            int count = ReadInt();
            List<Vector3> values = new List<Vector3>(count);
            for (int i = 0; i < count; ++i)
            {
                values.Add(ReadVector3());
            }
            return values;
        }

        public List<Vector2> ReadVector2Lst()
        {
            int count = ReadInt();
            List<Vector2> values = new List<Vector2>(count);
            for (int i = 0; i < count; ++i)
            {
                values.Add(ReadVector2());
            }
            return values;
        }


        public void WriteQuaternionPosition(Quaternion q, Vector3 pos)
        {
            WriteQuaternion(q);
            WriteVector3(pos);
        }

        public void ReadQuaternionPosition(ref Quaternion q, ref Vector3 pos)
        {
            q = ReadQuaternion();
            pos = ReadVector3();
        }

    }
}
