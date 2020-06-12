using System;
using System.Collections.Generic;

namespace Nullspace
{
    public class StateController<T>
    {
        private Dictionary<string, StateControllerParameter> mParameters;
        private StateEntity<T> mEntryState;
        private StateEntity<T> mExitState;
        private StateEntity<T> mAnyStates;
        private List<StateEntity<T>> mStateSet;

        public StateController()
        {
            mParameters = new Dictionary<string, StateControllerParameter>();
            mStateSet = new List<StateEntity<T>>();
        }

        public StateEntity<T> Current { get; set; }

        /// <summary>
        /// 添加一个新状态
        /// </summary>
        /// <param name="stateType">状态类别</param>
        /// <returns></returns>
        public StateEntity<T> AddState(T stateType)
        {
            int index = mStateSet.FindIndex((item) => { return stateType.Equals(item.StateType); });
            StateEntity<T> entity = null;
            if (index < 0)
            {
                entity = new StateEntity<T>(stateType, this);
            }
            else
            {
                entity = mStateSet[index];
            }
            return entity;
        }

        public void Set<U>(string paraName, U value)
        {
            if (mParameters.ContainsKey(paraName))
            {
                StateParameterDataType type = StateParameterDataType.TRIGGER;
                Type vType = typeof(U);
                if (vType == typeof(int))
                {
                    type = StateParameterDataType.INT;
                }
                else if (vType == typeof(float))
                {
                    type = StateParameterDataType.FLOAT;
                }
                else if (vType == typeof(bool))
                {
                    type = StateParameterDataType.BOOLEAN;
                }
                else
                {

                }
                Assert(type == mParameters[paraName].DataType, "wrong value type");
                mParameters[paraName].Value = value;
                Update(paraName);
            }
        }

        public void Update(string paramName)
        {
            if (Current != null && Current.ContainParameter(paramName))
            {
                T nextTransfer;
                bool isDirty = Current.CheckTransfer(out nextTransfer);
                if (isDirty)
                {
                    Assert(!nextTransfer.Equals(Current.StateType), "wrong changed state");
                    // todo
                }
            }
        }

        /// <summary>
        /// 该控制参数名不能重复
        /// </summary>
        /// <param name="paraName">参数名</param>
        /// <param name="dataType">参数类型</param>
        /// <param name="ctlValue">参数控制值</param>
        /// <returns></returns>
        public StateController<T> AddParameter(string paraName, StateParameterDataType dataType, object ctlValue)
        {
            if (!mParameters.ContainsKey(paraName))
            {
                StateControllerParameter param = new StateControllerParameter();
                param.Name = paraName;
                param.DataType = dataType;
                param.Value = ctlValue;
                mParameters.Add(paraName, param);
            }
            else
            {
                // duplicated
            }
            return this;
        }


        public bool CheckCondition(StateCondition condition)
        {
            Assert(condition != null, "wrong condition");
            StateControllerParameter param = mParameters[condition.Name];
            Assert(param != null, "wrong parameter");
            switch (param.DataType)
            {
                case StateParameterDataType.TRIGGER:
                    return true;
                case StateParameterDataType.BOOLEAN:
                    return param.Value.Equals(condition.Value);
                case StateParameterDataType.FLOAT:
                case StateParameterDataType.INT: // int 也当作 浮点比较
                    return CheckFloat((float)condition.Value, (float)param.Value, condition.CompareType);
            }
            return true;
        }

        public static bool CheckFloat(float a, float b, StateConditionType type)
        {
            switch (type)
            {
                case StateConditionType.EQUAL:
                    return a == b;
                case StateConditionType.GREATER:
                    return a > b;
                case StateConditionType.GREATER_EQUAL:
                    return a >= b;
                case StateConditionType.LESS:
                    return a < b;
                case StateConditionType.LESS_EQUAL:
                    return a <= b;
                case StateConditionType.NOT_EQUAL:
                    return a != b;
            }
            return false;
        }

        public static void Assert(bool cond, string message)
        {
            if (!cond)
            {
                throw new Exception(message);
            }
        }
    }
}
