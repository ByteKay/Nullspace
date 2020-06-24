using System;
using System.Collections.Generic;
using Nullspace;

namespace Nullspace
{
    [AttributeUsage(AttributeTargets.Class)]
    public class XmlDataAttribute : Attribute
    {
        public string mFileName;
        public string mDescription;
        public XmlDataAttribute(string fileName, string desc = "")
        {
            mFileName = fileName;
            mDescription = desc;
        }
    }



    public abstract class XmlData
    {
        public const string mKeyFieldName = "Id";
        public const string mKeyValueFieldName = "DataMap";
        public const string mXmlSuffix = ".xml";
        public const string mFileName = "fileName";
        public int Id { get; set; }

    }

    public class SortById<T> : IComparer<T> where T : XmlData
    {
        public int Compare(T x, T y)
        {
            return x.Id.CompareTo(y.Id);
        }
    }

    public abstract class XmlData<T> : XmlData where T : XmlData<T>
    {
        public static SortById<T> SortInstance = new SortById<T>();
        public static void CheckDuplicatedDatas(string prefix, List<T> xmlDatas)
        {
            Dictionary<int, List<T>> tmp = new Dictionary<int, List<T>>();
            foreach (T t in xmlDatas)
            {
                if (!tmp.ContainsKey(t.Id))
                {
                    tmp.Add(t.Id, new List<T>());
                }
                tmp[t.Id].Add(t);
            }
            bool hasDup = false;
            string idStr = prefix;
            foreach (var item in tmp)
            {
                if (item.Value.Count > 1)
                {
                    idStr = idStr + " " + item.Key;
                    hasDup = true;
                }
            }
            if (hasDup)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog(prefix, idStr, "确定");
#endif
            }
        }

        private static Dictionary<int, T> mDataMap = null;
        public static Dictionary<int, T> DataMap
        {
            get
            {
                if (mDataMap == null)
                {
                    GetDataMap();
                }
                if (mDataMap == null)
                {
                    DebugUtils.Error("XmlData", "mDataMap == null wrong");
                }
                return mDataMap;
            }
            set
            {
                mDataMap = value;
            }
        }

        public static T GetData(int id)
        {
            if (DataMap != null && DataMap.ContainsKey(id))
            {
                return DataMap[id];
            }
            return null;
        }

        private static void GetDataMap()
        {
            var type = typeof(T);
            XmlDataAttribute attr = XmlDataAttribute.GetCustomAttribute(type, typeof(XmlDataAttribute), false) as XmlDataAttribute;
            if (attr != null)
            {
                var result = XmlDataLoader.Instance.FormatXMLData(attr.mFileName, typeof(Dictionary<int, T>), type);
                mDataMap = result as Dictionary<int, T>;
            }
            else
            {
                mDataMap = new Dictionary<int, T>();
            }
        }
    }
}
