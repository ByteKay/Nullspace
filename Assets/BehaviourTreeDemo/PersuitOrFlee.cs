using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Nullspace
{
    public class DistanceCondition : BTConditionSingleOperation<PersuitOrFlee>
    {

        public DistanceCondition(ConditionOperationType logic, object targetValue, MethodInfo getter) : base(logic, BTConditionValueType.NUMBER, targetValue, getter)
        {

        }

        public override BTNodeState Process(PersuitOrFlee obj)
        {
            return base.Process(obj);
        }
    }

    public class DistanceLessAction : BTActionNode<PersuitOrFlee>
    {
        public override BTNodeState Process(PersuitOrFlee obj)
        {
            obj.Flee();
            return BTNodeState.Success;
        }
    }

    public class DistanceGreaterAction : BTActionNode<PersuitOrFlee>
    {
        public override BTNodeState Process(PersuitOrFlee obj)
        {
            obj.Follow();
            return BTNodeState.Success;
        }
    }

    public class DistanceStayAction : BTActionNode<PersuitOrFlee>
    {
        public override BTNodeState Process(PersuitOrFlee obj)
        {
            obj.Stay();
            return BTNodeState.Success;
        }
    }


    public class PersuitOrFlee : MonoBehaviour
    {
        private static MethodInfo GetDistanceMethod;
        static PersuitOrFlee()
        {
            Type type = typeof(PersuitOrFlee);
            GetDistanceMethod = type.GetMethod("GetDistance");
        }

        public Transform Target;
        private float FleeSpeed = 6.0f;
        private float FollowSpeed = 3.0f;

        private BehaviorTreeRoot<PersuitOrFlee> Tree;
        
        // Use this for initialization
        void Start()
        {
            BTSelectorNode<PersuitOrFlee> root = new BTSelectorNode<PersuitOrFlee>();
            Tree = new BehaviorTreeRoot<PersuitOrFlee>(root);

            BTSequenceNode<PersuitOrFlee> lessSeq = new BTSequenceNode<PersuitOrFlee>();
            root.AddChild(lessSeq);
            lessSeq.AddChild(new DistanceCondition(ConditionOperationType.LESS, 3.0f, GetDistanceMethod));
            lessSeq.AddChild(new DistanceLessAction());

            BTSequenceNode<PersuitOrFlee> greaterSeq = new BTSequenceNode<PersuitOrFlee>();
            root.AddChild(greaterSeq);
            greaterSeq.AddChild(new DistanceCondition(ConditionOperationType.GREATER, 8.0f, GetDistanceMethod));
            greaterSeq.AddChild(new DistanceGreaterAction());

            root.AddChild(new DistanceStayAction());
        }

        // Update is called once per frame
        void Update()
        {
            if (Target != null)
            {
                Tree.Process(this);
            }
        }

        public float GetDistance()
        {
            if (Target != null)
            {
                Vector3 dir = Target.position - transform.position;
                dir.y = 0;
                return dir.magnitude;
            }
            return 0;
        }

        // 还存在一点Bug
        public void Flee()
        {
            Vector3 dir = Direction();
            dir = new Vector3(-dir.z, 0, dir.x);
            transform.forward = dir;
            transform.position += FleeSpeed * Time.deltaTime * dir;

        }

        public void Follow()
        {
            Vector3 dir = Direction();
            transform.forward = dir;
            transform.position += FollowSpeed * Time.deltaTime * dir;
        }

        private Vector3 Direction()
        {
            Vector3 dir = Target.position - transform.position;
            dir.y = 0;
            dir.Normalize();
            return dir;
        }

        public void Stay()
        {

        }
    }

}

