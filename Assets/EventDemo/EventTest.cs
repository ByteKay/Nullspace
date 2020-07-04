using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class TestInst : IEventManager
    {
        private void Func1()
        {
            DebugUtils.Info("TestInst", "Func1");
            // 第一次 还是会 执行 Func2
            // 后一次 才不会 被执行
            EnumEventDispatcher.RemoveEventListener(EnumEventType.JoystickRelease, Func2);
        }

        private void Func2()
        {
            DebugUtils.Info("TestInst", "Func2");
        }

        private static void Func3()
        {
            DebugUtils.Info("TestInst", "Func3");
        }

        public void Dispose()
        {
            
        }

        public void AddListeners()
        {
            // 按照 先后顺序 触发执行
            // 这里 this 会被 Action 的 Target 引用，必须 RemoveListeners
            EnumEventDispatcher.AddEventListener(EnumEventType.JoystickRelease, Func1);
            EnumEventDispatcher.AddEventListener(EnumEventType.JoystickRelease, Func2);
            EnumEventDispatcher.AddEventListener(EnumEventType.JoystickRelease, Func3);
        }

        public void RemoveListeners()
        {
            EnumEventDispatcher.RemoveEventListener(EnumEventType.JoystickRelease, Func1);
            EnumEventDispatcher.RemoveEventListener(EnumEventType.JoystickRelease, Func2);
            EnumEventDispatcher.RemoveEventListener(EnumEventType.JoystickRelease, Func3);
        }

        ~TestInst()
        {
            DebugUtils.Info("TestInst", "~TestInst");
        }
    }

    public class EventTest : MonoBehaviour
    {
        private static bool isInitialid = false;
        GameObject child;
        TestInst week = null;
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
                week = new TestInst();
                week.AddListeners();
                // 实际上，这里还是会调用 week 注册的事件
                // 一定要 注销事件
                // week = null;
              
            }
        }

        private void DestroyChild()
        {
            GameObject.Destroy(child);
            week.RemoveListeners();
            week = null;
            // GC 会在任意时刻调用， ~TestInst 被调用
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
            EnumEventDispatcher.TriggerEvent(EnumEventType.JoystickRelease);
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



