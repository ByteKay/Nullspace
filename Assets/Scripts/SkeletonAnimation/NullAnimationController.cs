
using System.Collections.Generic;

namespace NullAnimation
{
    public class NullAnimationController
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
        private List<NullAnimationClip> mRunningClips;

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

        public void Schedule(NullAnimationClip clip)
        {

        }

        public void Unschedule(NullAnimationClip clip)
        {

        }

        public void UnscheduleClips(NullAnimationClipTemplate myTemplate)
        {

        }

    }
}
