
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

    public abstract class BehaviourTreeNode<T>
    {
        protected BehaviourTreeNode<T> mRunningNode;
        protected BTNodeState mNodeState;
        public string Name { get; set; }

        public BehaviourTreeNode()
        {
            mRunningNode = null;
            mNodeState = BTNodeState.Ready;
        }

        public virtual void Enter(T obj)
        {

        }

        public virtual BTNodeState Run(T obj)
        {
            Enter(obj);
            mNodeState = Process(obj);
            Leave(obj);
            return mNodeState;
        }

        public virtual void Leave(T obj)
        {
            DebugUtils.Log(InfoType.Info, string.Format("Leave {0} {1}", Name, mNodeState));
        }

        public abstract BTNodeState Process(T obj);
    }

    public class BehaviorTreeRoot<T> : BehaviourTreeNode<T>
    {
        public BehaviourTreeNode<T> Root { get; set; }
        public BehaviorTreeRoot()
        {
            Name = "root";
            Root = null;
        }

        public BehaviorTreeRoot(BehaviourTreeNode<T> root)
        {
            Root = root;
            Name = "root";
        }

        public override BTNodeState Process(T obj)
        {
            mNodeState = Root.Run(obj);
            return mNodeState;
        }

        
    }
}
