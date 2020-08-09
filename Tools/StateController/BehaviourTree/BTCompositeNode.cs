
using System;
using System.Collections.Generic;
using System.Collections;
using Object = System.Object;
using UnityEngine;

namespace Nullspace
{
    public class BTCompositeNode<T> : BehaviourTreeNode<T>
    {
        protected List<BehaviourTreeNode<T>> mChildren;
        public BTCompositeNode() : base()
        {
            mChildren = new List<BehaviourTreeNode<T>>();
        }

        public void AddChild(BehaviourTreeNode<T> node)
        {
            mChildren.Add(node);
        }

        public override BTNodeState Process(T obj)
        {
            return BTNodeState.Ready;
        }
    }

    public class BTSelectorNode<T> : BTCompositeNode<T>
    {
        public override BTNodeState Process(T obj)
        {
            int index = 0;
            if (mNodeState == BTNodeState.Running && mRunningNode != null)
            {
                index = mChildren.IndexOf(mRunningNode);
                if (index == -1)
                {
                    mNodeState = BTNodeState.Ready;
                    index = 0;
                }
            }
            for (int i = index; i < mChildren.Count; ++i)
            {
                mNodeState = mChildren[i].Run(obj);
                if (mNodeState == BTNodeState.Running)
                {
                    mRunningNode = mChildren[i];
                    return mNodeState;
                }
                if (mNodeState == BTNodeState.Success)
                {
                    return mNodeState;
                }
            }
            mNodeState = BTNodeState.Failure;
            return mNodeState;
        }
    }

    public class BTSequenceNode<T> : BTCompositeNode<T>
    {
        public override BTNodeState Process(T obj)
        {
            int index = 0;
            if (mNodeState == BTNodeState.Running && mRunningNode != null)
            {
                index = mChildren.IndexOf(mRunningNode);
                if (index == -1)
                {
                    mNodeState = BTNodeState.Ready;
                    index = 0;
                }
            }
            for (int i = index; i < mChildren.Count; ++i)
            {
                mNodeState = mChildren[i].Run(obj);
                if (mNodeState == BTNodeState.Running)
                {
                    mRunningNode = mChildren[i];
                    return mNodeState;
                }
                if (mNodeState == BTNodeState.Failure)
                {
                    return mNodeState;
                }
            }
            mNodeState = BTNodeState.Success;
            return mNodeState;
        }
    }

    public class BTRandomSelectorNode<T> : BTCompositeNode<T>
    {
        public override BTNodeState Process(T obj)
        {
            int[] seq = MathUtils.RandomShuffle(mChildren.Count);
            foreach (int index in seq)
            {
                if (mChildren[index].Run(obj) == BTNodeState.Success)
                {
                    return BTNodeState.Success;
                }
            }
            return BTNodeState.Failure;
        }

    }

    // 并行，与 Sequence 不同，节点同时运行
    //  &&  || 操作
    // 只是优化 可使用 协程 StartCor
    public abstract class BTParallelNode<T> : BTCompositeNode<T>
    {
        // 并行节点除了继承 BehaviourTreeNode 外，还需要实现 BTParallelFunc
        public interface IBTParallelFunc
        {
            IEnumerator RunCode();
            BTNodeState NodeStatus();
        }

        private List<Coroutine> mCoroutines = new List<Coroutine>();
        private MonoBehaviour behaviour;
        public override BTNodeState Process(T obj)
        {
            // todo by kay
            //if (mNodeState == BTNodeState.Ready || mNodeState == BTNodeState.Success)
            //{
            //    if (!(obj is MonoBehaviour))
            //    {
            //        throw new Exception("exception: " + "obj should extends MonoBehaviour");
            //    }
            //    behaviour = (MonoBehaviour)obj;
            //    foreach (BehaviourTreeNode node in mChildren)
            //    {
            //        if (typeof(IBTParallelFunc).IsAssignableFrom(node.GetType()))
            //        {
            //            mCoroutines.Add(behaviour.StartCoroutine(((IBTParallelFunc)node).RunCode()));
            //        }
            //        else
            //        {
            //            throw new Exception("exception: " + node.GetType().Name + " should implements IBTParallelFunc");
            //        }
            //    }
            //    mNodeState = BTNodeState.Running;
            //}
            //else if (mNodeState == BTNodeState.Running)
            //{
            //    mNodeState = Handle();
            //}
            return mNodeState;
        }

        protected abstract BTNodeState Handle();

        protected void Cancel()
        {
            if (behaviour != null)
            {
                foreach (Coroutine cor in mCoroutines)
                {
                    behaviour.StopCoroutine(cor);
                }
            }
        }
    }

    public class BTAndParaNode<T> : BTParallelNode<T>
    {
        protected override BTNodeState Handle()
        {
            BTNodeState result = BTNodeState.Running;
            int count = 0;
            foreach (BehaviourTreeNode<T> node in mChildren)
            {
                if (((IBTParallelFunc)node).NodeStatus() == BTNodeState.Failure)
                {
                    result = BTNodeState.Failure;
                    break;
                }
                else if (((IBTParallelFunc)node).NodeStatus() == BTNodeState.Running)
                {
                    continue;
                }
                else if (((IBTParallelFunc)node).NodeStatus() == BTNodeState.Success)
                {
                    count++;
                }
            }
            if (count == mChildren.Count)
            {
                result = BTNodeState.Success;
            }
            return result;
        }
    }

    public class BTOrParaNode<T> : BTParallelNode<T>
    {
        protected override BTNodeState Handle()
        {
            BTNodeState result = BTNodeState.Running;
            int count = 0;
            foreach (BehaviourTreeNode<T> node in mChildren)
            {
                if (((IBTParallelFunc)node).NodeStatus() == BTNodeState.Success)
                {
                    result = BTNodeState.Success;
                    break;
                }
                else if (((IBTParallelFunc)node).NodeStatus() == BTNodeState.Running)
                {
                    continue;
                }
                else if (((IBTParallelFunc)node).NodeStatus() == BTNodeState.Failure)
                {
                    count++;
                }
            }
            if (count == mChildren.Count)
            {
                result = BTNodeState.Failure;
            }
            return result;
        }
    }

}
