using System;
namespace Nullspace
{
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
}
