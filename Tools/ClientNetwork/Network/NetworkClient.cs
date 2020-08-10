
using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;

namespace Nullspace
{
    public partial class NetworkClient
    {
        protected static byte[] mBufferHead = new byte[NetworkHead.Size()];
        protected byte[] mContents;
        protected Thread mReceiveThread;
        protected void Receive()
        {
            if (ProcessHead() && ProcessContent())
            {
                ProcessPacket();
            }
        }

        protected bool ProcessHead()
        {
            bool flag = Receive(ref mBufferHead, mBufferHead.Length);
            if (flag)
            {
                mHead.InitializeBy(mBufferHead);
            }
            return flag;
        }

        protected bool ProcessContent()
        {
            bool flag = true;
            mContents = null; // reset
            if (mHead.mLength > 0)
            {
                mContents = new byte[mHead.mLength];
                flag = Receive(ref mContents, mHead.mLength);
            }
            return flag;
        }

        protected bool Receive(ref byte[] ptr, int total)
        {
            int len = 0;
            do
            {
                int size = mClientSocket.Receive(ptr, len, total - len, SocketFlags.None);
                if (size == -1 || size == 0)
                {
                    StateCtl.Set(StateParamName, StateParameterValue.Connectted2Reconnectting);
                    return false;
                }
                else
                {
                    len += size;
                }
            } while (len < total);
            return true;
        }

        protected void ProcessPacket()
        {
            NetworkPacket packet = new NetworkPacket(mHead.Clone(), mContents, this);
            NetworkEventHandler.AddPacket(packet);
        }

        protected void ReceiveMessage()
        {
            while (!mIsStop)
            {
                try
                {
                    if (IsConnectted())
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
                    if (mIsStop)
                    {
                        break;
                    }
                    DebugUtils.Log(InfoType.Warning, e.Message);
                    mReconnectImmediate = true;
                    // 重新连接
                    StateCtl.Set(StateParamName, StateParameterValue.Connectted2Reconnectting);
                }
                Thread.Sleep(0);
            }
        }
    }

    public partial class NetworkClient
    {
        private Queue<byte[]> mNeedSendMessages;
        private List<byte> mSendPack;
        private Thread mSendThread;

        protected void Send(byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
            {
                int len = mClientSocket.Send(bytes, 0, bytes.Length, SocketFlags.None);
                if (len <= 0)
                {
                    StateCtl.Set(StateParamName, StateParameterValue.Connectted2Reconnectting);
                }
            }
        }

        protected void SendMessage()
        {
            while (!mIsStop)
            {
                try
                {
                    if (IsConnectted())
                    {
                        mSendPack.Clear();
                        while (mNeedSendMessages.Count > 0)
                        {
                            byte[] msg = null;
                            lock (mSendLock)
                            {
                                msg = mNeedSendMessages.Dequeue();
                            }
                            if (msg != null)
                            {
                                mSendPack.AddRange(msg);
                            }
                        }
                        if (mSendPack.Count > 0)
                        {
                            Send(mSendPack.ToArray());
                            mSendPack.Clear();
                        }
                    }
                    else
                    {
                        mSendWait.WaitOne();
                    }
                }
                catch (Exception e)
                {
                    if (mIsStop)
                    {
                        break;
                    }
                    DebugUtils.Log(InfoType.Warning, e.Message);
                    mReconnectImmediate = true;
                    // 重新连接
                    StateCtl.Set(StateParamName, StateParameterValue.Connectted2Reconnectting);
                }
                Thread.Sleep(16);
            }
        }
    }

    public partial class NetworkClient
    {
        private static EventWaitHandle mReceiveWait = new AutoResetEvent(false);
        private static EventWaitHandle mSendWait = new AutoResetEvent(false);
        private static object mSendLock = new object();
        protected static NetworkHead mHead = new NetworkHead();

        protected Properties mConfig;
        protected string mIP;
        protected int mPort;
        protected int mReconnectCount = 0;
        protected int mReconnectMaxCount = 5;
        protected int mReconnectTimerInterval = 2000;

        private IPAddress mAddress;
        private Socket mClientSocket;

        public void Initialize(Properties prop)
        {
            mConfig = prop;
            InitData();
            InitState();
            // 开始
            StateCtl.Set(StateParamName, StateParameterValue.None2Initialized);
        }

        protected void InitData()
        {
            mIP = mConfig.GetString("IP", null);
            mPort = mConfig.GetInt("port");
            mReconnectCount = mConfig.GetInt("reconnect_count");
            mReconnectMaxCount = mConfig.GetInt("reconnect_max_count");
            mReconnectMaxCount = mConfig.GetInt("reconnect_timer_interval");
            mAddress = IPAddress.Parse(mIP);

            mNeedSendMessages = new Queue<byte[]>();
            mSendPack = new List<byte>();
        }

        protected void InitSocket()
        {
            mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            mClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            mClientSocket.NoDelay = true;
        }

    }

    public partial class NetworkClient
    {
        protected StateController<NetworkConnectState> StateCtl;
        protected const string StateParamName = "State";


        protected bool mIsStop = true;
        protected int mReconnectTimerId = -1;
        protected bool mReconnectImmediate = false;
        protected float mReconnectTimepoint = 0;

        protected static class StateParameterValue
        {
            public const int None2Initialized = -1;
            public const int Initialized2Connectting = 0;

            public const int Connectting2Connectted = 1;
            public const int Connectting2Disconnected = 2;
            public const int Connectting2Reconnectting = 3;

            public const int Connectted2Reconnectting = 5;
            public const int Connectted2Disconnected = 6;

            public const int Reconnectting2ConnectFailed = 4;
            public const int Reconnectting2Connectted = 7;
            public const int Reconnectting2Disconnected = 8;
        }

        public NetworkConnectState State { get { return StateCtl.Current.StateType; } }

        protected bool IsConnectted()
        {
            return State == NetworkConnectState.Connectted;
        }

        protected bool Connect()
        {
            bool connectted = false;
            try
            {
                if (mClientSocket == null)
                {
                    InitSocket();
                }
                IAsyncResult result = mClientSocket.BeginConnect(mAddress, mPort,
                    (IAsyncResult res) =>
                    {
                        mClientSocket.EndConnect(res);
                    }, mClientSocket);
                // 这里要改写成异步
                connectted = result.AsyncWaitHandle.WaitOne(2000, true);
            }
            catch (Exception e)
            {
                DebugUtils.Log(InfoType.Error, "Connect Exception: " + e.Message);
            }
            return connectted;
        }

        protected void EnterConnectting()
        {
            DebugUtils.Log(InfoType.Info, "EnterConnectting");
            bool connectted = Connect();
            if (connectted)
            {
                StateCtl.Set(StateParamName, StateParameterValue.Connectting2Connectted);
            }
            else
            {
                StateCtl.Set(StateParamName, StateParameterValue.Connectting2Reconnectting);
            }
        }

        protected void EnterReconnectting()
        {
            DebugUtils.Log(InfoType.Info, "EnterReconnectting");
            mReconnectTimepoint = DateTimeUtils.GetTimeStampSeconds();
            // 发送或接收数据检测到链接失效，立马重连一次
            if (mReconnectImmediate)
            {
                Reconnectting();
            }
            else
            {
                AddReconnectTimer();
            }
        }

        protected void Reconnectting()
        {
            bool connectted = Connect();
            mReconnectCount++;
            DebugUtils.Log(InfoType.Info, string.Format("Reconnectting {0}/{1}", mReconnectCount, mReconnectMaxCount));
            if (connectted)
            {
                float reconnectElappsed = DateTimeUtils.GetTimeStampSeconds() - mReconnectTimepoint;
                DebugUtils.Log(InfoType.Info, string.Format("Reconnectting Cost: {0} s", reconnectElappsed));
                StateCtl.Set(StateParamName, StateParameterValue.Reconnectting2Connectted);
            }
            else
            {
                AddReconnectTimer();
            }
        }

        protected void AddReconnectTimer()
        {
            if (mReconnectCount < mReconnectMaxCount)
            {
                mReconnectTimerId = TimerTaskQueue.Instance.AddTimer(mReconnectTimerInterval, 0, Reconnectting);
            }
            else
            {
                StateCtl.Set(StateParamName, StateParameterValue.Reconnectting2ConnectFailed);
            }
        }

        protected void DelReconnectTimer()
        {
            TimerTaskQueue.Instance.DelTimer(mReconnectTimerId);
        }

        protected void LeaveReconnectting()
        {
            DelReconnectTimer();
            mReconnectTimerId = -1;
            mReconnectCount = 0;
        }

        protected void EnterConnectFailed()
        {
            Stop();
            DebugUtils.Log(InfoType.Error, "EnterConnectFailed");
        }

        protected void EnterConnectted()
        {
            mSendWait.Set();
            mReceiveWait.Set();
            DebugUtils.Log(InfoType.Info, "EnterConnectted");
        }

        protected void EnterDisconnected()
        {
            Stop();
            DebugUtils.Log(InfoType.Info, "EnterDisconnected");
        }

        protected virtual void Stop()
        {
            mIsStop = true;
            mReceiveThread.Interrupt();
            mSendThread.Interrupt();
            mSendWait.Reset();
            mReceiveWait.Reset();
            Close();
        }

        protected void Close()
        {
            if (mClientSocket != null)
            {
                if (mClientSocket.Connected)
                {
                    mClientSocket.Shutdown(SocketShutdown.Both);
                }
                mClientSocket.Close();
                mClientSocket = null;
            }
        }

        protected void EnterAlready()
        {
            // 初始化Socket
            InitSocket();
            // 初始化 发送和接收 线程
            InitThread();
            // 开始连接
            StateCtl.Set(StateParamName, StateParameterValue.Initialized2Connectting);
        }

        protected void InitState()
        {
            StateCtl = new StateController<NetworkConnectState>();
            StateCtl.AddParameter(StateParamName, StateParameterDataType.INT, StateParameterValue.None2Initialized);

            StateCtl.AddState(NetworkConnectState.None).AsCurrent().AddTransfer(NetworkConnectState.Initialized).With(StateParamName, ConditionOperationType.EQUAL, StateParameterValue.None2Initialized);
            StateCtl.AddState(NetworkConnectState.Initialized).AddTransfer(NetworkConnectState.Connectting).With(StateParamName, ConditionOperationType.EQUAL, StateParameterValue.Initialized2Connectting);

            StateCtl.AddState(NetworkConnectState.Connectting).AddTransfer(NetworkConnectState.Connectted).With(StateParamName, ConditionOperationType.EQUAL, StateParameterValue.Connectting2Connectted);
            StateCtl.AddState(NetworkConnectState.Connectting).AddTransfer(NetworkConnectState.Disconnected).With(StateParamName, ConditionOperationType.EQUAL, StateParameterValue.Connectting2Disconnected);
            StateCtl.AddState(NetworkConnectState.Connectting).AddTransfer(NetworkConnectState.Reconnectting).With(StateParamName, ConditionOperationType.EQUAL, StateParameterValue.Connectting2Reconnectting);


            StateCtl.AddState(NetworkConnectState.Connectted).AddTransfer(NetworkConnectState.Reconnectting).With(StateParamName, ConditionOperationType.EQUAL, StateParameterValue.Connectted2Reconnectting);
            StateCtl.AddState(NetworkConnectState.Connectted).AddTransfer(NetworkConnectState.Disconnected).With(StateParamName, ConditionOperationType.EQUAL, StateParameterValue.Connectted2Disconnected);

            StateCtl.AddState(NetworkConnectState.Reconnectting).AddTransfer(NetworkConnectState.Connectted).With(StateParamName, ConditionOperationType.EQUAL, StateParameterValue.Reconnectting2Connectted);
            StateCtl.AddState(NetworkConnectState.Reconnectting).AddTransfer(NetworkConnectState.Disconnected).With(StateParamName, ConditionOperationType.EQUAL, StateParameterValue.Reconnectting2Disconnected);
            StateCtl.AddState(NetworkConnectState.Reconnectting).AddTransfer(NetworkConnectState.ConnectFailed).With(StateParamName, ConditionOperationType.EQUAL, StateParameterValue.Reconnectting2ConnectFailed);

            StateCtl.AddState(NetworkConnectState.Initialized).Enter(EnterAlready);
            StateCtl.AddState(NetworkConnectState.Connectting).Enter(EnterConnectting);
            StateCtl.AddState(NetworkConnectState.Reconnectting).Enter(EnterReconnectting).Exit(LeaveReconnectting);
            StateCtl.AddState(NetworkConnectState.ConnectFailed).Enter(EnterConnectFailed);
            StateCtl.AddState(NetworkConnectState.Connectted).Enter(EnterConnectted);
            StateCtl.AddState(NetworkConnectState.Disconnected).Enter(EnterDisconnected);
        }


        protected void InitThread()
        {
            mIsStop = false;
            mReconnectImmediate = false;
            mReceiveThread = new Thread(ReceiveMessage);
            mReceiveThread.Start();
            mSendThread = new Thread(SendMessage);
            mSendThread.Start();
        }
    }

}
