using System;

namespace Nullspace
{
    public class IState
    {
        public IState Enter(IAgent agent)
        {
            return this;
        }
        public IState Process(IAgent agent, Action action)
        {
            return this;
        }
        public IState Exit(IAgent agent, Action action)
        {
            return this;
        }
    }
}
