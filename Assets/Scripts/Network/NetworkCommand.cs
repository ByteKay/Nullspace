
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Nullspace
{
    public abstract class NetworkCommand
    {
        public abstract void HandlePacket(NetworkPacket packet);

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CommandTypeAttribute : Attribute
    {
        public CommandTypeAttribute(int id, string desc)
        {
            Id = id;
            Description = desc;
        }
        public CommandTypeAttribute()
        {

        }
        public int Id { get; set; }

        public string Description { get; set; }

    }

    public class NetworkCommandFactory
    {
        private static Dictionary<int, Type> mAllCommandClasses = new Dictionary<int, Type>();
        private static bool isRegistered = false;

        public static void RegisterCommand()
        {
            if (!isRegistered)
            {
                mAllCommandClasses.Add(CommandType.HeartCodec, typeof(HeartCommand));
                mAllCommandClasses.Add(CommandType.GM, typeof(GMCommand));
                isRegistered = true;
            }
        }
        public static void RegisterCommand(string spacename, Assembly ass)
        {
            if (!isRegistered)
            {
                var types = ass.GetTypes();
                foreach (var item in types)
                {
                    if (item.Namespace == spacename)
                    {
                        var type = item.BaseType;
                        while (type != null)
                        {
                            if (type == typeof(NetworkCommand))
                            {
                                CommandTypeAttribute attr = CommandTypeAttribute.GetCustomAttribute(item, typeof(CommandTypeAttribute), false) as CommandTypeAttribute;
                                if (!mAllCommandClasses.ContainsKey(attr.Id))
                                {
                                    mAllCommandClasses.Add(attr.Id, item);
                                }
                                break;
                            }
                            else
                            {
                                type = type.BaseType;
                            }
                        }
                    }
                }
                isRegistered = true;
            }
        }

        public static NetworkCommand GetCommand(int mid)
        {
            if (mAllCommandClasses.ContainsKey(mid))
            {
                return (NetworkCommand)Activator.CreateInstance(mAllCommandClasses[mid]);
            }
            return null;
        }
    }

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
