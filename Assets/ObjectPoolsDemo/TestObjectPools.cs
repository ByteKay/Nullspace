using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public class TestObjectPools : MonoBehaviour
    {
        private Callback<int, string> callback_intstr;
        private Callback<string, string> callback_strstr;
        private Callback<string, int> callback_strint;
        // Use this for initialization
        void Start()
        {
            callback_intstr = null;
        }

        private void Update()
        {
            TimerTaskQueue.Instance.Tick();
        }

        private void OnGUI()
        {
            if (callback_intstr == null && GUILayout.Button("Acquire"))
            {
                callback_intstr = ObjectPools.Instance.Acquire<Callback<int, string>>();
                callback_strstr = ObjectPools.Instance.Acquire<Callback<string, string>>();
                callback_strint = ObjectPools.Instance.Acquire<Callback<string, int>>();
            }

            if (callback_intstr != null && GUILayout.Button("Release"))
            {
                ObjectPools.Instance.Release(callback_intstr);
                ObjectPools.Instance.Release(callback_strstr);
                ObjectPools.Instance.Release(callback_strint);
                callback_intstr = null;
            }
        }

        private void OnDestroy()
        {
            ObjectPools.Instance.Release(callback_intstr);
            ObjectPools.Instance.Release(callback_strstr);
            ObjectPools.Instance.Release(callback_strint);
        }
    }

}


