
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Nullspace
{
    public enum JoystickListenerType
    {
        PRESS,
        RELEASE,
        VALUE_CHANGED
    }

    public enum JoystickDirection
    {
        HORIZONTAL = 1,
        VERTICAL = 2,
        ALL = HORIZONTAL | VERTICAL,
    }

    public class JoystickEvent : UnityEvent<Vector2>
    {

    }

    public class JoystickControl : EventTrigger
    {
        private static int FINGER_NOT_VALID = -1;
        private Transform InnerTransform;
        private JoystickEvent mPressEvent;
        private JoystickEvent mReleaseEvent;
        private JoystickEvent mValueChanged;
        private bool IsDraging { set; get; }
        private int FingerId { set; get; }
        private Vector2 InitializedPos { get; set; }

        public float MaxRadius;
        public JoystickDirection ActiveDirection;
        public void Awake()
        {
            InnerTransform = transform.Find("InnerRegion");
            Debug.Assert(InnerTransform != null, "InnerRegion child not found");
            mPressEvent = new JoystickEvent();
            mReleaseEvent = new JoystickEvent();
            mValueChanged = new JoystickEvent();
            FingerId = FINGER_NOT_VALID;
            ActiveDirection = JoystickDirection.ALL;
            MaxRadius = 100;
        }

        public void ResetJoystick()
        {
            FingerId = FINGER_NOT_VALID;
            InnerTransform.localPosition = Vector3.zero;
        }

        private void OnDisable()
        {
            ResetJoystick();
        }

        public void AddListener(JoystickListenerType type, UnityAction<Vector2> call)
        {
            switch (type)
            {
                case JoystickListenerType.PRESS:
                    mPressEvent.AddListener(call);
                    break;
                case JoystickListenerType.RELEASE:
                    mReleaseEvent.AddListener(call);
                    break;
                case JoystickListenerType.VALUE_CHANGED:
                    mValueChanged.AddListener(call);
                    break;
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.pointerId < -1 || IsDraging)
            {
                return;
            }
            FingerId = eventData.pointerId;
            InitializedPos = eventData.position;
            mPressEvent.Invoke(eventData.position);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (FingerId != eventData.pointerId)
            {
                return;
            }
            ResetJoystick();
            mReleaseEvent.Invoke(eventData.position);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (FingerId != eventData.pointerId)
            {
                return;
            }
            Vector2 direction = eventData.position - InitializedPos;
            float radius = Mathf.Clamp(direction.magnitude, 0, MaxRadius);
            direction.Normalize();
            direction = direction * radius;
            Vector2 localPosition = Vector2.zero;
            if (0 != (ActiveDirection & JoystickDirection.HORIZONTAL))
            {
                localPosition.x = direction.x;
            }
            if (0 != (ActiveDirection & JoystickDirection.VERTICAL))
            {
                localPosition.y = direction.y;
            }
            InnerTransform.localPosition = localPosition;
            mValueChanged.Invoke(localPosition / MaxRadius);
        }

    }
}
