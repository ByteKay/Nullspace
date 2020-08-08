
using System.Collections.Generic;

namespace Nullspace
{
    // 管理器：按key分组
    public class GameDataGroup<M, T> : GameData<T> where T : GameDataGroup<M, T>, new()
    {
        protected static Dictionary<M, GameDataCollection<T>> mDataMapList;
        protected static Dictionary<M, GameDataCollection<T>> Data
        {
            get
            {
                if (mDataMapList == null)
                {
                    InitByFileUrl();
                    if (mDataMapList == null)
                    {
                        DebugUtils.Log(string.Format("Wrong FileName: {0}", typeof(T).FullName));
                    }
                }
                return mDataMapList;
            }
        }
        protected static void SetData(List<T> allDatas)
        {
            mDataMapList = new Dictionary<M, GameDataCollection<T>>();
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
                if (!mDataMapList.ContainsKey(key1))
                {
                    mDataMapList.Add(key1, new GameDataCollection<T>());
                }
                mDataMapList[key1].Add(t);
            }
            LogLoadedEnd("" + mDataMapList.Count);
        }
        protected static void Clear()
        {
            if (mDataMapList != null)
            {
                mDataMapList.Clear();
                mDataMapList = null;
                DebugUtils.Log(string.Format("Clear {0}", typeof(T).FullName));
            }
        }
        public static GameDataCollection<T> Get(M m)
        {
            if (Data != null)
            {
                if (Data.ContainsKey(m))
                {
                    return Data[m];
                }
                else
                {
                    DebugUtils.Log(string.Format("Not Found Key1:{1}", m));
                }
            }
            DebugUtils.Log(string.Format("Data Null : {0}", typeof(T).FullName));
            return null;
        }

    }
}
