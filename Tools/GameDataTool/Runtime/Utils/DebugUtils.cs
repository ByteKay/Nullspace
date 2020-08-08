using System;
using System.Diagnostics;

namespace Nullspace
{
    public class DebugUtils
    {
        private static Action<string> LogAction = null;

        public static void SetLogAction(Action<string> logAction)
        {
            LogAction = logAction;
        }

        public static void Assert(bool isTrue, string message)
        {
            if (!isTrue)
            {
                Log(message);
            }
            Debug.Assert(isTrue, message);
        }

        public static void Log(string info)
        {
            if (LogAction != null)
            {
                LogAction(info);
            }
        }
    }
}
