#region Statements

using System;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

#endregion

namespace EpicTransport
{
    [Serializable]
    public struct EpicOptions
    {
        public int ConnectionTimeOut;
        public int MaxConnections;
        public ProductUserId ConnectionAddress;
        [NonSerialized] public PacketReliability[] Channels;
    }
}
