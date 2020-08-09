using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Nullspace
{
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
            mDay = DateTimeUtils.FormatTime(DateTime.Now);
            mFilePath = mConfig.FilePath;
            mFileStream = new StreamWriter(mFilePath, true, Encoding.UTF8);
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
            mFileStream = new StreamWriter(mFilePath, true, Encoding.Unicode);
            mFileStream.AutoFlush = false;
        }
    }

}
