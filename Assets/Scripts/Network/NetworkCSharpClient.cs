﻿using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Nullspace
{
    public class NetworkCSharpClient : AbstractNetworkClient
    {
        private byte[] mBufferHead = new byte[NetworkHeadFormat.Size()];
        private IPAddress mAddress;
        private Socket mClientSocket;
        public NetworkCSharpClient(string ip, int port) : base(ip, port)
        {

        }
        protected override void Close()
        {
            if (mClientSocket != null)
            {
                if (mClientSocket.Connected)
                {
                    mClientSocket.Shutdown(SocketShutdown.Both);
                    mClientSocket.Close();
                    mClientSocket = null;
                }
                SetConnectState(NetworkConnectState.Disconnected);
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
                    SetConnectState(NetworkConnectState.Connectted);
                    Debug.Log("connect: " + mIP);
                }
                else
                {
                    SetConnectState(NetworkConnectState.Reconnectting);
                }
            }
            catch (Exception e)
            {
                SetConnectState(NetworkConnectState.Reconnectting);
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
                    SetConnectState(NetworkConnectState.Reconnectting);
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
                    SetConnectState(NetworkConnectState.Reconnectting);
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
