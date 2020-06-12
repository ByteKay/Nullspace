
using Object = System.Object;

namespace Nullspace
{
    public enum BTNodeState
    {
        Failure = 0,
        Success,
        Running,
        Ready,
        Error
    }

    public abstract class BehaviourTreeNode
    {
        protected BehaviourTreeNode mRunningNode;
        protected BTNodeState mNodeState;
        public string Name { get; set; }

        public BehaviourTreeNode()
        {
            mRunningNode = null;
            mNodeState = BTNodeState.Ready;
        }

        public virtual void Enter(Object obj)
        {

        }

        public virtual BTNodeState Run(Object obj)
        {
            Enter(obj);
            mNodeState = Process(obj);
            Leave(obj);
            return mNodeState;
        }

        public virtual void Leave(Object obj)
        {
            DebugUtils.Info("Leave", "{0} {1}", Name, mNodeState);
        }

        public abstract BTNodeState Process(Object obj);
    }

    public class BehaviorTreeRoot : BehaviourTreeNode
    {
        public BehaviourTreeNode Root { get; set; }
        public BehaviorTreeRoot()
        {

        }

        public BehaviorTreeRoot(BehaviourTreeNode root)
        {
            Root = root;
        }

        public override BTNodeState Process(Object obj)
        {
            mNodeState = Root.Run(obj);
            return mNodeState;
        }

        
    }
}
