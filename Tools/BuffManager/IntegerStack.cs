using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public class IntegerStack
    {
        public class Modifier
        {
            public IntegerStack Parent;
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

            public Modifier(IntegerStack parent)
            {
                Parent = parent;
                Value = 0;
            }

            public Modifier(IntegerStack parent, int value)
            {
                Parent = parent;
                Value = value;
            }
        }

        public Action<int> OnValueChanged;
        private int baseValue;
        private int lastValue;
        private bool isDirty = true;
        private int delta;
        public List<Modifier> FlatModifiers = new List<Modifier>();

        public float Delta
        {
            get
            {
                return delta;
            }
        }

        public void InvokeChanged()
        {
            delta = Value - lastValue;
            lastValue = Value;
            if (OnValueChanged != null)
            {
                OnValueChanged(Value);
            }
        }

        public int Value
        {
            get
            {
                if (isDirty)
                {
                    int modifiedValue = baseValue;
                    modifiedValue += Sum(FlatModifiers);
                    lastValue = modifiedValue;
                }
                return lastValue;
            }
            set
            {
                BaseValue = value;
            }
        }

        public int BaseValue
        {
            get
            {
                return baseValue;
            }
            set
            {
                baseValue = value;
                isDirty = true;
            }
        }

        public Modifier AddFlatModifier(int startingValue)
        {
            var mod = new Modifier(this, startingValue);
            FlatModifiers.Add(mod);
            isDirty = true;
            InvokeChanged();
            return mod;
        }

        public void RemoveFlatModifier(Modifier modifier)
        {
            FlatModifiers.Remove(modifier);
            isDirty = true;
            InvokeChanged();
        }

        protected int Sum(List<Modifier> modifiers)
        {
            int total = 0;
            for (int i = 0; i < modifiers.Count; i++)
            {
                total += modifiers[i].Value;
            }
            return total;
        }

    }

}
