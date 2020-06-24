// #define LoadXmlAsync        // 同步接口，异步加载
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if LoadXmlAsync
using System.Threading;
#endif

namespace Nullspace
{
    public class XmlDataLoader
    {
        public static XmlDataLoader Instance = new XmlDataLoader();
        protected Action<int, int, string> mProgress = null;
        protected Action mFinished = null;
        protected string mNameSpace;
        protected Type mType;
        private List<Type> mGameDataTypes = null;

#if LoadXmlAsync
        private bool mLoadResult = true;    //多线程加载过程中可能有问题
        struct XmlThreadData
        {
            public string FileName;
            public Type DicType;
            public Type Type;
            public PropertyInfo p;
            public XmlDataLoader Loader;
            public int index;
            public int count;
            public string FileContent;
        }

        static void FormatXMLDataThread(object obj)
        {
            XmlThreadData xmlData = (XmlThreadData)obj;
            var result = xmlData.Loader.FormatXMLData(xmlData.FileName, xmlData.DicType, xmlData.Type, xmlData.FileContent);
            if (result == null)
            {
                xmlData.Loader.mLoadResult = false;
                return;
            }
            xmlData.p.GetSetMethod().Invoke(null, new object[] { result });
            if (xmlData.Loader.mProgress != null)
            {
                xmlData.Loader.mProgress(xmlData.index, xmlData.count, xmlData.FileName);
            }
        }
#endif

        public bool InitAndLoad(string nameSpace, string flag)
        {
            Init(nameSpace);
            UnityEngine.Profiling.Profiler.BeginSample("InitAndLoad");
            bool ret = LoadData(flag);
            UnityEngine.Profiling.Profiler.EndSample();
            return ret;
        }

        public void Init(string space, Action<int, int, string> progress = null, Action finished = null)
        {
            mProgress = progress;
            mFinished = finished;
            if (mProgress == null)
            {
                mProgress = DefaultProgress;
            }
            if (mFinished == null)
            {
                mFinished = DefaultCompleted;
            }
            mNameSpace = space;
            mType = this.GetType();
            LoadDataTypes();
        }

        public object FormatXMLData(string fileName, Type dicType, Type type, string fileContent = null)
        {
            object result = null;
            if (string.IsNullOrEmpty(fileName))
            {
                DebugUtils.Warning("FormatXMLData", "fileName IsNullOrEmpty");
                return result;
            }
            try
            {
                result = dicType.GetConstructor(Type.EmptyTypes).Invoke(null);
                Dictionary<Int32, Dictionary<String, String>> map;//Int32 为 mId, string 为 属性名, string 为 属性值
                if (fileContent != null && XmlFileUtils.LoadIntMap(fileContent, out map))
                {
                    var props = type.GetProperties();//获取实体属性
                    foreach (var item in map)
                    {
                        var t = type.GetConstructor(Type.EmptyTypes).Invoke(null);//构造实体实例
                        foreach (var prop in props)
                        {
                            if (prop.Name == XmlData.mKeyFieldName)
                            {
                                prop.SetValue(t, item.Key, null);
                            }
                            else
                            {
                                if (item.Value.ContainsKey(prop.Name))
                                {
                                    var value = XmlFileUtils.GetValue(item.Value[prop.Name], prop.PropertyType);
                                    prop.SetValue(t, value, null);
                                }
                            }
                        }
                        dicType.GetMethod("Add").Invoke(result, new object[] { item.Key, t });
                    }
                    DebugUtils.Info(fileName + " Loaded ", map.Count);
                }
                else
                {
                    result = null;
                    DebugUtils.Warning(fileName + " Not Founded ", "Please Check Timestamps right");
                }
            }
            catch (Exception ex)
            {
                DebugUtils.Error("XmlData", ex.Message);
            }
            return result;
        }

        public bool LoadData(string flag)
        {
            bool ret = false;
            try
            {
                ret = LoadData(flag, mGameDataTypes);
                Resources.UnloadUnusedAssets();
                GC.Collect();
                if (mFinished != null)
                {
                    mFinished();
                }
            }
            catch (Exception ex)
            {
                DebugUtils.Error("XmlData", ex.Message);
            }
            return ret;
        }

        private void LoadDataTypes()
        {
            if (mGameDataTypes == null || mGameDataTypes.Count == 0)
            {
                mGameDataTypes = new List<Type>();
                Assembly ass = mType.Assembly;
                var types = ass.GetTypes();
                foreach (var item in types)
                {
                    if (item.Namespace == mNameSpace)
                    {
                        var type = item.BaseType;
                        while (type != null)
                        {
                            if (type == typeof(XmlData) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(XmlData<>)))
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
        }

        private void UnloadAB()
        {

        }

        private bool LoadData(string timeStamps, List<Type> gameDataType)
        {
            UnloadAB();
            var count = gameDataType.Count;
            var i = 1;
#if LoadXmlAsync
            mLoadResult = true;
            List<Thread> threads = new List<Thread>();
#endif
            foreach (var item in gameDataType)
            {
                var p = item.GetProperty(XmlData.mKeyValueFieldName, ~BindingFlags.DeclaredOnly);
                XmlDataAttribute attr = XmlDataAttribute.GetCustomAttribute(item, typeof(XmlDataAttribute), false) as XmlDataAttribute;
                if (p != null && attr != null)
                {
                    string content = GetXmlContent(attr.mFileName + timeStamps);
                    if (String.IsNullOrEmpty(content))
                    {
                        // 没有内容，文件不存在或者时间戳有问题，加载失败
                        DebugUtils.Info("XmlData", attr.mFileName + timeStamps, " IsNullOrEmpty");
                        return false;
                    }
#if LoadXmlAsync
                    XmlThreadData xmlData;
                    xmlData.FileName = attr.mFileName + timeStamps;
                    xmlData.DicType = p.PropertyType;
                    xmlData.Type = item;
                    xmlData.p = p;
                    xmlData.Loader = this;
                    xmlData.index = i;
                    xmlData.count = count;
                    xmlData.FileContent = content;
                    Thread t = new Thread(new ParameterizedThreadStart(FormatXMLDataThread));
                    t.Start(xmlData);
                    threads.Add(t);
#else
                    UnityEngine.Profiling.Profiler.BeginSample("LoadData");
                    var result = FormatXMLData(attr.mFileName, p.PropertyType, item, content);
                    if(result == null)
                    {
                        return false;
                    }
                    p.GetSetMethod().Invoke(null, new object[] { result });
                    string name = attr != null ? attr.mFileName : null;
                    if (mProgress != null)
                    {
                        mProgress(i, count, name);
                    }
                    UnityEngine.Profiling.Profiler.EndSample();
#endif
                }
                i++;
            }

#if LoadXmlAsync
            DebugUtils.Info("XmlData", "thread.Join began");
            foreach (var thread in threads)
            {
                thread.Join();
            }

            UnloadAB();

            DebugUtils.Info("XmlData", "thread.Join end");
            // 多线程加载过程中可能有问题
            return mLoadResult;
#else
            return true;
#endif

        }

        private static void DefaultCompleted()
        {
            DebugUtils.Info("XmlData", "DefaultCompleted");
        }

        private static void DefaultProgress(int index, int count, string name)
        {
            DebugUtils.Info("XmlData", string.Format("{0} {1} of {2} completed", name, index, count));
        }

        private string GetXmlContent(string fileName)
        {
            string content = null;
#if UNITY_EDITOR && !EDITOR_AB
            if (!fileName.EndsWith(XmlData.mXmlSuffix))
            {
                fileName = fileName + XmlData.mXmlSuffix;
            }
            fileName = string.Format("{0}/{1}/{2}", Application.dataPath, "XmlData", fileName);
            content = XmlFileUtils.LoadText(fileName);
#endif
            return content;
        }
    }
}