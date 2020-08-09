using System;
using Object = System.Object;
using UnityEngine;
using System.Reflection;

namespace Nullspace
{

    public class BTConditionNode<T> : BehaviourTreeNode<T>
    {
        public override BTNodeState Process(T obj)
        {
            return BTNodeState.Ready;
        }
    }

    public enum BTConditionBoolean
    {
        AND,
        OR,
    }

    public enum BTConditionValueType
    {
        NUMBER, // float
        STRING
    }

    // <,>,==,>=,<=,!=
    public class BTConditionSingleOperation<T> : BehaviourTreeNode<T>
    {
        private ConditionOperationType OperationLogic;
        private BTConditionValueType ValueType;
        private object TargetValue;
        private MethodInfo GetterFunc;

        public BTConditionSingleOperation(ConditionOperationType operation, BTConditionValueType valueType, object targetValue, MethodInfo getter) : base()
        {
            OperationLogic = operation;
            ValueType = valueType;
            TargetValue = targetValue;
            GetterFunc = getter;
        }


        public override BTNodeState Process(T obj)
        {
            object value = GetterFunc.Invoke(obj, null);
            bool result = false;
            if (ValueType == BTConditionValueType.NUMBER)
            {
                result = CompareNumberValue((float)value, (float)TargetValue);
            }
            else if (ValueType == BTConditionValueType.STRING)
            {
                result = CompareStringValue((string)value, (string)TargetValue);
            }
            return result ? BTNodeState.Success : BTNodeState.Failure;
        }

        private bool CompareNumberValue(float current, float target)
        {
            bool result = false;
            switch (OperationLogic)
            {
                case ConditionOperationType.EQUAL:
                    result = current == target;
                    break;
                case ConditionOperationType.GREATER:
                    result = current > target;
                    break;
                case ConditionOperationType.GREATER_EQUAL:
                    result = current >= target;
                    break;
                case ConditionOperationType.LESS:
                    result = current < target;
                    break;
                case ConditionOperationType.LESS_EQUAL:
                    result = current <= target;
                    break;
                case ConditionOperationType.NOT_EQUAL:
                    result = current != target;
                    break;
            }
            return result;
        }

        private bool CompareStringValue(string current, string target)
        {
            bool result = false;
            switch (OperationLogic)
            {
                case ConditionOperationType.EQUAL:
                    result = current == target;
                    break;
                case ConditionOperationType.NOT_EQUAL:
                    result = current != target;
                    break;
                case ConditionOperationType.GREATER:
                case ConditionOperationType.GREATER_EQUAL:
                case ConditionOperationType.LESS:
                case ConditionOperationType.LESS_EQUAL:
                    throw new Exception("不支持字符串 大于 或 小于 比较！");
            }
            return result;
        }
    }


    // AND
    // a < b < c 
    // a < b <= c 
    // a <= b < c 
    // a <= b <= c 
    // and so on

    // OR
    // a < b or b > c 
    // a < b or b >= c 
    // a > b or b < c 
    // a >= b or b <= c
    // and so on
    public class BTConditionRangeOperation<T> : BehaviourTreeNode<T>
    {
        private BTConditionSingleOperation<T> LeftOperation;
        private BTConditionSingleOperation<T> RightOperation;
        private BTConditionBoolean RangeBoolean; // || && 
        public BTConditionRangeOperation(BTConditionBoolean rangeBoolean, ConditionOperationType leftOperation, object leftValue, ConditionOperationType rightOperation, object rightValue, BTConditionValueType valueType, MethodInfo getter) : base()
        {
            LeftOperation = new BTConditionSingleOperation<T>(leftOperation, valueType, leftOperation, getter);
            RightOperation = new BTConditionSingleOperation<T>(leftOperation, valueType, leftOperation, getter);
            RangeBoolean = rangeBoolean;
        }

        public override BTNodeState Process(T obj)
        {
            BTNodeState state = LeftOperation.Process(obj);
            if (RangeBoolean == BTConditionBoolean.AND)
            {
                if (state == BTNodeState.Success)
                {
                    state = RightOperation.Process(obj);
                }
            }
            else if (RangeBoolean == BTConditionBoolean.OR)
            {
                if (state == BTNodeState.Failure)
                {
                    state = RightOperation.Process(obj);
                }
            }

            return state;
        }
    }

}
