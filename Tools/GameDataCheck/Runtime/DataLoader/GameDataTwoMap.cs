using System;
using System.Collections.Generic;

namespace Nullspace
{
    // 管理器：两个索引
    public class GameDataTwoMap<T> : GameData<T> where T : GameDataTwoMap<T>, new()
    {
        protected static Dictionary<int, Dictionary<int, T>> mDataMapMap;
        public static Dictionary<int, Dictionary<int, T>> Data
        {
            get
            {
                if (mDataMapMap == null)
                {
                    Init();
                    if (mDataMapMap == null)
                    {
                        throw new Exception("wrong fileName: " + typeof(T).FullName);
                    }
                }
                return mDataMapMap;
            }
        }
        protected static void SetData(List<T> allDatas)
        {
            mDataMapMap = new Dictionary<int, Dictionary<int, T>>();
            int key1 = -1;
            int key2 = -2;
            List<string> keyNameList = typeof(T).GetField("KeyNameList").GetValue(null) as List<string>;
            bool isDelayInitialized = (bool)typeof(T).GetField("IsDelayInitialized").GetValue(null);
            foreach (T t in allDatas)
            {
                int cnt = AssignKeyProp(t, keyNameList, ref key1, ref key2);
                if (!isDelayInitialized)
                {
                    t.IsInitialized();
                }
                if (!mDataMapMap.ContainsKey(key1))
                {
                    mDataMapMap.Add(key1, new Dictionary<int, T>());
                }
                mDataMapMap[key1].Add(key2, t);
            }
        }
        protected static void Clear()
        {
            if (mDataMapMap != null)
            {
                mDataMapMap.Clear();
                mDataMapMap = null;
            }
        }
    }
}
