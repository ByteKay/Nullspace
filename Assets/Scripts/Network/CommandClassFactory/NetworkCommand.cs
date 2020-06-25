
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

}
