
using System;

namespace Nullspace
{
    public class BTActionNode : BehaviourTreeNode
    {
        public override BTNodeState Process(Object obj)
        {
            return BTNodeState.Ready;
        }
    }


}
