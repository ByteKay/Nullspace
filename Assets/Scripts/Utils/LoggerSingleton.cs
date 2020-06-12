
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace Nullspace
{
    /// <summary>
    /// 日志等级声明。
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        /// <summary>
        /// 缺省
        /// </summary>
        NONE = 0,
        /// <summary>
        /// 调试
        /// </summary>
        DEBUG = 1,
        /// <summary>
        /// 信息
        /// </summary>
        INFO = 2,
        /// <summary>
        /// 警告
        /// </summary>
        WARNING = 4,
        /// <summary>
        /// 错误
        /// </summary>
        ERROR = 8,
        /// <summary>
        /// 异常
        /// </summary>
        EXCEPT = 16,
        /// <summary>
        /// 关键错误
        /// </summary>
        CRITICAL = 32,
    }


    public class LogFileWriter
    {
        private List<string> mCacheMessages = new List<string>();
        private LoggerConfig mConfig;
        private string mDay;
        private string mFilePath;
        private int mIndex = 0;
        private StreamWriter mFileStream;
        private Thread mThread = null;
        private bool isStopped = false;

        public LogFileWriter(LoggerConfig config)
        {
            mConfig = config;
            mDay = TimeUtils.FormatTime(DateTime.Now);
            mFilePath = mConfig.FilePath;
            mFileStream = new StreamWriter(mFilePath, true, UTF8Encoding.UTF8);
            mFileStream.AutoFlush = false;
            StartLoging();
        }

        public void Log(string message)
        {
            lock (this)
            {
                mCacheMessages.Add(message);
            }
        }

        public void Stop()
        {
            isStopped = true;
        }

        private void StartLoging()
        {
            if (mThread == null)
            {
                mThread = new Thread(Check);
                mThread.Start();
            }
        }

        private void Check()
        {
            while (true)
            {
                List<string> back = null;
                if (mCacheMessages.Count > 0)
                {
                    lock (this)
                    {
                        back = mCacheMessages;
                        mCacheMessages = new List<string>();
                    }
                }
                if (back != null)
                {
                    foreach (string msg in back)
                    {
                        if (msg.Substring(0, 10) == mDay)
                        {
                            mFileStream.WriteLine(msg);
                        }
                        else
                        {
                            mFileStream.Flush();
                            mFileStream.Close();
                            RenameNextSeq();
                            mIndex = 0;
                            mDay = msg.Substring(0, 10);
                        }
                    }
                    mFileStream.Flush();
                    FileInfo info = new FileInfo(mFilePath);
                    if ((info.Length >> 10) > mConfig.FileMaxSize) // kb
                    {
                        mFileStream.Close();
                        RenameNextSeq();
                    }
                }
                if (isStopped)
                {
                    mFileStream.Flush();
                    mFileStream.Close();
                    break;
                }
                else
                {
                    Thread.Sleep(mConfig.FlushInterval);
                }
            }

        }

        private void RenameNextSeq()
        {
            string rename = null;
            for (int i = mIndex + 1; ; ++i)
            {
                rename = string.Format("{0}/{1}_{2}_{3}.{4}", mConfig.Directory, mConfig.FileName, mDay, i, mConfig.FileExtention);
                if (!File.Exists(rename))
                {
                    mIndex = i;
                    break;
                }
            }
            File.Move(mFilePath, rename);
            mFileStream = new StreamWriter(mFilePath, true, UTF8Encoding.Unicode);
            mFileStream.AutoFlush = false;
        }
    }

    public class LoggerSingleton : Singleton<LoggerSingleton>
    {
        private LogFileWriter mFileWriter;
        private LoggerConfig Config;
        private Action<string> mLog;
        private bool isInitialize = false;

        public void Stop()
        {
            if (mFileWriter != null)
            {
                mFileWriter.Stop();
            }
        }

        public void Initialize(LoggerConfig config)
        {
            if (!isInitialize)
            {
                Config = config;
                if (!Directory.Exists(Config.Directory))
                {
                    Directory.CreateDirectory(Config.Directory);
                }
                mFileWriter = new LogFileWriter(Config);
                mLog = mFileWriter.Log;
                isInitialize = true;
            }
        }

        public void AddLogOut(Action<string> logOut)
        {
            mLog += logOut;
        }

        public void LogDebug(string message)
        {
            if ((Config.mLogLevel & LogLevel.DEBUG) == LogLevel.DEBUG)
            {
                string msg = GeneratorMessage(LogLevel.DEBUG, message);
                mLog(msg);
            }
        }

        public void LogInfo(string message)
        {
            if ((Config.mLogLevel & LogLevel.INFO) == LogLevel.INFO)
            {
                string msg = GeneratorMessage(LogLevel.INFO, message);
                mLog(msg);
            }
        }

        public void LogWarning(string message)
        {
            if ((Config.mLogLevel & LogLevel.WARNING) == LogLevel.WARNING)
            {
                string msg = GeneratorMessage(LogLevel.WARNING, message);
                mLog(msg);
            }
        }

        public void LogError(string message)
        {
            if ((Config.mLogLevel & LogLevel.ERROR) == LogLevel.ERROR)
            {
                string msg = GeneratorMessage(LogLevel.ERROR, message);
                mLog(msg);
            }
        }

        public void LogExcept(string message)
        {
            if ((Config.mLogLevel & LogLevel.EXCEPT) == LogLevel.EXCEPT)
            {
                string msg = GeneratorMessage(LogLevel.EXCEPT, message);
                mLog(msg);
            }
        }

        public void LogCritical(string message)
        {
            if ((Config.mLogLevel & LogLevel.CRITICAL) == LogLevel.CRITICAL)
            {
                string msg = GeneratorMessage(LogLevel.CRITICAL, message);
                mLog(msg);
            }
        }

        private string GeneratorMessage(LogLevel loglevel, string message)
        {
            return string.Format("{0} {1} {2}", TimeUtils.FormatTimeHMS(DateTime.Now), loglevel, message);
        }
    }

}
