using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;

namespace Nullspace
{
    // public
    public partial class GameDataManager
    {
        public static void Init<T>(string fileUrl) where T : GameData<T>, new()
        {
            string[] strs = fileUrl.Split('#');
            Debug.Assert(strs.Length == 2, "wrong url: " + fileUrl);
            Init<T>(strs[0].Trim(), strs[1].Trim());
        }

        public static void Init<T>(string fileName, string tagName) where T : GameData<T>, new()
        {
            if (!FileLoaded.ContainsKey(fileName))
            {
                SecurityElement root = LoadXmlFile(fileName);
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

        public static void ClearData<T>()
        {
            typeof(T).GetMethod("Clear").Invoke(null, null);
        }

        public static void ClearFileData()
        {
            FileLoaded.Clear();
        }

        public static void ClearAllData()
        {
            foreach (Type t in mGameDataTypes)
            {
                t.GetMethod("Clear").Invoke(null, null);
            }
        }
    }

    // private
    public partial class GameDataManager
    {
        private static List<Type> mGameDataTypes = new List<Type>();
        static GameDataManager()
        {
            LoadGameDataTypes();
        }
        private static void LoadGameDataTypes()
        {
            Assembly ass = typeof(GameDataManager).Assembly;
            Type[] types = ass.GetTypes();
            foreach (Type item in types)
            {
                if (item.Namespace == "GameData")
                {
                    var type = item.BaseType;
                    while (type != null)
                    {
                        if (type == typeof(GameData) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(GameData<>)))
                        {
                            mGameDataTypes.Add(item);
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
        private static Dictionary<string, SecurityElement> FileLoaded = new Dictionary<string, SecurityElement>();
        private static SecurityElement LoadXmlFile(string filePath)
        {
            return LoadXmlContent(System.IO.File.ReadAllText(filePath));
        }
        private static SecurityElement LoadXmlContent(string content)
        {
            SecurityParser securityParser = new SecurityParser();
            securityParser.LoadXml(content);
            return securityParser.ToXml();
        }
        private static SecurityElement SerchChildByName(SecurityElement root, string serchNames, bool recursive = true)
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
                        return root;
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
            MethodInfo method = typeof(T).GetMethod("SetOriginData", BindingFlags.NonPublic);
            foreach (SecurityElement chlid in p.Children)
            {
                T t = new T();
                method.Invoke(t, new object[] { chlid });
                allDataList.Add(t);
            }
            Type type = typeof(T);
            type.GetMethod("SetData", BindingFlags.NonPublic).Invoke(null, new object[] { allDataList });
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
                    var v = GameDataUtils.ParseString(value, prop.PropertyType);
                    prop.SetValue(instance, value, null);
                }
            }
        }

    }
}
