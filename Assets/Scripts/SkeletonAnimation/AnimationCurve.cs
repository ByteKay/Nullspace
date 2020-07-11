using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Animation
{
    public class AnimationCurve
    {
        public enum InterpolationType
        {
            BEZIER,
            BSPLINE,
            FLAT,
            HERMITE,
            LINEAR,
            SMOOTH,
            STEP,
            QUADRATIC_IN,
            QUADRATIC_OUT,
            QUADRATIC_IN_OUT,
            QUADRATIC_OUT_IN,
            CUBIC_IN,
            CUBIC_OUT,
            CUBIC_IN_OUT,
            CUBIC_OUT_IN,
            QUARTIC_IN,
            QUARTIC_OUT,
            QUARTIC_IN_OUT,
            QUARTIC_OUT_IN,
            QUINTIC_IN,
            QUINTIC_OUT,
            QUINTIC_IN_OUT,
            QUINTIC_OUT_IN,
            SINE_IN,
            SINE_OUT,
            SINE_IN_OUT,
            SINE_OUT_IN,
            EXPONENTIAL_IN,
            EXPONENTIAL_OUT,
            EXPONENTIAL_IN_OUT,
            EXPONENTIAL_OUT_IN,
            CIRCULAR_IN,
            CIRCULAR_OUT,
            CIRCULAR_IN_OUT,
            CIRCULAR_OUT_IN,
            ELASTIC_IN,
            ELASTIC_OUT,
            ELASTIC_IN_OUT,
            ELASTIC_OUT_IN,
            OVERSHOOT_IN,
            OVERSHOOT_OUT,
            OVERSHOOT_IN_OUT,
            OVERSHOOT_OUT_IN,
            BOUNCE_IN,
            BOUNCE_OUT,
            BOUNCE_IN_OUT,
            BOUNCE_OUT_IN
        }

        public struct Point
        {
            public float time;
            public float value;
            public float inValue;
            public float outValue;
            public InterpolationType type;
        }

        private uint mComponentCount;
        private uint mComponentSize;
        private uint[] mQuaternionOffset;
        private uint mPointCount;
        private Point[] mPoints;
    }
}
