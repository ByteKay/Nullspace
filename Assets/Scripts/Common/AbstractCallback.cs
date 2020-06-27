
using System;
namespace Nullspace
{
    public abstract class AbstractCallback : ObjectCacheBase
    {
        public abstract Delegate Handler
        {
            get;
            set;
        }
        public override void Initialize()
        {

        }
        public override void Clear()
        {

        }

        public abstract void Run();
    }

    public class Callback : AbstractCallback
    {
        private Action mAction;
        public override Delegate Handler
        {
            get { return mAction; }
            set { mAction = value as Action; }
        }

        public override void Run()
        {
            mAction();
        }
    }

    public class Callback<T> : AbstractCallback
    {
        private Action<T> mAction;
        public T Arg1 { get; set; }
        public override Delegate Handler
        {
            get { return mAction; }
            set { mAction = value as Action<T>; }
        }

        public override void Run()
        {
            mAction(Arg1);
        }
    }

    public class Callback<T, U> : AbstractCallback
    {
        private Action<T, U> mAction;
        public T Arg1 { get; set; }

        public U Arg2 { get; set; }
        public override Delegate Handler
        {
            get { return mAction; }
            set { mAction = value as Action<T, U>; }
        }

        public override void Run()
        {
            mAction(Arg1, Arg2);
        }
    }

    public class Callback<T, U, V> : AbstractCallback
    {
        private Action<T, U, V> mAction;
        public T Arg1 { get; set; }

        public U Arg2 { get; set; }

        public V Arg3 { get; set; }

        public override Delegate Handler
        {
            get { return mAction; }
            set { mAction = value as Action<T, U, V>; }
        }

        public override void Run()
        {
            mAction(Arg1, Arg2, Arg3);
        }
    }
    public class Callback<T, U, V, W> : AbstractCallback
    {
        private Action<T, U, V, W> mAction;
        public T Arg1 { get; set; }
        public U Arg2 { get; set; }
        public V Arg3 { get; set; }
        public W Arg4 { get; set; }
        public override Delegate Handler
        {
            get { return mAction; }
            set { mAction = value as Action<T, U, V, W>; }
        }

        public override void Run()
        {
            mAction(Arg1, Arg2, Arg3, Arg4);
        }
    }
}
