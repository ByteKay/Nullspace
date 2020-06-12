
using UnityEngine;

namespace Nullspace
{
    public static class CommandType
    {
        public const int HeartCodec = 1;
        public const int GM = 6000;

        public const int GMTerrain = 6001;
        public const int GMEffect = 6002;
        public const int GMPet = 6003;
        public const int GMRemove = 6004;
        
    }

    [CommandTypeAttribute(CommandType.HeartCodec, "心跳")]
    public class HeartCommand : NetworkCommand
    {
        public override void HandlePacket(NetworkPacket packet)
        {
            Debug.Log("received: " + packet.mHead.mType);
        }
    }

    [CommandTypeAttribute(CommandType.GM, "中控")]
    public class GMCommand : NetworkCommand
    {
        public override void HandlePacket(NetworkPacket packet)
        {
            Debug.Log("received: " + packet.mHead.mAddition);
        }
    }
}
