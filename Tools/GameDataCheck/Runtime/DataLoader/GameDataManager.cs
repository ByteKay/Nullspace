using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using GameData;
namespace Nullspace
{
    // public
    public partial class GameDataManager
    {
        /// <summary>
        /// 根据fileUrl初始化 类别 T 的数据。
        /// </summary>
        /// <typeparam name="T">className 对应的 类型</typeparam>
        /// <param name="fileUrl">分两块数据"fileName#className"</param>
        public static void InitByFile<T>(string fileUrl) where T : GameData<T>, new()
        {
            string[] strs = fileUrl.Split('#');
            Debug.Assert(strs.Length == 2, "wrong url: " + fileUrl);
            InitByFile<T>(strs[0].Trim(), strs[1].Trim());
        }
        
        public static void InitByFile<T>(string fileName, string tagName) where T : GameData<T>, new()
        {
            if (!FileLoaded.ContainsKey(fileName))
            {
                SecurityElement root = LoadXml(fileName);
                if (root != null)
                {
                    FileLoaded.Add(fileName, root);
                }
            }
            if (!FileLoaded.ContainsKey(fileName))
            {
                throw new Exception("wrong xml element: " + fileName);
            }
            SecurityElement p = SerchChildByName(FileLoaded[fileName], tagName, false);
            if (p != null)
            {
                Init<T>(p);
            }
        }

        /// <summary>
        /// 根据xml内容构造 T 数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="content">xml文本内容</param>
        public static void InitByProperties<T>(Properties p) where T : GameData<T>, new()
        {
            if (p != null)
            {
                Init<T>(p);
            }
        }

        public static void ClearXmlData()
        {
            FileLoaded.Clear();
        }

        /// <summary>
        /// 删除所有文件的数据
        /// </summary>
        public static void ClearAllData()
        {
            ClearXmlData();
            foreach (var t in mGameDataTypes)
            {
                ClearFileData(t.Key);
            }
        }

        /// <summary>
        /// 删除一个文件对应的数据
        /// </summary>
        /// <param name="fileName">文件名</param>
        public static void ClearFileData(string fileName)
        {
            if (FileLoaded.ContainsKey(fileName))
            {
                FileLoaded.Remove(fileName);
            }
            if (mGameDataTypes.ContainsKey(fileName))
            {
                foreach (var type in mGameDataTypes[fileName])
                {
                    ClearData(type);
                }
            }
        }

        /// <summary>
        /// 删除一个文件下的某一个类型数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void ClearData<T>() where T : GameData<T>, new()
        {
            ClearData(typeof(T));
        }

        public static int TypeCount()
        {
            return mGameDataTypes.Count;
        }

        public static void SetDir(string dir, bool isAssetbundle)
        {
            FileDir = dir;
            IsAssetBundle = isAssetbundle;
        }
    }

    // private
    public partial class GameDataManager
    {
        private static object[] GenericParameterArrayOne = new object[1];
        private const string GameDataNamespaceName = "GameData";
        private static bool IsAssetBundle = false;
        private static string FileDir = ".";


        private static Dictionary<string, List<Type>> mGameDataTypes = new Dictionary<string, List<Type>>();
        private static Dictionary<string, SecurityElement> FileLoaded = new Dictionary<string, SecurityElement>();

        static GameDataManager()
        {
            LoadGameDataTypes();
        }

        private static void ClearData(Type t)
        {
            t.GetMethod("Clear", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Invoke(null, null);
        }
        private static void LoadGameDataTypes()
        {
            ClearAllData();
            mGameDataTypes.Clear();
            Assembly ass = typeof(EmptyGameData).Assembly;
            Type[] types = ass.GetTypes();
            Type gameDataGenericType = typeof(GameData<>);
            Type gameDataType = typeof(GameData);
            foreach (Type item in types)
            {
                if (item.Namespace == GameDataNamespaceName)
                {
                    var type = item.BaseType;
                    while (type != null)
                    {
                        if (type == gameDataType || type.IsGenericType && type.GetGenericTypeDefinition() == gameDataGenericType)
                        {
                            string fileUrl = item.GetField("FileUrl").GetValue(null) as string;
                            string[] strs = fileUrl.Split('#');
                            string fileName = strs[0].Trim();
                            if (!mGameDataTypes.ContainsKey(fileName))
                            {
                                mGameDataTypes.Add(fileName, new List<Type>());
                            }
                            mGameDataTypes[fileName].Add(item);
                            break;
                        }
                        else
                        {
                            type = type.BaseType;
                        }
                    }
                }
            }
        }
        private static SecurityElement LoadXml(string fileName)
        {
            string filePath = string.Format("{0}/{1}", FileDir, fileName);
            if (IsAssetBundle)
            {
                return LoadXmlAsset(filePath);
            }
            else
            {
                return LoadXmlFile(filePath);
            }
        }
        private static SecurityElement LoadXmlFile(string filePath)
        {
            filePath += "_client.xml";
            if (System.IO.File.Exists(filePath))
            {
                return LoadXmlContent(System.IO.File.ReadAllText(filePath));
            }
            return null;
        }
        private static SecurityElement LoadXmlAsset(string filePath)
        {
            UnityEngine.AssetBundle ab = UnityEngine.AssetBundle.LoadFromFile(filePath);
            string xmlContent = ((UnityEngine.TextAsset)ab.mainAsset).text;
            ab.Unload(true);
            UnityEngine.AssetBundle.UnloadAllAssetBundles(false);
            return LoadXmlContent(xmlContent);
        }
        private static SecurityElement LoadXmlContent(string content)
        {
            SecurityParser securityParser = new SecurityParser();
            securityParser.LoadXml(content);
            return securityParser.ToXml();
        }
        private static SecurityElement SerchChildByName(SecurityElement root, string serchNames, bool recursive)
        {
            if (root.Tag == serchNames)
            {
                return root;
            }
            if (root.Children != null)
            {
                foreach (SecurityElement chlid in root.Children)
                {
                    if (chlid.Tag == serchNames)
                    {
                        return chlid;
                    }
                    if (recursive)
                    {
                        SecurityElement p = SerchChildByName(chlid, serchNames, recursive);
                        if (p != null)
                        {
                            return p;
                        }
                    }
                }
            }
            return null;
        }
        private static void Init<T>(SecurityElement p) where T : GameData<T>, new()
        {
            List<T> allDataList = new List<T>();
            Type type = typeof(T);
            MethodInfo method = type.GetMethod("SetOriginData", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (SecurityElement chlid in p.Children)
            {
                T t = new T();
                GenericParameterArrayOne[0] = chlid;
                method.Invoke(t, GenericParameterArrayOne);
                allDataList.Add(t);
            }
            p.Children.Clear(); // 断开引用
            GenericParameterArrayOne[0] = allDataList;
            type.GetMethod("SetData", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Invoke(null, GenericParameterArrayOne);
        }

        private static T Init<T>(Properties p) where T : GameData<T>, new()
        {
            T t = new T();
            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                string value = p.GetString(prop.Name, null);
                if (value != null)
                {
                    var v = GameDataUtils.ToObject(value, prop.PropertyType);
                    prop.SetValue(t, v, null);
                }
            }
            return t;
        }
        
        private static void Initialize<T>(Properties p, ref T instance) where T : new()
        {
            instance = new T();
            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                string value = p.GetString(prop.Name, null);
                if (value != null)
                {
                    var v = GameDataUtils.ToObject(value, prop.PropertyType);
                    prop.SetValue(instance, value, null);
                }
            }
        }
    }
}
