﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
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
        ToString,
        ToObject,
    }

    public partial class GameDataUtils
    {
        /// <summary>
        /// 不支持 List 嵌套 其他 集合 类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string ToXml<T>(List<T> target)
        {
            string name = typeof(T).Name;
            SecurityElement root = new SecurityElement(name + "s");
            PropertyInfo[] infos = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (T t in target)
            {
                SecurityElement child = new SecurityElement(name);
                root.AddChild(child);
                foreach (PropertyInfo info in infos)
                {
                    object value = info.GetValue(t, null);
                    object defaultV = GetDefault(info.PropertyType);
                    if (value != null && !value.Equals(defaultV))
                    {
                        string v = ToString(value);
                        child.AddAttribute(info.Name, v);
                    }
                }
            }
            return root.ToString();
        }

        public static string ToString<T>(T target)
        {
            Type type = typeof(T);
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    object ret = typeof(GameDataUtils).GetMethod("ToListString", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                        .MakeGenericMethod(type.GetGenericArguments())
                        .Invoke(null, new object[] { target });
                    return ret != null ? ret.ToString() : null;
                }
                else if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    object ret = typeof(GameDataUtils).GetMethod("ToMapString", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                        .MakeGenericMethod(type.GetGenericArguments())
                        .Invoke(null, new object[] { target });
                    return ret != null ? ret.ToString() : null;
                }
                DebugUtils.Assert(false, "ToString Not Found Type: " + typeof(T).FullName);
                return null;
            }
            else if (type.BaseType == typeof(Enum))
            {
                return EnumUtils.EnumToString(target);
            }
            MethodInfo info = GetToStringMethod<T>();
            DebugUtils.Assert(info != null, "GetToStringMethod Not Found Type: " + typeof(T).FullName);
            GenericParametersObjectOne[0] = target;
            object result = info.Invoke(null, GenericParametersObjectOne);
            return result != null ? result.ToString() : null;
        }

        public static T ToObject<T>(string str)
        {
            return (T)ToObject(str, typeof(T));
        }

        public static object ToObject(string str, Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type[] types = type.GetGenericArguments();
                    Type t = types[0];
                    List<string> list = ToListObject(str);
                    var result = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                    foreach (var item in list)
                    {
                        var v = ToObject(item, t);
                        type.GetMethod("Add").Invoke(result, new object[] { v });
                    }
                    return result;
                }
                else if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    Type[] types = type.GetGenericArguments();
                    Dictionary<string, string> map = ToMapObject(str);
                    var result = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                    foreach (var item in map)
                    {
                        var key = ToObject(item.Key, types[0]);
                        var v = ToObject(item.Value, types[1]);
                        type.GetMethod("Add").Invoke(result, new object[] { key, v });
                    }
                    return result;
                }
                DebugUtils.Assert(false, "ToObject Not Found Type: " + type.FullName);
                return null;
            }
            else if (type.BaseType == typeof(Enum))
            {
                return EnumUtils.StringToEnum(str, type);
            }
            GenericParameterTypesOne[0] = type;
            MethodInfo keyMethod = typeof(GameDataUtils).GetMethod("GetToObjectMethod", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).MakeGenericMethod(GenericParameterTypesOne);
            DebugUtils.Assert(keyMethod != null, "GetToObjectMethod Not Found Type1: " + type.FullName);
            MethodInfo method = (MethodInfo)keyMethod.Invoke(null, null);
            DebugUtils.Assert(method != null, "GetToObjectMethod Not Found Type2: " + type.FullName);
            GenericParametersObjectTwo[0] = str;
            GenericParametersObjectTwo[1] = GetDefault(type);
            bool res = (bool)method.Invoke(null, GenericParametersObjectTwo);
            DebugUtils.Assert(res, string.Format("Data {0} Not Right Type {1}", str, type.FullName));
            return GenericParametersObjectTwo[1];
        }
    }

    public partial class GameDataUtils
    {
        private const int CacheLen = 1024 * 1024;
        private static byte[] CacheBytes = new byte[CacheLen];

        private const char KEY_VALUE_SPRITER = ':';
        private const char MAP_SPRITER = ',';
        private const char LIST_SPRITER = ';';
        private static Regex RegexVector = new Regex("-?\\d+\\.\\d+");
        private static Dictionary<Type, MethodInfo> ReadMethodMap = new Dictionary<Type, MethodInfo>();
        private static Dictionary<Type, MethodInfo> WriteMethodMap = new Dictionary<Type, MethodInfo>();
        private static Dictionary<Type, MethodInfo> ToObjectMethodMap = new Dictionary<Type, MethodInfo>();
        private static Dictionary<Type, MethodInfo> ToStringMethodMap = new Dictionary<Type, MethodInfo>();
        private static object[] GenericParametersObjectOne = new object[1];
        private static object[] GenericParametersObjectTwo = new object[2];
        private static Type[] GenericParameterTypesOne = new Type[1];

        static GameDataUtils()
        {
            ReadMethodMap.Clear();
            WriteMethodMap.Clear();
            ToObjectMethodMap.Clear();
            ToStringMethodMap.Clear();
            Type type = typeof(GameDataUtils);
            MethodInfo[] infos = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);
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
                        case GameDataReadWriteType.ToString:
                            ToStringMethodMap.Add(readWriteType.Type, info);
                            break;
                        case GameDataReadWriteType.ToObject:
                            ToObjectMethodMap.Add(readWriteType.Type, info);
                            break;
                    }
                }
            }
        }

        protected static MatchCollection MatchVector(string inputString)
        {
            return RegexVector.Matches(inputString);
        }

        protected static MethodInfo GetMethod<T>(Dictionary<Type, MethodInfo> readWriteMap)
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

        protected static MethodInfo GetToObjectMethod<T>()
        {
            return GetMethod<T>(ToObjectMethodMap);
        }

        protected static MethodInfo GetToStringMethod<T>()
        {
            return GetMethod<T>(ToStringMethodMap);
        }

        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private static string ToListString<T>(List<T> list)
        {
            if (list.Count == 0)
            {
                return null;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in list)
                {
                    string key = ToString(item);
                    sb.AppendFormat("{0}{1}", key, LIST_SPRITER);
                }
                sb.Remove(sb.Length - 1, 1);
                return sb.ToString();
            }
        }

        private static string ToMapString<T, U>(IEnumerable<KeyValuePair<T, U>> map)
        {
            if (map == null || map.Count() == 0)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            foreach (var item in map)
            {
                string key = ToString<T>(item.Key);
                string value = ToString<U>(item.Value);
                sb.AppendFormat("{0}{1}{2}{3}", key, KEY_VALUE_SPRITER, value, MAP_SPRITER);
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        private static Dictionary<string, string> ToMapObject(string str)
        {
            Dictionary<string, string>  map = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(str))
            {
                return map;
            }
            string[] pairs = str.Trim().Split(MAP_SPRITER);
            for (int i = 0; i < pairs.Length; ++i)
            {
                string[] pair = pairs[i].Split(KEY_VALUE_SPRITER);
                DebugUtils.Assert(pair.Length == 2, "");
                map.Add(pair[0].Trim(), pair[1].Trim());
            }
            return map;
        }

        private static List<string> ToListObject(string str)
        {
            List<string> values = new List<string>();
            if (string.IsNullOrEmpty(str))
            {
                return values;
            }
            string[] strs = str.Trim().Split(LIST_SPRITER);
            for (int i = 0; i < strs.Length; ++i)
            {
                values.Add(strs[i].Trim());
            }
            return values;
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(byte))]
        private static bool ToObject(string str, out byte v)
        {
            return byte.TryParse(str, out v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(bool))]
        private static bool ToObject(string str, out bool v)
        {
            return bool.TryParse(str, out v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(float))]
        private static bool ToObject(string str, out float v)
        {
            return float.TryParse(str, out v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(short))]
        private static bool ToObject(string str, out short v)
        {
            return short.TryParse(str, out v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(int))]
        private static bool ToObject(string str, out int v)
        {
            return int.TryParse(str, out v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(long))]
        private static bool ToObject(string str, out long v)
        {
            return long.TryParse(str, out v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(ushort))]
        private static bool ToObject(string str, out ushort v)
        {
            return ushort.TryParse(str, out v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(uint))]
        private static bool ToObject(string str, out uint v)
        {
            return uint.TryParse(str, out v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(ulong))]
        private static bool ToObject(string str, out ulong v)
        {
            return ulong.TryParse(str, out v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(string))]
        private static bool ToObject(string str, out string v)
        {
            v = str;
            return true;
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(DateTime))]
        private static bool ToObject(string str, out DateTime v)
        {
            v = DateTimeUtils.GetTime(str);
            return true;
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(Matrix4x4))]
        private static bool ToObject(string str, out Matrix4x4 v)
        {
            v = Matrix4x4.zero;
            if (str != null)
            {
                MatchCollection mc = MatchVector(str);
                if (mc.Count == 16)
                {

                    bool res = float.TryParse(mc[0].Value, out v.m00);
                    res &= float.TryParse(mc[1].Value, out v.m01);
                    res &= float.TryParse(mc[2].Value, out v.m02);
                    res &= float.TryParse(mc[3].Value, out v.m03);
                    res &= float.TryParse(mc[4].Value, out v.m10);
                    res &= float.TryParse(mc[5].Value, out v.m11);
                    res &= float.TryParse(mc[6].Value, out v.m12);
                    res &= float.TryParse(mc[7].Value, out v.m13);
                    res &= float.TryParse(mc[8].Value, out v.m20);
                    res &= float.TryParse(mc[9].Value, out v.m21);
                    res &= float.TryParse(mc[10].Value, out v.m22);
                    res &= float.TryParse(mc[11].Value, out v.m23);
                    res &= float.TryParse(mc[12].Value, out v.m30);
                    res &= float.TryParse(mc[13].Value, out v.m31);
                    res &= float.TryParse(mc[14].Value, out v.m32);
                    res &= float.TryParse(mc[15].Value, out v.m33);
                    return res;
                }
                DebugUtils.Assert(false, string.Format("Error attempting to parse property {0} as an Matrix4x4.", str));
            }
            return false;
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(Vector2))]
        private static bool ToObject(string str, out Vector2 v)
        {
            v = Vector2.zero;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 2)
            {
                bool res = float.TryParse(collection[0].Value, out v.x);
                res &= float.TryParse(collection[1].Value, out v.y);
                return true;
            }
            return false;
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(Vector3))]
        private static bool ToObject(string str, out Vector3 v)
        {
            v = Vector3.zero;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 3)
            {
                bool res = float.TryParse(collection[0].Value, out v.x);
                res &= float.TryParse(collection[1].Value, out v.y);
                res &= float.TryParse(collection[2].Value, out v.z);
                return true;
            }
            return false;
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(Vector3Int))]
        private static bool ToObject(string str, out Vector3Int v)
        {
            v = Vector3Int.zero;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 3)
            {
                int m,n,p;
                bool res = int.TryParse(collection[0].Value, out m);
                res &= int.TryParse(collection[1].Value, out n);
                res &= int.TryParse(collection[2].Value, out p);
                v.Set(m, n, p);
                return true;
            }
            return false;
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(Vector4))]
        private static bool ToObject(string str, out Vector4 v)
        {
            v = Vector4.zero;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 4)
            {
                bool res = float.TryParse(collection[0].Value, out v.x);
                res &= float.TryParse(collection[1].Value, out v.y);
                res &= float.TryParse(collection[2].Value, out v.z);
                res &= float.TryParse(collection[3].Value, out v.w);
                return true;
            }
            return false;
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(Quaternion))]
        private static bool ToObject(string str, out Quaternion v)
        {
            v = Quaternion.identity;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 4)
            {
                bool res = float.TryParse(collection[0].Value, out v.x);
                res &= float.TryParse(collection[1].Value, out v.y);
                res &= float.TryParse(collection[2].Value, out v.z);
                res &= float.TryParse(collection[3].Value, out v.w);
                return true;
            }
            return false;
        }

        [GameDataReadWrite(GameDataReadWriteType.ToObject, typeof(Color))]
        private static bool ToObject(string str, out Color v)
        {
            v = Color.black;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 3)
            {
                bool res = float.TryParse(collection[0].Value, out v.r);
                res &= float.TryParse(collection[1].Value, out v.g);
                res &= float.TryParse(collection[2].Value, out v.b);
                v.a = 1;
                return true;
            }
            if (collection.Count == 4)
            {
                bool res = float.TryParse(collection[0].Value, out v.r);
                res &= float.TryParse(collection[1].Value, out v.g);
                res &= float.TryParse(collection[2].Value, out v.b);
                res &= float.TryParse(collection[3].Value, out v.a);
                return true;
            }
            return false;
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(byte))]
        private static string ToString(byte v)
        {
            return string.Format("{0}", v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(bool))]
        private static string ToString(bool v)
        {
            return string.Format("{0}", v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(float))]
        private static string ToString(float v)
        {
            return string.Format("{0}", v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(short))]
        private static string ToString(short v)
        {
            return string.Format("{0}", v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(int))]
        private static string ToString(int v)
        {
            return string.Format("{0}", v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(long))]
        private static string ToString(long v)
        {
            return string.Format("{0}", v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(ushort))]
        private static string ToString(ushort v)
        {
            return string.Format("{0}", v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(uint))]
        private static string ToString(uint v)
        {
            return string.Format("{0}", v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(ulong))]
        private static string ToString(ulong v)
        {
            return string.Format("{0}", v);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(string))]
        private static string ToString(string v)
        {
            return v;
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(DateTime))]
        private static string ToString(DateTime dt)
        {
            return DateTimeUtils.FormatTimeHMS(dt);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(Matrix4x4))]
        private static string ToString(Matrix4x4 v)
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

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(Vector2))]
        private static string ToString(Vector2 v)
        {
            return string.Format("{0},{1}", v.x, v.y);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(Vector3))]
        private static string ToString(Vector3 v)
        {
            return string.Format("{0},{1},{2}", v.x, v.y, v.z);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(Vector3Int))]
        private static string ToString(Vector3Int v)
        {
            return string.Format("{0},{1},{2}", v.x, v.y, v.z);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(Vector4))]
        private static string ToString(Vector4 v)
        {
            return string.Format("{0},{1},{2},{3}", v.x, v.y, v.z, v.w);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(Quaternion))]
        private static string ToString(Quaternion v)
        {
            return string.Format("{0},{1},{2},{3}", v.x, v.y, v.z, v.w);
        }

        [GameDataReadWrite(GameDataReadWriteType.ToString, typeof(Color))]
        private static string ToString(Color v)
        {
            return string.Format("{0},{1},{2},{3}", v.r, v.g, v.b, v.a);
        }

    }

    public partial class GameDataUtils
    {
        protected static bool IsStreamType<T>()
        {
            return typeof(T).GetInterface("INullStream", false) != null;
        }

        protected static MethodInfo GetReadMethod<T>()
        {
            return GetMethod<T>(ReadMethodMap);
        }

        protected static MethodInfo GetWriteMethod<T>()
        {
            return GetMethod<T>(WriteMethodMap);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="useCount">-1表示需要读int；表示不读任何数据,返回 true</param>
        /// <returns></returns>
        public static bool ReadMap<U, V>(NullMemoryStream stream, out Dictionary<U, V> map, int useCount) where V : new()
        {
            map = new Dictionary<U, V>();
            if (useCount == 0)
            {
                return true;
            }
            MethodInfo keyMethod = GetReadMethod<U>();
            MethodInfo valueMethod = GetReadMethod<V>();
            DebugUtils.Assert(keyMethod != null && valueMethod != null, string.Format("ReadMap Not Found Type U:{0},V:{1}", typeof(U).FullName, typeof(V).FullName));
            bool res = false;
            int count = useCount;
            // 处理 -1 和 大于0 的情况
            if (count > 0 || ReadInt(stream, out count))
            {
                U u = default(U);
                V v = IsStreamType<V>() ? new V() : default(V);
                GenericParametersObjectTwo[0] = stream;
                for (int i = 0; i < count; ++i)
                {
                    GenericParametersObjectTwo[1] = u;
                    res &= (bool)keyMethod.Invoke(null, GenericParametersObjectTwo);
                    u = (U)GenericParametersObjectTwo[1];
                    GenericParametersObjectTwo[1] = v;
                    res &= (bool)valueMethod.Invoke(null, GenericParametersObjectTwo);
                    v = (V)GenericParametersObjectTwo[1];
                    map.Add(u, v);
                }
            }
            return res;
        }

        public static int WriteMap<U, V>(NullMemoryStream stream, Dictionary<U, V> values, bool ignoreWriteCount)
        {
            MethodInfo keyMethod = GetWriteMethod<U>();
            MethodInfo valueMethod = GetWriteMethod<V>();
            DebugUtils.Assert(keyMethod != null && valueMethod != null, string.Format("WriteMap Not Found Type U:{0},V:{1}", typeof(U).FullName, typeof(V).FullName));
            int size = ignoreWriteCount ? 0 : WriteInt(stream, values.Count);
            GenericParametersObjectTwo[0] = stream;
            foreach (var pair in values)
            {
                GenericParametersObjectTwo[1] = pair.Key;
                size += (int)keyMethod.Invoke(null, GenericParametersObjectTwo);
                GenericParametersObjectTwo[1] = pair.Value;
                size += (int)valueMethod.Invoke(null, GenericParametersObjectTwo);
            }
            return size;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="useCount">-1表示需要读int；表示不读任何数据,返回 true</param>
        /// <returns></returns>
        public static bool ReadList<T>(NullMemoryStream stream, out List<T> values, int useCount) where T : new()
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
                DebugUtils.Assert(info != null, "" + typeof(T).FullName);
                GenericParametersObjectTwo[0] = stream;
                GenericParametersObjectTwo[1] = v;
                for (int i = 0; i < count; ++i)
                {
                    info.Invoke(null, GenericParametersObjectTwo);
                    values.Add((T)GenericParametersObjectTwo[1]);
                }
                return true;
            }
            return false;
        }

        public static int WriteList<T>(NullMemoryStream stream, List<T> values, bool ignoreWriteCount)
        {
            int size = ignoreWriteCount ? 0 : WriteInt(stream, values.Count);
            MethodInfo info = GetWriteMethod<T>();
            DebugUtils.Assert(info != null, "" + typeof(T).FullName);
            GenericParametersObjectTwo[0] = stream;
            for (int i = 0; i < values.Count; ++i)
            {
                GenericParametersObjectTwo[1] = values[i];
                size += (int)info.Invoke(null, GenericParametersObjectTwo);
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
