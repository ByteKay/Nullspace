using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Runtime.InteropServices;

namespace Nullspace
{

    public static class DebugUtils
    {
        [Flags]
        public enum DisplayType
        {
            Unity = 1,
            Remote = 2,
            All = Unity | Remote,
        }

        public enum LogLevel
        {
            OFF,
            Error,
            EVENT,
            Info,
            Debug,
            Warning,
            MAX,
        }
        private const string mSpliter = "$";
        private const int TrimStrBufferSize = 4096;
        private const int StrBufferInitSize = 256;
        private static StringBuilder StrBuffer = new StringBuilder(StrBufferInitSize);

        static DebugUtils()
        {
            Debug("", "VeryFirstLog");
#if !UNITY_EDITOR
			Application.logMessageReceived += DebugUtils.UnityLogCallback;
#endif
        }

        static public void Debug<T1>(string tag, T1 arg1)
        {
            WriteLog(LogLevel.Debug, tag, arg1);
        }

        static public void Debug<T1, T2>(string tag, T1 arg1, T2 arg2)
        {
            WriteLog(LogLevel.Debug, tag, arg1, arg2);
        }

        static public void Debug<T1, T2, T3>(string tag, T1 arg1, T2 arg2, T3 arg3)
        {
            WriteLog(LogLevel.Debug, tag, arg1, arg2, arg3);
        }

        static public void Debug<T1, T2, T3, T4>(string tag, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            WriteLog(LogLevel.Debug, tag, arg1, arg2, arg3, arg4);
        }

        static public void Info<T1>(string tag, T1 arg1)
        {
            WriteLog(LogLevel.Info, tag, arg1);
        }

        static public void Info<T1, T2>(string tag, T1 arg1, T2 arg2)
        {
            WriteLog(LogLevel.Info, tag, arg1, arg2);
        }

        static public void Info<T1, T2, T3>(string tag, T1 arg1, T2 arg2, T3 arg3)
        {
            WriteLog(LogLevel.Info, tag, arg1, arg2, arg3);
        }

        static public void Info<T1, T2, T3, T4>(string tag, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            WriteLog(LogLevel.Info, tag, arg1, arg2, arg3);
        }

        static public void Warning<T1>(string tag, T1 arg1)
        {
            WriteLog(LogLevel.Warning, tag, arg1);
        }

        static public void Warning<T1, T2>(string tag, T1 arg1, T2 arg2)
        {
            WriteLog(LogLevel.Warning, tag, arg1, arg2);
        }

        static public void Warning<T1, T2, T3>(string tag, T1 arg1, T2 arg2, T3 arg3)
        {
            WriteLog(LogLevel.Warning, tag, arg1, arg2, arg3);
        }

        static public void Warning<T1, T2, T3, T4>(string tag, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            WriteLog(LogLevel.Warning, tag, arg1, arg2, arg3);
        }

        static public void Error<T1>(string tag, T1 arg1)
        {
            WriteLog(LogLevel.Error, tag, arg1);
        }

        static public void Error<T1, T2>(string tag, T1 arg1, T2 arg2)
        {
            WriteLog(LogLevel.Error, tag, arg1, arg2);
        }

        static public void Error<T1, T2, T3>(string tag, T1 arg1, T2 arg2, T3 arg3)
        {
            WriteLog(LogLevel.Error, tag, arg1, arg2, arg3);
        }

        static public void Error<T1, T2, T3, T4>(string tag, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            WriteLog(LogLevel.Error, tag, arg1, arg2, arg3, arg4);
        }

        static public void UnityLogCallback(string strCondition, string strStackTrace, LogType eType)
        {
            switch (eType)
            {
                case LogType.Log:
                    {
                        Debug("Unity", strCondition);
                    }
                    break;
                case LogType.Warning:
                    {
                        Warning("Unity", strCondition);
                    }
                    break;
                case LogType.Exception:
                case LogType.Error:
                    {
                        Error("Unity", strCondition);
                    }
                    break;
            }
        }

        static private void TrimStringBuffer()
        {
            if (StrBuffer.Length != 0)
            {
                StrBuffer.Length = 0;
            }
            if (StrBuffer.Length >= TrimStrBufferSize)
            {
                StrBuffer.Capacity = StrBufferInitSize;
            }
        }

        static private void AppendErrorStack(LogLevel level, string tag)
        {
            if (level == LogLevel.Error)
            {
                System.Diagnostics.StackTrace pStackTrace = new System.Diagnostics.StackTrace(3, true);
                StrBuffer.Append(":");
                StrBuffer.Append(pStackTrace);
            }
        }

        static private void WriteLog<T1>(LogLevel level, string tag, T1 arg1)
        {
            TrimStringBuffer();

            StrBuffer.Append(tag);
            StrBuffer.Append(mSpliter);
            StrBuffer.Append(arg1);
            AppendErrorStack(level, tag);

            WriteLog(level);
        }

        static private void WriteLog<T1, T2>(LogLevel level, string tag, T1 arg1, T2 arg2)
        {
            TrimStringBuffer();

            StrBuffer.Append(tag);
            StrBuffer.Append(mSpliter);
            StrBuffer.Append(arg1);
            StrBuffer.Append(arg2);
            AppendErrorStack(level, tag);

            WriteLog(level);
        }

        static private void WriteLog<T1, T2, T3>(LogLevel level, string tag, T1 arg1, T2 arg2, T3 arg3)
        {
            TrimStringBuffer();

            StrBuffer.Append(tag);
            StrBuffer.Append(mSpliter);
            StrBuffer.Append(arg1);
            StrBuffer.Append(arg2);
            StrBuffer.Append(arg3);
            AppendErrorStack(level, tag);

            WriteLog(level);
        }

        static private void WriteLog<T1, T2, T3, T4>(LogLevel level, string tag, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            TrimStringBuffer();

            StrBuffer.Append(tag);
            StrBuffer.Append(mSpliter);
            StrBuffer.Append(arg1);
            StrBuffer.Append(arg2);
            StrBuffer.Append(arg3);
            StrBuffer.Append(arg4);
            AppendErrorStack(level, tag);

            WriteLog(level);
        }

        static private void WriteLog(LogLevel level)
        {
#if UNITY_EDITOR
            switch (level)
            {
                case LogLevel.Debug: { UnityEngine.Debug.Log(StrBuffer.ToString()); } break;
                case LogLevel.Info: { UnityEngine.Debug.Log(StrBuffer.ToString()); } break;
                case LogLevel.Warning: { UnityEngine.Debug.LogWarning(StrBuffer.ToString()); } break;
                case LogLevel.Error: { UnityEngine.Debug.LogError(StrBuffer.ToString()); } break;
            }
#else
			PrintLog((int)level, StrBuffer.ToString());
#endif
        }

        public static void PrintLog(int nLevel, string msg)
        {

        }
    }

}
