using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class JoystickTarget : MonoBehaviour
    {
        public JoystickControl mJoystickCtl;
        public float Speed = 10.0f;

        private void Awake()
        {
            mJoystickCtl.AddListener(JoystickListenerType.VALUE_CHANGED, JoystickDrag);
        }
        
        private void JoystickDrag(Vector2 touchPos)
        {
            Vector3 direction = new Vector3(touchPos.x, 0, touchPos.y);
            transform.rotation = Quaternion.LookRotation(new Vector3(touchPos.x, 0, touchPos.y));
            transform.position += transform.forward * Speed * Time.deltaTime;
        }
    }
}



