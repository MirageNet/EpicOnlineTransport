using Epic.OnlineServices;

namespace EpicChill.Transport
{
    public struct EpicMessage
    {
        public readonly InternalMessage EventType;
        public readonly byte[] Data;
        public readonly ProductUserId ConnectionId;
        public readonly int Channel;

        public EpicMessage(ProductUserId connectionId,int channel, InternalMessage eventType, byte[] data)
        {
            EventType = eventType;
            Data = data;
            ConnectionId = connectionId;
            Channel = channel;
        }
    }
}
