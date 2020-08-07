using System;
using System.Collections.Generic;

namespace Nullspace
{

    // 管理器：唯一索引
    public class GameDataOneMap<M, T> : GameData<T> where T : GameDataOneMap<M, T>, new()
    {
        protected static Dictionary<M, T> mDataMap;
        public static Dictionary<M, T> Data
        {
            get
            {
                if (mDataMap == null)
                {
                    Init();
                    if (mDataMap == null)
                    {
                        throw new Exception("wrong fileName: " + typeof(T).FullName);
                    }
                }
                return mDataMap;
            }
        }
        protected static void SetData(List<T> allDatas)
        {
            mDataMap = new Dictionary<M, T>();
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
                if (!mDataMap.ContainsKey(key1))
                {
                    mDataMap.Add(key1, t);
                }
                else
                {
                    GameDataManager.Log(string.Format("duplicated key: {0} ", key1));
                }
                
            }
            LogLoadedEnd("" + mDataMap.Count);
        }
        protected static void Clear()
        {
            if (mDataMap != null)
            {
                mDataMap.Clear();
                mDataMap = null;
                GameDataManager.Log(string.Format("Clear {0}", typeof(T).FullName));
            }
        }
    }
}
