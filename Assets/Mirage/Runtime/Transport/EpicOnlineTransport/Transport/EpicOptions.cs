#region Statements

using System;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using UnityEngine;

#endregion

namespace EpicServicesPeer
{
    [Serializable]
    public struct EpicOptions
    {
        public int ConnectionTimeOut;
        public int MaxConnections;
        [Tooltip("New connection attempt on server socket id to use.")] public string SocketConnectionID;
        public ProductUserId ConnectionAddress;
        [NonSerialized] public PacketReliability[] Channels;
    }
}
