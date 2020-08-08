﻿using System;
using System.Diagnostics;

namespace Nullspace
{
    public enum InfoType
    {
        Info,
        Warning,
        Error
    }

    public class DebugUtils
    {
        private static Action<InfoType, string> LogAction = null;

        public static void SetLogAction(Action<InfoType, string> logAction)
        {
            LogAction = logAction;
        }

        public static void Assert(bool isTrue, string message)
        {
            if (!isTrue)
            {
                Log(InfoType.Error, message);
            }
            Debug.Assert(isTrue, message);
        }

        public static void Log(InfoType infoType, string info)
        {
            if (LogAction != null)
            {
                LogAction(infoType, info);
            }
        }
    }
}
