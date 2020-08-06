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
            bool isDelayInitialized = (bool)typeof(T).GetField("IsDelayInitialized").GetValue(null);
            if (!isDelayInitialized)
            {
                foreach (T t in allDatas)
                {
                    t.IsInitialized();
                }
            }
            mDataList = new List<T>();
            mDataList.AddRange(allDatas);
        }

        protected static void Clear()
        {
            if (mDataList != null)
            {
                mDataList.Clear();
                mDataList = null;
            }
        }
    }
}
