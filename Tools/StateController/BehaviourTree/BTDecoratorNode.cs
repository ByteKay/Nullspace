
using System;
namespace Nullspace
{
    public class BTDecoratorNode<T> : BehaviourTreeNode<T>
    {
        protected BehaviourTreeNode<T> mChild = null;
        public override BTNodeState Process(T obj)
        {
            return BTNodeState.Ready;
        }
        public void Proxy(BehaviourTreeNode<T> child)
        {
            mChild = child;
        }
    }
    // 直到 success
    public class BTUntilSuccessNode<T> : BTDecoratorNode<T>
    {
        public override BTNodeState Process(T obj)
        {
            if (mChild.Process(obj) == BTNodeState.Success)
            {
                return BTNodeState.Success;
            }
            return BTNodeState.Running;
        }
    }

    // 直到 fail
    public class BTUtilFailureNode<T> : BTDecoratorNode<T>
    {
        public override BTNodeState Process(T obj)
        {
            if (mChild.Process(obj) == BTNodeState.Failure)
            {
                return BTNodeState.Success;
            }
            return BTNodeState.Running;
        }
    }

    // 计数器
    public class BTCounterLimitNode<T> : BTDecoratorNode<T>
    {
        private int mRunningLimitCount;
        private int mRunningCount = 0;
        public BTCounterLimitNode(int limit) : base()
        {
            mRunningLimitCount = limit;
        }
        public override BTNodeState Process(T obj)
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
    public class BTTimerLimitNode<T> : BTDecoratorNode<T>
    {
        private BTTimerTask mTimerTask;
        public BTTimerLimitNode(float interval) : base()
        {
            mTimerTask = new BTTimerTask(interval, null);
        }

        public override BTNodeState Process(T obj)
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
    public class BTTimerNode<T> : BTDecoratorNode<T>
    {
        private BTTimerTask mTimerTask;
        public BTTimerNode(float interval) : base()
        {
            mTimerTask = new BTTimerTask(interval, null);
        }

        public override BTNodeState Process(T obj)
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
    public class BTTSimpleTimerNode<T> : BehaviourTreeNode<T>
    {
        private BTTimerTask mTimerTask;
        public BTTSimpleTimerNode(float interval, Callback callback) : base()
        {
            mTimerTask = new BTTimerTask(interval, callback);
        }

        public override BTNodeState Process(T obj)
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
    public class BTInvertNode<T> : BTDecoratorNode<T>
    {
        public override BTNodeState Process(T obj)
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
