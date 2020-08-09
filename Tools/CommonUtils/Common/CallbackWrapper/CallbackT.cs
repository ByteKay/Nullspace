﻿using System;

namespace Nullspace
{
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
}
