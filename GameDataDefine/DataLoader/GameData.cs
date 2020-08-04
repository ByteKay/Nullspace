
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
        protected static bool IsDelayInitialized = true;
        protected static List<string> KeyNameList = null;
        protected static string FileUrl;
    }

    public partial class GameData<T> : GameData where T : GameData<T>, new()
    {
        public bool IsInitialized()
        {
            if (!mIsInitialized)
            {
                Initialize(this);
            }
            return mIsInitialized;
        }
        protected static int AssignKeyProp<U>(U instance, List<string> keyNameList, ref int key1, ref int key2) where U : GameData<T>
        {
            if (instance.mOriginData.Attributes != null)
            {
                if (keyNameList.Count == 0)
                {
                    return 0;
                }
                if (keyNameList.Count > 2)
                {
                    throw new Exception("Max Count is 2, but is " + keyNameList.Count);
                }
                for (int i = 0; i < keyNameList.Count; ++i)
                {
                    string key = keyNameList[i];
                    int value = (int)instance.mOriginData.Attributes[key];
                    PropertyInfo info = typeof(U).GetProperty(key);
                    Debug.Assert(info != null, "");
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
                return keyNameList.Count;
            }
            return 0;
        }  
        private SecurityElement mOriginData = null;
        private bool mIsInitialized = false;
        private void SetOriginData(SecurityElement originData)
        {
            mOriginData = originData;
        }
        protected static void Init()
        {
            GameDataManager.Init<T>(FileUrl);
        }
        private void Initialize(GameData<T> instance)
        {
            if (mOriginData != null && mOriginData.Attributes != null)
            {
                PropertyInfo[] props = typeof(T).GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    if (KeyNameList != null && KeyNameList.Contains(prop.Name))
                    {
                        continue;
                    }
                    if (mOriginData.Attributes.ContainsKey(prop.Name))
                    {
                        string value = (string)mOriginData.Attributes[prop.Name];
                        var v = GameDataUtils.ParseString(value, prop.PropertyType);
                        prop.SetValue(instance, value, null);
                    }
                }
            }
            mOriginData = null; // 释放
            mIsInitialized = true;
        }
    }
}
