
using System;
namespace Nullspace
{
    public class BTDecoratorNode : BehaviourTreeNode
    {
        protected BehaviourTreeNode mChild = null;
        public override BTNodeState Process(Object obj)
        {
            return BTNodeState.Ready;
        }
        public void Proxy(BehaviourTreeNode child)
        {
            mChild = child;
        }
    }
    // 直到 success
    public class BTUntilSuccessNode : BTDecoratorNode
    {
        public override BTNodeState Process(Object obj)
        {
            if (mChild.Process(obj) == BTNodeState.Success)
            {
                return BTNodeState.Success;
            }
            return BTNodeState.Running;
        }
    }

    // 直到 fail
    public class BTUtilFailureNode : BTDecoratorNode
    {
        public override BTNodeState Process(Object obj)
        {
            if (mChild.Process(obj) == BTNodeState.Failure)
            {
                return BTNodeState.Success;
            }
            return BTNodeState.Running;
        }
    }

    // 计数器
    public class BTCounterLimitNode : BTDecoratorNode
    {
        private int mRunningLimitCount;
        private int mRunningCount = 0;
        public BTCounterLimitNode(int limit) : base()
        {
            mRunningLimitCount = limit;
        }
        public override BTNodeState Process(Object obj)
        {
            mNodeState = mChild.Process(obj);
            if (mNodeState == BTNodeState.Running)
            {
                mRunningCount++;
                if (mRunningCount > mRunningLimitCount)
                {
                    mRunningCount = 0;
                    mNodeState = BTNodeState.Failure;
                }
            }
            else
            {
                mRunningCount = 0;
            }
            return mNodeState;
        }
    }

    // 运行时间内
    public class BTTimerLimitNode : BTDecoratorNode
    {
        private BTTimerTask mTimerTask;
        public BTTimerLimitNode(float interval) : base()
        {
            mTimerTask = new BTTimerTask(interval, null);
        }

        public override BTNodeState Process(Object obj)
        {
            mNodeState = mChild.Process(obj);
            if (mNodeState == BTNodeState.Running)
            {
                bool flag = mTimerTask.Process(obj);
                if (flag)
                {
                    mNodeState = BTNodeState.Failure;
                }
            }
            else
            {
                mTimerTask.Stop();
            }
            return mNodeState;
        }
    }

    // 定时器
    public class BTTimerNode : BTDecoratorNode
    {
        private BTTimerTask mTimerTask;
        public BTTimerNode(float interval) : base()
        {
            mTimerTask = new BTTimerTask(interval, null);
        }

        public override BTNodeState Process(Object obj)
        {
            bool flag = mTimerTask.Process(obj);
            if (flag)
            {
                mNodeState = mChild.Process(obj);
            }
            return mNodeState;
        }
    }

    // 直接装饰器 定时器
    public class BTTSimpleTimerNode : BehaviourTreeNode
    {
        private BTTimerTask mTimerTask;
        public BTTSimpleTimerNode(float interval, Action<Object> action) : base()
        {
            mTimerTask = new BTTimerTask(interval, action);
        }

        public override BTNodeState Process(Object obj)
        {
            bool flag = mTimerTask.Process(obj);
            if (flag)
            {
                return BTNodeState.Success;
            }
            return BTNodeState.Failure;
        }
    }

    // 取非
    public class BTInvertNode : BTDecoratorNode
    {
        public override BTNodeState Process(Object obj)
        {
            mNodeState = mChild.Process(obj);
            if (mNodeState == BTNodeState.Failure)
            {
                mNodeState = BTNodeState.Success;
            }
            else if (mNodeState == BTNodeState.Success)
            {
                mNodeState = BTNodeState.Failure;
            }
            return mNodeState;
        }
    }


}
