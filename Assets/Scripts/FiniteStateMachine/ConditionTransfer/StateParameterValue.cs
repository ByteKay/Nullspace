using System;
using System.Collections.Generic;

namespace Nullspace
{

    public class StateParameterValue
    {
        public string Name;
        public object Value;
    }

    public class StateControllerParameter : StateParameterValue
    {
        public StateParameterDataType DataType;
    }

    public class StateCondition : StateParameterValue
    {
        public ConditionOperationType CompareType;
    }

}
