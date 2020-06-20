using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class JoystickTarget : MonoBehaviour
    {
        public JoystickControl mJoystickCtl;
        public float Speed = 10.0f;

        private void Start()
        {
            mJoystickCtl.AddListener(JoystickListenerType.VALUE_CHANGED, JoystickDrag);
        }
        
        private void JoystickDrag(Vector2 touchPos)
        {
            if (touchPos.sqrMagnitude > 0)
            {
                Vector3 direction = new Vector3(touchPos.x, 0, touchPos.y);
                transform.forward = direction;
                transform.position += transform.forward * Speed * Time.deltaTime;
            }
        }
    }
}



