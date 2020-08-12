using System;

namespace Nullspace
{
    public abstract class AbstractCallback : ObjectKey
    {
        public static AbstractCallback Create(Action action)
        {
            Callback callback = ObjectPools.Instance.Acquire<Callback>();
            callback.Handler = action;
            return callback;
        }
        public static AbstractCallback Create<T>(Action<T> action, T arg1)
        {
            Callback<T> callback = ObjectPools.Instance.Acquire<Callback<T>>();
            callback.Handler = action;
            callback.Arg1 = arg1;
            return callback;
        }

        public static AbstractCallback Create<U, V>(Action<U, V> action, U arg1, V arg2)
        {
            Callback<U, V> callback = ObjectPools.Instance.Acquire<Callback<U, V>>();
            callback.Handler = action;
            callback.Arg1 = arg1;
            callback.Arg2 = arg2;
            return callback;
        }
        public static AbstractCallback Create<U, V, W>(Action<U, V, W> action, U arg1, V arg2, W arg3)
        {
            Callback<U, V, W> callback = ObjectPools.Instance.Acquire<Callback<U, V, W>>();
            callback.Handler = action;
            callback.Arg1 = arg1;
            callback.Arg2 = arg2;
            callback.Arg3 = arg3;
            return callback;
        }
        public static AbstractCallback Create<U, V, W, T>(Action<U, V, W, T> action, U arg1, V arg2, W arg3, T arg4)
        {
            Callback<U, V, W, T> callback = ObjectPools.Instance.Acquire<Callback<U, V, W, T>>();
            callback.Handler = action;
            callback.Arg1 = arg1;
            callback.Arg2 = arg2;
            callback.Arg3 = arg3;
            callback.Arg4 = arg4;
            return callback;
        }


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
        public override void Destroy()
        {

        }

        public abstract void Run();
    }

}
