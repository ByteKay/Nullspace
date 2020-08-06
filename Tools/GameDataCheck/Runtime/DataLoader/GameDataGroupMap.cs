using System;
using System.Collections.Generic;

namespace Nullspace
{

    // 管理器：按key分组
    public class GameDataGroupMap<T> : GameData<T> where T : GameDataGroupMap<T>, new()
    {
        protected static Dictionary<int, List<T>> mDataMapList;
        public static Dictionary<int, List<T>> Data
        {
            get
            {
                if (mDataMapList == null)
                {
                    Init();
                    if (mDataMapList == null)
                    {
                        throw new Exception("wrong fileName: " + typeof(T).FullName);
                    }
                }
                return mDataMapList;
            }
        }
        protected static void SetData(List<T> allDatas)
        {
            mDataMapList = new Dictionary<int, List<T>>();
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
                if (!mDataMapList.ContainsKey(key1))
                {
                    mDataMapList.Add(key1, new List<T>());
                }
                mDataMapList[key1].Add(t);
            }
        }
        protected static void Clear()
        {
            if (mDataMapList != null)
            {
                mDataMapList.Clear();
                mDataMapList = null;
            }
        }
    }
}
