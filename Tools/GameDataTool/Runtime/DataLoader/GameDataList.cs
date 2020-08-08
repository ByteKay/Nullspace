using System;

using System.Collections.Generic;

namespace Nullspace
{
    // 管理器：数组列表
    public class GameDataList<T> : GameData<T> where T : GameDataList<T>, new()
    {
        protected static GameDataCollection<T> mDataList;
        public static GameDataCollection<T> Data
        {
            get
            {
                if (mDataList == null)
                {
                    InitByFileUrl();
                    if (mDataList == null)
                    {
                        throw new Exception("wrong fileName: " + typeof(T).FullName);
                    }
                }
                return mDataList;
            }
        }

        protected static int Count
        {
            get
            {
                return Data != null ? Data.Count : 0;
            }
            
        }

        protected static void SetData(List<T> allDatas)
        {
            if (IsImmediateLoad())
            {
                foreach (T t in allDatas)
                {
                    t.Initialize();
                }
            }
            mDataList = new GameDataCollection<T>();
            mDataList.AddRange(allDatas);
            LogLoadedEnd("" + mDataList.Count);
        }

        protected static void Clear()
        {
            if (mDataList != null)
            {
                mDataList.Reset();
                mDataList = null;
                DebugUtils.Log(string.Format("Clear {0}", typeof(T).FullName));
            }
        }
    }
}
