

namespace Nullspace
{
    public partial class NetworkEventHandler
    {

        private void RegisterCommandEvent()
        {
            IntEventDispatcher.AddEventListener<NetworkPacket>(NetworkCommandType.HeartCodec, HandleHeartEvent);
        }

        private void UnregisterCommandEvent()
        {
            IntEventDispatcher.RemoveEventListener<NetworkPacket>(NetworkCommandType.HeartCodec, HandleHeartEvent);
        }

        private void HandleHeartEvent(NetworkPacket packet)
        {
            DebugUtils.Log(InfoType.Info,  "CommandType: " + packet.mHead.mType);
        }
    }
}
