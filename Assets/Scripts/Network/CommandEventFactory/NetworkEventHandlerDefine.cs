

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
            DebugUtils.Info("HandleHeartEvent",  "CommandType: ", packet.mHead.mType);
        }
    }
}
