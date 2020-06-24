
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public abstract class TimerCallback : ObjectCacheBase
    {
        public abstract Delegate Handler
        {
            get;
            set;
        }

        public abstract void Run();
    }

    public class Callback : TimerCallback
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

    public class Callback<T> : TimerCallback
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

    public class Callback<T, U> : TimerCallback
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

    public class Callback<T, U, V> : TimerCallback
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
    public class Callback<T, U, V, W> : TimerCallback
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
