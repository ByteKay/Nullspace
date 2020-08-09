using System;

namespace Nullspace
{
    public abstract class AbstractCallback : ObjectKey
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

}
