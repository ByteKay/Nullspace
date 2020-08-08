
using System.Collections;
using System.Collections.Generic;

namespace Nullspace
{
    public class GameDataCollection<T> : IEnumerator<T> where T : GameData<T>, new()
    {
        private List<T> mDatas;
        private IEnumerator mItr;
        public GameDataCollection()
        {
            mDatas = new List<T>();
            mItr = mDatas.GetEnumerator();
        }

        public void Add(T t)
        {
            mDatas.Add(t);
        }
        public void AddRange(IEnumerable<T> datas)
        {
            mDatas.AddRange(datas);
        }
        public int Count
        {
            get { return mDatas.Count; }
        }

        public T Current
        {
            get
            {
                T t = (T)mItr.Current;
                t.Initialize();
                return t;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Dispose()
        {
            mDatas.Clear();
            Reset();
        }

        public bool MoveNext()
        {
            return mItr.MoveNext();
        }

        public void Reset()
        {
            mItr = mDatas.GetEnumerator();
        }
    }


}
