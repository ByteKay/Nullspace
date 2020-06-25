
using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Nullspace
{
    public enum ClientConnectState
    {
        Already,
        Connectted,
        Reconnectting,
        Disconnected,
    }

    public abstract class AbstractNetworkClient
    {
        private static object mSendLock = new object();
        private static int mReconnectInterval = 5000;
        private static int mHeartbeatInterval = 5000;
        private static byte[] mHeartBytes = null;
        private static EventWaitHandle mSendWait = new AutoResetEvent(false);
        private static EventWaitHandle mReceiveWait = new AutoResetEvent(false);
        private Queue<byte[]> mNeedSendMessages = new Queue<byte[]>();
        private int mReconnectTimerId = int.MaxValue;
        private int mHeartTimerId = int.MaxValue;
        protected string mIP = null;
        protected int mPort;
        protected NetworkHeadFormat mHead;
        protected byte[] mContents;
        private Thread mReceiveThread;
        private Thread mSendThread;
        private bool isStop;
        public int mSessionId = -1;
        public ClientConnectState ConnectState { get; set; }
        public AbstractNetworkClient(string ip, int port)
        {
            try
            {
                InitMember(ip, port);
                Init();
            }
            catch (Exception e)
            {
                Debug.Log("error: " + e.Message);
            }
        }

        public void Start()
        {
            if (IsConnectState(ClientConnectState.Already))
            {
                Connect();
                InitThread();
            }
        }
        
        public void Enqueue(byte[] msg)
        {
            if (IsConnectState(ClientConnectState.Connectted))
            {
                lock (mSendLock)
                {
                    mNeedSendMessages.Enqueue(msg);
                }
            }
        }
        public virtual void Stop()
        {
            isStop = true;
            mReceiveThread.Interrupt();
            mSendThread.Interrupt();
            Close();
            mSendWait.Reset();
            mReceiveWait.Reset();
        }
        protected abstract void Init();
        protected abstract void Connect();
        protected abstract void Send(byte[] bytes);
        protected abstract void Close();
        protected abstract bool ProcessHead();
        protected abstract bool ProcessContent();



        private void Reconnect()
        {
            Connect();
            if (IsConnectState(ClientConnectState.Connectted))
            {
                // to do
            }
        }
        private void Receive()
        {
            if (ProcessHead() && ProcessContent())
            {
                ProcessPacket();
            }
        }
        private void ProcessPacket()
        {
            NetworkPacket packet = new NetworkPacket(mHead.Clone(), mContents, this);
            NetworkCommandHandler.Instance.AddPacket(packet);
        }
        protected void SetConnectState(ClientConnectState state)
        {
            if (ConnectState == state)
            {
                return;
            }
            ConnectState = state;
            if (IsConnectState(ClientConnectState.Reconnectting))
            {
                if (mReconnectTimerId == int.MaxValue)
                {
                    mReconnectTimerId = TimerTaskQueue.Instance.AddTimer(1000, mReconnectInterval, StartConnect);
                }
                if (mHeartTimerId != int.MaxValue)
                {
                    TimerTaskQueue.Instance.DelTimer(mHeartTimerId);
                    mHeartTimerId = int.MaxValue;
                }
            }
            else if (IsConnectState(ClientConnectState.Connectted))
            {
                if (mReconnectTimerId != int.MaxValue)
                {
                    TimerTaskQueue.Instance.DelTimer(mReconnectTimerId);
                    mReconnectTimerId = int.MaxValue;
                }
                if (mHeartTimerId == int.MaxValue)
                {
                    mHeartTimerId = TimerTaskQueue.Instance.AddTimer(1000, mHeartbeatInterval, HeartBeat);
                }
                mSendWait.Set();
                mReceiveWait.Set();
            }
            else if(IsConnectState(ClientConnectState.Disconnected))
            {
                if (mHeartTimerId != int.MaxValue)
                {
                    TimerTaskQueue.Instance.DelTimer(mHeartTimerId);
                    mHeartTimerId = int.MaxValue;
                }
                if (mReconnectTimerId != int.MaxValue)
                {
                    TimerTaskQueue.Instance.DelTimer(mReconnectTimerId);
                    mReconnectTimerId = int.MaxValue;
                }
            }
        }
        private bool IsConnectState(ClientConnectState state)
        {
            return ConnectState == state;
        }
        private void GetIP(string ip)
        {
            IPHostEntry entry = Dns.GetHostEntry(ip);
            foreach (var item in entry.AddressList)
            {
                string[] valus = item.ToString().Split('.');
                bool flag = true;
                int it;
                foreach (string v in valus)
                {
                    if (!int.TryParse(v, out it))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    mIP = item.ToString();
                    break;
                }
            }
            if (mIP == null)
            {
                throw new Exception("IP BUG");
            }
        }
        private void InitThread()
        {
            mReceiveThread = new Thread(ReceiveMessage);
            mReceiveThread.Start();
            mSendThread = new Thread(SendMessage);
            mSendThread.Start();
        }

        private void StartConnect()
        {
            if (IsConnectState(ClientConnectState.Reconnectting))
            {
                Close();
                mNeedSendMessages.Clear();
                Reconnect();
            }
        }

        // 这里实际上可以一帧末尾并包发送。一次发送就好
        // 或者 设置一次的发射字节数量上限值，多次发送。
        private void SendMessage()
        {
            while (!isStop)
            {
                try
                {
                    if (IsConnectState(ClientConnectState.Connectted))
                    {
                        while (mNeedSendMessages.Count > 0)
                        {
                            byte[] msg = null;
                            lock (mSendLock)
                            {
                                msg = mNeedSendMessages.Dequeue();
                            }
                            if (msg != null)
                            {
                                Send(msg);
                            }
                        }
                    }
                    else
                    {
                        mSendWait.WaitOne();
                    }
                }
                catch (Exception e)
                {
                    if (isStop)
                    {
                        break;
                    }
                    SetConnectState(ClientConnectState.Reconnectting);
                    Debug.Log(e.Message);
                }
                Thread.Sleep(0);
            }
        }
        private void ReceiveMessage()
        {
            while (!isStop)
            {
                try
                {
                    if (IsConnectState(ClientConnectState.Connectted))
                    {
                        Receive();
                    }
                    else
                    {
                        mReceiveWait.WaitOne();
                    }
                }
                catch (Exception e)
                {
                    if (isStop)
                    {
                        break;
                    }
                    SetConnectState(ClientConnectState.Reconnectting);
                    Debug.Log(e.Message);
                }
                Thread.Sleep(0);
            }
        }
        private void InitMember(string ip, int port)
        {
            isStop = false;
            mIP = ip;
            // GetIP(ip);
            mHead = new NetworkHeadFormat();
            mContents = null;
            mPort = port;
            ConnectState = ClientConnectState.Already;
            NetworkHeadFormat temp = new NetworkHeadFormat();
            temp.mType = CommandType.HeartCodec;
            mHeartBytes = temp.Merge(null);
        }
        private void HeartBeat()
        {
            Enqueue(mHeartBytes);
        }
    }

}
