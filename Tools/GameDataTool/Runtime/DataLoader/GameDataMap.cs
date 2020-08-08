﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Nullspace
{

    // 管理器：唯一索引
    public class GameDataMap<M, T> : GameData<T> where T : GameDataMap<M, T>, new()
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
                    DebugUtils.Log(string.Format("duplicated key: {0} ", key1));
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
                DebugUtils.Log(string.Format("Clear {0}", typeof(T).FullName));
            }
        }
    }

    // 管理器：两个索引
    public class GameDataMap<M, N, T> : GameData<T> where T : GameDataMap<M, N, T>, new()
    {
        protected static Dictionary<M, Dictionary<N, T>> mDataMapMap;
        public static Dictionary<M, Dictionary<N, T>> Data
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
            mDataMapMap = new Dictionary<M, Dictionary<N, T>>();
            M key1 = default(M);
            N key2 = default(N);
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
                    mDataMapMap.Add(key1, new Dictionary<N, T>());
                }
                if (!mDataMapMap[key1].ContainsKey(key2))
                {
                    mDataMapMap[key1].Add(key2, t);
                }
                else
                {
                    DebugUtils.Log(string.Format("duplicated key: {0} {1}", key1, key2));
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
                DebugUtils.Log(string.Format("Clear {0}", typeof(T).FullName));
            }
        }
    }
}
