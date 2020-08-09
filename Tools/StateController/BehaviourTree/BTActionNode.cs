
using System;

namespace Nullspace
{
    public class BTActionNode<T> : BehaviourTreeNode<T>
    {
        public override BTNodeState Process(T obj)
        {
            return BTNodeState.Ready;
        }
    }


}
