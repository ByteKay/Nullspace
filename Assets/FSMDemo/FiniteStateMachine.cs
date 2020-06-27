using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public enum AIState
    {
        FLEE,
        FOLLOW,
        IDLE
    }

    public class AIParameterName
    {
        public const string Distance = "distance";
    }


    public class FiniteStateMachine : MonoBehaviour
    {
        public Transform Target;
        private StateController<AIState> StateCtl;
        private float FleeSpeed = 6.0f;
        private float FollowSpeed = 3.0f;

        private void Start()
        {
            StateCtl = new StateController<AIState>();
            StateCtl.AddParameter(AIParameterName.Distance, StateParameterDataType.FLOAT, 6.0f);
            StateCtl.AddState(AIState.IDLE).AsCurrent().Enter(IdleEnter).Process(IdleProcess).Exit(IdleExit).AddTransfer(AIState.FLEE).With(AIParameterName.Distance, ConditionOperationType.LESS, 4.0f);
            StateCtl.AddState(AIState.FLEE).Enter(FleeEnter).Process(FleeProcess).Exit(FleeExit).AddTransfer(AIState.IDLE).With(AIParameterName.Distance, ConditionOperationType.GREATER_EQUAL, 4.0f);
            StateCtl.AddState(AIState.FOLLOW).Enter(FollowEnter).Process(FollowProcess).Exit(FollowExit).AddTransfer(AIState.IDLE).With(AIParameterName.Distance, ConditionOperationType.LESS, 10.0f);
            StateCtl.AddState(AIState.IDLE).AddTransfer(AIState.FOLLOW).With(AIParameterName.Distance, ConditionOperationType.GREATER_EQUAL, 10.0f);
        }

        private void Update()
        {
            if (Target != null)
            {
                Vector3 dir = Target.position - transform.position;
                dir.y = 0;
                float dis = dir.magnitude;
                StateCtl.Set(AIParameterName.Distance, dis);
                Move(dir.normalized);
            }
        }



        private void Move(Vector3 dir)
        {
            switch (StateCtl.State)
            {
                case AIState.FLEE:
                    dir = new Vector3(-dir.z, 0, dir.x);
                    transform.forward = dir;
                    transform.position += FleeSpeed * Time.deltaTime * dir;
                    break;
                case AIState.FOLLOW:
                    transform.forward = dir;
                    transform.position += FollowSpeed * Time.deltaTime * dir;
                    break;
            }

        }

        private void IdleEnter()
        {
            DebugUtils.Info("FiniteStateMachine", "IdleEnter");
        }

        private void IdleProcess()
        {
            DebugUtils.Info("FiniteStateMachine", "IdleProcess");
        }

        private void IdleExit()
        {
            DebugUtils.Info("FiniteStateMachine", "IdleExit");
        }

        private void FleeEnter()
        {
            DebugUtils.Info("FiniteStateMachine", "FleeEnter");
        }

        private void FleeProcess()
        {
            DebugUtils.Info("FiniteStateMachine", "FleeProcess");
        }

        private void FleeExit()
        {
            DebugUtils.Info("FiniteStateMachine", "FleeExit");
        }

        private void FollowEnter()
        {
            DebugUtils.Info("FiniteStateMachine", "FollowEnter");
        }

        private void FollowProcess()
        {
            DebugUtils.Info("FiniteStateMachine", "FollowProcess");
        }

        private void FollowExit()
        {
            DebugUtils.Info("FiniteStateMachine", "FollowExit");
        }
    }

}


