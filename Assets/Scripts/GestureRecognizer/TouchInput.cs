
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nullspace
{
    public class TouchInput : MonoBehaviour, GestureListener
    {
        private TouchManager TouchManager { get; set; }
        private bool MouseDownFlag = false;
        private void Awake()
        {
            TouchManager = new TouchManager(2);
            TouchManager.RegisterGestureListener(this);
            Input.simulateMouseWithTouches = true;
        }

        public void Update()
        {
            // Handle native touch events
            foreach (Touch touch in Input.touches)
            {
                HandleTouch(touch.fingerId, touch.position, touch.phase);
            }

            // Simulate touch events from mouse events
            if (Input.touchCount == 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("1");
                    HandleTouch(0, Input.mousePosition, TouchPhase.Began);
                }
                if (Input.GetMouseButton(0))
                {
                    Debug.Log("2");
                    HandleTouch(0, Input.mousePosition, TouchPhase.Moved);
                }
                if (Input.GetMouseButtonUp(0))
                {
                    Debug.Log("3");
                    HandleTouch(0, Input.mousePosition, TouchPhase.Ended);
                }
            }

            TouchManager.Update();
        }

        private void HandleTouch(int touchFingerId, Vector3 position, TouchPhase touchPhase)
        {
            if (touchPhase == TouchPhase.Began)
            {
                TouchManager.AddTouch((int)position.x, (int)position.y, touchFingerId);
            }
            else if (touchPhase == TouchPhase.Ended)
            {
                TouchManager.ReleaseTouch((int)position.x, (int)position.y, touchFingerId);
            }
            else if (touchPhase == TouchPhase.Moved)
            {
                TouchManager.TouchMove((int)position.x, (int)position.y, touchFingerId);
            }
            else if (touchPhase == TouchPhase.Stationary)
            {
                TouchManager.TouchMove((int)position.x, (int)position.y, touchFingerId);
            }
            else if (touchPhase == TouchPhase.Canceled)
            {
                TouchManager.ReleaseTouch((int)position.x, (int)position.y, touchFingerId);
            }
        }

        private void OnDestroy()
        {
            TouchManager.UnRegisterGestureListener(this);
            TouchManager.Clear();
        }

        public void GestureEvent(BaseGestureEvent gestureEvent)
        {
            switch (gestureEvent.GetEventType())
            {
                case GestureEventType.GESTURE_TAP:
                    Debug.Log("GESTURE_TAP");
                    break;
                case GestureEventType.GESTURE_DOUBLE_CLICK:
                    Debug.Log("GESTURE_DOUBLE_CLICK");
                    break;
                case GestureEventType.GESTURE_LONG_TAP:
                    Debug.Log("GESTURE_LONG_TAP");
                    break;
                case GestureEventType.GESTURE_BEGIN_MOVE:
                    Debug.Log("GESTURE_BEGIN_MOVE");
                    break;
                case GestureEventType.GESTURE_MOVE:
                    Debug.Log("GESTURE_MOVE");
                    break;
                case GestureEventType.GESTURE_END_MOVE:
                    Debug.Log("GESTURE_END_MOVE");
                    break;
                case GestureEventType.GESTURE_DRAG:
                    Debug.Log("GESTURE_DRAG");
                    break;
                case GestureEventType.GESTURE_DRAG_MOVE:
                    Debug.Log("GESTURE_DRAG_MOVE");
                    break;
                case GestureEventType.GESTURE_DROP:
                    Debug.Log("GESTURE_DROP");
                    break;
                case GestureEventType.GESTURE_ARC:
                    Debug.Log("GESTURE_ARC");
                    break;
                case GestureEventType.GESTURE_PINCH:
                    Debug.Log("GESTURE_PINCH");
                    break;
                case GestureEventType.GESTURE_SWIPE:
                    Debug.Log("GESTURE_SWIPE");
                    break;
                case GestureEventType.GESTURE_ROTATE:
                    Debug.Log("GESTURE_ROTATE");
                    break;
                case GestureEventType.GESTURE_UNKNOWN:
                    Debug.Log("GESTURE_UNKNOWN");
                    break;
                default:
                    Debug.Log("wrong GESTURE");
                    break;
            }

        }
    }
}
