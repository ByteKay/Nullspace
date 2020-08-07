
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security;

namespace Nullspace
{
    public class GameData
    {

    }

    /// <summary>
    /// 通过 excel 导出
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class GameData<T>
    {

    }

    public partial class GameData<T> : GameData where T : GameData<T>, new()
    {
        protected static int AssignKeyProp<U>(U instance, List<string> keyNameList, ref uint key1, ref uint key2) where U : GameData<T>
        {
            if (instance.mOriginData.Attributes != null)
            {
                if (keyNameList == null || keyNameList.Count == 0)
                {
                    return 0;
                }
                DebugUtils.Assert(keyNameList.Count <= 2, "So Many Key: " + keyNameList.Count);
                for (int i = 0; i < keyNameList.Count; ++i)
                {
                    string key = keyNameList[i];
                    DebugUtils.Assert(instance.mOriginData.Attributes.ContainsKey(key), "Not Contain Key: " + key);
                    string nodeV = instance.mOriginData.Attributes[key].ToString();
                    uint value = GameDataUtils.ToObject<uint>(nodeV);
                    PropertyInfo info = typeof(U).GetProperty(key);
                    DebugUtils.Assert(info != null, "");
                    info.SetValue(instance, value, null);
                    if (i == 0)
                    {
                        key1 = value;
                    }
                    else if (i == 1)
                    {
                        key2 = value;
                    }
                }
                GameDataManager.Log(string.Format("{0} {1}", key1, key2));
                return keyNameList.Count;
            }
            return 0;
        }
        protected static void Init()
        {
            string fileUrl = typeof(T).GetField("FileUrl").GetValue(null) as string;
            LogLoadedBegin(fileUrl);
            GameDataManager.InitByFile<T>(fileUrl);
        }

        protected static bool IsImmediateLoad()
        {
            return GameDataManager.ForceImmediate || !(bool)typeof(T).GetField("IsDelayInitialized").GetValue(null);
        }

        protected static void LogLoadedBegin(string fileUrl)
        {
            GameDataManager.Log(string.Format("***** GameDataTypeName: {0} FileUrl: {1} Begin ***** ", typeof(T).FullName, fileUrl));
        }

        protected static void LogLoadedEnd(string dataInfo)
        {
            GameDataManager.Log(string.Format("***** GameDataTypeName: {0}, IsImmediateLoad: {1}, Info: {2} End ***** ", typeof(T).FullName, IsImmediateLoad(), dataInfo));
        }

        public bool IsInitialized()
        {
            if (!mIsInitialized)
            {
                Initialize(this);
            }
            return mIsInitialized;
        }
        protected SecurityElement mOriginData = null;
        protected bool mIsInitialized = false;
        protected void SetOriginData(SecurityElement originData)
        {
            mOriginData = originData;
        }
        private void Initialize(GameData<T> instance)
        {
            if (mOriginData != null && mOriginData.Attributes != null)
            {
                PropertyInfo[] props = typeof(T).GetProperties();
                List<string> keyNameList = typeof(T).GetField("KeyNameList").GetValue(null) as List<string>;
                foreach (PropertyInfo prop in props)
                {
                    if (keyNameList != null && keyNameList.Contains(prop.Name))
                    {
                        continue;
                    }
                    if (mOriginData.Attributes.ContainsKey(prop.Name))
                    {
                        string value = (string)mOriginData.Attributes[prop.Name];
                        var v = GameDataUtils.ToObject(value, prop.PropertyType);
                        prop.SetValue(instance, v, null);
                    }
                }
            }
            mOriginData = null; // 释放
            mIsInitialized = true;
        }
    }
}
