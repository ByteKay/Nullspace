using System;
using System.Collections.Generic;
using System.Text;

namespace Nullspace
{
    // 管理器：两个索引
    public class GameDataTwoMap<T> : GameData<T> where T : GameDataTwoMap<T>, new()
    {
        protected static Dictionary<uint, Dictionary<uint, T>> mDataMapMap;
        public static Dictionary<uint, Dictionary<uint, T>> Data
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
            mDataMapMap = new Dictionary<uint, Dictionary<uint, T>>();
            uint key1 = uint.MaxValue;
            uint key2 = uint.MaxValue;
            List<string> keyNameList = typeof(T).GetField("KeyNameList").GetValue(null) as List<string>;
            bool isImmediateInitialized = IsImmediateLoad();
            foreach (T t in allDatas)
            {
                int cnt = AssignKeyProp(t, keyNameList, ref key1, ref key2);
                if (isImmediateInitialized)
                {
                    t.IsInitialized();
                }
                if (!mDataMapMap.ContainsKey(key1))
                {
                    mDataMapMap.Add(key1, new Dictionary<uint, T>());
                }
                if (!mDataMapMap[key1].ContainsKey(key2))
                {
                    mDataMapMap[key1].Add(key2, t);
                }
                else
                {
                    GameDataManager.Log(string.Format("duplicated key: {0} {1}", key1, key2));
                }
            }
            StringBuilder sb = new StringBuilder();
            foreach (var item in mDataMapMap)
            {
                var data = item.Value.GetEnumerator();
                data.MoveNext();
                var key = data.Current.Key;
                sb.AppendFormat("(Key:({0}, {1}), Count:{2}) ", item.Key, key, item.Value.Count);
            }
            LogLoadedEnd(sb.ToString());
        }
        protected static void Clear()
        {
            if (mDataMapMap != null)
            {
                mDataMapMap.Clear();
                mDataMapMap = null;
                GameDataManager.Log(string.Format("Clear {0}", typeof(T).FullName));
            }
        }
    }
}
