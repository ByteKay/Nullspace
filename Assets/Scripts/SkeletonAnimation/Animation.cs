
using System.Collections.Generic;

namespace Animation
{
    public class Animation
    {
        public enum AnimationType
        {
            AT_UNKNOWN = 0,
            AT_SIMPLE = 1,
            AT_SKELETON = 2
        }
        protected AnimationTrackList mAnimationTracks; 
        protected AnimationType mAnimationType;
        private AnimationController mController;       
        private string mId;
        private AnimationClipTemplate mDefaultClipTemplate;
        private List<AnimationClipTemplate> mClipTemplates;


    }
}
