
namespace Animation
{
    public class AnimationTarget
    {
        public enum TargetType
        {
            SCALAR,
            TRANSFORM
        }
        protected TargetType mTargetType;
        public TargetType GetTargetType() { return mTargetType; }

        Animation CreateAnimation(string id, int propertyId, uint keyCount, uint[] keyTimes, float[] keyValues, AnimationCurve.InterpolationType type)
        {
            return null;
        }

        Animation CreateAnimation(string id, int propertyId, uint keyCount, uint[] keyTimes, float[] keyValues, float[] keyInValue, float[] keyOutValue, AnimationCurve.InterpolationType type)
        {
            return null;
        }

        Animation CreateAnimation(string id, string url)
        {
            return null;
        }

        Animation CreateAnimationFromTo(string id, int propertyId, float[] from, float[] to, AnimationCurve.InterpolationType type, uint duration)
        {
            return null;
        }

        Animation CreateAnimationFromBy(string id, int propertyId, float[] from, float[] by, AnimationCurve.InterpolationType type, uint duration)
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
