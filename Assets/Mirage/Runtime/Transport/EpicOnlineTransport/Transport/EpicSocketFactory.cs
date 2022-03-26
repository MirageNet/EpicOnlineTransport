using System;
using System.Net;
using Mirage.SocketLayer;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;

namespace EpicTransport
{
    [RequireComponent(typeof(EOSManager))]
    [DisallowMultipleComponent]
    public class EpicSocketFactory : SocketFactory
    {
        #region Overrides of SocketFactory

        /// <summary>Max size for packets sent to or received from Socket
        /// <para>Called once when Sockets are created</para></summary>
        public override int MaxPacketSize => 1200;

        /// <summary>Creates a <see cref="ISocket"/> to be used by <see cref="Peer"/> on the server</summary>
        /// <exception cref="NotSupportedException">Throw when Server is not supported on current platform</exception>
        public override ISocket CreateServerSocket()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Creates the <see cref="EndPoint"/> that the Server Socket will bind to</summary>
        /// <exception cref="NotSupportedException">Throw when Client is not supported on current platform</exception>
        public override IEndPoint GetBindEndPoint()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Creates a <see cref="ISocket"/> to be used by <see cref="Peer"/> on the client</summary>
        /// <exception cref="NotSupportedException">Throw when Client is not supported on current platform</exception>
        public override ISocket CreateClientSocket()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Creates the <see cref="EndPoint"/> that the Client Socket will connect to using the parameter given</summary>
        /// <exception cref="NotSupportedException">Throw when Client is not supported on current platform</exception>
        public override IEndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
