
using System.Collections.Generic;

namespace NullAnimation
{
    public class NullAnimation
    {
        public enum AnimationType
        {
            AT_UNKNOWN = 0,
            AT_SIMPLE = 1,
            AT_SKELETON = 2
        }
        protected NullAnimationTrackList mAnimationTracks; 
        protected AnimationType mAnimationType;
        private NullAnimationController mController;       
        private string mId;
        private NullAnimationClipTemplate mDefaultClipTemplate;
        private List<NullAnimationClipTemplate> mClipTemplates;
    }
}
