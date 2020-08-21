using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public abstract class EventField
    {
        public event Action OnChanged;

        public void ResetEvents()
        {
            OnChanged = null;
        }

        public void InvokeChanged()
        {
            if (OnChanged != null)
            {
                OnChanged();
            }
        }
    }

    public class EventField<T> : EventField
    {
        private T mInternalValue;

        public EventField()
        {
        }

        public EventField(T defaultValue)
        {
            mInternalValue = defaultValue;
        }

        public T Value
        {
            get
            {
                return mInternalValue;
            }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(mInternalValue, value))
                {
                    mInternalValue = value;
                    InvokeChanged();
                }
            }
        }
    }

}
