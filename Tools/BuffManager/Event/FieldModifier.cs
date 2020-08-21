using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public class Modifier
    {
        public ValueStack Parent;
        public int currentValue;

        public int Value
        {
            get
            {
                return currentValue;
            }
            set
            {
                currentValue = value;
                Parent.isDirty = true;
                Parent.InvokeChanged();
            }
        }

        public Modifier(ValueStack parent)
        {
            Parent = parent;
            Value = 0;
        }

        public Modifier(ValueStack parent, int value)
        {
            Parent = parent;
            Value = value;
        }
    }
}
