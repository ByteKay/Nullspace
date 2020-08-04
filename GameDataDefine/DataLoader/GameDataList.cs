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
                }
                if (mDataList == null)
                {
                    throw new Exception("wrong fileName: " + FileUrl);
                }
                return mDataList;
            }
        }
        private static void SetData(List<T> allDatas)
        {
            mDataList = allDatas;
        }
        private static void Clear()
        {
            if (mDataList != null)
            {
                mDataList.Clear();
                mDataList = null;
            }
        }
    }
}
