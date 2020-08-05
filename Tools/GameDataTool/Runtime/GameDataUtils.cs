
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Nullspace
{
    public class GameDataReadWriteAttribute : Attribute
    {
        public GameDataReadWriteType ReadWriteType { get; set; }
        public Type Type;
        public GameDataReadWriteAttribute(GameDataReadWriteType readWriteType, Type type)
        {
            ReadWriteType = readWriteType;
            Type = type;
        }
    }
    public enum GameDataReadWriteType
    {
        Read = 0,
        Write,
        ParseObject,
        ParseString,
    }

    public class GameDataUtils
    {
        private const int CacheLen = 1024 * 1024;
        private static byte[] CacheBytes = new byte[CacheLen];

        private const char KEY_VALUE_SPRITER = ':';
        private const char MAP_SPRITER = ',';
        private const char LIST_SPRITER = ';';
        private static Regex RegexVector = new Regex("-?\\d+\\.\\d+");
        private static Dictionary<Type, MethodInfo> ReadMethodMap = new Dictionary<Type, MethodInfo>();
        private static Dictionary<Type, MethodInfo> WriteMethodMap = new Dictionary<Type, MethodInfo>();
        private static Dictionary<Type, MethodInfo> ParseStringMethodMap = new Dictionary<Type, MethodInfo>();
        private static Dictionary<Type, MethodInfo> ParseObjectMethodMap = new Dictionary<Type, MethodInfo>();
        static GameDataUtils()
        {
            ReadMethodMap.Clear();
            WriteMethodMap.Clear();
            ParseStringMethodMap.Clear();
            ParseObjectMethodMap.Clear();
            Type type = typeof(GameDataUtils);
            MethodInfo[] infos = type.GetMethods();
            foreach (var info in infos)
            {
                object[] attributes = info.GetCustomAttributes(typeof(GameDataReadWriteAttribute), false);
                if (attributes.Length > 0)
                {
                    GameDataReadWriteAttribute readWriteType = (GameDataReadWriteAttribute)attributes[0];
                    switch (readWriteType.ReadWriteType)
                    {
                        case GameDataReadWriteType.Read:
                            ReadMethodMap.Add(readWriteType.Type, info);
                            break;
                        case GameDataReadWriteType.Write:
                            WriteMethodMap.Add(readWriteType.Type, info);
                            break;
                        case GameDataReadWriteType.ParseObject:
                            ParseObjectMethodMap.Add(readWriteType.Type, info);
                            break;
                        case GameDataReadWriteType.ParseString:
                            ParseStringMethodMap.Add(readWriteType.Type, info);
                            break;
                    }
                }
            }
        }
        private static MatchCollection MatchVector(string inputString)
        {
            return RegexVector.Matches(inputString);
        }
        private static bool IsStreamType<T>()
        {
            return typeof(T).GetInterface("INullStream", false) != null;
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
        private static MethodInfo GetParseStringMethod<T>()
        {
            return GetMethod<T>(ParseStringMethodMap);
        }
        private static MethodInfo GetParseObjectMethod<T>()
        {
            return GetMethod<T>(ParseObjectMethodMap);
        }
        public static int WriteMap<U, V>(NullMemoryStream stream, Dictionary<U, V> values, bool ignoreWriteCount)
        {
            MethodInfo keyMethod = GetWriteMethod<U>();
            MethodInfo valueMethod = GetWriteMethod<V>();
            System.Diagnostics.Debug.Assert(keyMethod != null && valueMethod != null, "WriteMap");
            int size = ignoreWriteCount ? 0 : WriteInt(stream, values.Count);
            object[] parameters = new object[2];
            parameters[0] = stream;
            foreach (var pair in values)
            {
                parameters[1] = pair.Key;
                size += (int)keyMethod.Invoke(null, parameters);
                parameters[1] = pair.Value;
                size += (int)valueMethod.Invoke(null, parameters);
            }
            return size;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="useCount">-1表示需要读int；表示不读任何数据,返回 true</param>
        /// <returns></returns>
        public static bool ReadMap<U, V>(NullMemoryStream stream, out Dictionary<U, V> map, int useCount = -1) where V : new()
        {
            map = new Dictionary<U, V>();
            if (useCount == 0)
            {
                return true;
            }
            MethodInfo keyMethod = GetReadMethod<U>();
            MethodInfo valueMethod = GetReadMethod<V>();
            System.Diagnostics.Debug.Assert(keyMethod != null && valueMethod != null, "ReadMap");
            bool res = false;
            int count = useCount;
            // 处理 -1 和 大于0 的情况
            if (count > 0 || ReadInt(stream, out count))
            {
                U u = default(U);
                V v = IsStreamType<V>() ? new V() : default(V);
                object[] keyParameters = new object[2];
                keyParameters[0] = stream;
                object[] valueParameters = new object[2];
                valueParameters[0] = stream;
                for (int i = 0; i < count; ++i)
                {
                    keyParameters[1] = u;
                    valueParameters[1] = v;
                    res &= (bool)keyMethod.Invoke(null, keyParameters);
                    res &= (bool)valueMethod.Invoke(null, valueParameters);
                    map.Add((U)keyParameters[1], (V)valueParameters[1]);
                }
            }
            return res;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="useCount">-1表示需要读int；表示不读任何数据,返回 true</param>
        /// <returns></returns>
        public static bool ReadList<T>(NullMemoryStream stream, out List<T> values, int useCount = -1) where T : new()
        {
            values = new List<T>();
            int count = useCount;
            if (count == 0)
            {
                return true;
            }
            if (count > 0 || ReadInt(stream, out count))
            {
                values.Capacity = count;
                T v = IsStreamType<T>() ? new T() : default(T);
                MethodInfo info = GetReadMethod<T>();
                Debug.Assert(info != null, "" + typeof(T).FullName);
                object[] parameters = new object[2] { stream, v };
                for (int i = 0; i < count; ++i)
                {
                    info.Invoke(null, parameters);
                    values.Add((T)parameters[1]);
                }
                return true;
            }
            return false;
        }
        public static int WriteList<T>(NullMemoryStream stream, List<T> values, bool ignoreWriteCount)
        {
            int size = ignoreWriteCount ? 0 : WriteInt(stream, values.Count);
            MethodInfo info = GetWriteMethod<T>();
            Debug.Assert(info != null, "" + typeof(T).FullName);
            object[] parameters = new object[2];
            parameters[0] = stream;
            for (int i = 0; i < values.Count; ++i)
            {
                parameters[1] = values[i];
                size += (int)info.Invoke(null, parameters);
            }
            return size;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(INullStream))]
        public static bool ReadStream(NullMemoryStream stream, ref INullStream v)
        {
            return v.LoadFromStream(stream);
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(INullStream))]
        public static int WriteStream(NullMemoryStream stream, INullStream v)
        {
            return v.SaveToStream(stream);
        }
        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
        public static object ParseString(string str, Type type)
        {
            object defaultV = GetDefault(type);
            MethodInfo keyMethod = typeof(NullMemoryStream).GetMethod("GetParseStringMethod").MakeGenericMethod(new Type[] { type });
            if (keyMethod == null)
            {
                return defaultV;
            }
            object[] keyParameters = new object[] { str, defaultV };
            keyMethod.Invoke(null, keyParameters);
            return keyParameters[1];
        }
        public static T ParseString<T>(string str)
        {
            MethodInfo keyMethod = GetParseStringMethod<T>();
            if (keyMethod == null)
            {
                return default(T);
            }
            object[] keyParameters = new object[] { str, default(T) };
            keyMethod.Invoke(null, keyParameters);
            return (T)keyParameters[1];
        }
        public static string ParseObject<T>(T obj)
        {
            MethodInfo keyMethod = GetParseObjectMethod<T>();
            if (keyMethod == null)
            {
                return null;
            }
            object[] keyParameters = new object[1] { obj };
            return (string)keyMethod.Invoke(null, keyParameters);
        }
        public static string ParseMap<U, V>(Dictionary<U, V> map) where V : new()
        {
            MethodInfo keyMethod = GetParseObjectMethod<U>();
            MethodInfo valueMethod = GetParseObjectMethod<V>();
            Debug.Assert(keyMethod != null && valueMethod != null, "ParseMap");
            object[] keyParameters = new object[1];
            object[] valueParameters = new object[1];
            StringBuilder builder = new StringBuilder();
            int index = 0;
            int count = map.Count;
            foreach (var item in map)
            {
                index++;
                keyParameters[0] = item.Key;
                valueParameters[0] = item.Value;
                string key = (string)keyMethod.Invoke(null, keyParameters);
                string value = (string)valueMethod.Invoke(null, valueParameters);
                builder.AppendFormat("{0}{1}{2}", key, KEY_VALUE_SPRITER, value);
                if (index < count)
                {
                    builder.Append(MAP_SPRITER);
                }
            }
            return builder.ToString();
        }
        public static string ParseList<T>(List<T> values) where T : new()
        {
            StringBuilder builder = new StringBuilder();
            MethodInfo info = GetParseObjectMethod<T>();
            Debug.Assert(info != null, "" + typeof(T).FullName);
            object[] parameters = new object[1];
            for (int i = 0; i < values.Count; ++i)
            {
                parameters[0] = values[i];
                string v = (string)info.Invoke(null, parameters);
                builder.Append(v);
                if (i < values.Count - 1)
                {
                    builder.Append(LIST_SPRITER);
                }
            }
            return builder.ToString();
        }
        public static bool ParseMap<U, V>(string str, out Dictionary<U, V> map)
        {
            map = new Dictionary<U, V>();
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            string[] pairs = str.Split(MAP_SPRITER);
            MethodInfo keyMethod = GetParseStringMethod<U>();
            MethodInfo valueMethod = GetParseStringMethod<V>();
            Debug.Assert(keyMethod != null && valueMethod != null, "ParseMap");
            object[] keyParameters = new object[2] { null, default(U) };
            object[] valueParameters = new object[2] { null, default(V) };
            for (int i = 0; i < pairs.Length; ++i)
            {
                string[] pair = pairs[i].Split(KEY_VALUE_SPRITER);
                if (pair.Length != 2)
                {
                    return false;
                }
                keyParameters[0] = pair[0];
                valueParameters[0] = pair[1];
                keyMethod.Invoke(null, keyParameters);
                valueMethod.Invoke(null, valueParameters);
                map.Add((U)keyParameters[1], (V)valueParameters[1]);
            }
            return true;
        }
        public static bool ParseList<T>(string str, out List<T> values) where T : new()
        {
            string[] strs = str.Split(LIST_SPRITER);
            values = new List<T>(strs.Length);
            MethodInfo info = GetParseStringMethod<T>();
            Debug.Assert(info != null, "" + typeof(T).FullName);
            object[] parameters = new object[1];
            for (int i = 0; i < strs.Length; ++i)
            {
                parameters[0] = strs[i];
                info.Invoke(null, parameters);
                values.Add((T)parameters[0]);
            }
            return true;
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(byte))]
        public static bool ParseByte(string str, out byte v)
        {
            return byte.TryParse(str, out v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(bool))]
        public static bool ParseBool(string str, out bool v)
        {
            return bool.TryParse(str, out v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(float))]
        public static bool ParseFloat(string str, out float v)
        {
            return float.TryParse(str, out v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(short))]
        public static bool ParseShort(string str, out short v)
        {
            return short.TryParse(str, out v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(int))]
        public static bool ParseInt(string str, out int v)
        {
            return int.TryParse(str, out v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(long))]
        public static bool ParseLong(string str, out long v)
        {
            return long.TryParse(str, out v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(ushort))]
        public static bool ParseUShort(string str, out ushort v)
        {
            return ushort.TryParse(str, out v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(uint))]
        public static bool ParseUInt(string str, out uint v)
        {
            return uint.TryParse(str, out v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(ulong))]
        public static bool ParseULong(string str, out ulong v)
        {
            return ulong.TryParse(str, out v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(string))]
        public static bool ParseString(string str, out string v)
        {
            v = str;
            return true;
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(Matrix4x4))]
        public static bool ParseMatrix4x4(string str, out Matrix4x4 v)
        {
            v = Matrix4x4.zero;
            if (str != null)
            {
                MatchCollection mc = MatchVector(str);
                if (mc.Count == 16)
                {
                    v.m00 = float.Parse(mc[0].Value);
                    v.m01 = float.Parse(mc[1].Value);
                    v.m02 = float.Parse(mc[2].Value);
                    v.m03 = float.Parse(mc[3].Value);
                    v.m10 = float.Parse(mc[4].Value);
                    v.m11 = float.Parse(mc[5].Value);
                    v.m12 = float.Parse(mc[6].Value);
                    v.m13 = float.Parse(mc[7].Value);
                    v.m20 = float.Parse(mc[8].Value);
                    v.m21 = float.Parse(mc[9].Value);
                    v.m22 = float.Parse(mc[10].Value);
                    v.m23 = float.Parse(mc[11].Value);
                    v.m30 = float.Parse(mc[12].Value);
                    v.m31 = float.Parse(mc[13].Value);
                    v.m32 = float.Parse(mc[14].Value);
                    v.m33 = float.Parse(mc[15].Value);
                    return true;
                }
                Debug.Assert(false, string.Format("Error attempting to parse property {0} as an Matrix4x4.", str));
            }
            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(Vector2))]
        public static bool ParseVector2(string str, out Vector2 v)
        {
            v = Vector2.zero;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 2)
            {
                v.x = float.Parse(collection[0].Value);
                v.y = float.Parse(collection[1].Value);
                return true;
            }
            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(Vector3))]
        public static bool ParseVector3(string str, out Vector3 v)
        {
            v = Vector3.zero;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 3)
            {
                v.x = float.Parse(collection[0].Value);
                v.y = float.Parse(collection[1].Value);
                v.z = float.Parse(collection[2].Value);
                return true;
            }
            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(Vector3Int))]
        public static bool ParseVector3Int(string str, out Vector3Int v)
        {
            v = Vector3Int.zero;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 3)
            {
                v.x = int.Parse(collection[0].Value);
                v.y = int.Parse(collection[1].Value);
                v.z = int.Parse(collection[2].Value);
                return true;
            }
            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(Vector4))]
        public static bool ParseVector4(string str, out Vector4 v)
        {
            v = Vector4.zero;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 4)
            {
                v.x = float.Parse(collection[0].Value);
                v.y = float.Parse(collection[1].Value);
                v.z = float.Parse(collection[2].Value);
                v.w = float.Parse(collection[3].Value);
                return true;
            }
            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(Quaternion))]
        public static bool ParseQuaternion(string str, out Quaternion v)
        {
            v = Quaternion.identity;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 4)
            {
                v.x = float.Parse(collection[0].Value);
                v.y = float.Parse(collection[1].Value);
                v.z = float.Parse(collection[2].Value);
                v.w = float.Parse(collection[3].Value);
                return true;
            }
            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseString, typeof(Color))]
        public static bool ParseColor(string str, out Color v)
        {
            v = Color.black;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 3)
            {
                v.r = float.Parse(collection[0].Value);
                v.g = float.Parse(collection[1].Value);
                v.b = float.Parse(collection[2].Value);
                v.a = 1;
                return true;
            }
            if (collection.Count == 4)
            {
                v.r = float.Parse(collection[0].Value);
                v.g = float.Parse(collection[1].Value);
                v.b = float.Parse(collection[2].Value);
                v.a = float.Parse(collection[3].Value);
                return true;
            }
            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(byte))]
        public string ParseByte(byte v)
        {
            return string.Format("{0}", v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(bool))]
        public string ParseBool(bool v)
        {
            return string.Format("{0}", v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(float))]
        public string ParseFloat(float v)
        {
            return string.Format("{0}", v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(short))]
        public string ParseShort(short v)
        {
            return string.Format("{0}", v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(int))]
        public static string ParseInt(int v)
        {
            return string.Format("{0}", v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(long))]
        public static string ParseLong(long v)
        {
            return string.Format("{0}", v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(ushort))]
        public static string ParseUShort(ushort v)
        {
            return string.Format("{0}", v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(uint))]
        public static string ParseUInt(uint v)
        {
            return string.Format("{0}", v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(ulong))]
        public static string ParseULong(ulong v)
        {
            return string.Format("{0}", v);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(string))]
        public static string ParseString(string v)
        {
            return v;
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(Matrix4x4))]
        public static string ParseMatrix4x4(Matrix4x4 v)
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                v.m00,
                v.m01,
                v.m02,
                v.m03,
                v.m10,
                v.m11,
                v.m12,
                v.m13,
                v.m20,
                v.m21,
                v.m22,
                v.m23,
                v.m30,
                v.m31,
                v.m32,
                v.m33
                );
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(Vector2))]
        public static string ParseVector2(Vector2 v)
        {
            return string.Format("{0},{1}", v.x, v.y);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(Vector3))]
        public static string ParseVector3(Vector3 v)
        {
            return string.Format("{0},{1},{2}", v.x, v.y, v.z);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(Vector3Int))]
        public static string ParseVector3Int(Vector3Int v)
        {
            return string.Format("{0},{1},{2}", v.x, v.y, v.z);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(Vector4))]
        public static string ParseVector4(Vector4 v)
        {
            return string.Format("{0},{1},{2},{3}", v.x, v.y, v.z, v.w);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(Quaternion))]
        public static string ParseQuaternion(Quaternion v)
        {
            return string.Format("{0},{1},{2},{3}", v.x, v.y, v.z, v.w);
        }
        [GameDataReadWrite(GameDataReadWriteType.ParseObject, typeof(Color))]
        public static string ParseColor(Color v)
        {
            return string.Format("{0},{1},{2},{3}", v.r, v.g, v.b, v.a);
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(byte[]))]
        private static byte[] ReadBytes(NullMemoryStream stream, int count)
        {
            if (stream.CanRead(count))
            {
                byte[] bytes = CacheBytes;
                if (CacheLen > count)
                {
                    bytes = new byte[count];
                }
                int size = stream.Read(bytes, 0, count);
                return bytes;
            }
            return null;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(byte))]
        public static bool ReadByte(NullMemoryStream stream, out byte v)
        {
            v = 0;
            byte[] bytes = ReadBytes(stream, sizeof(byte));
            if (bytes == null)
            {
                return false;
            }
            v = bytes[0];
            return true;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(bool))]
        public static bool ReadBool(NullMemoryStream stream, out bool v)
        {
            v = false;
            byte[] bytes = ReadBytes(stream, sizeof(bool));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToBoolean(bytes, 0);
            return true;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(float))]
        public static bool ReadFloat(NullMemoryStream stream, out float v)
        {
            v = 0;
            byte[] bytes = ReadBytes(stream, sizeof(float));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToSingle(bytes, 0);
            return true;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(short))]
        public static bool ReadShort(NullMemoryStream stream, out short v)
        {
            v = 0;
            byte[] bytes = ReadBytes(stream, sizeof(short));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToInt16(bytes, 0);
            return true;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(int))]
        public static bool ReadInt(NullMemoryStream stream, out int v)
        {
            v = 0;
            byte[] bytes = ReadBytes(stream, sizeof(int));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToInt32(bytes, 0);
            return true;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(long))]
        public static bool ReadLong(NullMemoryStream stream, out long v)
        {
            v = 0;
            byte[] bytes = ReadBytes(stream, sizeof(long));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToInt64(bytes, 0);
            return true;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(ushort))]
        public static bool ReadUShort(NullMemoryStream stream, out ushort v)
        {
            v = 0;
            byte[] bytes = ReadBytes(stream, sizeof(ushort));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToUInt16(bytes, 0);
            return true;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(uint))]
        public static bool ReadUInt(NullMemoryStream stream, out uint v)
        {
            v = 0;
            byte[] bytes = ReadBytes(stream, sizeof(uint));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToUInt32(bytes, 0);
            return true;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(ulong))]
        public static bool ReadULong(NullMemoryStream stream, out ulong v)
        {
            v = 0;
            byte[] bytes = ReadBytes(stream, sizeof(ulong));
            if (bytes == null)
            {
                return false;
            }
            v = BitConverter.ToUInt64(bytes, 0);
            return true;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(string))]
        public static bool ReadString(NullMemoryStream stream, out string v)
        {
            v = null;
            ushort len = 0;
            if (ReadUShort(stream, out len))
            {
                byte[] bytes = ReadBytes(stream, len);
                if (bytes == null)
                {
                    return false;
                }
                v = Encoding.UTF8.GetString(bytes, 0, len);
            }
            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(Vector2))]
        public static bool ReadVector2(NullMemoryStream stream, out Vector2 v)
        {
            v = Vector2.zero;
            float x = 0, y = 0;
            if (ReadFloat(stream, out x) && ReadFloat(stream, out y))
            {
                v = new Vector2(x, y);
                return true;
            }
            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(Vector3))]
        public static bool ReadVector3(NullMemoryStream stream, out Vector3 v)
        {
            v = Vector3.zero;
            float x = 0, y = 0, z = 0;
            if (ReadFloat(stream, out x) && ReadFloat(stream, out y) && ReadFloat(stream, out z))
            {
                v = new Vector3(x, y, z);
                return true;
            }
            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(Vector4))]
        public static bool ReadVector4(NullMemoryStream stream, out Vector4 v)
        {
            v = Vector4.zero;
            float x = 0, y = 0, z = 0, w = 0;
            if (ReadFloat(stream, out x) && ReadFloat(stream, out y) && ReadFloat(stream, out z) && ReadFloat(stream, out w))
            {
                v = new Vector4(x, y, z, w);
                return true;
            }
            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(Quaternion))]
        public static bool ReadQuaternion(NullMemoryStream stream, out Quaternion v)
        {
            v = Quaternion.identity;
            float x = 0, y = 0, z = 0, w = 0;
            if (ReadFloat(stream, out x) && ReadFloat(stream, out y) && ReadFloat(stream, out z) && ReadFloat(stream, out w))
            {
                v = new Quaternion(x, y, z, w);
                return true;
            }
            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(Color))]
        public static bool ReadColor(NullMemoryStream stream, out Color v)
        {
            v = Color.black;
            float r = 0, g = 0, b = 0, a = 0;
            if (ReadFloat(stream, out r) && ReadFloat(stream, out g) && ReadFloat(stream, out b) && ReadFloat(stream, out a))
            {
                v = new Color(r, g, b, a);
                return true;
            }
            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(Matrix4x4))]
        public static bool ReadMatrix4x4(NullMemoryStream stream, out Matrix4x4 m)
        {
            m = Matrix4x4.zero;
            Vector4 col0 = Vector4.zero;
            Vector4 col1 = Vector4.zero;
            Vector4 col2 = Vector4.zero;
            Vector4 col3 = Vector4.zero;
            if (ReadVector4(stream, out col0) && ReadVector4(stream, out col1) && ReadVector4(stream, out col2) && ReadVector4(stream, out col3))
            {
                m.SetColumn(0, col0);
                m.SetColumn(1, col1);
                m.SetColumn(2, col2);
                m.SetColumn(3, col3);
                return true;
            }

            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.Read, typeof(Vector3Int))]
        public static bool ReadVector3Int(NullMemoryStream stream, ref Vector3Int v)
        {
            v = Vector3Int.zero;
            int x = 0, y = 0, z = 0;
            if (ReadInt(stream, out x) && ReadInt(stream, out y) && ReadInt(stream, out z))
            {
                v.x = x;
                v.y = y;
                v.z = z;
                return true;
            }
            return false;
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(string))]
        public static int WriteString(NullMemoryStream stream, string str)
        {
            if (str == null)
            {
                str = "";
            }
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            ushort len = (ushort)bytes.Length;
            int size = WriteUShort(stream, len);
            size += WriteBytes(stream, bytes, len);
            return size;
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(byte[]))]
        public static int WriteBytes(NullMemoryStream stream, byte[] bytes, int count)
        {
            stream.Write(bytes, 0, count);
            return bytes.Length;
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(byte))]
        public static int WriteByte(NullMemoryStream stream, byte value)
        {
            WriteByte(stream, value);
            return sizeof(byte);
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(bool))]
        public static int WriteBool(NullMemoryStream stream, bool value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(stream, bytes, bytes.Length);
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(float))]
        public static int WriteFloat(NullMemoryStream stream, float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(stream, bytes, bytes.Length);
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(short))]
        public static int WriteShort(NullMemoryStream stream, short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(stream, bytes, bytes.Length);
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(int))]
        public static int WriteInt(NullMemoryStream stream, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(stream, bytes, bytes.Length);
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(long))]
        public static int WriteLong(NullMemoryStream stream, long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(stream, bytes, bytes.Length);
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(ushort))]
        public static int WriteUShort(NullMemoryStream stream, ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(stream, bytes, bytes.Length);
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(uint))]
        public static int WriteUInt(NullMemoryStream stream, uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(stream, bytes, bytes.Length);
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(ulong))]
        public static int WriteULong(NullMemoryStream stream, ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytes(stream, bytes, bytes.Length);
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(Vector2))]
        public static int WriteVector2(NullMemoryStream stream, Vector2 value)
        {
            int size = WriteFloat(stream, value.x);
            size += WriteFloat(stream, value.y);
            return size;
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(Vector3))]
        public static int WriteVector3(NullMemoryStream stream, Vector3 value)
        {
            int size = WriteFloat(stream, value.x);
            size += WriteFloat(stream, value.y);
            size += WriteFloat(stream, value.z);
            return size;
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(Vector4))]
        public static int WriteVector4(NullMemoryStream stream, Vector4 value)
        {
            int size = WriteFloat(stream, value.x);
            size += WriteFloat(stream, value.y);
            size += WriteFloat(stream, value.z);
            size += WriteFloat(stream, value.w);
            return size;
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(Quaternion))]
        public static int WriteQuaternion(NullMemoryStream stream, Quaternion value)
        {
            int size = WriteFloat(stream, value.x);
            size += WriteFloat(stream, value.y);
            size += WriteFloat(stream, value.z);
            size += WriteFloat(stream, value.w);
            return size;
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(Color))]
        public static int WriteColor(NullMemoryStream stream, Color value)
        {
            int size = WriteFloat(stream, value.r);
            size += WriteFloat(stream, value.g);
            size += WriteFloat(stream, value.b);
            size += WriteFloat(stream, value.a);
            return size;
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(Matrix4x4))]
        public static int WriteMatrix4x4(NullMemoryStream stream, Matrix4x4 value)
        {
            int size = WriteVector4(stream, value.GetColumn(0));
            size += WriteVector4(stream, value.GetColumn(1));
            size += WriteVector4(stream, value.GetColumn(2));
            size += WriteVector4(stream, value.GetColumn(3));
            return size;
        }
        [GameDataReadWrite(GameDataReadWriteType.Write, typeof(Vector3Int))]
        public static int WriteVector3Int(NullMemoryStream stream, Vector3Int value)
        {
            int size = WriteInt(stream, value.x);
            size += WriteInt(stream, value.y);
            size += WriteInt(stream, value.z);
            return size;
        }
    }
}
