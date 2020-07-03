using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{

    public class EventTest : MonoBehaviour
    {
        private static bool isInitialid = false;
        GameObject child;
        // Use this for initialization
        void Start ()
        {
            if (!isInitialid)
            {
                isInitialid = true;
                child = new GameObject("Child");
                EventTest test = child.AddComponent<EventTest>();
                test.AddEvent();
                InvokeRepeating("Send", 2, 2);
                Invoke("DestroyChild", 5);
            }
        }

        private void DestroyChild()
        {
            GameObject.Destroy(child);
        }

        public void AddEvent()
        {
            // 按照 先后顺序 触发执行
            EnumEventDispatcher.AddEventListener(EnumEventType.JoystickPress, Func1);
            EnumEventDispatcher.AddEventListener(EnumEventType.JoystickPress, Func2);
            EnumEventDispatcher.AddEventListener(EnumEventType.JoystickPress, Func3);
            
        }

	    // Update is called once per frame
	    void Send ()
        {
            EnumEventDispatcher.TriggerEvent(EnumEventType.JoystickPress);
        }

        private void Func1()
        {
            DebugUtils.Info("EventTest", "Func1");
            // 第一次 还是会 执行 Func2
            // 后一次 才不会 被执行
            EnumEventDispatcher.RemoveEventListener(EnumEventType.JoystickPress, Func2);
        }

        private void Func2()
        {
            DebugUtils.Info("EventTest", "Func2");
        }

        private static void Func3()
        {
            DebugUtils.Info("EventTest", "Func3");
        }
    }
}



