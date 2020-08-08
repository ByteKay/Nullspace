
using System.Collections.Generic;

namespace Nullspace
{

    // 管理器：唯一索引
    public class GameDataMap<M, T> : GameData<T> where T : GameDataMap<M, T>, new()
    {
        protected static Dictionary<M, T> mDataMap;
        protected static Dictionary<M, T> Data
        {
            get
            {
                if (mDataMap == null)
                {
                    InitByFileUrl();
                    if (mDataMap == null)
                    {
                        DebugUtils.Log(InfoType.Error, "Wrong FileName ClassTypeName: " + typeof(T).FullName);
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
            List<string> keyNameList = typeof(T).GetField(KeyNameListName).GetValue(null) as List<string>;
            bool isImmediateInitialized = IsImmediateLoad();
            foreach (T t in allDatas)
            {
                int cnt = AssignKeyProp(t, keyNameList, ref key1, ref key2);
                if (isImmediateInitialized)
                {
                    t.Initialize();
                }
                if (!mDataMap.ContainsKey(key1))
                {
                    mDataMap.Add(key1, t);
                }
                else
                {
                    DebugUtils.Log(InfoType.Error, string.Format("Duplicated Key: {0} ", key1));
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
                DebugUtils.Log(InfoType.Info, string.Format("Clear {0}", typeof(T).FullName));
            }
        }

        public static T Get(M m)
        {
            if (Data != null)
            {
                if (Data.ContainsKey(m))
                {
                    T res = Data[m];
                    res.Initialize();
                    return res;
                }
                else
                {
                    DebugUtils.Log(InfoType.Error, string.Format("Not Found Key1:{1}", m));
                }
            }
            DebugUtils.Log(InfoType.Error, string.Format("Data Null : {0}", typeof(T).FullName));
            return null;
        }
    }

    // 管理器：两个索引
    public class GameDataMap<M, N, T> : GameData<T> where T : GameDataMap<M, N, T>, new()
    {
        protected static Dictionary<M, Dictionary<N, T>> mDataMapMap;
        protected static Dictionary<M, Dictionary<N, T>> Data
        {
            get
            {
                if (mDataMapMap == null)
                {
                    InitByFileUrl();
                    if (mDataMapMap == null)
                    {
                        DebugUtils.Log(InfoType.Info, "Wrong File ClassTypeName: " + typeof(T).FullName);
                    }
                }
                return mDataMapMap;
            }
        }
        protected static void SetData(List<T> allDatas)
        {
            mDataMapMap = new Dictionary<M, Dictionary<N, T>>();
            M key1 = default(M);
            N key2 = default(N);
            List<string> keyNameList = typeof(T).GetField(KeyNameListName).GetValue(null) as List<string>;
            bool isImmediateInitialized = IsImmediateLoad();
            foreach (T t in allDatas)
            {
                int cnt = AssignKeyProp(t, keyNameList, ref key1, ref key2);
                if (isImmediateInitialized)
                {
                    t.Initialize();
                }
                if (!mDataMapMap.ContainsKey(key1))
                {
                    mDataMapMap.Add(key1, new Dictionary<N, T>());
                }
                if (!mDataMapMap[key1].ContainsKey(key2))
                {
                    mDataMapMap[key1].Add(key2, t);
                }
                else
                {
                    DebugUtils.Log(InfoType.Error, string.Format("Duplicated Key: {0} {1}", key1, key2));
                }
            }
            LogLoadedEnd("" + mDataMapMap.Count);
        }
        protected static void Clear()
        {
            if (mDataMapMap != null)
            {
                mDataMapMap.Clear();
                mDataMapMap = null;
                DebugUtils.Log(InfoType.Info, string.Format("Clear {0}", typeof(T).FullName));
            }
        }
        
        public static T Get(M m, N n)
        {
            if (Data != null)
            {
                if (Data.ContainsKey(m))
                {
                    if (Data[m].ContainsKey(n))
                    {
                        T res = Data[m][n];
                        res.Initialize();
                        return res;
                    }
                    else
                    {
                        DebugUtils.Log(InfoType.Error, string.Format("Not Found Key2:{0} In Key1:{1}", n, m));
                    }
                }
                else
                {
                    DebugUtils.Log(InfoType.Info, string.Format("Not Found Key1:{1}", m));
                }
            }
            DebugUtils.Log(InfoType.Error, string.Format("Data Null : {0}", typeof(T).FullName));
            return null;
        }

    }
}
