
namespace NullAnimation
{
    public class NullAnimationTarget
    {
        public enum NullTargetType
        {
            SCALAR,
            TRANSFORM
        }
        protected NullTargetType mTargetType;
        public NullTargetType GetTargetType() { return mTargetType; }

        NullAnimation CreateAnimation(string id, int propertyId, uint keyCount, uint[] keyTimes, float[] keyValues, NullAnimationCurve.InterpolationType type)
        {
            return null;
        }

        NullAnimation CreateAnimation(string id, int propertyId, uint keyCount, uint[] keyTimes, float[] keyValues, float[] keyInValue, float[] keyOutValue, NullAnimationCurve.InterpolationType type)
        {
            return null;
        }

        NullAnimation CreateAnimation(string id, string url)
        {
            return null;
        }

        NullAnimation CreateAnimationFromTo(string id, int propertyId, float[] from, float[] to, NullAnimationCurve.InterpolationType type, uint duration)
        {
            return null;
        }

        NullAnimation CreateAnimationFromBy(string id, int propertyId, float[] from, float[] by, NullAnimationCurve.InterpolationType type, uint duration)
        {
            return null;
        }


        private void ConvertByValues(uint propertyId, uint componentCount, float[] from, float[] by)
        {

        }

        private void ConvertQuaternionByValues(float[] from, float[] by)
        {
            
        }

        private void ConvertScaleByValues(float[] from, float[] by, uint componentCount)
        {

        }

        private void ConvertByValues(float[] from, float[] by, uint componentCount)
        {

        }
    }
}
