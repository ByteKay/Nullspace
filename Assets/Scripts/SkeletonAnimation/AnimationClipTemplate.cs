
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
            protected List<ListenerEvent> mListeners;

            public List<ListenerEvent> GetListeners() { return mListeners; }
            public List<Listener> GetBeginListeners() { return mBeginListeners; }
            public List<Listener> GetEndListeners() { return mEndListeners; }

            public void AddBeginListener(Listener listener)
            {

            }

            public void RemoveBeginListener(Listener listener)
            {

            }

            public bool HasBeginListener(Listener listener)
            {
                return false;
            }

            public void AddEndListener(Listener listener)
            {

            }

            public void RemoveEndListener(Listener listener)
            {

            }

            public bool HasEndListener(Listener listener)
            {
                return false;
            }

            public void AddListener(Listener listener, uint eventTime)
            {

            }

            public void RemoveListener(Listener listener, uint eventTime)
            {

            }

            public void RemoveListener(Listener listener)
            {

            }

            public bool HasListener(Listener listener, uint eventTime)
            {
                return false;
            }
        }
        
    }
}
