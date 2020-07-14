
using System.Collections.Generic;

namespace NullAnimation
{
    public partial class NullAnimationClip
    {
        protected NullAnimationClipTemplate.NullListenerManager mListenerManager;
        public void AddBeginListener(NullAnimationClipTemplate.NullListener listener)
        {
            mListenerManager.AddBeginListener(listener);
        }
        public void RemoveBeginListener(NullAnimationClipTemplate.NullListener listener)
        {
            mListenerManager.RemoveBeginListener(listener);
        }
        public bool HasBeginListener(NullAnimationClipTemplate.NullListener listener)
        {
            return mListenerManager.HasBeginListener(listener);
        }
        public void AddEndListener(NullAnimationClipTemplate.NullListener listener)
        {
            mListenerManager.AddEndListener(listener);
        }
        public void RemoveEndListener(NullAnimationClipTemplate.NullListener listener)
        {
            mListenerManager.RemoveEndListener(listener);
        }
        public bool HasEndListener(NullAnimationClipTemplate.NullListener listener)
        {
            return mListenerManager.HasEndListener(listener);
        }
        public void AddListener(NullAnimationClipTemplate.NullListener listener, uint eventTime)
        {
            mListenerManager.AddListener(listener, eventTime);
        }
        public void RemoveListener(NullAnimationClipTemplate.NullListener listener, uint eventTime)
        {
            mListenerManager.RemoveListener(listener, eventTime);
        }
        public void RemoveListener(NullAnimationClipTemplate.NullListener listener)
        {
            mListenerManager.RemoveListener(listener);
        }
        public bool HasListener(NullAnimationClipTemplate.NullListener listener, uint eventTime)
        {
            return mListenerManager.HasListener(listener, eventTime);
        }

        public List<NullAnimationClipTemplate.NullListenerEvent> GetListeners()
        {
            return mListenerManager.GetListeners();
        }
        public List<NullAnimationClipTemplate.NullListener> GetBeginListeners()
        {
            return mListenerManager.GetBeginListeners();
        }
        public List<NullAnimationClipTemplate.NullListener> GetEndListeners()
        {
            return mListenerManager.GetEndListeners();
        }

    }

    public partial class NullAnimationClip
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
        protected List<NullAnimationValue> mValues;
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
        private NullAnimationClip mCrossFadeToClip;
        // The amount of time that has elapsed for the crossfade.
        private float mCrossFadeOutElapsed;                         
        // The duration of the cross fade.
        private uint mCrossFadeOutDuration;                
        // The clip's blendweight.
        private float mBlendWeight;
        



    }
}
