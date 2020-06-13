
using UnityEngine;

namespace Nullspace
{
    public class TouchPoint
    {
        public Vector2 point;
        public long time;

        public TouchPoint()
        {
            time = 0;
            point = Vector2.zero;
        }
        public TouchPoint(Vector2 p, long t)
        {
            time = t;
            point = p;
        }

        public bool IsValid() { return time != 0; }
    }


}
