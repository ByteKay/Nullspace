
using System.Collections.Generic;

namespace Animation
{
    public partial class AnimationClip
    {
        protected AnimationClipTemplate.ListenerManager mListenerManager;
        public void AddBeginListener(AnimationClipTemplate.Listener listener)
        {
            mListenerManager.AddBeginListener(listener);
        }
        public void RemoveBeginListener(AnimationClipTemplate.Listener listener)
        {
            mListenerManager.RemoveBeginListener(listener);
        }
        public bool HasBeginListener(AnimationClipTemplate.Listener listener)
        {
            return mListenerManager.HasBeginListener(listener);
        }
        public void AddEndListener(AnimationClipTemplate.Listener listener)
        {
            mListenerManager.AddEndListener(listener);
        }
        public void RemoveEndListener(AnimationClipTemplate.Listener listener)
        {
            mListenerManager.RemoveEndListener(listener);
        }
        public bool HasEndListener(AnimationClipTemplate.Listener listener)
        {
            return mListenerManager.HasEndListener(listener);
        }
        public void AddListener(AnimationClipTemplate.Listener listener, uint eventTime)
        {
            mListenerManager.AddListener(listener, eventTime);
        }
        public void RemoveListener(AnimationClipTemplate.Listener listener, uint eventTime)
        {
            mListenerManager.RemoveListener(listener, eventTime);
        }
        public void RemoveListener(AnimationClipTemplate.Listener listener)
        {
            mListenerManager.RemoveListener(listener);
        }
        public bool HasListener(AnimationClipTemplate.Listener listener, uint eventTime)
        {
            return mListenerManager.HasListener(listener, eventTime);
        }

        public List<AnimationClipTemplate.ListenerEvent> GetListeners()
        {
            return mListenerManager.GetListeners();
        }
        public List<AnimationClipTemplate.Listener> GetBeginListeners()
        {
            return mListenerManager.GetBeginListeners();
        }
        public List<AnimationClipTemplate.Listener> GetEndListeners()
        {
            return mListenerManager.GetEndListeners();
        }

    }

    public partial class AnimationClip
    {
        // Bit representing whether AnimationClip is a running clip in AnimationController
        public const byte CLIP_IS_PLAYING_BIT = 0x01;
        // Bit representing whether the AnimationClip has actually been started (ie: received first call to update())
        public const byte CLIP_IS_STARTED_BIT = 0x02;
        // Bit representing that a cross fade has started.
        public const byte CLIP_IS_FADING_OUT_STARTED_BIT = 0x04;
        // Bit representing whether the clip is fading out.
        public const byte CLIP_IS_FADING_OUT_BIT = 0x08;
        // Bit representing whether the clip is fading out. 
        public const byte CLIP_IS_FADING_IN_BIT = 0x10;
        // Bit representing whether the clip has ended and should be removed from the AnimationController.
        public const byte CLIP_IS_MARKED_FOR_REMOVAL_BIT = 0x20;
        // Bit representing if the clip should be restarted by the AnimationController.
        public const byte CLIP_IS_RESTARTED_BIT = 0x40;
        // Bit representing if the clip is currently paused.
        public const byte CLIP_IS_PAUSED_BIT = 0x80;
        // Bit mask for all the state bits.
        public const byte CLIP_ALL_BITS = 0xFF;

        protected float mPercentComplete;
        // AnimationValue holder.
        protected List<AnimationValue> mValues;
        protected uint mActivedStartTime;
        protected uint mActivedEndTime;

        // AnimationClip ID.
        private string mId;                                    
        private bool mManualControl;
        // Bit flag used to keep track of the clip's current state.
        private byte mStateBits;
        // The clip's repeat count.
        private float mRepeatCount;
        // Time spent blending the last frame of animation with the first frame, when looping.        
        private uint mLoopBlendTime;
        // The active duration of the clip. 
        private uint mActiveDuration;
        // The speed that the clip is playing. Default is 1.0. Negative goes in reverse.
        private float mSpeed;
        // The game time when this clip was actually started.
        private double mTimeStarted;
        // Time elapsed while the clip is running.
        private float mElapsedTime;
        // The clip to cross fade to. 
        private AnimationClip mCrossFadeToClip;
        // The amount of time that has elapsed for the crossfade.
        private float mCrossFadeOutElapsed;                         
        // The duration of the cross fade.
        private uint mCrossFadeOutDuration;                
        // The clip's blendweight.
        private float mBlendWeight;
        



    }
}
