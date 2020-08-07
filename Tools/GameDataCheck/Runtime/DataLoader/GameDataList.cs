using System;
using System.Collections.Generic;

namespace Nullspace
{
    // 管理器：数组列表
    public class GameDataList<T> : GameData<T> where T : GameDataList<T>, new()
    {
        protected static List<T> mDataList;
        public static List<T> Data
        {
            get
            {
                if (mDataList == null)
                {
                    Init();
                    if (mDataList == null)
                    {
                        throw new Exception("wrong fileName: " + typeof(T).FullName);
                    }
                }
                return mDataList;
            }
        }

        protected static void SetData(List<T> allDatas)
        {
            if (IsImmediateLoad())
            {
                foreach (T t in allDatas)
                {
                    t.IsInitialized();
                }
            }
            mDataList = new List<T>();
            mDataList.AddRange(allDatas);
            LogLoadedEnd("" + mDataList.Count);
        }

        protected static void Clear()
        {
            if (mDataList != null)
            {
                mDataList.Clear();
                mDataList = null;
                GameDataManager.Log(string.Format("Clear {0}", typeof(T).FullName));
            }
        }
    }
}
