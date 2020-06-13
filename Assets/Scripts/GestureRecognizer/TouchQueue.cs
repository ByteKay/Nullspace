
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nullspace
{
    public enum TouchArcShape
    {
        ARC_NONE = 0,
        ARC_UP = 1,
        ARC_DOWN = 2,
    }

    public enum TouchDirection
    {
        DIR_NONE = 0x00000000,
        DIR_TOP = 0x00000001,
        DIR_TOP_RIGHT = 0x00000002,
        DIR_RIGHT = 0x00000004,
        DIR_BOTTOM_RIGHT = 0x00000008,
        DIR_BOTTOM = 0x00000010,
        DIR_BOTTOM_LEFT = 0x00000020,
        DIR_LEFT = 0x00000040,
        DIR_TOP_LEFT = 0x00000080,
    }

    public class TouchQueue
    {
        protected LinkedList<TouchPoint> mTouchTrack;
        protected bool mActived;
        protected int mTouchIndex;
        public TouchQueue(int index)
        {
            mTouchIndex = index;
            mActived = false;
            mTouchTrack = new LinkedList<TouchPoint>();
        }

        public virtual void Clear()
        {
            mActived = false;
            mTouchTrack.Clear();
        }

        public virtual bool IsActived() { return mActived; }
        public virtual int GetTouchIndex() { return mTouchIndex; }

        //try add touch point in track queue, if the queue is not actived, the input will be ignored
        public virtual void AddTouch(int x, int y, long time)
        {
            Clear();
            mActived = true;
            mTouchTrack.AddLast(new TouchPoint(new Vector2(x, y), time));
        }

        public virtual void TouchMove(int x, int y, long time)
        {
            if (mActived)
            {
                mTouchTrack.AddLast(new TouchPoint(new Vector2(x, y), time));
            }
        }
        public virtual void ReleaseTouch(int x, int y, long time)
        {
            if (!mActived)
            {
                return;
            }
            mTouchTrack.AddLast(new TouchPoint(new Vector2(x, y), time));
            mActived = false;
        }

        //try deactive the queue, after these calls, the input should be ignored, even the touch event still triggered
        public virtual void ForceReleaseTouch(int x, int y, long time)
        {
            ReleaseTouch(x, y, time);
        }

        public virtual void ForceReleaseTouch()
        {
            if (!mActived)
            {
                return;
            }
            if (mTouchTrack.Count == 1)
            {
                TouchPoint p = mTouchTrack.First.Value;
                ReleaseTouch((int)p.point.x, (int)p.point.y, p.time + 200);
            }
            mActived = false;
        }

        public long GetDuration()
        {
            if (mTouchTrack.Count < 2)
            {
                return 0;
            }
            return mTouchTrack.Last.Value.time - mTouchTrack.First.Value.time;
        }

        public long GetCurrentDuration(long current)
        {
            if (mTouchTrack.Count == 0)
            {
                return 0;
            }
            return current - mTouchTrack.First.Value.time;
        }

        public bool GetMovingSpeeds(ref float maxSpeed, ref float avgSpeed)
        {
            if (mTouchTrack.Count < 2)
            {
                return false;
            }
            maxSpeed = 0.0f;
            bool res = false;
            for (int i = 1; i < mTouchTrack.Count; i++)
            {
                TouchPoint p0 = mTouchTrack.ElementAt(i - 1);
                TouchPoint p1 = mTouchTrack.ElementAt(i);
                long dt = p1.time - p0.time;
                if (dt == 0)
                {
                    continue;
                }
                float speed = (p1.point - p0.point).magnitude * 1000.0f / dt;
                if (speed > maxSpeed)
                {
                    maxSpeed = speed;
                }
                res = true;
            }
            avgSpeed = 0.0f;
            if (res)
            {
                TouchPoint p0 = mTouchTrack.First.Value;
                TouchPoint p1 = mTouchTrack.Last.Value;
                long dt = p1.time - p0.time;
                if (dt != 0)
                {
                    avgSpeed = (p1.point - p0.point).magnitude * 1000.0f / dt;
                }
            }
            return true;
        }

        public int GetTouchPointCount()
        {
            return mTouchTrack.Count;
        }

        private static Vector3[] _const_directions = new Vector3[8]
        {
                new Vector3(0.0f, -1.0f, 0.0f),                 //top
                new Vector3(0.707107f, -0.707107f, 0.0f),       //right-top
                new Vector3(1.0f, 0.0f, 0.0f),                  //right
                new Vector3(0.707107f, 0.707107f, 0.0f),        //right-bottom
                new Vector3(0.0f, 1.0f, 0.0f),                  //bottom
                new Vector3(-0.707107f, 0.707107f, 0.0f),       //left-bottom
                new Vector3(-1.0f, 0.0f, 0.0f),                 //left
                new Vector3(-0.707107f, -0.707107f, 0.0f)       //left-top
         };


        public bool IsArcTrack(int minXDistance, float minYChangePersent, ref  TouchArcShape arcType, ref TouchDirection direction)
        {
            // unity 世界坐标系为 是左手坐标系。
            // 此处的计算是 右手。后面需要调整
            arcType = TouchArcShape.ARC_NONE;
            direction = TouchDirection.DIR_NONE;

            int trackCount = mTouchTrack.Count;

            TouchPoint p0 = mTouchTrack.First.Value;
            TouchPoint p1 = mTouchTrack.Last.Value;
            Vector3 vec = new Vector3(p1.point.x - p0.point.x, p1.point.y - p0.point.y, 0.0f);
            vec.Normalize();

            if (trackCount >= 4)
            {
                int dx = (int)Mathf.Abs(p1.point.x - p0.point.x);
                if (dx > minXDistance)
                {
                    Quaternion m = Quaternion.FromToRotation(vec, Vector3.right);
                    List<float> yArray = new List<float>();
                    int topYIndex = 0;
                    float maxYDist = -1e20f;
                    for (int i = 1; i < trackCount - 1; i++)
                    {
                        TouchPoint p = mTouchTrack.ElementAt(i);
                        Vector3 pt = new Vector3(p.point.x - p0.point.x, p.point.y - p0.point.y, 0.0f );
                        Vector3 myPt = m * pt;
                        yArray.Add(myPt[1]);
                        float yDist = myPt[1] - vec[1];
                        if (Mathf.Abs(yDist) > maxYDist)
                        {
                            maxYDist = Mathf.Abs(yDist);
                            topYIndex = i;
                        }
                    }
                    float dist = (p1.point - p0.point).magnitude;
                    float yChangePersent = maxYDist / dist;
                    if (yChangePersent >= minYChangePersent)
                    {
                        if (yArray[topYIndex] > 0)
                        {
                            arcType = TouchArcShape.ARC_UP;
                        }
                        else
                        {
                            arcType = TouchArcShape.ARC_DOWN;
                        }

                        if (p1.point.x > p0.point.x)
                        {
                            direction = TouchDirection.DIR_RIGHT;
                        }
                        else
                        {
                            direction = TouchDirection.DIR_LEFT;
                        }

                        return true;
                    }
                }
            }
            //not a movement in curve, determin the direction
            int closestIndex = 100;
            float maxDot = -1e20f;
            for (int i = 0; i < 8; i++)
            {
                float dot = Vector3.Dot(vec, _const_directions[i]);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    closestIndex = i;
                }
            }
            switch (closestIndex)
            {
                case 0:
                    direction = TouchDirection.DIR_TOP;
                    break;
                case 1:
                    direction = TouchDirection.DIR_TOP_RIGHT;
                    break;
                case 2:
                    direction = TouchDirection.DIR_RIGHT;
                    break;
                case 3:
                    direction = TouchDirection.DIR_BOTTOM_RIGHT;
                    break;
                case 4:
                    direction = TouchDirection.DIR_BOTTOM;
                    break;
                case 5:
                    direction = TouchDirection.DIR_BOTTOM_LEFT;
                    break;
                case 6:
                    direction = TouchDirection.DIR_LEFT;
                    break;
                case 7:
                    direction = TouchDirection.DIR_TOP_LEFT;
                    break;
                default:
                    direction = TouchDirection.DIR_NONE;
                    Debug.Assert(false);
                    break;
            }
            return false;
        }

        public void GetAbsMaxMovingDistance(ref int x, ref int y)
        {
            if (mTouchTrack.Count < 2)
            {
                x = y = 0;
            }
            else
            {
                x = y = int.MinValue;
                for (int i = 1; i < mTouchTrack.Count; i++)
                {
                    TouchPoint p0 = mTouchTrack.ElementAt(i - 1);
                    TouchPoint p1 = mTouchTrack.ElementAt(i);
                    int tx = (int)Mathf.Abs(p1.point.x - p0.point.x);
                    int ty = (int)Mathf.Abs(p1.point.y - p0.point.y);
                    if (x < tx)
                    {
                        x = tx;
                    }
                    if (y < ty)
                    {
                        y = ty;
                    }
                }
            }
        }

        public void GetTrackStartingPosition(ref int x, ref int y)
        {
            if (mTouchTrack.Count == 0)
            {
                x = y = 0;
            }
            else
            {
                TouchPoint p = mTouchTrack.First.Value;
                x = (int)p.point.x;
                y = (int)p.point.y;
            }
        }

        public void GetTrackEndingPosition(ref int x, ref int y)
        {
            if (mTouchTrack.Count == 0)
            {
                x = y = 0;
            }
            else
            {
                TouchPoint p = mTouchTrack.Last.Value;
                x = (int)p.point.x;
                y = (int)p.point.y;
            }

        }

    }
}
