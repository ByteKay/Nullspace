
using System;
using System.Diagnostics;

namespace Animation
{
    public class AnimationValue
    {
        // The number of float values for the property.
        private uint mComponentCount;  
        // The number of bytes of memory the property is.
        private uint mComponentSize;    
        // The current value of the property.
        private float[] mValue;

        public float GetFloat(uint index)
        {
            Debug.Assert(index < mComponentCount, "wrong");
            return mValue[index];
        }

        public void SetFloat(uint index, float value)
        {
            Debug.Assert(index < mComponentCount);
            mValue[index] = value;
        }

        public void GetFloats(uint index, float[] values, uint count)
        {
            Debug.Assert(mValue != null && values != null && index < mComponentCount && (index + count) <= mComponentCount);
            Array.Copy(mValue, index, values, 0, count);
        }

        public void SetFloats(uint index, float[] values, uint count)
        {
            Debug.Assert(mValue != null && values != null && index < mComponentCount && (index + count) <= mComponentCount);
            Array.Copy(values, 0, mValue, index, count);
        }

        public float[] GetValues()
        {
            return mValue;
        }

        public uint GetComponentCount()
        {
            return mComponentCount;
        }

        public uint GetComponentSize()
        {
            return mComponentSize;
        }
    }
}
