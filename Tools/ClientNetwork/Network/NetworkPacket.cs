namespace Nullspace
{
    public class NetworkPacket
    {
        public NetworkHead mHead;
        public byte[] mContent;

        public NetworkClient mClient;

        public NetworkPacket()
        {

        }

        public NetworkPacket(NetworkHead head, byte[] content, NetworkClient client)
        {
            mHead = head;
            mContent = content;
            mClient = client;
        }

        public byte[] GetBytes()
        {
            return mHead.Merge(mContent);
        }

        public int CommandId { get { return mHead.mType; } }
    }

}

