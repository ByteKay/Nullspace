
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nullspace
{
    public class BaseGestureRecognizer
    {
        private static uint _MAX_INTERVAL_OF_DOUBLE_CLICK_ = 300;
        private static uint _MIN_STEADY_TIME_FOR_DRAG = 200;
        private static uint _MIN_TIME_FOR_LONG_TAP = 500;
        private static uint _MAX_TIME_FOR_SWIPE = 700;
        private static float _MIN_X_RATIO_FOR_ARC = 0.5f;
        private static float _MIN_Y_CHANGE_PERSENT_FOR_ARC = 0.25f;
        private static float _MAX_DISTANCE_RATIO_FOR_STEADY = 0.005f;
        private static float _MAX_ANGLE_COS_VALUE_FOR_ROTATE = 0.98f;
        private static float _MAX_SWIPE_DURATION_FOR_WHOLE_SCREEN = 0.5f;


        private static TouchQueueInfomation _empty_infomation = new TouchQueueInfomation();

        private static bool _inMultiTouchMove = false;
        private static int _lastMultiTouchMoveX = 0;
        private static int _lastMultiTouchMoveY = 0;

        private static string NAME_ID = "BaseGestureRecognizer";

        enum TouchQueueChangingMode
        {
            TQC_NONE = 0,
            TQC_PRESS = 1,
            TQC_MOVE = 2,
            TQC_RELEASE = 3,
        }

        protected string mId;
        protected BaseGestureEvent mCurrentGestureEvent;
        protected uint mMaxIntervalOfDoubleClick;
        protected uint mMinSteadyTimeForDrag;
        protected uint mMinTimeForLongTap;
        protected uint mMaxSwipeDuration;
        protected float mMinSpeedForSwipe;
        protected float mMaxAngleCosValForRotate;
        protected int mMinXDistanceForArc;
        protected float mMinYChangePersentForArc;
        protected int mMaxSteadyMoveDistanceX;
        protected int mMaxSteadyMoveDistanceY;

        protected LinkedList<TouchQueueInfomation> mChangedTouchQueues;

        public BaseGestureRecognizer()
        {
            mId = NAME_ID;
            InitializeDefaultParameters();
        }

        public string GetId()
        {
            return mId;
        }

        public static BaseGestureRecognizer Create(string id)
        {
            if (id.Equals(NAME_ID))
            {
                return new BaseGestureRecognizer();
            }
            return null;
        }

        public virtual void Initialize()
        {
            InitializeDefaultParameters();
        }

        public virtual void ResetCurrentGesture()
        {
            mCurrentGestureEvent = new BaseGestureEvent();
        }

        public BaseGestureEvent GetCurrentGestureEvent()
        {
            return mCurrentGestureEvent.IsValid() ? mCurrentGestureEvent : null;
        }

        public virtual void Update(long currentTime)
        {

        }


        private void InitializeDefaultParameters()
        {
            mMaxIntervalOfDoubleClick = _MAX_INTERVAL_OF_DOUBLE_CLICK_;
            mMinSteadyTimeForDrag = _MIN_STEADY_TIME_FOR_DRAG;
            mMinTimeForLongTap = _MIN_TIME_FOR_LONG_TAP;
            mMinYChangePersentForArc = _MIN_Y_CHANGE_PERSENT_FOR_ARC;
            mMaxSwipeDuration = _MAX_TIME_FOR_SWIPE;
            mMaxAngleCosValForRotate = _MAX_ANGLE_COS_VALUE_FOR_ROTATE;
            int width = Screen.width;
            mMinXDistanceForArc = (int)(_MIN_X_RATIO_FOR_ARC * width + 0.5f);
            mMaxSteadyMoveDistanceX = (int)(_MAX_DISTANCE_RATIO_FOR_STEADY * width + 0.5f);
            int height = Screen.height;
            mMaxSteadyMoveDistanceY = (int)(_MAX_DISTANCE_RATIO_FOR_STEADY * height + 0.5f);
            mMinSpeedForSwipe = Mathf.Sqrt(((width * width) + (height * height))) / _MAX_SWIPE_DURATION_FOR_WHOLE_SCREEN;
        }

        protected TouchQueueInfomation FindQueueInfomation(TouchQueue queue)
        {
            for (int i = 0; i < mChangedTouchQueues.Count; i++)
            {
                if (mChangedTouchQueues.ElementAt(i).touchQueue == queue)
                {
                    return mChangedTouchQueues.ElementAt(i);
                }
            }
            return _empty_infomation;
        }

        protected void TryRemoveTouchQueueInfomation(TouchQueue queue)
        {
            for (int i = 0; i < mChangedTouchQueues.Count; i++)
            {
                TouchQueueInfomation info = mChangedTouchQueues.ElementAt(i);
                if (info.touchQueue == queue)
                {
                    mChangedTouchQueues.Remove(info);
                    queue.Clear();
                    break;
                }
            }
        }

        public bool TryAddTouchQueueChanging(TouchQueue queue, int changingMode, long time)
        {
            TouchQueueInfomation info = FindQueueInfomation(queue);
            if (info.IsEmpty())
            {
                if (changingMode == (int)TouchQueueChangingMode.TQC_PRESS)
                {
                    TouchQueueInfomation tqcInfo = new TouchQueueInfomation(queue, changingMode, time);
                    mChangedTouchQueues.AddLast(tqcInfo);
                    return true;
                }
                return false;
            }
            else
            {
                if (changingMode == (int)TouchQueueChangingMode.TQC_RELEASE)
                {
                    Debug.Assert((info.lastChangingMode == (int)TouchQueueChangingMode.TQC_PRESS) || (info.lastChangingMode == (int)TouchQueueChangingMode.TQC_MOVE));
                    info.releaseTime = time;
                    info.repeatTimes++;
                }
                bool changed = (info.lastChangingMode == changingMode);
                info.lastChangingMode = changingMode;

                return changed;
            }
        }

        protected int GetActiveTouchQueueCount()
        {
            int count = mChangedTouchQueues.Count;
            int val = 0;
            for (int i = 0; i < count; ++i)
            {
                if (mChangedTouchQueues.ElementAt(i).touchQueue.IsActived())
                {
                    val++;
                }
            }
            return val;
        }

        protected virtual void OnTapState(ref TouchQueueInfomation info, uint time)
        {
            int x = 0;
            int y = 0;
            if (!info.touchQueue.IsActived())
            {
                // touch release
                if (info.repeatTimes <= 1)
                {
                    info.touchQueue.GetTrackStartingPosition(ref x, ref y);
                    if (info.releaseTime + mMaxIntervalOfDoubleClick < time)
                    {
                        // tap event
                        // ResetCurrentGesture();
                        mCurrentGestureEvent = new GestureTapEvent(x, y, time, 1);
                    }
                    else
                    {
                        mChangedTouchQueues.AddLast(info);
                    }
                    return;
                }
                else
                {
                    // double click
                    info.curState = TouchState.STATE_DOUBLE_TAP;
                    mChangedTouchQueues.AddLast(info);
                    return;
                }
            }
            else
            {
                info.touchQueue.GetAbsMaxMovingDistance(ref x, ref y);
                if ((x > mMaxSteadyMoveDistanceX) || (y > mMaxSteadyMoveDistanceY))
                {
                    // point moved, change to swipe or move state
                    float maxSpeed = 0;
                    float avgSpeed = 0;
                    bool gotSpeed = info.touchQueue.GetMovingSpeeds(ref maxSpeed, ref avgSpeed);
                    if (gotSpeed && (maxSpeed < mMinSpeedForSwipe))
                    {
                        info.curState = TouchState.STATE_MOVE;
                    }
                    else
                    {
                        info.curState = TouchState.STATE_SWIPE;
                    }
                    mChangedTouchQueues.AddLast(info);
                    return;
                }
                info.touchQueue.GetTrackStartingPosition(ref x, ref y);
                if (info.touchQueue.GetCurrentDuration(time) > mMinSteadyTimeForDrag)
                {
                    info.curState = TouchState.STATE_DRAG;
                    /*
                    ResetCurrentGesture();
                    new (mCurrentGestureEvent) GestureDragEvent(x, y, time, 1);
                    */
                    mChangedTouchQueues.AddLast(info);
                    return;
                }
                mChangedTouchQueues.AddLast(info);
            }
        }

        protected virtual void OnSwipeState(ref TouchQueueInfomation info, uint time)
        {
            int x = 0;
            int y = 0;
            info.touchQueue.GetTrackStartingPosition(ref x, ref y);
            if (!info.touchQueue.IsActived() || (info.touchQueue.GetCurrentDuration(time) >= mMaxSwipeDuration))
            {
                TouchArcShape arcType = TouchArcShape.ARC_NONE;
                TouchDirection direction = TouchDirection.DIR_NONE;
                bool isArc = info.touchQueue.IsArcTrack(mMinXDistanceForArc, mMinYChangePersentForArc, ref arcType, ref direction);
                if (isArc)
                {
                    mCurrentGestureEvent = new GestureArcEvent(x, y, time, 1, arcType, direction);
                }
                else
                {
                    mCurrentGestureEvent = new GestureSwipeEvent(x, y, time, 1, direction);
                }

                //force deactive the touch queue when it expired
                if (info.touchQueue.IsActived())
                {
                    info.touchQueue.ForceReleaseTouch();
                }
            }
            else
            {
                mChangedTouchQueues.AddLast(info);
            }
        }
        protected virtual void OnLongTapState(ref TouchQueueInfomation info, uint time)
        {
            int x = 0;
            int y = 0;
            info.touchQueue.GetTrackStartingPosition(ref x, ref y);
            if (!info.touchQueue.IsActived())
            {
                mCurrentGestureEvent = new GestureLongTapEvent(x, y, time, 1, info.touchQueue.GetDuration());
            }
            else
            {
                mChangedTouchQueues.AddLast(info);
            }
        }

        protected virtual void OnDoubleTapState(ref TouchQueueInfomation info, uint time)
        {
            int x = 0;
            int y = 0;
            if (!info.touchQueue.IsActived())
            {
                info.touchQueue.GetTrackStartingPosition(ref x, ref y);
                mCurrentGestureEvent = new GestureDoubleClickEvent(x, y, time, 1);
            }
            else
            {
                info.touchQueue.GetAbsMaxMovingDistance(ref x, ref y);
                if ((x > mMaxSteadyMoveDistanceX) || (y > mMaxSteadyMoveDistanceY))
                {
                    // point moved, change to swipe state
                    info.curState = TouchState.STATE_SWIPE;
                    mChangedTouchQueues.AddLast(info);
                    return;
                }
                if (info.touchQueue.GetCurrentDuration(time) > mMinSteadyTimeForDrag)
                {
                    info.curState = TouchState.STATE_DRAG;
                    mChangedTouchQueues.AddLast(info);
                    return;
                }
                mChangedTouchQueues.AddLast(info);
            }
        }

        protected virtual void OnMoveState(ref TouchQueueInfomation info, uint time)
        {
            int x = 0;
            int y = 0;
            info.touchQueue.GetTrackEndingPosition(ref x, ref y);
            if (!info.touchQueue.IsActived())
            {
                mCurrentGestureEvent = new GestureEndMoveEvent(x, y, time, 1);
            }
            else
            {
                mCurrentGestureEvent = new GestureMoveEvent(x, y, time, 1);
                mChangedTouchQueues.AddLast(info);
            }
        }

        protected virtual void OnDragState(ref TouchQueueInfomation info, uint time)
        {
            int x = 0;
            int y = 0;
            if (!info.touchQueue.IsActived())
            {
                //release touch in drag state, triggered tap or long-tap event
                info.touchQueue.GetTrackStartingPosition(ref x, ref y);
                if (info.touchQueue.GetCurrentDuration(time) >= mMinTimeForLongTap)
                {
                    mCurrentGestureEvent = new GestureLongTapEvent(x, y, time, 1, info.touchQueue.GetDuration());
                }
                else
                {
                    mCurrentGestureEvent = new GestureTapEvent(x, y, time, 1);
                }
            }
            else
            {
                info.touchQueue.GetAbsMaxMovingDistance(ref x, ref y);
                if ((x > mMaxSteadyMoveDistanceX) || (y > mMaxSteadyMoveDistanceY))
                {
                    // point moved, trigger drag event and change to drag-move state
                    info.touchQueue.GetTrackStartingPosition(ref x, ref y);
                    mCurrentGestureEvent = new GestureDragEvent(x, y, time, 1);
                    info.curState = TouchState.STATE_DRAG_MOVE;
                    mChangedTouchQueues.AddLast(info);
                    return;
                }
                mChangedTouchQueues.AddLast(info);
            }

        }
        protected virtual void OnDragMoveState(ref TouchQueueInfomation info, uint time)
        {
            int x = 0;
            int y = 0;
            info.touchQueue.GetTrackEndingPosition(ref x, ref y);
            if (!info.touchQueue.IsActived())
            {
                mCurrentGestureEvent = new GestureDropEvent(x, y, time, 1);
            }
            else
            {
                mCurrentGestureEvent = new GestureDragMoveEvent(x, y, time, 1);
                mChangedTouchQueues.AddLast(info);
            }
        }

        protected virtual void OnMultiTouch(uint time)
        {
            LinkedList<TouchQueue> temp = new LinkedList<TouchQueue>();
            int count = mChangedTouchQueues.Count;
            for (int i = 0; i < count; ++i)
            {
                if (mChangedTouchQueues.ElementAt(i).touchQueue.IsActived())
                {
                    temp.AddLast(mChangedTouchQueues.ElementAt(i).touchQueue);
                    mChangedTouchQueues.ElementAt(i).curState = TouchState.STATE_MULTI;
                }
            }
            count = temp.Count;
            if (count == 2) // rotate pinch
            {
                int x0 = 0, y0 = 0, x1 = 0, y1 = 0;
                temp.ElementAt(0).GetTrackStartingPosition(ref x0, ref y0);
                Vector3 p0 = new Vector3(x0, y0, 0.0f);
                temp.ElementAt(0).GetTrackEndingPosition(ref x1, ref y1);
                Vector3 p1 = new Vector3(x1, y1, 0.0f);
                Vector3 v0 = new Vector3(x1 - x0, y1 - y0, 0.0f);
                temp.ElementAt(1).GetTrackStartingPosition(ref x0, ref y0);
                Vector3 p2 = new Vector3(x0, y0, 0.0f);
                temp.ElementAt(1).GetTrackEndingPosition(ref x1, ref y1);
                Vector3 p3 = new Vector3(x1, y1, 0.0f);
                Vector3 v1 = new Vector3(x1 - x0, y1 - y0, 0.0f);
                float len1 = v0.magnitude;
                float len2 = v1.magnitude;
                if ((len1 < _MAX_DISTANCE_RATIO_FOR_STEADY) && (len2 < _MAX_DISTANCE_RATIO_FOR_STEADY))
                {
                    //hold stady, exit anyway
                    return;
                }
                if ((len1 < _MAX_DISTANCE_RATIO_FOR_STEADY) || (len2 < _MAX_DISTANCE_RATIO_FOR_STEADY))
                {
                    //roc todo, buggy here
                    mCurrentGestureEvent = new GestureRotateEvent((int)p1.x, (int)p1.y, time, 2, 1.0f);
                }
                else
                {
                    float dot = Vector3.Dot(v0, v1);
                    if (dot > 0.0f)
                    {
                        //two tracks in the same direction, move
                        p0 = (p1 + p3) * 0.5f;
                        mCurrentGestureEvent = new GestureMoveEvent((int)p0.x, (int)p0.y, time, 2);
                        _inMultiTouchMove = true;
                        _lastMultiTouchMoveX = (int)p0.x;
                        _lastMultiTouchMoveY = (int)p0.y;
                    }
                    else
                    {
                        //pinch
                        mCurrentGestureEvent = new GesturePinchEvent((int)p3.x, (int)p3.y, time, 2, (p1 - p3).sqrMagnitude / (p0 - p2).sqrMagnitude);
                    }
                }
            }
        }
    }
}
