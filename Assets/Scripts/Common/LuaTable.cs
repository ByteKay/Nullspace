using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Nullspace
{
    /// <summary>
    /// 固定键值对为 string 与 object 的 Lua table 字典
    /// </summary>
    public class LuaTable : IEnumerable<KeyValuePair<string, object>>
    {
        private Dictionary<string, object> Dictionary;
        private Dictionary<string, bool> KeyIsStringDict;

        public int Count { get { return Dictionary.Count; } }

        public Dictionary<string, object>.KeyCollection Keys { get { return Dictionary.Keys; } }
        public Dictionary<string, object>.ValueCollection Values { get { return Dictionary.Values; } }

        public object this[string key]
        {
            get
            {
                return Dictionary[key];
            }
            set
            {
                Dictionary[key] = value;
            }
        }

        public LuaTable()
        {
            Dictionary = new Dictionary<string, object>();
            KeyIsStringDict = new Dictionary<string, bool>();
        }

        public void Add(int key, object value)
        {
            Add(key.ToString(), false, value);
        }

        public void Add(float key, object value)
        {

            Add(Convert.ToInt32(key).ToString(), false, value);
        }

        public void Add(string key, object value)
        {
            Add(key, true, value);
        }

        public void Add(string key, bool isString, object value)
        {
            Dictionary.Add(key, value);
            KeyIsStringDict.Add(key, isString);
        }

        public bool Remove(string key)
        {
            return Dictionary.Remove(key);
        }

        public void Clear()
        {
            Dictionary.Clear();
        }

        public bool IsKeyString(string key)
        {
#if UNITY_IPHONE
		    bool bRet = true;
		    if(KeyIsStringDict.ContainsKey(key))
		    {
			    bRet = KeyIsStringDict[key];
		    }
		    return bRet;
#else
            return KeyIsStringDict.GetValueOrDefault(key, true);
#endif
        }

        public bool IsLuaTable(string key)
        {
            if (ContainsKey(key))
            {
                var obj = Dictionary[key];
                if (obj.GetType() == typeof(LuaTable))
                {
                    return true;
                }
            }
            return false;
        }

        public LuaTable GetLuaTable(string key)
        {
            if (IsLuaTable(key))
                return Dictionary[key] as LuaTable;
            else
                return null;
        }

        public bool TryGetLuaTable(string key, out LuaTable value)
        {
            if (IsLuaTable(key))
            {
                value = Dictionary[key] as LuaTable;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public bool TryGetValue(string key, out object value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        public bool ContainsKey(string key)
        {
            return Dictionary.ContainsKey(key);
        }

        public bool ContainsValue(object value)
        {
            return Dictionary.ContainsValue(value);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        public override string ToString()
        {
            var luatable = PackLuaTable(this);
            return luatable;
        }

        /// <summary>
        /// 将 Lua table 打包成字符串
        /// </summary>
        /// <param name="luaTable"></param>
        /// <returns></returns>
        public static string PackLuaTable(LuaTable luaTable)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('{');
            if (luaTable != null)
            {
                foreach (var item in luaTable)
                {
                    //拼键
                    if (luaTable.IsKeyString(item.Key))
                    {
                        sb.Append(EncodeString(item.Key));
                    }
                    else
                    {
                        sb.Append(item.Key);
                    }
                    sb.Append('=');

                    //拼值
                    var valueType = item.Value.GetType();
                    if (valueType == typeof(string))
                    {
                        sb.Append(EncodeString(item.Value as string));
                    }
                    else if (valueType == typeof(LuaTable))
                    {
                        sb.Append(PackLuaTable(item.Value as LuaTable));
                    }
                    else
                    {
                        sb.Append(item.Value.ToString());
                    }
                    sb.Append(',');
                }
                //若lua table为空则不删除
                if (luaTable.Count != 0)
                {
                    //去掉最后一个逗号
                    sb.Remove(sb.Length - 1, 1);
                }
            }
            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>
        /// 将字符串转成 Lua table 可识别的格式。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string EncodeString(string value)
        {
            if (value.Length > 999)
            {
                DebugUtils.Warning("PackLuaTable", "EncodeString overflow: " + value);
            }
            return string.Concat('s', Encoding.UTF8.GetBytes(value).Length.ToString("000"), value);
        }

        /// <summary>
        /// 将 Lua table 字符串转换为 Lua table 实体
        /// </summary>
        /// <param name="inputString">Lua table 字符串</param>
        /// <param name="result">实体对象</param>
        /// <returns>返回 true/false 表示是否成功</returns>
        public static bool ParseLuaTable(string inputString, out LuaTable result)
        {
            string trimString = inputString.Trim();
            if (trimString[0] != '{' || trimString[trimString.Length - 1] != '}')
            {
                result = null;
                return false;
            }
            else if (trimString.Length == 2)
            {
                result = new LuaTable();
                return true;
            }

            var index = 0;
            object obj;
            var flag = DecodeLuaTable(inputString, ref index, out obj);
            if (flag)
            {
                result = obj as LuaTable;
            }
            else
            {
                result = null;
            }
            return flag;
        }

        /// <summary>
        /// 将 Byte流 转换为 Lua table 实体
        /// </summary>
        /// <param name="inputString">Byte[]</param>
        /// <param name="result">实体对象</param>
        /// <returns>返回 true/false 表示是否成功</returns>
        public static bool ParseLuaTable(byte[] inputString, out LuaTable result)
        {
            if (inputString[0] != '{' || inputString[inputString.Length - 1] != '}')
            {
                result = null;
                return false;
            }
            else if (inputString.Length == 2)
            {
                result = new LuaTable();
                return true;
            }

            var index = 0;
            object obj;
            var flag = DecodeLuaTable(inputString, ref index, out obj);
            if (flag)
            {
                result = obj as LuaTable;
            }
            else
            {
                result = null;
            }
            return flag;
        }
        private static bool DecodeLuaTable(string inputString, ref int index, out object result)
        {
            var luaTable = new LuaTable();
            result = luaTable;
            if (!WaitChar(inputString, '{', ref index))
            {
                return false;
            }
            try
            {
                //如果下一个字符为右大括号表示为空Lua table
                if (WaitChar(inputString, '}', ref index))
                {
                    return true;
                }
                while (index < inputString.Length)
                {
                    string key;
                    bool isString;
                    object value;
                    //匹配键
                    DecodeKey(inputString, ref index, out key, out isString);
                    //匹配键值对分隔符
                    WaitChar(inputString, '=', ref index);
                    //转换实体
                    var flag = DecodeLuaValue(inputString, ref index, out value);
                    if (flag)
                    {
                        luaTable.Add(key, isString, value);
                    }
                    if (!WaitChar(inputString, ',', ref index))
                    {
                        break;
                    }
                }
                WaitChar(inputString, '}', ref index);
                return true;
            }
            catch (Exception e)
            {
                DebugUtils.Error("Parse LuaTable Error", inputString + e.ToString());
                return false;
            }
        }
        private static bool DecodeLuaTable(byte[] inputString, ref int index, out object result)
        {
            var luaTable = new LuaTable();
            result = luaTable;
            if (!WaitChar(inputString, '{', ref index))
            {
                return false;
            }
            try
            {
                //如果下一个字符为右大括号表示为空Lua table
                if (WaitChar(inputString, '}', ref index))
                {
                    return true;
                }
                while (index < inputString.Length)
                {
                    string key;
                    bool isString;
                    object value;
                    //匹配键
                    DecodeKey(inputString, ref index, out key, out isString);
                    //匹配键值对分隔符
                    WaitChar(inputString, '=', ref index);
                    //转换实体
                    var flag = DecodeLuaValue(inputString, ref index, out value);
                    if (flag)
                    {
                        luaTable.Add(key, isString, value);
                    }
                    if (!WaitChar(inputString, ',', ref index))
                    {
                        break;
                    }
                }
                WaitChar(inputString, '}', ref index);
                return true;

            }
            catch (Exception e)
            {
                DebugUtils.Error("Parse LuaTable Error ", inputString + e.ToString());
                return false;
            }
        }

        /// <summary>
        /// 判断下个字符是否为期望的字符。
        /// </summary>
        /// <param name="inputString">Lua table字符串</param>
        /// <param name="c">期望的字符</param>
        /// <param name="index">字符串偏移量</param>
        /// <returns>返回 true/false 表示是否为期望的字符</returns>
        public static bool WaitChar(string inputString, char c, ref int index)
        {
            var szLen = inputString.IndexOf(c, index);
            if (szLen == index)
            {
                index++;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 判断下个字符是否为期望的字符。
        /// </summary>
        /// <param name="inputString">Lua table字符串</param>
        /// <param name="c">期望的字符</param>
        /// <param name="index">字符串偏移量</param>
        /// <returns>返回 true/false 表示是否为期望的字符</returns>
        public static bool WaitChar(byte[] inputString, char c, ref int index)
        {
            if ((byte)c == inputString[index])
            {
                index++;
                return true;
            }
            return false;
        }

        private static bool DecodeLuaValue(string inputString, ref int index, out object value)
        {
            var firstChar = inputString[index];
            if (firstChar == 's')
            {
                //value is string
                var szLen = inputString.Substring(index + 1, 3);
                var lenth = Int32.Parse(szLen);
                index += 4;
                if (lenth > 0)
                {
                    value = inputString.Substring(index, lenth);
                    index += lenth;
                    return true;
                }
                else
                {
                    value = "";
                    return true;
                }
            }
            //如果第一个字符为花括号，表示接下来的内容为列表或实体类型
            else if (firstChar == '{')
            {
                return DecodeLuaTable(inputString, ref index, out value);
            }
            else
            {
                //value is number
                var i = index;
                while (++index < inputString.Length)
                {
                    if (inputString[index] == ',' || inputString[index] == '}')
                    {
                        if (index > i)
                        {
                            value = inputString.Substring(i, index - i);
                            return true;
                        }
                    }
                }
                DebugUtils.Error("Decode Lua Table Value Error", + index + " " + inputString);
                value = null;
                return false;
            }
        }

        public static bool DecodeKey(byte[] inputString, ref int index, out string key, out bool isString)
        {
            if (inputString[index] == 's')
            {
                //key is string
                var szLen = Encoding.UTF8.GetString(inputString, index + 1, 3);
                var length = Int32.Parse(szLen);
                if (length > 0)
                {
                    index += 4;
                    key = Encoding.UTF8.GetString(inputString, index, length);
                    isString = true;
                    index += length;
                    return true;
                }
                key = "";
                isString = true;
                DebugUtils.Error("Decode Lua Table Key Error: ", index + " " + inputString);
                return false;
            }
            else
            {
                //key is number
                int offset = 0;
                while (index + offset < inputString.Length && inputString[index + offset] != '=')
                {
                    offset++;
                }

                if (offset > 0)
                {
                    key = Encoding.UTF8.GetString(inputString, index, offset);
                    index = index + offset;
                    isString = false;
                    return true;
                }
                else
                {
                    key = "-1";
                    isString = false;
                    DebugUtils.Error("Decode Lua Table Key Error ", index + " " + inputString);
                    return false;
                }
            }
        }
        private static bool DecodeLuaValue(byte[] inputString, ref int index, out object value)
        {
            var firstChar = inputString[index];
            if (firstChar == 's')
            {
                //value is string
                var szLen = Encoding.UTF8.GetString(inputString, index + 1, 3);
                var length = Int32.Parse(szLen);
                index += 4;
                if (length > 0)
                {
                    value = Encoding.UTF8.GetString(inputString, index, length);
                    index += length;
                    return true;
                }
                else
                {
                    value = "";
                    return true;
                }
            }
            //如果第一个字符为花括号，表示接下来的内容为列表或实体类型
            else if (firstChar == '{')
            {
                return DecodeLuaTable(inputString, ref index, out value);
            }
            else
            {
                //value is number
                var i = index;
                while (++index < inputString.Length)
                {
                    if (inputString[index] == ',' || inputString[index] == '}')
                    {
                        if (index > i)
                        {
                            value = Encoding.UTF8.GetString(inputString, i, index - i);
                            return true;
                        }
                    }
                }
                DebugUtils.Error("Decode Lua Table Value Error", index + " " + inputString);
                value = null;
                return false;
            }
        }
        /// <summary>
        /// 解析 Lua table 的键。
        /// </summary>
        /// <param name="inputString">Lua table字符串</param>
        /// <param name="index">字符串偏移量</param>
        /// <param name="result">键</param>
        /// <returns>返回 true/false 表示是否成功</returns>
        public static bool DecodeKey(string inputString, ref int index, out string key, out bool isString)
        {
            if (inputString[index] == 's')
            {
                //key is string
                var szLen = inputString.Substring(index + 1, 3);
                var lenth = int.Parse(szLen);
                if (lenth > 0)
                {
                    index += 4;
                    key = inputString.Substring(index, lenth);
                    isString = true;
                    index += lenth;
                    return true;
                }
                key = "";
                isString = true;
                DebugUtils.Error("Decode LuaTable Key Error", index + " " + inputString);
                return false;
            }
            else
            {
                //key is number
                var szLen = inputString.IndexOf('=', index);
                if (szLen > -1)
                {
                    var lenth = szLen - index;
                    key = inputString.Substring(index, lenth);
                    index = szLen;
                    isString = false;
                    return true;
                }
                else
                {
                    key = "-1";
                    isString = false;
                    DebugUtils.Error("Decode LuaTable Key Error", index + " " + inputString);
                    return false;
                }
            }
        }

        /// <summary>
        /// 转换复杂类型的对象到LuaTable，不支持基础类型直接转换。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool PackLuaTable(object target, out LuaTable result)
        {
            var type = target.GetType();
            if (type == typeof(LuaTable))
            {
                result = target as LuaTable;
                return true;
            }
            if (type.IsGenericType)
            {
                //容器类型
                //目前只支持列表与字典的容器类型转换
                if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    return PackLuaTable(target as IDictionary, out result);
                }
                else
                {
                    return PackLuaTable(target as IList, out result);
                }
            }
            else
            {
                //实体类型
                result = new LuaTable();
                try
                {
                    var props = type.GetProperties(~BindingFlags.Static);
                    for (int i = 0; i < props.Length; i++)
                    {
                        var prop = props[i];
                        if (IsBaseType(prop.PropertyType))
                        {
                            result.Add(i + 1, prop.GetGetMethod().Invoke(target, null));
                        }
                        else
                        {
                            LuaTable lt;
                            var value = prop.GetGetMethod().Invoke(target, null);
                            var flag = PackLuaTable(value, out lt);
                            if (flag)
                            {
                                result.Add(i + 1, lt);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugUtils.Error("PackLuaTable", "Entity Error: ", ex.Message);
                }
            }
            return true;
        }

        /// <summary>
        /// 转换列表类型的对象到LuaTable。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool PackLuaTable(IList target, out LuaTable result)
        {
            Type[] types = target.GetType().GetGenericArguments();
            result = new LuaTable();
            try
            {
                for (int i = 0; i < target.Count; i++)
                {
                    if (IsBaseType(types[0]))
                    {
                        if (types[0] == typeof(bool))
                        {
                            result.Add(i + 1, (bool)target[i] ? 1 : 0);
                        }
                        else
                        {
                            result.Add(i + 1, target[i]);
                        }
                    }
                    else
                    {
                        LuaTable value;
                        var flag = PackLuaTable(target[i], out value);
                        if (flag)
                        {
                            result.Add(i + 1, value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugUtils.Error("PackLuaTable", "List Error: " + ex.Message);
            }
            return true;
        }

        /// <summary>
        /// 转换字典类型的对象到LuaTable。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool PackLuaTable(IDictionary target, out LuaTable result)
        {
            Type[] types = target.GetType().GetGenericArguments();
            result = new LuaTable();
            try
            {
                foreach (DictionaryEntry item in target)
                {
                    if (IsBaseType(types[1]))
                    {
                        object value;
                        //判断值是否布尔类型，是则做特殊转换
                        if (types[1] == typeof(bool))
                        {
                            value = (bool)item.Value ? 1 : 0;
                        }
                        else
                        {
                            value = item.Value;
                        }
                        //判断键是否为整型，是则标记键为整型，转lua table字符串时有用
                        if (types[0] == typeof(int))
                        {
                            result.Add(item.Key.ToString(), false, value);
                        }
                        else
                        {
                            result.Add(item.Key.ToString(), value);
                        }
                    }
                    else
                    {
                        LuaTable value = null;
                        var flag = PackLuaTable(item.Value, out value);
                        if (flag)
                        {
                            if (types[0] == typeof(int))
                            {
                                result.Add(item.Key.ToString(), false, value);
                            }
                            else
                            {
                                result.Add(item.Key.ToString(), value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugUtils.Error("PackLuaTable", "Dictionary Error: " + ex.Message);
            }
            return true;
        }

        /// <summary>
        /// 判断类型是否为基础类型。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsBaseType(Type type)
        {
            if (type == typeof(byte) || type == typeof(sbyte)
                || type == typeof(short) || type == typeof(ushort)
                || type == typeof(int) || type == typeof(uint)
                || type == typeof(long) || type == typeof(ulong)
                || type == typeof(float) || type == typeof(double)
                || type == typeof(string) || type == typeof(bool))
            {
                return true;
            }
            return false;
        }
    }


    public static class DictionaryExtend
    {
        public static T Get<T>(this T[] array, int index)
        {
            if (array == null)
            {
                DebugUtils.Error("DictionaryExtend", "Array is null.");
                return default(T) == null ? GetDefaultValue<T>() : default(T);
            }
            else if (array.Length <= index)
            {
                DebugUtils.Error("DictionaryExtend", String.Format("Index '{0}' is out of range.", index));
                return default(T) == null ? GetDefaultValue<T>() : default(T);
            }
            else
            {
                return array[index];
            }
        }

        public static T Get<T>(this List<T> list, int index)
        {
            if (list == null)
            {
                DebugUtils.Error("DictionaryExtend", "List is null.");
                return default(T) == null ? GetDefaultValue<T>() : default(T);
            }
            else if (list.Count <= index)
            {
                DebugUtils.Error("DictionaryExtend", String.Format("Index '{0}' is out of range.", index));
                return default(T) == null ? GetDefaultValue<T>() : default(T);
            }
            else
            {
                return list[index];
            }
        }

        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary == null)
            {
                DebugUtils.Error("DictionaryExtend", "Dictionary is null.");
                return default(TValue) == null ? GetDefaultValue<TValue>() : default(TValue);
            }
            else if (!dictionary.ContainsKey(key))
            {
                DebugUtils.Error("DictionaryExtend", String.Format("Key '{0}' is not exist.", key));
                return default(TValue) == null ? GetDefaultValue<TValue>() : default(TValue);
            }
            else
            {
                return dictionary[key];
            }
        }

        public static T GetDefaultValue<T>()
        {
            var constructor = typeof(T).GetConstructor(Type.EmptyTypes);
            if (constructor == null)
                return default(T);
            else
                return (T)constructor.Invoke(null);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefaultValueProvider<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueProvider)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValueProvider();
        }
    }

}
