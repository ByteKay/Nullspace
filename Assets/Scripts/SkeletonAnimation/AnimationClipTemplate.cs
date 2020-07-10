
using System.Collections.Generic;

namespace Animation
{
    public class AnimationClipTemplate
    {
        private ListenerManager mListenerManager;
        private string mId;                                    
        private Animation mAnimation;                              
        private uint mEnabledGroupId;                              
        private uint mStartTime;                           
        private uint mEndTime;                             
        private uint mDuration;                            
        private float mRepeatCount;                                 
        private uint mLoopBlendTime;                        
        private uint mActiveDuration;                      
        private float mSpeed;                                       
        
        public abstract class Listener
        {
            public uint bindingGroup;

            public Listener()
            {

            }

            public Listener(uint groupId)
            {
                bindingGroup = groupId;
            }

            public enum EventType
            {
                BEGIN,
                END,
                TIME
            };

            public abstract void AnimationEvent(AnimationClip clip, EventType type, uint time);
        };

        public struct ListenerEvent
        {
            public ListenerEvent(Listener listener, uint eventTime)
            {
                mListener = listener;
                mEventTime = eventTime;
            }

            public Listener mListener;
            public uint mEventTime; 
        };

        public class ListenerManager
        {
            protected List<Listener> mBeginListeners;
            protected List<Listener> mEndListeners;
            protected List<Listener> mListeners;
        }
        
    }
}
