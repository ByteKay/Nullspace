using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;

namespace Nullspace
{
    public static class XmlFileUtils
    {
        /// <summary>
        /// 键值分隔符： ‘:’
        /// </summary>
        private const Char KEY_VALUE_SPRITER = ':';
        /// <summary>
        /// 字典项分隔符： ‘,’
        /// </summary>
        private const Char MAP_SPRITER = ',';
        /// <summary>
        /// 数组分隔符： ','
        /// </summary>
        private const Char LIST_SPRITER = ';';

        // 可以扩展成递归模式，多个类型嵌套的
        public static void SaveXML<T>(string path, List<T> data)
        {
            string attrName = typeof(T).Name;
            var root = new System.Security.SecurityElement(attrName + "s");
            
            var props = typeof(T).GetProperties();
            var keyProp = props[0];         
            foreach (var prop in props)
            {
                if (XmlData.mKeyFieldName == prop.Name)
                {
                    keyProp = prop;
                    break;
                }
            }
            foreach (var item in data)
            {
                // 下面在LoadXML时，key id必须先加载，所以这里第一个创建 id
                var xml = new System.Security.SecurityElement(attrName);
                object obj = keyProp.GetGetMethod().Invoke(item, null);
                xml.AddChild(new System.Security.SecurityElement(keyProp.Name, obj.ToString()));
                foreach (var prop in props)
                {
                    if (prop == keyProp)
                    {
                        continue;
                    }
                    var type = prop.PropertyType;
                    String result = String.Empty;
                    obj = prop.GetGetMethod().Invoke(item, null);
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        object ret = typeof(XmlFileUtils).GetMethod("PackMap")
                        .MakeGenericMethod(type.GetGenericArguments())
                        .Invoke(null, new object[] { obj, KEY_VALUE_SPRITER, MAP_SPRITER });
                        result = ret != null ? ret.ToString() : null;
                    }
                    else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        object ret = typeof(XmlFileUtils).GetMethod("PackList")
                        .MakeGenericMethod(type.GetGenericArguments())
                        .Invoke(null, new object[] { obj, LIST_SPRITER });
                        result = ret != null ? ret.ToString() : null;
                    }
                    else if (type.BaseType == typeof(Enum))
                    {
                        Type underType = Enum.GetUnderlyingType(type);
                        if (underType == typeof(Int32))
                        {
                            obj = EnumUtils.EnumToInt(obj) + "";
                        }
                        result = obj.ToString();
                    }
                    else
                    {
                        result = obj != null ? obj.ToString() : null;
                    }
                    if (result != null)
                    {
                        xml.AddChild(new System.Security.SecurityElement(prop.Name, result));
                    }
                }
                root.AddChild(xml);
                
            }
            // 格式化
            XElement element = XElement.Parse(root.ToString());
            string output = path;
            if (!output.EndsWith(".xml"))
            {
                output = output + ".xml";
            }
            XmlFileUtils.SaveText(output, element.ToString());
        }

        public static List<T> LoadXML<T>(string path)
        {
            var text = LoadTextFile(path);
            List<T> list = new List<T>();
            try
            {
                if (String.IsNullOrEmpty(text))
                {
                    return list;
                }
                Type type = typeof(T);
                var xml = XmlFileUtils.LoadXML(text);
                Dictionary<Int32, Dictionary<String, String>> map = XmlFileUtils.LoadIntMap(xml, text);
                var props = type.GetProperties();
                foreach (var item in map)
                {
                    var obj = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                    foreach (var prop in props)
                    {
                        if (prop.Name == XmlData.mKeyFieldName)
                        {
                            prop.SetValue(obj, item.Key, null);
                        }
                        else
                            try
                            {
                                if (item.Value.ContainsKey(prop.Name))
                                {
                                    var value = GetValue(item.Value[prop.Name], prop.PropertyType);
                                    prop.SetValue(obj, value, null);
                                }
                            }
                            catch (Exception ex)
                            {
                                DebugUtils.Error("XmlData", "LoadXML error {0}: {1} {2}", ex.Message, item.Value[prop.Name], prop.PropertyType );
                            }
                    }
                    list.Add((T)obj);
                }
            }
            catch (Exception ex)
            {
                DebugUtils.Error("XmlData", "Error {} : {}", ex.Message, text);
            }
            return list;
        }

        public static string LoadTextFile(String fileName)
        {
            if (File.Exists(fileName))
            {
                using (StreamReader sr = File.OpenText(fileName))
                {
                    return sr.ReadToEnd();
                }
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// 将字典字符串转换为键类型与值类型都为整型的字典对象。
        /// </summary>
        /// <param name="strMap">字典字符串</param>
        /// <param name="keyValueSpriter">键值分隔符</param>
        /// <param name="mapSpriter">字典项分隔符</param>
        /// <returns>字典对象</returns>
        public static Dictionary<Int32, Int32> ParseMapIntInt(String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
        {
            Dictionary<Int32, Int32> result = new Dictionary<Int32, Int32>();
            var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
            foreach (var item in strResult)
            {
                int key;
                int value;
                if (int.TryParse(item.Key, out key) && int.TryParse(item.Value, out value))
                {
                    result.Add(key, value);
                }
                else
                {
                    DebugUtils.Error("XmlData", String.Format("Parse failure: {0}, {1}", item.Key, item.Value));
                }
            }
            return result;
        }
        /// <summary>
        /// 将字典字符串转换为键类型为整型，值类型为单精度浮点数的字典对象。
        /// </summary>
        /// <param name="strMap">字典字符串</param>
        /// <param name="keyValueSpriter">键值分隔符</param>
        /// <param name="mapSpriter">字典项分隔符</param>
        /// <returns>字典对象</returns>
        public static Dictionary<Int32, float> ParseMapIntFloat(String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
        {
            var result = new Dictionary<Int32, float>();
            var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
            foreach (var item in strResult)
            {
                int key;
                float value;
                if (int.TryParse(item.Key, out key) && float.TryParse(item.Value, out value))
                {
                    result.Add(key, value);
                }
                else
                {
                    DebugUtils.Error("XmlData", String.Format("Parse failure: {0}, {1}", item.Key, item.Value));
                }
            }
            return result;
        }
        /// <summary>
        /// 将字典字符串转换为键类型为整型，值类型为字符串的字典对象。
        /// </summary>
        /// <param name="strMap">字典字符串</param>
        /// <param name="keyValueSpriter">键值分隔符</param>
        /// <param name="mapSpriter">字典项分隔符</param>
        /// <returns>字典对象</returns>
        public static Dictionary<Int32, String> ParseMapIntString(String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
        {
            Dictionary<Int32, String> result = new Dictionary<Int32, String>();
            var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
            foreach (var item in strResult)
            {
                int key;
                if (int.TryParse(item.Key, out key))
                {
                    result.Add(key, item.Value);
                }
                else
                {
                    DebugUtils.Error("XmlData", String.Format("Parse failure: {0}", item.Key));
                }
            }
            return result;
        }
        /// <summary>
        /// 将字典字符串转换为键类型为字符串，值类型为单精度浮点数的字典对象。
        /// </summary>
        /// <param name="strMap">字典字符串</param>
        /// <param name="keyValueSpriter">键值分隔符</param>
        /// <param name="mapSpriter">字典项分隔符</param>
        /// <returns>字典对象</returns>
        public static Dictionary<String, float> ParseMapStringFloat(String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
        {
            Dictionary<String, float> result = new Dictionary<String, float>();
            var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
            foreach (var item in strResult)
            {
                float value;
                if (float.TryParse(item.Value, out value))
                {
                    result.Add(item.Key, value);
                }
                else
                {
                    DebugUtils.Error("XmlData", String.Format("Parse failure: {0}", item.Value));
                }
            }
            return result;
        }
        /// <summary>
        /// 将字典字符串转换为键类型为字符串，值类型为整型的字典对象。
        /// </summary>
        /// <param name="strMap">字典字符串</param>
        /// <param name="keyValueSpriter">键值分隔符</param>
        /// <param name="mapSpriter">字典项分隔符</param>
        /// <returns>字典对象</returns>
        public static Dictionary<String, Int32> ParseMapStringInt(String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
        {
            Dictionary<String, Int32> result = new Dictionary<String, Int32>();
            var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
            foreach (var item in strResult)
            {
                int value;
                if (int.TryParse(item.Value, out value))
                {
                    result.Add(item.Key, value);
                }
                else
                {
                    DebugUtils.Error("XmlData", String.Format("Parse failure: {0}", item.Value));
                }
            }
            return result;
        }
        /// <summary>
        /// 将字典字符串转换为键类型为 T，值类型为 U 的字典对象。
        /// </summary>
        /// <typeparam name="T">字典Key类型</typeparam>
        /// <typeparam name="U">字典Value类型</typeparam>
        /// <param name="strMap">字典字符串</param>
        /// <param name="keyValueSpriter">键值分隔符</param>
        /// <param name="mapSpriter">字典项分隔符</param>
        /// <returns>字典对象</returns>
        public static Dictionary<T, U> ParseMapAny<T, U>(String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
        {
            var typeT = typeof(T);
            var typeU = typeof(U);
            var result = new Dictionary<T, U>();
            //先转为字典
            var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
            foreach (var item in strResult)
            {
                try
                {
                    object key1 = GetValue(item.Key, typeT);
                    object value1 = GetValue(item.Value, typeU);
                    if (key1 != null && value1 != null)
                    {
                        T key = (T)key1;
                        U value = (U)value1;
                        result.Add(key, value);
                    }
                }
                catch (Exception)
                {
                    DebugUtils.Error("XmlData", String.Format("Parse failure: {0}, {1}", item.Key, item.Value));
                }
            }
            return result;
        }
        /// <summary>
        /// 将字典字符串转换为键类型与值类型都为字符串的字典对象。
        /// </summary>
        /// <param name="strMap">字典字符串</param>
        /// <param name="keyValueSpriter">键值分隔符</param>
        /// <param name="mapSpriter">字典项分隔符</param>
        /// <returns>字典对象</returns>
        public static Dictionary<String, String> ParseMap(String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
        {
            Dictionary<String, String> result = new Dictionary<String, String>();
            if (String.IsNullOrEmpty(strMap))
            {
                return result;
            }

            var map = strMap.Split(mapSpriter);//根据字典项分隔符分割字符串，获取键值对字符串
            for (int i = 0; i < map.Length; i++)
            {
                if (String.IsNullOrEmpty(map[i]))
                {
                    continue;
                }

                var keyValuePair = map[i].Split(keyValueSpriter);//根据键值分隔符分割键值对字符串
                if (keyValuePair.Length == 2)
                {
                    if (!result.ContainsKey(keyValuePair[0]))
                    {
                        result.Add(keyValuePair[0], keyValuePair[1]);
                    }
                    else
                    {
                        DebugUtils.Error("XmlData", String.Format("Key {0} already exist, index {1} of {2}.", keyValuePair[0], i, strMap));
                    }
                }
                else
                {
                    DebugUtils.Error("XmlData", String.Format("KeyValuePair are not match: {0}, index {1} of {2}.", map[i], i, strMap));
                }
            }
            return result;
        }
        /// <summary>
        /// 将字典对象转换为字典字符串。
        /// </summary>
        /// <typeparam name="T">字典Key类型</typeparam>
        /// <typeparam name="U">字典Value类型</typeparam>
        /// <param name="map">字典对象</param>
        /// <returns>字典字符串</returns>
        public static String PackMap<T, U>(IEnumerable<KeyValuePair<T, U>> map, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
        {
            if (map.Count() == 0)
            {
                return null;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in map)
                {
                    sb.AppendFormat("{0}{1}{2}{3}", item.Key, keyValueSpriter, item.Value, mapSpriter);
                }
                return sb.ToString().Remove(sb.Length - 1, 1);
            }
        }
        /// <summary>
        /// 将列表字符串转换为类型为 T 的列表对象。
        /// </summary>
        /// <typeparam name="T">列表值对象类型</typeparam>
        /// <param name="strList">列表字符串</param>
        /// <param name="listSpriter">数组分隔符</param>
        /// <returns>列表对象</returns>
        public static List<T> ParseListAny<T>(String strList, Char listSpriter = LIST_SPRITER)
        {
            var type = typeof(T);
            var list = ParseList(strList, listSpriter);
            var result = new List<T>();
            foreach (var item in list)
            {
                object v = GetValue(item, type);
                if (v != null)
                {
                    result.Add((T)v);
                }
            }
            return result;
        }
        /// <summary>
        /// 将列表字符串转换为字符串的列表对象。
        /// </summary>
        /// <param name="strList">列表字符串</param>
        /// <param name="listSpriter">数组分隔符</param>
        /// <returns>列表对象</returns>
        public static List<String> ParseList(String strList, Char listSpriter = LIST_SPRITER)
        {
            var result = new List<String>();
            if (String.IsNullOrEmpty(strList))
            {
                return result;
            }

            var trimString = strList.Trim();
            if (String.IsNullOrEmpty(strList))
            {
                return result;
            }
            var detials = trimString.Split(listSpriter);//.Substring(1, trimString.Length - 2)
            foreach (var item in detials)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    result.Add(item.Trim());
                }
            }

            return result;
        }
        /// <summary>
        /// 将列表对象转换为列表字符串。
        /// </summary>
        /// <typeparam name="T">列表值对象类型</typeparam>
        /// <param name="list">列表对象</param>
        /// <param name="listSpriter">列表分隔符</param>
        /// <returns>列表字符串</returns>
        public static String PackList<T>(List<T> list, Char listSpriter = LIST_SPRITER)
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
                    sb.AppendFormat("{0}{1}", item, listSpriter);
                }
                sb.Remove(sb.Length - 1, 1);
                return sb.ToString();
            }

        }
        public static String PackArray<T>(T[] array, Char listSpriter = LIST_SPRITER)
        {
            var list = new List<T>();
            list.AddRange(array);
            return PackList(list, listSpriter);
        }
        /// <summary>
        /// 将字符串转换为对应类型的值。
        /// </summary>
        /// <param name="value">字符串值内容</param>
        /// <param name="type">值的类型</param>
        /// <returns>对应类型的值</returns>
        public static object GetValue(String value, Type type)
        {
            if (type == typeof(Vector3))
            {
                Vector3 result;
                ParseVector3(value == "" ? "0,0,0" : value, out result);
                return result;
            }
            if (type == null)
            {
                return null;
            }
            if (type == typeof(string))
            {
                return value;
            }
            if (type == typeof(Int32))
            {
                return Convert.ToInt32(Convert.ToDouble(value == "" ? "-1" : value));
            }
            if (type == typeof(float))
            {
                return float.Parse(value == "" ? "0.0" : value);
            }
            if (type == typeof(byte))
            {
                return Convert.ToByte(Convert.ToDouble(value == "" ? "0" : value));
            }
            if (type == typeof(sbyte))
            {
                return Convert.ToSByte(Convert.ToDouble(value == "" ? "0" : value));
            }
            if (type == typeof(UInt32))
            {
                return Convert.ToUInt32(Convert.ToDouble(value == "" ? "0" : value));
            }
            if (type == typeof(Int16))
            {
                return Convert.ToInt16(Convert.ToDouble(value == "" ? "0" : value));
            }
            if (type == typeof(Int64))
            {
                return Convert.ToInt64(Convert.ToDouble(value == "" ? "0" : value));
            }
            if (type == typeof(UInt16))
            {
                return Convert.ToUInt16(Convert.ToDouble(value == "" ? "0" : value));
            }
            if (type == typeof(UInt64))
            {
                return Convert.ToUInt64(Convert.ToDouble(value == "" ? "0" : value));
            }
            if (type == typeof(double))
            {
                return double.Parse(value == "" ? "0.0" : value);
            }
            if (type == typeof(bool))
            {
                if (value == "0")
                {
                    return false;
                }
                else if (value == "1")
                {
                    return true;
                }
                else
                {
                    return bool.Parse(value == "" ? "0" : value);
                }
            }
            if (type.BaseType == typeof(Enum))
            {
                return GetValue(value == "" ? "0" : value, Enum.GetUnderlyingType(type));
            }
            if (type == typeof(Vector4))
            {
                Vector4 result;
                ParseVector4(value == "" ? "0,0,0,0" : value, out result);
                return result;
            }
            if (type == typeof(Vector2))
            {
                Vector2 result;
                ParseVector2(value == "" ? "0,0" : value, out result);
                return result;
            }
            if (type == typeof(Quaternion))
            {
                Quaternion result;
                ParseQuaternion(value == "" ? "0,0,0,1" : value, out result);
                return result;
            }
            if (type == typeof(Color))
            {
                Color result;
                ParseColor(value == "" ? "0,0,0,0" : value, out result);
                return result;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type[] types = type.GetGenericArguments();
                var map = ParseMap(value);
                var result = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                foreach (var item in map)
                {
                    var key = GetValue(item.Key, types[0]);
                    var v = GetValue(item.Value, types[1]);
                    type.GetMethod("Add").Invoke(result, new object[] { key, v });
                }
                return result;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type t = type.GetGenericArguments()[0];
                var list = ParseList(value);
                var result = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                foreach (var item in list)
                {
                    var v = GetValue(item, t);
                    type.GetMethod("Add").Invoke(result, new object[] { v });
                }
                return result;
            }
            return null;
        }
        /// <summary>
        /// 将指定格式(255, 255, 255, 255) 转换为 Color 
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="result"></param>
        /// <returns>返回 true/false 表示是否成功</returns>
        public static bool ParseColor(string inputString, out Color result)
        {
            string trimString = inputString.Trim();
            trimString = trimString.Replace("(", "");
            trimString = trimString.Replace(")", "");
            result = new Color();
            try
            {
                string[] detail = trimString.Split(',');
                for (int i = 0; i < detail.Length; ++i)
                {
                    result[i] = float.Parse(detail[i].Trim()) / 255;
                }
                return true;
            }
            catch (Exception e)
            {
                DebugUtils.Error("XmlData", "Parse Color error: " + trimString + e.ToString());
                return false;
            }
        }
        /// <summary>
        /// 将指定格式(1.0, 2) 转换为 Vector2
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="result"></param>
        /// <returns>返回 true/false 表示是否成功</returns>
        public static bool ParseVector2(string inputString, out Vector2 result)
        {
            string trimString = inputString.Trim();
            trimString = trimString.Replace("(", "");
            trimString = trimString.Replace(")", "");
            result = new Vector2();
            try
            {
                string[] detail = trimString.Split(',');
                for (int i = 0; i < detail.Length; ++i)
                {
                    result[i] = float.Parse(detail[i].Trim());
                }
                return true;
            }
            catch (Exception e)
            {
                DebugUtils.Error("XmlData", "Parse Vector2 error: " + trimString + e.ToString());
                return false;
            }
        }

        static Regex RegexVector = new Regex("-?\\d+\\.\\d+");

        /// <summary>
        /// 将指定格式(1.0, 2, 3.4) 转换为 Vector3 
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="result"></param>
        /// <returns>返回 true/false 表示是否成功</returns>
        public static bool ParseVector3(string inputString, out Vector3 result)
        {
            MatchCollection collects = MatchVector(inputString);
            result = new Vector3();
            try
            {
                if (collects.Count == 3)
                {
                    result[0] = float.Parse(collects[0].Value);
                    result[1] = float.Parse(collects[1].Value);
                    result[2] = float.Parse(collects[2].Value);
                }
                else if(collects.Count == 2)
                {
                    result[0] = float.Parse(collects[0].Value);
                    result[2] = float.Parse(collects[1].Value);
                }
                else if (collects.Count == 1)
                {
                    result[0] = float.Parse(collects[0].Value);
                }
            }
            catch (Exception e)
            {
                DebugUtils.Error("XmlData", "Parse Vector3 error: " + inputString + e.ToString());
                return false;
            }
            return true;
        }

        public static MatchCollection MatchVector(string inputString)
        {
            return RegexVector.Matches(inputString);
        }

        /// <summary>
        /// 将指定格式(1.0, 2, 3.4, 1.0) 转换为 Vector4
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="result"></param>
        /// <returns>返回 true/false 表示是否成功</returns>
        public static bool ParseVector4(string inputString, out Vector4 result)
        {
            string trimString = inputString.Trim();
            trimString = trimString.Replace("(", "");
            trimString = trimString.Replace(")", "");
            result = new Vector4();
            try
            {
                string[] detail = trimString.Split(',');
                for (int i = 0; i < detail.Length; ++i)
                {
                    result[i] = float.Parse(detail[i].Trim());
                }
                return true;
            }
            catch (Exception e)
            {
                DebugUtils.Error("XmlData", "Parse Vector4 error: " + trimString + e.ToString());
                return false;
            }
        }
        /// <summary>
        /// 将指定格式(1.0, 2, 3.4) 转换为 Vector3 
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="result"></param>
        /// <returns>返回 true/false 表示是否成功</returns>
        public static bool ParseQuaternion(string inputString, out Quaternion result)
        {
            string trimString = inputString.Trim();
            trimString = trimString.Replace("(", "");
            trimString = trimString.Replace(")", "");
            result = new Quaternion();
            try
            {
                string[] detail = trimString.Split(',');
                for (int i = 0; i < detail.Length; ++i)
                {
                    result[i] = float.Parse(detail[i].Trim());
                }
                return true;
            }
            catch (Exception e)
            {
                DebugUtils.Error("XmlData", "Parse Quaternion error: " + trimString + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 从指定的 URL 加载 map 数据。
        /// </summary>
        /// <param name="fileName">文件的 URL，该文件包含要加载的 XML 文档</param>
        /// <param name="key">XML 文档关键字</param>
        /// <returns>map 数据</returns>
        public static Dictionary<String, Dictionary<String, String>> LoadMap(String fileName, out String key)
        {
            key = Path.GetFileNameWithoutExtension(fileName);
            var xml = Load(fileName);
            if (xml != null)
            {
                return LoadMap(xml);
            }
            return null;
        }

        /// <summary>
        /// 从指定的 URL 加载 map 数据。
        /// </summary>
        /// <param name="fileName">文件的 URL，该文件包含要加载的 XML 文档</param>
        /// <param name="map">map 数据</param>
        /// <returns>是否加载成功</returns>
        public static Boolean LoadMap(String fileName, out Dictionary<String, Dictionary<String, String>> map)
        {
            try
            {
                var xml = Load(fileName);
                map = null;
                if (xml != null)
                {
                    map = LoadMap(xml);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                DebugUtils.Error("LoadMap", ex.Message);
                map = null;
                return false;
            }
        }

        /// <summary>
        /// 从指定的 URL 加载 map 数据。
        /// </summary>
        /// <param name="file">文件的 URL，该文件包含要加载的 XML 文档 或者 XML文本内容</param>
        /// <param name="map">map 数据</param>
        /// <returns>是否加载成功</returns>
        public static Boolean LoadIntMap(String content, out Dictionary<Int32, Dictionary<String, String>> map)
        {
            try
            {
                if (String.IsNullOrEmpty(content))
                {
                    DebugUtils.Error("IsNullOrEmpty", "File not exist");
                    map = null;
                    return false;
                }
                SecurityElement xml = LoadFromXmlStr(content);
                if (xml == null)
                {
                    DebugUtils.Error("LoadIntMap", "File not exist: " + content);
                    map = null;
                    return false;
                }
                else
                {
                    map = LoadIntMap(xml, content);
                    return true;
                }
            }
            catch (Exception ex)
            {
                DebugUtils.Error("LoadIntMap", ex.Message);
                map = null;
                return false;
            }
        }

        /// <summary>
        /// 从指定的 XML 文档加载 map 数据。
        /// </summary>
        /// <param name="xml">XML 文档</param>
        /// <returns>map 数据</returns>
        public static Dictionary<Int32, Dictionary<String, String>> LoadIntMap(SecurityElement xml, string source)
        {
            var result = new Dictionary<Int32, Dictionary<String, String>>();
            if (xml.Children == null)
            {
                return result;
            }
            var index = 0;
            foreach (SecurityElement subMap in xml.Children)
            {
                index++;
                if (subMap.Children == null || subMap.Children.Count == 0)
                {
                    Debug.LogError("empty row in row NO." + index + " of " + source);
                    continue;
                }
                string text = (subMap.Children[0] as SecurityElement).Text;
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }
                Int32 key = Int32.Parse(text);
                if (result.ContainsKey(key))
                {
                    DebugUtils.Error("LoadIntMap", String.Format("Key {0} already exist, in {1}.", key, source));
                    continue;
                }

                var children = new Dictionary<String, String>();
                result.Add(key, children);
                for (int i = 1; i < subMap.Children.Count; i++)
                {
                    var node = subMap.Children[i] as SecurityElement;
                    if (node != null)
                    {
                        //对属性名称部分后缀进行裁剪, 去除 字段类型标签
                        string tag = node.Tag;
                        if (!children.ContainsKey(tag))
                        {
                            if (String.IsNullOrEmpty(node.Text))
                            {
                                children.Add(tag, "IsNullOrEmpty");
                            }
                            else
                            {
                                children.Add(tag, node.Text.Trim());
                            }
                        }
                        else
                        {
                            DebugUtils.Error("LoadIntMap", String.Format("Key {0} already exist, index {1} of {2}.", node.Tag, i, node.ToString()));
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 从指定的 XML 文档加载 map 数据。
        /// </summary>
        /// <param name="xml">XML 文档</param>
        /// <returns>map 数据</returns>
        public static Dictionary<String, Dictionary<String, String>> LoadMap(SecurityElement xml)
        {
            var result = new Dictionary<String, Dictionary<String, String>>();
            if (xml != null)
            {
                foreach (SecurityElement subMap in xml.Children)
                {
                    String key = (subMap.Children[0] as SecurityElement).Text.Trim();
                    if (result.ContainsKey(key))
                    {
                        Debug.LogError(String.Format("Key {0} already exist, in {1}.", key, xml.ToString()));
                        continue;
                    }

                    var children = new Dictionary<string, string>();
                    result.Add(key, children);
                    for (int i = 1; i < subMap.Children.Count; i++)
                    {
                        var node = subMap.Children[i] as SecurityElement;
                        if (node != null && !children.ContainsKey(node.Tag))
                        {
                            if (String.IsNullOrEmpty(node.Text))
                            {
                                children.Add(node.Tag, "IsNullOrEmpty");
                            }
                            else
                            {
                                children.Add(node.Tag, node.Text.Trim());
                            }
                        }
                        else
                        {
                            if (node != null)
                            {
                                DebugUtils.Error("LoadMap", String.Format("Key {0} already exist, index {1} of {2}.", node.Tag, i, node.ToString()));
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static String LoadText(String fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    using (StreamReader sr = File.OpenText(fileName))
                    {
                        return sr.ReadToEnd();
                    }
                }
                else
                {
                    return String.Empty;
                }
            }
            catch (Exception ex)
            {
                DebugUtils.Error("LoadText", ex);
                return "";
            }
        }

        /// <summary>
        /// 从指定的 URL 加载 XML 文档。
        /// </summary>
        /// <param name="fileName">文件的 URL，该文件包含要加载的 XML 文档。</param>
        /// <exception cref="System.ArgumentNullException">fileName 为 null。</exception>
        /// <exception cref="System.Security.SecurityException">调用方没有所要求的权限。</exception>
        /// <exception cref="System.ArgumentException">文件名为空，只包含空白，或包含无效字符。</exception>
        /// <exception cref="System.UnauthorizedAccessException"> 对 fileName 的访问被拒绝。</exception>
        /// <exception cref="System.IO.PathTooLongException">指定的路径、文件名或者两者都超出了系统定义的最大长度。例如，在基于 Windows 的平台上，路径必须小于 248 个字符，文件名必须小于 260个字符。</exception>
        /// <exception cref="System.NotSupportedException">fileName 字符串中间有一个冒号 (:)。</exception>
        /// <exception cref="System.OutOfMemoryException">内存不足，无法为返回的字符串分配缓冲区。</exception>
        /// <exception cref="System.IO.IOException">发生 I/O 错误。</exception>
        /// <returns>编码安全对象的 XML 对象模型。</returns>
        public static SecurityElement Load(String fileName)
        {
            String xmlText = LoadText(fileName);
            if (String.IsNullOrEmpty(xmlText))
            {
                return null;
            }
            else
            {
                return LoadXML(xmlText);
            }
        }

        /// <summary>
        /// 从指定的 URL 加载 XML 文档。
        /// </summary>
        /// <param name="fileName">文件的 URL，该文件包含要加载的 XML 文档。</param>
        /// <exception cref="System.ArgumentNullException">fileName 为 null。</exception>
        /// <exception cref="System.Security.SecurityException">调用方没有所要求的权限。</exception>
        /// <exception cref="System.ArgumentException">文件名为空，只包含空白，或包含无效字符。</exception>
        /// <exception cref="System.UnauthorizedAccessException"> 对 fileName 的访问被拒绝。</exception>
        /// <exception cref="System.IO.PathTooLongException">指定的路径、文件名或者两者都超出了系统定义的最大长度。例如，在基于 Windows 的平台上，路径必须小于 248 个字符，文件名必须小于 260个字符。</exception>
        /// <exception cref="System.NotSupportedException">fileName 字符串中间有一个冒号 (:)。</exception>
        /// <exception cref="System.OutOfMemoryException">内存不足，无法为返回的字符串分配缓冲区。</exception>
        /// <exception cref="System.IO.IOException">发生 I/O 错误。</exception>
        /// <returns>编码安全对象的 XML 对象模型。</returns>
        public static SecurityElement LoadFromXmlStr(String xmlContent)
        {
            if (String.IsNullOrEmpty(xmlContent))
            {
                return null;
            }
            else
            {
                return LoadXML(xmlContent);
            }
        }

        /// <summary>
        /// 从指定的字符串加载 XML 文档。
        /// </summary>
        /// <param name="xml">包含要加载的 XML 文档的字符串。</param>
        /// <returns>编码安全对象的 XML 对象模型。</returns>
        public static SecurityElement LoadXML(String xml)
        {
            try
            {
                SecurityParser securityParser = new SecurityParser();
                securityParser.LoadXml(xml);
                return securityParser.ToXml();
            }
            catch (Exception ex)
            {
                DebugUtils.Error("LoadXML", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 保持 XML 文档。
        /// </summary>
        /// <param name="fileName">文档名称</param>
        /// <param name="xml">XML内容</param>
        public static void SaveBytes(String fileName, byte[] buffer)
        {
            if (!Directory.Exists(GetDirectoryName(fileName)))
            {
                Directory.CreateDirectory(GetDirectoryName(fileName));
            }
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                using (BinaryWriter sw = new BinaryWriter(fs))
                {
                    //开始写入
                    sw.Write(buffer);
                    //清空缓冲区
                    sw.Flush();
                    //关闭流
                    sw.Close();
                }
                fs.Close();
            }
        }

        /// <summary>
        /// 保存 Text 文档。
        /// </summary>
        /// <param name="fileName">文档名称</param>
        /// <param name="text">XML内容</param>
        public static void SaveText(String fileName, String text)
        {
            if (!Directory.Exists(GetDirectoryName(fileName)))
            {
                Directory.CreateDirectory(GetDirectoryName(fileName));
            }
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            File.WriteAllText(fileName, text);
        }

        public static string GetDirectoryName(string fileName)
        {
            int index = fileName.LastIndexOf('/');
            index = index == -1 ? fileName.LastIndexOf("\\") : index;
            if (index == -1)
            {
                index = fileName.LastIndexOf(':');
                if (index == -1)
                {
                    return ".";
                }
                else
                {
                    return fileName;
                }
            }
            return fileName.Substring(0, index);
        }
    }
}