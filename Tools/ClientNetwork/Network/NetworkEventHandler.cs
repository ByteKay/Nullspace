﻿
using System.Collections.Generic;

namespace Nullspace
{
    /// <summary>
    /// should initialize firstly
    /// </summary>
    public partial class NetworkEventHandler
    {
        private static object mLock = new object();
        private static Queue<NetworkPacket> mCommandPacket = new Queue<NetworkPacket>();

        public static void Initialize()
        {
            RegisterCommandEvent();
        }

        public static void AddPacket(NetworkPacket packet)
        {
            lock (mLock)
            {
                mCommandPacket.Enqueue(packet);
            }
        }

        public static void Update()
        {
            NetworkPacket packet = null;
            lock (mLock)
            {
                while (mCommandPacket.Count > 0)
                {
                    packet = mCommandPacket.Dequeue();
                    if (packet != null)
                    {
                        IntEventDispatcher.TriggerEvent(packet.CommandId, packet);
                    }
                }
            }
        }

        public static void Clear()
        {
            UnregisterCommandEvent();
            lock (mLock)
            {
                mCommandPacket.Clear();
            }
        }
        private static void RegisterCommandEvent()
        {
            IntEventDispatcher.AddEventListener<NetworkPacket>(NetworkCommandType.HeartCodec, HandleHeartEvent);
        }

        private static void UnregisterCommandEvent()
        {
            IntEventDispatcher.RemoveEventListener<NetworkPacket>(NetworkCommandType.HeartCodec, HandleHeartEvent);
        }

        private static void HandleHeartEvent(NetworkPacket packet)
        {
            DebugUtils.Log(InfoType.Info, "CommandType: " + packet.mHead.mType);
        }
        
    }


}
