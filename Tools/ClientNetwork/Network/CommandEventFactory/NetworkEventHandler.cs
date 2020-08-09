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
    public partial class NetworkEventHandler : Singleton<NetworkEventHandler>
    {
        private object mLock = new object();
        private Queue<NetworkPacket> mCommandPacket;

        private void Awake()
        {
            mCommandPacket = new Queue<NetworkPacket>();
        }

        public void Initialize()
        {
            RegisterCommandEvent();
        }

        public void AddPacket(NetworkPacket packet)
        {
            if (!IsDestroy())
            {
                lock (mLock)
                {
                    if (!IsDestroy())
                    {
                        mCommandPacket.Enqueue(packet);
                    }
                }
            }
        }

        public void Update()
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterCommandEvent();
            lock (mLock)
            {
                mCommandPacket.Clear();
            }
        }


    }


}
