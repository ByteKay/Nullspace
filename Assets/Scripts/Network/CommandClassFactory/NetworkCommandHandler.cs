using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Nullspace
{
    /// <summary>
    /// should initialize firstly
    /// </summary>
    public class NetworkCommandHandler
    {
        public static NetworkCommandHandler Instance = new NetworkCommandHandler();
        private static object mLock = new object();
        private static EventWaitHandle mWaitHandle;
        private static bool isClose;
        private static bool isInitialized = false;

        public Thread mHandleThread = null;
        private Queue<NetworkPacket> mCommandPacket;
        private NetworkCommandHandler()
        {

        }
        public void Initialize()
        {
            if (!isInitialized)
            {
                NetworkCommandFactory.RegisterCommand();
                isInitialized = true;
                mWaitHandle = new AutoResetEvent(false);
                mCommandPacket = new Queue<NetworkPacket>();
                isClose = false;
                mHandleThread = new Thread(HandlePacket) { IsBackground = true };
                mHandleThread.Start();
            }
        }
        public void Initialize(string spacename, Assembly ass)
        {
            if (!isInitialized)
            {
                NetworkCommandFactory.RegisterCommand(spacename, ass);
                isInitialized = true;
                mWaitHandle = new AutoResetEvent(false);
                mCommandPacket = new Queue<NetworkPacket>();
                isClose = false;
                mHandleThread = new Thread(HandlePacket) { IsBackground = true };
                mHandleThread.Start();
            }
        }

        public void Close()
        {
            mWaitHandle.Set();
            isClose = true;
            mHandleThread.Join();
            mWaitHandle.Close();
            mCommandPacket.Clear();
        }

        public void HandlePacket()
        {
            while (!isClose)
            {
                NetworkPacket packet = null;
                lock (mLock)
                {
                    if (mCommandPacket.Count > 0)
                    {
                        packet = mCommandPacket.Dequeue();
                    }
                }
                if (packet != null)
                {
                    Handle(packet);
                }
                else
                {
                    mWaitHandle.WaitOne();
                }
            }
        }

        private void Handle(NetworkPacket packet)
        {
            NetworkCommand command = NetworkCommandFactory.GetCommand(packet.mHead.mType);
            if (command != null)
            {
                command.HandlePacket(packet);
            }
        }

        public void AddPacket(NetworkPacket packet)
        {
            lock (mLock)
            {
                mCommandPacket.Enqueue(packet);
            }
            mWaitHandle.Set();
        }
    }


}
