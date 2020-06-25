
using UnityEngine;

namespace Nullspace
{

    [NetworkCommandTypeAttributeAttribute(NetworkCommandType.HeartCodec, "心跳")]
    public class HeartCommand : NetworkCommand
    {
        public override void HandlePacket(NetworkPacket packet)
        {
            Debug.Log("received: " + packet.mHead.mType);
        }
    }

    [NetworkCommandTypeAttributeAttribute(NetworkCommandType.GM, "中控")]
    public class GMCommand : NetworkCommand
    {
        public override void HandlePacket(NetworkPacket packet)
        {
            Debug.Log("received: " + packet.mHead.mAddition);
        }
    }
}
