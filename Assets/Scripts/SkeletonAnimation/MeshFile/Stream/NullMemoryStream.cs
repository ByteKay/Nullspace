using Nullspace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace NullMesh
{
    public partial class NullMemoryStream
    {
        private class ReadWriteAttribute : Attribute
        {
            public ReadWriteType ReadWriteType { get; set; }
            public Type Type;
            public ReadWriteAttribute(ReadWriteType readWriteType, Type type)
            {
                ReadWriteType = readWriteType;
                Type = type;
            }
        }

        private enum ReadWriteType
        {
            Read = 0,
            Write,
        }
        private const int CacheLen = 1024 * 1024;
        private static byte[] CacheBytes = new byte[CacheLen];

        private static Dictionary<Type, MethodInfo> ReadMethodMap = new Dictionary<Type, MethodInfo>();
        private static Dictionary<Type, MethodInfo> WriteMethodMap = new Dictionary<Type, MethodInfo>();
        static NullMemoryStream()
        {
            ReadMethodMap.Clear();
            WriteMethodMap.Clear();
            Type type = typeof(NullMemoryStream);
            MethodInfo[] infos = type.GetMethods();
            foreach (var info in infos)
            {
                object[] attributes = info.GetCustomAttributes(typeof(ReadWriteAttribute), false);
                if (attributes.Length > 0)
                {
                    ReadWriteAttribute readWriteType = (ReadWriteAttribute)attributes[0];
                    switch (readWriteType.ReadWriteType)
                    {
                        case ReadWriteType.Read:
                            ReadMethodMap.Add(readWriteType.Type, info);
                            break;
                        case ReadWriteType.Write:
                            WriteMethodMap.Add(readWriteType.Type, info);
                            break;
                    }
                }
            }
        }

        private static bool IsStreamType<T>()
        {
            return typeof(T).GetInterface("IStream", false) != null;
        }

        private static MethodInfo GetMethod<T>(Dictionary<Type, MethodInfo> readWriteMap)
        {
            Type type = typeof(T);
            if (readWriteMap.ContainsKey(type))
            {
                return readWriteMap[type];
            }
            Type stream = typeof(INullStream);
            if (IsStreamType<T>() && readWriteMap.ContainsKey(stream))
            {
                return readWriteMap[stream];
            }
            return null;
        }

        private static MethodInfo GetReadMethod<T>()
        {
            return GetMethod<T>(ReadMethodMap);
        }

        private static MethodInfo GetWriteMethod<T>()
        {
            return GetMethod<T>(WriteMethodMap);
        }

        public static NullMemoryStream ReadFromFile(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            NullMemoryStream stream = new NullMemoryStream(fileStream);
            return stream;
        }

        public static NullMemoryStream ReadFromBytes(byte[] bytes)
        {
            MemoryStream memoryStream = new MemoryStream(bytes);
            NullMemoryStream stream = new NullMemoryStream(memoryStream);
            return stream;
        }

        public static NullMemoryStream WriteToFile(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            NullMemoryStream stream = new NullMemoryStream(fileStream);
            return stream;
        }
    }

    /// <summary>
    /// 待完善
    /// 
    /// float -> ushort (65536)
    /// 1. normal 压缩
    /// 2. uv 压缩
    /// 3. quaternion 压缩
    /// 
    /// int -> ushort
    /// 1. 小于 65536
    /// 
    /// bool list -> byte array
    /// 1. bool -> 1bit
    /// 
    /// </summary>
    /// 
    public partial class NullMemoryStream : IDisposable
    { 
        private Stream mStream;
        private NullMemoryStream(Stream stream)
        {
            mStream = stream;
            mStream.Seek(0, SeekOrigin.Begin);
        }

        public void Dispose()
        {
            if (mStream != null)
            {
                mStream.Close();
                mStream = null;
            }
        }

        private bool CanRead(int size)
        {
            return mStream.Position + size <= mStream.Length;
        }

        public int WriteMap<U, V>(Dictionary<U, V> values, bool ignoreWriteCount)
        {
            MethodInfo keyMethod = GetWriteMethod<U>();
            MethodInfo valueMethod = GetWriteMethod<V>();
            UnityEngine.Debug.Assert(keyMethod != null && valueMethod != null, "WriteMap");
            int size = ignoreWriteCount ? 0 : WriteInt(values.Count);
            object[] parameters = new object[1];
            foreach (var pair in values)
            {
                parameters[0] = pair.Key;
                size += (int)keyMethod.Invoke(this, parameters);
                parameters[0] = pair.Value;
                size += (int)valueMethod.Invoke(this, parameters);
            }
            return size;
        }

        public bool ReadMap<U, V>(out Dictionary<U, V> map, int useCount = 0) where V : new()
        {
            MethodInfo keyMethod = GetReadMethod<U>();
            MethodInfo valueMethod = GetReadMethod<V>();
            UnityEngine.Debug.Assert(keyMethod != null && valueMethod != null, "ReadMap");
            bool res = false;
            int count = useCount;
            map = null;
            if (count > 0 || ReadInt(out count))
            {
                map = new Dictionary<U, V>(count);
                U u = default(U);
                V v = IsStreamType<V>() ? new V() : default(V);
                object[] keyParameters = new object[1];
                object[] valueParameters = new object[1];
                for (int i = 0; i < count; ++i)
                {
                    keyParameters[0] = u;
                    valueParameters[0] = v;
                    res &= (bool)keyMethod.Invoke(this, keyParameters);
                    res &= (bool)valueMethod.Invoke(this, valueParameters);
                    map.Add((U)keyParameters[0], (V)valueParameters[0]);
                }
            }
            return res;
        }

        public bool ReadList<T>(out List<T> values, int useCount = 0)
        {
            values = null;
            int count = useCount;
            if (count > 0 || ReadInt(out count))
            {
                values = new List<T>(count);
                T v = default(T);
                MethodInfo info = GetReadMethod<T>();
                UnityEngine.Debug.Assert(info != null, "" + typeof(T).FullName);
                object[] parameters = new object[] { v };
                for (int i = 0; i < count; ++i)
                {
                    info.Invoke(this, parameters);
                    values.Add((T)parameters[0]);
                }
                return true;
            }
            return false;
        }

        public int WriteList<T>(List<T> values, bool ignoreWriteCount)
        {
            int size = ignoreWriteCount ? 0 : WriteInt(values.Count);
            MethodInfo info = GetWriteMethod<T>();
            UnityEngine.Debug.Assert(info != null, "" + typeof(T).FullName);
            object[] parameters = new object[1];
            for (int i = 0; i < values.Count; ++i)
            {
                parameters[0] = values[i];
                size += (int)info.Invoke(this, parameters);
            }
            return size;
        }

        [ReadWrite(ReadWriteType.Read, typeof(INullStream))]
        public bool ReadStream(ref INullStream v)
        {
            return v.LoadFromStream(this);
        }

        [ReadWrite(ReadWriteType.Write, typeof(INullStream))]
        public int WriteStream(INullStream v)
        {
            return v.SaveToStream(this);
        }
    }

    public partial class NullMemoryStream
    {
        [ReadWrite(ReadWriteType.Read, typeof(byte[]))]
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

        [ReadWrite(ReadWriteType.Read, typeof(byte))]
        public bool ReadByte(out byte v)
        {
            v = 0;
            byte[] bytes = ReadBytes(sizeof(byte));
            if (bytes == null)
            {
                return false;
            }
            v = bytes[0];
            return true;
        }

        [ReadWrite(ReadWriteType.Read, typeof(bool))]
        public bool ReadBool(out bool v)
        {
            v = false;
            byte[] bytes = ReadBytes(sizeof(bool));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToBoolean(bytes, 0);
            return true;
        }

        [ReadWrite(ReadWriteType.Read, typeof(float))]
        public bool ReadFloat(out float v)
        {
            v = 0;
            byte[] bytes = ReadBytes(sizeof(float));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToSingle(bytes, 0);
            return true;
        }

        [ReadWrite(ReadWriteType.Read, typeof(short))]
        public bool ReadShort(out short v)
        {
            v = 0;
            byte[] bytes = ReadBytes(sizeof(short));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToInt16(bytes, 0);
            return true;
        }

        [ReadWrite(ReadWriteType.Read, typeof(int))]
        public bool ReadInt(out int v)
        {
            v = 0;
            byte[] bytes = ReadBytes(sizeof(int));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToInt32(bytes, 0);
            return true;
        }

        [ReadWrite(ReadWriteType.Read, typeof(long))]
        public bool ReadLong(out long v)
        {
            v = 0;
            byte[] bytes = ReadBytes(sizeof(long));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToInt64(bytes, 0);
            return true;
        }

        [ReadWrite(ReadWriteType.Read, typeof(ushort))]
        public bool ReadUShort(out ushort v)
        {
            v = 0;
            byte[] bytes = ReadBytes(sizeof(ushort));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToUInt16(bytes, 0);
            return true;
        }

        [ReadWrite(ReadWriteType.Read, typeof(uint))]
        public bool ReadUInt(out uint v)
        {
            v = 0;
            byte[] bytes = ReadBytes(sizeof(uint));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToUInt32(bytes, 0);
            return true;
        }

        [ReadWrite(ReadWriteType.Read, typeof(ulong))]
        public bool ReadULong(out ulong v)
        {
            v = 0;
            byte[] bytes = ReadBytes(sizeof(ulong));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToUInt64(bytes, 0);
            return true;
        }

        [ReadWrite(ReadWriteType.Read, typeof(string))]
        public bool ReadString(out string v)
        {
            v = null;
            ushort len = 0;
            if (ReadUShort(out len))
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

        [ReadWrite(ReadWriteType.Read, typeof(Vector2))]
        public bool ReadVector2(out Vector2 v)
        {
            v = Vector2.zero;
            float x = 0, y = 0;
            if (ReadFloat(out x) && ReadFloat(out y))
            {
                v = new Vector2(x, y);
                return true;
            }
            return false;
        }

        [ReadWrite(ReadWriteType.Read, typeof(Vector3))]
        public bool ReadVector3(out Vector3 v)
        {
            v = Vector3.zero;
            float x = 0, y = 0, z = 0;
            if (ReadFloat(out x) && ReadFloat(out y) && ReadFloat(out z))
            {
                v = new Vector3(x, y, z);
                return true;
            }
            return false;
        }

        [ReadWrite(ReadWriteType.Read, typeof(Vector4))]
        public bool ReadVector4(out Vector4 v)
        {
            v = Vector4.zero;
            float x = 0, y = 0, z = 0, w = 0;
            if (ReadFloat(out x) && ReadFloat(out y) && ReadFloat(out z) && ReadFloat(out w))
            {
                v = new Vector4(x, y, z, w);
                return true;
            }
            return false;
        }

        [ReadWrite(ReadWriteType.Read, typeof(Quaternion))]
        public bool ReadQuaternion(out Quaternion v)
        {
            v = Quaternion.identity;
            float x = 0, y = 0, z = 0, w = 0;
            if (ReadFloat(out x) && ReadFloat(out y) && ReadFloat(out z) && ReadFloat(out w))
            {
                v = new Quaternion(x, y, z, w);
                return true;
            }
            return false;
        }

        [ReadWrite(ReadWriteType.Read, typeof(Color))]
        public bool ReadColor(out Color v)
        {
            v = Color.black;
            float r = 0, g = 0, b = 0, a = 0;
            if (ReadFloat(out r) && ReadFloat(out g) && ReadFloat(out b) && ReadFloat(out a))
            {
                v = new Color(r, g, b, a);
                return true;
            }
            return false;
        }

        [ReadWrite(ReadWriteType.Read, typeof(Matrix4x4))]
        public bool ReadMatrix4x4(out Matrix4x4 m)
        {
            m = Matrix4x4.zero;
            Vector4 col0 = Vector4.zero;
            Vector4 col1 = Vector4.zero;
            Vector4 col2 = Vector4.zero;
            Vector4 col3 = Vector4.zero;
            if (ReadVector4(out col0) && ReadVector4(out col1) && ReadVector4(out col2) && ReadVector4(out col3))
            {
                m.SetColumn(0, col0);
                m.SetColumn(1, col1);
                m.SetColumn(2, col2);
                m.SetColumn(3, col3);
                return true;
            }

            return false;
        }

        [ReadWrite(ReadWriteType.Read, typeof(Vector3Int))]
        public bool ReadVector3Int(ref Vector3Int v)
        {
            v = Vector3Int.zero;
            int x = 0, y = 0, z = 0;
            if (ReadInt(out x) && ReadInt(out y) && ReadInt(out z))
            {
                v.x = x;
                v.y = y;
                v.z = z;
                return true;
            }
            return false;
        }
    }

    public partial class NullMemoryStream
    {
        [ReadWrite(ReadWriteType.Write, typeof(string))]
        public int WriteString(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            ushort len = (ushort)bytes.Length;
            int size = WriteUShort(len);
            size += WriteBytes(bytes, len);
            return size;
        }

        [ReadWrite(ReadWriteType.Write, typeof(byte[]))]
        public int WriteBytes(byte[] bytes, int count)
        {
            mStream.Write(bytes, 0, count);
            return bytes.Length;
        }

        [ReadWrite(ReadWriteType.Write, typeof(byte))]
        public int WriteByte(byte value)
        {
            mStream.WriteByte(value);
            return sizeof(byte);
        }

        [ReadWrite(ReadWriteType.Write, typeof(bool))]
        public int WriteBool(bool value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }

        [ReadWrite(ReadWriteType.Write, typeof(float))]
        public int WriteFloat(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }

        [ReadWrite(ReadWriteType.Write, typeof(short))]
        public int WriteShort(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }

        [ReadWrite(ReadWriteType.Write, typeof(int))]
        public int WriteInt(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }

        [ReadWrite(ReadWriteType.Write, typeof(long))]
        public int WriteLong(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }
        [ReadWrite(ReadWriteType.Write, typeof(ushort))]
        public int WriteUShort(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }
        [ReadWrite(ReadWriteType.Write, typeof(uint))]
        public int WriteUInt(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }
        [ReadWrite(ReadWriteType.Write, typeof(ulong))]
        public int WriteULong(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(bytes, bytes.Length);
        }
        [ReadWrite(ReadWriteType.Write, typeof(Vector2))]
        public int WriteVector2(Vector2 value)
        {
            int size = WriteFloat(value.x);
            size += WriteFloat(value.y);
            return size;
        }
        [ReadWrite(ReadWriteType.Write, typeof(Vector3))]
        public int WriteVector3(Vector3 value)
        {
            int size = WriteFloat(value.x);
            size += WriteFloat(value.y);
            size += WriteFloat(value.z);
            return size;
        }
        [ReadWrite(ReadWriteType.Write, typeof(Vector4))]
        public int WriteVector4(Vector4 value)
        {
            int size = WriteFloat(value.x);
            size += WriteFloat(value.y);
            size += WriteFloat(value.z);
            size += WriteFloat(value.w);
            return size;
        }

        [ReadWrite(ReadWriteType.Write, typeof(Quaternion))]
        public int WriteQuaternion(Quaternion value)
        {
            int size = WriteFloat(value.x);
            size += WriteFloat(value.y);
            size += WriteFloat(value.z);
            size += WriteFloat(value.w);
            return size;
        }

        [ReadWrite(ReadWriteType.Write, typeof(Color))]
        public int WriteColor(Color value)
        {
            int size = WriteFloat(value.r);
            size += WriteFloat(value.g);
            size += WriteFloat(value.b);
            size += WriteFloat(value.a);
            return size;
        }

        [ReadWrite(ReadWriteType.Write, typeof(Matrix4x4))]
        public int WriteMatrix4x4(Matrix4x4 value)
        {
            int size = WriteVector4(value.GetColumn(0));
            size += WriteVector4(value.GetColumn(1));
            size += WriteVector4(value.GetColumn(2));
            size += WriteVector4(value.GetColumn(3));
            return size;
        }

        [ReadWrite(ReadWriteType.Write, typeof(Vector3Int))]
        public int WriteVector3Int(Vector3Int value)
        {
            int size = WriteInt(value.x);
            size += WriteInt(value.y);
            size += WriteInt(value.z);
            return size;
        }

    }
}
