using System;
using Object = System.Object;
using UnityEngine;

namespace Nullspace
{
    public class BTConditionNode : BehaviourTreeNode
    {
        public override BTNodeState Process(Object obj)
        {
            return BTNodeState.Ready;
        }
    }

}
