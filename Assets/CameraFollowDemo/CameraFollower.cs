using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{

    public class CameraFollower : MonoBehaviour
    {
        public Transform Target;
        public float MoveSpeed;
        public float TurnSmoothing;
        [Range(0f, 10f)]
        [SerializeField]
        public float TurnSpeed;

        private bool LockCursor = false;
        private float TiltMax = 75f;
        private float TiltMin = 45f;

        protected Transform CameraTransform;
        protected Transform CameraPivot;
        protected Vector3 LastTargetPosition;

        protected Vector3 LastTouchPosition;

        private Vector3 PivotEulers;
        private Quaternion PivotTargetRot;
        private Quaternion TransformTargetRot;

        private bool IsAutoTargetPlayer { get; set; }
        private FollowUpdateType UpdateType { get; set; }
        private float LookAngle { get; set; }
        private float TiltAngle { get; set; }
        private int FingerId { get; set; }
        private bool CanRotate { get; set; }
        public enum FollowUpdateType
        {
            FixedUpdate,
            LateUpdate,
            ManualUpdate,
        }

        private void Awake()
        {
            CameraTransform = GetComponentInChildren<Camera>().transform;
            CameraPivot = CameraTransform.parent;
            Cursor.lockState = LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !LockCursor;
            PivotEulers = CameraPivot.rotation.eulerAngles;
            PivotTargetRot = CameraPivot.transform.localRotation;
            TransformTargetRot = transform.localRotation;
            FingerId = -1;
            CanRotate = true;
        }

        private void Start()
        {
            IsAutoTargetPlayer = true;
            UpdateType = FollowUpdateType.LateUpdate;
            EnumEventDispatcher.AddEventListener<Vector2>(EnumEventType.JoystickPress, JoystickPress);
            EnumEventDispatcher.AddEventListener<Vector2>(EnumEventType.JoystickRelease, JoystickRelease);
        }

        private void JoystickPress(Vector2 touchPos)
        {
            CanRotate = false;
        }

        private void JoystickRelease(Vector2 touchPos)
        {
            CanRotate = true;
        }

        public void Update()
        {
            if (CanRotate)
            {
                HandleInputTouch();
                HandleMouchTouch();
                if (LockCursor && Input.GetMouseButtonUp(0))
                {
                    Cursor.lockState = LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
                    Cursor.visible = !LockCursor;
                }
            }
        }

        private void OnDestroy()
        {
            EnumEventDispatcher.RemoveEventListener<Vector2>(EnumEventType.JoystickPress, JoystickPress);
            EnumEventDispatcher.RemoveEventListener<Vector2>(EnumEventType.JoystickRelease, JoystickRelease);
        }

        private void HandleInputTouch()
        {
            foreach (Touch touch in Input.touches)
            {
                HandleTouch(touch.fingerId, touch.position, touch.phase);
            }
        }

        private void HandleMouchTouch()
        {             
            if (Input.touchCount == 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    HandleTouch(0, Input.mousePosition, TouchPhase.Began);
                }
                if (Input.GetMouseButton(0))
                {
                    HandleTouch(0, Input.mousePosition, TouchPhase.Moved);
                }
                if (Input.GetMouseButtonUp(0))
                {
                    HandleTouch(0, Input.mousePosition, TouchPhase.Ended);
                }
            }

        }

        private void HandleTouch(int touchFingerId, Vector3 position, TouchPhase touchPhase)
        {
            if (FingerId == -1)
            {
                FingerId = touchFingerId;
            }
            if (FingerId != touchFingerId)
            {
                return;
            }
            if (touchPhase == TouchPhase.Began)
            {
                LastTouchPosition = position;
            }
            else if (touchPhase == TouchPhase.Ended)
            {
                FingerId = -1;
            }
            else if (touchPhase == TouchPhase.Moved)
            {
                Vector3 deltaPosition = position - LastTouchPosition;
                LastTouchPosition = position;
                deltaPosition.Normalize();
                HandleRotationMovement(deltaPosition.x, deltaPosition.y);
            }
            else if (touchPhase == TouchPhase.Stationary)
            {
                // nothing
            }
            else if (touchPhase == TouchPhase.Canceled)
            {
                FingerId = -1;
            }
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


        protected virtual void FollowTarget(float deltaTime)
        {
            if (Target == null)
            {
                return;
            }
            transform.position = Vector3.Lerp(transform.position, Target.position, deltaTime * MoveSpeed);
        }

        private void FixedUpdate()
        {
            if (Target != null && UpdateType == FollowUpdateType.FixedUpdate)
            {
                FollowTarget(Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            if (Target != null && UpdateType == FollowUpdateType.LateUpdate)
            {
                FollowTarget(Time.deltaTime);
            }
        }

        private void ManualUpdate()
        {
            if (Target != null && UpdateType == FollowUpdateType.ManualUpdate)
            {
                FollowTarget(Time.deltaTime);
            }
        }

        private void HandleRotationMovement(float x, float y)
        {
            if (Time.timeScale < float.Epsilon)
            {
                return;
            }
            LookAngle += x * TurnSpeed;
            TransformTargetRot = Quaternion.Euler(0f, LookAngle, 0f);
            TiltAngle -= y * TurnSpeed;
            TiltAngle = Mathf.Clamp(TiltAngle, -TiltMin, TiltMax);
            PivotTargetRot = Quaternion.Euler(TiltAngle, PivotEulers.y, PivotEulers.z);
            if (TurnSmoothing > 0)
            {
                CameraPivot.localRotation = Quaternion.Slerp(CameraPivot.localRotation, PivotTargetRot, TurnSmoothing * Time.deltaTime);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, TransformTargetRot, TurnSmoothing * Time.deltaTime);
            }
            else
            {
                CameraPivot.localRotation = PivotTargetRot;
                transform.localRotation = TransformTargetRot;
            }
        }
    }
    
}
