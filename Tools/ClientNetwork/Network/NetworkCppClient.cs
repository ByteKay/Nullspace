using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Nullspace
{
    #region c# call c
    [StructLayout(LayoutKind.Sequential)]
    public struct in_addr
    {
        public UInt32 s_addr;
    };

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct sockaddr_in
    {
        /// short
        public Int16 sin_family;
        /// short
        public UInt16 sin_port;
        /// in_addr
        public in_addr sin_addr;
        /// char[8]
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string sin_zero;
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct sockaddr
    {
        /// u_short->unsigned short
        public ushort sa_family;
        /// char[14]
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string sa_data;
    }
    #endregion c# call c

    public class NetworkCppClient : AbstractNetworkClient
    {
        #region dll import
        private const string LibraryName = "libc";
        [DllImport(LibraryName)]
        public static extern int socket(int domain, int type, int protocol);

        [DllImport(LibraryName)]
        public static extern int connect(int fd, IntPtr addr, int len);

        [DllImport(LibraryName)]
        public static extern int read(int fd, System.IntPtr buf, [MarshalAsAttribute(UnmanagedType.SysUInt)] uint nbytes);

        [DllImport(LibraryName)]
        public static extern int recv(int fd, System.IntPtr buf, [MarshalAsAttribute(UnmanagedType.SysUInt)] uint n, int flags);

        [DllImport(LibraryName)]
        public static extern int send(int fd, System.IntPtr buf, [MarshalAsAttribute(UnmanagedType.SysUInt)] uint n, int flags);

        [DllImport(LibraryName)]
        public static extern int write(int fd, System.IntPtr buf, [MarshalAsAttribute(UnmanagedType.SysUInt)] uint n);

        [DllImport(LibraryName)]
        public static extern int setsockopt(int fd, int level, int optname, System.IntPtr optval, int optlen);

        [DllImport(LibraryName)] // windows closesocket()
        public static extern int close(int fd);

        [DllImport(LibraryName)]
        public static extern uint inet_addr(string cp);
        #endregion dll import

        private int mSocket;
        private uint mBufferHeadSize;
        private IntPtr mBufferHead;

        public NetworkCppClient(string ip, int port) : base(ip, port)
        {

        }

        protected override void Connect()
        {
            try
            {
                {
                    int AF_INET = 2;
                    int SOCK_STREAM = 1;
                    mSocket = socket(AF_INET, SOCK_STREAM, 0);
                }
                if (mSocket != -1)
                {
                    int conn = -1;
                    sockaddr_in addr;
                    {
                        int SOL_SOCKET = 1;
                        int SO_KEEPALIVE = 9;
                        int SO_REUSEADDR = 2;
                        int IPPROTO_TCP = 6;
                        int TCP_NODELAY = 1;
                        int size = 4;
                        IntPtr value = Marshal.AllocHGlobal(size);
                        Marshal.StructureToPtr(1, value, true);
                        int opt = setsockopt(mSocket, SOL_SOCKET, SO_KEEPALIVE, value, size);
                        opt = setsockopt(mSocket, SOL_SOCKET, SO_REUSEADDR, value, size);
                        opt = setsockopt(mSocket, IPPROTO_TCP, TCP_NODELAY, value, size);
                        Marshal.FreeHGlobal(value);
                    }
                    {
                        addr = new sockaddr_in();
                        addr.sin_family = (short)AddressFamily.InterNetwork;
                        addr.sin_addr = new in_addr();
                        addr.sin_addr.s_addr = inet_addr(mIP);
                        addr.sin_port = (ushort)System.Net.IPAddress.HostToNetworkOrder((short)mPort);
                    }
                    {
                        int len = Marshal.SizeOf(addr);
                        IntPtr addPtr = Marshal.AllocHGlobal(len);
                        Marshal.StructureToPtr(addr, addPtr, true);
                        conn = connect(mSocket, addPtr, len);
                        Marshal.FreeHGlobal(addPtr);
                    }
                    if (conn != -1)
                    {
                        SetConnectState(NetworkConnectState.Connectted);
                    }
                    else
                    {
                        SetConnectState(NetworkConnectState.Disconnected);
                    }
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log(InfoType.Warning, e.Message);
                SetConnectState(NetworkConnectState.Disconnected);
            }
        }
        protected override void Send(byte[] msg)
        {
            if (msg != null && msg.Length > 0)
            {
                IntPtr ptr = Marshal.AllocHGlobal(msg.Length);
                Marshal.Copy(msg, 0, ptr, msg.Length);
                if (send(mSocket, ptr, (uint)msg.Length, 0) == -1)
                {
                    SetConnectState(NetworkConnectState.Disconnected);
                }
                Marshal.FreeHGlobal(ptr);
            }
        }
        protected override void Close()
        {
            if (mSocket != -1)
            {
                close(mSocket);
                mSocket = -1;
            }
        }
        public override void Stop()
        {
            base.Stop();
            if (mBufferHead != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(mBufferHead);
                mBufferHead = IntPtr.Zero;
            }
        }
        protected override bool ProcessHead()
        {
            bool flag = Receive(ref mBufferHead, (int)mBufferHeadSize);
            if (flag)
            {
                byte[] bytes = new byte[mBufferHeadSize];
                Marshal.Copy(mBufferHead, bytes, 0, (int)mBufferHeadSize);
                mHead.InitializeBy(bytes);
            }
            return flag;
        }
        protected override bool ProcessContent()
        {
            bool flag = true;
            mContents = null; // reset
            if (mHead.mLength > 0)
            {
                IntPtr buffContent = Marshal.AllocHGlobal(mHead.mLength);
                flag = Receive(ref buffContent, mHead.mLength);
                if (flag)
                {
                    mContents = new byte[mHead.mLength];
                    Marshal.Copy(buffContent, mContents, 0, mHead.mLength);
                }
                Marshal.FreeHGlobal(buffContent);
            }
            return flag;
        }
        private bool Receive(ref IntPtr ptr, int total)
        {
            int len = 0;
            do
            {
                int size = recv(mSocket, ptr, (uint)(total - len), 0);
                if (size == -1 || size == 0)
                {
                    Debug.Log("receive 0 -1");
                    SetConnectState(NetworkConnectState.Disconnected);
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
            mSocket = -1;
            mHead = new NetworkHeadFormat();
            mBufferHeadSize = (uint)NetworkHeadFormat.Size();
            mBufferHead = Marshal.AllocHGlobal((int)mBufferHeadSize);
        }
    }
}
