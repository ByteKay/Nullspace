using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public abstract class EventField
    {
        public event Action onChanged;

        public void ResetEvents()
        {
            onChanged = null;
        }

        public void InvokeChanged()
        {
            if (onChanged != null)
            {
                onChanged();
            }
        }
    }

    public class EventField<T> : EventField
    {
        private T internalValue;

        public EventField()
        {
        }

        public EventField(T defaultValue)
        {
            internalValue = defaultValue;
        }

        public T Value
        {
            get
            {
                return internalValue;
            }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(internalValue, value))
                {
                    internalValue = value;
                    InvokeChanged();
                }
            }
        }
    }

}
