using System;
using System.Collections.Generic;

namespace Nullspace
{

    // 管理器：唯一索引
    public class GameDataOneMap<T> : GameData<T> where T : GameDataOneMap<T>, new()
    {
        protected static Dictionary<int, T> mDataMap;
        public static Dictionary<int, T> Data
        {
            get
            {
                if (mDataMap == null)
                {
                    Init();
                }
                if (mDataMap == null)
                {
                    throw new Exception("wrong fileName: " + FileUrl);
                }
                return mDataMap;
            }
        }
        private static void SetData(List<T> allDatas)
        {
            mDataMap = new Dictionary<int, T>();
            int key1 = -1;
            int key2 = -2;
            foreach (T t in allDatas)
            {
                int cnt = AssignKeyProp(t, KeyNameList, ref key1, ref key2);
                if (!IsDelayInitialized)
                {
                    t.IsInitialized();
                }
                mDataMap.Add(key1, t);
            }
        }
        private static void Clear()
        {
            if (mDataMap != null)
            {
                mDataMap.Clear();
                mDataMap = null;
            }
        }
    }
}
