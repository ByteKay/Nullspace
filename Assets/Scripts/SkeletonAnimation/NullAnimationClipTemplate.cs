
using System.Collections.Generic;

namespace NullAnimation
{
    public class NullAnimationClipTemplate
    {
        private NullListenerManager mListenerManager;
        private string mId;                                    
        private NullAnimation mAnimation;                              
        private uint mEnabledGroupId;                              
        private uint mStartTime;                           
        private uint mEndTime;                             
        private uint mDuration;                            
        private float mRepeatCount;                                 
        private uint mLoopBlendTime;                        
        private uint mActiveDuration;                      
        private float mSpeed;                                       
        
        public abstract class NullListener
        {
            public uint bindingGroup;

            public NullListener()
            {

            }

            public NullListener(uint groupId)
            {
                bindingGroup = groupId;
            }

            public enum EventType
            {
                BEGIN,
                END,
                TIME
            };

            public abstract void AnimationEvent(NullAnimationClip clip, EventType type, uint time);
        };

        public struct NullListenerEvent
        {
            public NullListenerEvent(NullListener listener, uint eventTime)
            {
                mListener = listener;
                mEventTime = eventTime;
            }

            public NullListener mListener;
            public uint mEventTime; 
        };

        public class NullListenerManager
        {
            protected List<NullListener> mBeginListeners;
            protected List<NullListener> mEndListeners;
            protected List<NullListenerEvent> mListeners;

            public List<NullListenerEvent> GetListeners() { return mListeners; }
            public List<NullListener> GetBeginListeners() { return mBeginListeners; }
            public List<NullListener> GetEndListeners() { return mEndListeners; }

            public void AddBeginListener(NullListener listener)
            {

            }

            public void RemoveBeginListener(NullListener listener)
            {

            }

            public bool HasBeginListener(NullListener listener)
            {
                return false;
            }

            public void AddEndListener(NullListener listener)
            {

            }

            public void RemoveEndListener(NullListener listener)
            {

            }

            public bool HasEndListener(NullListener listener)
            {
                return false;
            }

            public void AddListener(NullListener listener, uint eventTime)
            {

            }

            public void RemoveListener(NullListener listener, uint eventTime)
            {

            }

            public void RemoveListener(NullListener listener)
            {

            }

            public bool HasListener(NullListener listener, uint eventTime)
            {
                return false;
            }
        }
        
    }
}
