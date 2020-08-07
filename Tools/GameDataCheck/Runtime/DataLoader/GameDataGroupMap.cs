using System;
using System.Collections.Generic;
using System.Text;

namespace Nullspace
{

    // 管理器：按key分组
    public class GameDataGroupMap<M, T> : GameData<T> where T : GameDataGroupMap<M, T>, new()
    {
        protected static Dictionary<M, List<T>> mDataMapList;
        public static Dictionary<M, List<T>> Data
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
            mDataMapList = new Dictionary<M, List<T>>();
            M key1 = default(M);
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
                if (!mDataMapList.ContainsKey(key1))
                {
                    mDataMapList.Add(key1, new List<T>());
                }
                mDataMapList[key1].Add(t);
            }
            StringBuilder sb = new StringBuilder();
            foreach (var item in mDataMapList)
            {
                sb.AppendFormat("(Key:{0}, Count:{1}) ", item.Key, item.Value.Count);
            }
            LogLoadedEnd(sb.ToString());
        }

        protected static void Clear()
        {
            if (mDataMapList != null)
            {
                mDataMapList.Clear();
                mDataMapList = null;
                GameDataManager.Log(string.Format("Clear {0}", typeof(T).FullName));
            }
        }
    }
}
