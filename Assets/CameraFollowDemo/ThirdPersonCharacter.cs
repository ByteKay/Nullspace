using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Nullspace
{
    public class ThirdPersonCharacter : MonoBehaviour
    {
        public JoystickControl JoystickCtl;
        public float Speed = 5.0f;

        private Animator Animator { get; set; }

        private Vector3 MoveUpNormal { get; set; }
        private float TurnAmount;
        private float ForwardAmount;
        private bool IsCrouching;
        private bool IsGrounded;
        private Transform CameraTransform;

        private void Awake()
        {
            Animator = GetComponent<Animator>();
            IsCrouching = false;
            IsGrounded = true;
            MoveUpNormal = Vector3.up;
        }

        private void Start()
        {
            JoystickCtl.AddListener(JoystickListenerType.PRESS, JoystickPress);
            JoystickCtl.AddListener(JoystickListenerType.VALUE_CHANGED, JoystickDrag);
            JoystickCtl.AddListener(JoystickListenerType.RELEASE, JoystickRelease);
            if (Camera.main != null)
            {
                CameraTransform = Camera.main.transform;
            }
        }

        private void JoystickPress(Vector2 touchPos)
        {
            DebugUtils.Info("Move", "JoystickPress");
        }

        private void JoystickRelease(Vector2 touchPos)
        {
            Animator.SetFloat("Forward", 0);
            Animator.SetFloat("Turn", 0);
            // Move(Vector3.zero, false, false);
            DebugUtils.Info("Move", "JoystickRelease");
        }

        private void JoystickDrag(Vector2 touchPos)
        {
            if (touchPos.sqrMagnitude > 0)
            {
                Vector3 direction = new Vector3(touchPos.x, 0, touchPos.y);
                if (CameraTransform)
                {
                    Vector3 forward = Vector3.Scale(CameraTransform.forward, new Vector3(1, 0, 1)).normalized;
                    direction = direction.x * CameraTransform.right + direction.z * forward;
                }
                direction.Normalize();
                transform.forward = direction;
                Move(direction, false, false);
            }
        }


        private void Move(Vector3 move, bool crouch, bool jump)
        {
            if (move.magnitude > 1.0f)
            {
                move.Normalize();
            }
            // MoveUpNormal 可能会改变，当角色在斜坡上移动
            move = Vector3.ProjectOnPlane(move, MoveUpNormal);
            move = transform.InverseTransformDirection(move);
            TurnAmount = move.x;
            ForwardAmount = move.z;
            UpdateAnimator(move);
            DebugUtils.Info("Move", string.Format("{0} {1}", TurnAmount, ForwardAmount));
        }

        private void UpdateAnimator(Vector3 move)
        {
            Animator.SetFloat("Forward", ForwardAmount, 0.1f, Time.deltaTime);
            Animator.SetFloat("Turn", TurnAmount, 0.1f, Time.deltaTime);
            Animator.SetBool("Crouch", false);
            Animator.SetBool("OnGround", IsGrounded);
        }
    }
}


