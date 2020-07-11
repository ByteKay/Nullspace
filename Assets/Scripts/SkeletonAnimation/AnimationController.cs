
using System.Collections.Generic;

namespace Animation
{
    public class AnimationController
    {
        // The states that the AnimationController may be in.
        private enum State
        {
            RUNNING,
            IDLE,
            PAUSED,
            STOPPED
        }
        // The current state of the AnimationController.
        private State mState;                                  
        // A list of running AnimationClips.
        private List<AnimationClip> mRunningClips;

        private State GetState()
        {
            return mState;
        }

        private void Initialize()
        {

        }

        private void Finished()
        {

        }

        private void Resume()
        {

        }

        private void Pause()
        {

        }

        public void StopAllAnimations()
        {

        }

        public void Update(float elapsedTime)
        {

        }

        public void Schedule(AnimationClip clip)
        {

        }

        public void Unschedule(AnimationClip clip)
        {

        }

        public void UnscheduleClips(AnimationClipTemplate myTemplate)
        {

        }

    }
}
