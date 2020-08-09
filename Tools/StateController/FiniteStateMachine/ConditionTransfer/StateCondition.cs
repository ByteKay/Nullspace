
using System.Collections.Generic;

namespace Nullspace
{
    public class StateConditions
    {
        private List<StateCondition> mConditions;

        public StateConditions()
        {
            mConditions = new List<StateCondition>();
        }

        /// <summary>
        /// 添加条件
        /// </summary>
        /// <param name="parameterName">条件参数名</param>
        /// <param name="comType">比较类别</param>
        /// <param name="value">目标值</param>
        /// <returns></returns>
        public StateConditions With(string parameterName, ConditionOperationType comType, object value)
        {
            // 这里不去重复
            // 比如 条件是一个区间
            StateCondition condition = new StateCondition();
            condition.Name = parameterName;
            condition.Value = value;
            condition.CompareType = comType;
            mConditions.Add(condition);
            return this;
        }

        public bool IsSuccess<T>(StateController<T> controller)
        {
            foreach (StateCondition condition in mConditions)
            {
                if (!controller.CheckCondition(condition))
                {
                    return false;
                }
            }
            return true;
        }

        public bool Contain(string parameterName)
        {
            int index = mConditions.FindIndex((item) => { return item.Name == parameterName; });
            return index >= 0;
        }
    }

    


    

}
