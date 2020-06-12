
using UnityEngine;

namespace Nullspace
{
    class TestFireTimer : MonoBehaviour
    {
        private AutoCountTimer timer;

        private void Awake()
        {
            TestAutoTimer();
        }

        private void TestAutoTimer()
        {
            timer = new AutoCountTimer();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Start", GUILayout.Height(40), GUILayout.Width(100)))
            {
                if (timer.isActive)
                {
                    return;
                }
                timer.SetCount(1, 0);
                timer.AddTarget(CommandTimerType.Default);
                timer.Produce(ProcessState.PROCESS);
                timer.Control(ProcessState.START);
            }

            if (GUILayout.Button("Pause", GUILayout.Height(40), GUILayout.Width(100)))
            {
                if (timer.isActive)
                {
                    timer.Control(ProcessState.PAUSE);
                }
            }

            if (GUILayout.Button("Resume", GUILayout.Height(40), GUILayout.Width(100)))
            {
                if (timer.isActive)
                {
                    timer.Control(ProcessState.RESUME);
                }
            }

            if (GUILayout.Button("Stop", GUILayout.Height(40), GUILayout.Width(100)))
            {
                if (timer.isActive)
                {
                    timer.Control(ProcessState.STOP);
                }
            }

            if (GUILayout.Button("Push Target", GUILayout.Height(40), GUILayout.Width(100)))
            {
                if (timer.isActive)
                {
                    timer.AddTarget(CommandTimerType.Default);
                }
            }
        }
    }
}
