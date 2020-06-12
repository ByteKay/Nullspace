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
        public const string mKeyFieldName = "id";
        public const string mKeyValueFieldName = "DataMap";
        public const string mXmlSuffix = ".xml";
        public const string mFileName = "fileName";
        public int id { get; set; }
    }

    public abstract class XmlData<T> : XmlData where T : XmlData<T>
    {
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
