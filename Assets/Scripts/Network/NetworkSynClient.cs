using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Nullspace
{
    public class NetworkSynClient : AbstractNetworkClient
    {
        private byte[] mBufferHead = new byte[NetworkHeadFormat.Size()];
        private IPAddress mAddress;
        private Socket mClientSocket;
        public NetworkSynClient(string ip, int port) : base(ip, port)
        {

        }
        protected override void Close()
        {
            if (mClientSocket != null && mClientSocket.Connected)
            {
                mClientSocket.Shutdown(SocketShutdown.Both);
                mClientSocket.Close();
                mClientSocket = null;
            }
        }
        protected override void Connect()
        {
            try
            {
                mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                mClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                mClientSocket.NoDelay = true;
                IAsyncResult result = mClientSocket.BeginConnect(mAddress, mPort,
                    (IAsyncResult res) =>
                    {
                        mClientSocket.EndConnect(res);
                    }, mClientSocket);
                bool connect = result.AsyncWaitHandle.WaitOne(3000, true);
                if (connect)
                {
                    SetConnectState(ClientConnectState.Connectted);
                    Debug.Log("connect: " + mIP);
                }
                else
                {
                    SetConnectState(ClientConnectState.Disconnected);
                }
            }
            catch (Exception e)
            {
                SetConnectState(ClientConnectState.Disconnected);
                Debug.Log("test Connect Exception: " + e.Message);
            }

        }
        protected override bool ProcessHead()
        {
            bool flag = Receive(ref mBufferHead, mBufferHead.Length);
            if (flag)
            {
                mHead.InitializeBy(mBufferHead);
            }
            return flag;
        }
        protected override bool ProcessContent()
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
        protected override void Send(byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
            {
                int len = mClientSocket.Send(bytes, 0, bytes.Length, SocketFlags.None);
                if (len <= 0)
                {
                    SetConnectState(ClientConnectState.Disconnected);
                }
            }
        }
        private void SendAsyn(byte[] bytes)
        {
            IAsyncResult result = mClientSocket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None,
            (IAsyncResult iar) =>
            {
                Socket callback = iar.AsyncState as Socket;
                if (callback != null)
                {
                    callback.EndSend(iar);
                }
            }, mClientSocket);
        }
        private bool Receive(ref byte[] ptr, int total)
        {
            int len = 0;
            do
            {
                int size = mClientSocket.Receive(ptr, len, total - len, SocketFlags.None);
                if (size == -1 || size == 0)
                {
                    SetConnectState(ClientConnectState.Disconnected);
                    return false;
                }
                else
                {
                    len += size;
                }
            } while (len < total);
            return true;
        }
        protected override void Init()
        {
            mAddress = IPAddress.Parse(mIP);
        }
    }
}
