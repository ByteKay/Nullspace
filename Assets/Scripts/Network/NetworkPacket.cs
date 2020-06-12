namespace Nullspace
{
    public class NetworkPacket
    {
        public NetworkHeadFormat mHead;
        public byte[] mContent;
        public AbstractNetworkClient mClient;
        public NetworkPacket()
        {

        }
        public NetworkPacket(NetworkHeadFormat head, byte[] content, AbstractNetworkClient client)
        {
            mHead = head;
            mContent = content;
            mClient = client;
        }

        public byte[] GetBytes()
        {
            return mHead.Merge(mContent);
        }
    }

}

