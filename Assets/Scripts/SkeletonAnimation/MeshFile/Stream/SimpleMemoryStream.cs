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

        private byte[] ReadBytes(int count)
        {
            if (CanRead(count))
            {
                byte[] bytes = CacheBytes;
                if (CacheLen > count)
                {
                    bytes = new byte[count];
                }
                int size = mStream.Read(bytes, 0, count);
                return bytes;
            }
            return null;
        }

        public bool ReadByte(ref byte v)
        {
            byte[] bytes = ReadBytes(sizeof(byte));
            if (bytes == null)
            {
                return false;
            }
            v = bytes[0];
            return true;
        }

        public bool ReadBool(ref bool v)
        {
            byte[] bytes = ReadBytes(sizeof(bool));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToBoolean(bytes, 0);
            return true;
        }

        public bool ReadFloat(ref float v)
        {
            byte[] bytes = ReadBytes(sizeof(float));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToSingle(bytes, 0);
            return true;
        }

        public bool ReadShort(ref short v)
        {
            byte[] bytes = ReadBytes(sizeof(short));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToInt16(bytes, 0);
            return true;
        }

        public bool ReadInt(ref int v)
        {
            byte[] bytes = ReadBytes(sizeof(int));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToInt32(bytes, 0);
            return true;
        }

        public bool ReadInt64(ref long v)
        {
            byte[] bytes = ReadBytes(sizeof(long));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToInt64(bytes, 0);
            return true;
        }
        public bool ReadUShort(ref ushort v)
        {
            byte[] bytes = ReadBytes(sizeof(ushort));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToUInt16(bytes, 0);
            return true;
        }

        public bool ReadUInt(ref uint v)
        {
            byte[] bytes = ReadBytes(sizeof(uint));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToUInt32(bytes, 0);
            return true;
        }

        public bool ReadUInt64(ref ulong v)
        {
            byte[] bytes = ReadBytes(sizeof(ulong));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToUInt64(bytes, 0);
            return true;
        }

        public bool ReadString(ref string v)
        {
            ushort len = 0;
            if (ReadUShort(ref len))
            {
                byte[] bytes = ReadBytes(len);
                if (bytes == null)
                {
                    return false;
                }
                v = Encoding.UTF8.GetString(bytes, 0, len);
            }
            return false;
        }

        public int WriteString(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            ushort len = (ushort)bytes.Length;
            int size = WriteUShort(len);
            size += WriteBytes(bytes, len);
            return size;
        }

        public int WriteBytes(byte[] bytes, int count)
        {
            mStream.Write(bytes, 0, count);
            return bytes.Length;
        }

        public int WriteByte(byte value)
        {
            mStream.WriteByte(value);
            return sizeof(byte);
        }

        public int WriteBool(bool value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }

        public int WriteFloat(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }

        public int WriteShort(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }

        public int WriteInt(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }

        public int WriteInt64(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }

        public int WriteUShort(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }

        public int WriteUInt(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }

        public int WriteUInt64(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }

        public bool ReadBoolLst(ref List<bool> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                bool b = false;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadBool(ref b))
                    {
                        v.Add(b);
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public int WriteBoolLst(ref List<bool> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteBool(values[i]);
            }
            return size;
        }

        public bool ReadByteLst(ref List<byte> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                byte b = 0;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadByte(ref b))
                    {
                        v.Add(b);
                    }
                    else
                    {
                        return false;
                    }
                    
                }
                return true;
            }
            return false;
        }

        public int WriteByteLst(List<byte> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteByte(values[i]);
            }
            return size;
        }

        public bool ReadShortLst(ref List<short> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                short s = 0;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadShort(ref s))
                    {
                        v.Add(s);
                    }
                    else
                    {
                        return false;
                    }
                    
                }
            }
            return false;
        }

        public int WriteShortLst(List<short> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteShort(values[i]);
            }
            return size;
        }

        public bool ReadUShortLst(ref List<ushort> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                ushort s = 0;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadUShort(ref s))
                    {
                        v.Add(s);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public int WriteShortLst(List<ushort> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteUShort(values[i]);
            }
            return size;
        }

        public bool ReadIntLst(ref List<int> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                int s = 0;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadInt(ref s))
                    {
                        v.Add(s);
                    }
                    else
                    {
                        return false;
                    }

                }
            }
            return false;
        }

        public int WriteIntLst(List<int> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteInt(values[i]);
            }
            return size;
        }

        public bool ReadUIntLst(ref List<uint> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                uint s = 0;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadUInt(ref s))
                    {
                        v.Add(s);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public int WriteUIntLst(List<uint> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteUInt(values[i]);
            }
            return size;
        }

        public bool ReadUInt64Lst(ref List<ulong> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                ulong s = 0;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadUInt64(ref s))
                    {
                        v.Add(s);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public int WriteUInt64Lst(List<ulong> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteUInt64(values[i]);
            }
            return size;
        }

        public bool ReadFloatLst(ref List<float> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                float s = 0;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadFloat(ref s))
                    {
                        v.Add(s);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public int WriteFloatLst(List<float> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteFloat(values[i]);
            }
            return size;
        }
        public bool ReadStringLst(ref List<string> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                string s = null;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadString(ref s))
                    {
                        v.Add(s);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public int WriteStringLst(List<string> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteString(values[i]);
            }
            return size;
        }

        public int WriteVector2(Vector2 value)
        {
            int size = WriteFloat(value.x);
            size += WriteFloat(value.y);
            return size;
        }

        public bool ReadVector2(ref Vector2 v)
        {
            float x = 0, y = 0;
            if (ReadFloat(ref x) && ReadFloat(ref y))
            {
                v = new Vector2(x, y);
                return true;
            }
            return false;
        }

        public int WriteVector3(Vector3 value)
        {
            int size = WriteFloat(value.x);
            size += WriteFloat(value.y);
            size += WriteFloat(value.z);
            return size;
        }

        public bool ReadVector3(ref Vector3 v)
        {
            float x = 0, y = 0, z = 0;
            if (ReadFloat(ref x) && ReadFloat(ref y) && ReadFloat(ref z))
            {
                v = new Vector3(x, y, z);
                return true;
            }
            return false;
        }

        public int WriteVector4(Vector4 value)
        {
            int size = WriteFloat(value.x);
            size += WriteFloat(value.y);
            size += WriteFloat(value.z);
            size += WriteFloat(value.w);
            return size;
        }

        public bool ReadVector4(ref Vector4 v)
        {
            float x = 0, y = 0, z = 0, w = 0;
            if (ReadFloat(ref x) && ReadFloat(ref y) && ReadFloat(ref z) && ReadFloat(ref w))
            {
                v = new Vector4(x, y, z, w);
                return true;
            }
            return false;
        }

        public int WriteQuaternion(Quaternion value)
        {
            int size = WriteFloat(value.x);
            size += WriteFloat(value.y);
            size += WriteFloat(value.z);
            size += WriteFloat(value.w);
            return size;
        }

        public bool ReadQuaternion(ref Quaternion v)
        {
            float x = 0, y = 0, z = 0, w = 0;
            if (ReadFloat(ref x) && ReadFloat(ref y) && ReadFloat(ref z) && ReadFloat(ref w))
            {
                v = new Quaternion(x, y, z, w);
                return true;
            }
            return false;
        }

        public int WriteColor(Color value)
        {
            int size = WriteFloat(value.r);
            size += WriteFloat(value.g);
            size += WriteFloat(value.b);
            size += WriteFloat(value.a);
            return size;
        }

        public bool ReadColor(ref Color v)
        {
            float r = 0, g = 0, b = 0, a = 0;
            if (ReadFloat(ref r) && ReadFloat(ref g) && ReadFloat(ref b) && ReadFloat(ref a))
            {
                v = new Color(r, g, b, a);
                return true;
            }
            return false;
        }

        public int WriteMatrix4x4(Matrix4x4 value)
        {
            int size = WriteVector4(value.GetColumn(0));
            size += WriteVector4(value.GetColumn(1));
            size += WriteVector4(value.GetColumn(2));
            size += WriteVector4(value.GetColumn(3));
            return size;
        }

        public bool ReadMatrix4x4(ref Matrix4x4 m)
        {
            Vector4 col0 = Vector4.zero;
            Vector4 col1 = Vector4.zero;
            Vector4 col2 = Vector4.zero;
            Vector4 col3 = Vector4.zero;
            if (ReadVector4(ref col0) && ReadVector4(ref col1) && ReadVector4(ref col2) && ReadVector4(ref col3))
            {
                m.SetColumn(0, col0);
                m.SetColumn(1, col1);
                m.SetColumn(2, col2);
                m.SetColumn(3, col3);
                return true;
            }

            return false;
        }

        public int WriteColorLst(List<Color> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteColor(values[i]);
            }
            return size;
        }


        public int WriteQuaternionLst(List<Quaternion> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteQuaternion(values[i]);
            }
            return size;
        }

        public int WriteVector4Lst(List<Vector4> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteVector4(values[i]);
            }
            return size;
        }

        public int WriteVector3Lst(List<Vector3> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteVector3(values[i]);
            }
            return size;
        }

        public int WriteVector2Lst(List<Vector2> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteVector2(values[i]);
            }
            return size;
        }

        public int WriteVector3Int(Vector3Int value)
        {
            int size = WriteInt(value.x);
            size += WriteInt(value.y);
            size += WriteInt(value.z);
            return size;
        }

        public bool ReadVector3Int(ref Vector3Int v)
        {
            int x = 0, y = 0, z = 0;
            if (ReadInt(ref x) && ReadInt(ref y) && ReadInt(ref z))
            {
                v.x = x;
                v.y = y;
                v.z = z;
                return true;
            }
            return false;
        }


        public int WriteVector3IntLst(List<Vector3Int> values)
        {
            int size = WriteInt(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                size += WriteVector3Int(values[i]);
            }
            return size;
        }

        public bool ReadVector3IntLst(ref List<Vector3Int> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                Vector3Int s = Vector3Int.zero;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadVector3Int(ref s))
                    {
                        v.Add(s);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public bool ReadColorLst(ref List<Color> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                Color s = Color.black;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadColor(ref s))
                    {
                        v.Add(s);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public bool ReadQuaternionLst(ref List<Quaternion> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                Quaternion s = Quaternion.identity;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadQuaternion(ref s))
                    {
                        v.Add(s);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public bool ReadVector4Lst(ref List<Vector4> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                Vector4 s = Vector4.zero;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadVector4(ref s))
                    {
                        v.Add(s);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        public bool ReadVector3Lst(ref List<Vector3> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                Vector3 s = Vector3.zero;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadVector3(ref s))
                    {
                        v.Add(s);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public bool ReadVector2Lst(ref List<Vector2> v)
        {
            int count = 0;
            if (ReadInt(ref count))
            {
                v.Capacity = count;
                Vector2 s = Vector2.zero;
                for (int i = 0; i < count; ++i)
                {
                    if (ReadVector2(ref s))
                    {
                        v.Add(s);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public int WriteQuaternionPosition(Quaternion q, Vector3 pos)
        {
            int size = WriteQuaternion(q);
            size += WriteVector3(pos);
            return size;
        }

        public bool ReadQuaternionPosition(ref Quaternion q, ref Vector3 pos)
        {
            if (ReadQuaternion(ref q) && ReadVector3(ref pos))
            {
                return true;
            }
            return false;
        }

        public int WriteQuaternionPositionLst(List<Quaternion> qs, List<Vector3> poses)
        {
            int count = qs.Count;
            int size = WriteInt(count);
            for (int i = 0; i < count; ++i)
            {
                size += WriteQuaternion(qs[i]);
                size += WriteVector3(poses[i]);
            }
            return size;
        }

        public bool ReadQuaternionPositionLst(List<Quaternion> qs, List<Vector3> poses)
        {
            Quaternion q = Quaternion.identity;
            Vector3 pos = Vector3.zero;
            int cnt = 0;
            bool res = ReadInt(ref cnt);
            for (int i = 0; i < cnt; ++i)
            {
                ReadQuaternion(ref q);
                ReadVector3(ref pos);
                qs.Add(q);
                poses.Add(pos);
            }
            return false;
        }
    }
}
