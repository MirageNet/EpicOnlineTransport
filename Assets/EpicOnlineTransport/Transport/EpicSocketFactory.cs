using System;
using System.Net;
using Epic.Logging;
using Epic.OnlineServices.P2P;
using Mirage.SocketLayer;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;

namespace EpicServicesPeer
{
    [RequireComponent(typeof(EOSManager))]
    [DisallowMultipleComponent]
    public class EpicSocketFactory : SocketFactory
    {
        [SerializeField] private EpicOptions _options = new EpicOptions();

        /// <summary>Max size for packets sent to or received from Socket
        /// <para>Called once when Sockets are created</para></summary>
        public override int MaxPacketSize => 1200;

        /// <summary>Creates a <see cref="ISocket"/> to be used by <see cref="Peer"/> on the server</summary>
        /// <exception cref="NotSupportedException">Throw when Server is not supported on current platform</exception>
        public override ISocket CreateServerSocket()
        {
            return new EpicSocket(_options);
        }

        /// <summary>Creates the <see cref="EndPoint"/> that the Server Socket will bind to</summary>
        /// <exception cref="NotSupportedException">Throw when Client is not supported on current platform</exception>
        public override IEndPoint GetBindEndPoint()
        {
            return new EpicEndPoint();
        }

        /// <summary>Creates a <see cref="ISocket"/> to be used by <see cref="Peer"/> on the client</summary>
        /// <exception cref="NotSupportedException">Throw when Client is not supported on current platform</exception>
        public override ISocket CreateClientSocket()
        {
            return new EpicSocket(_options);
        }

        /// <summary>Creates the <see cref="EndPoint"/> that the Client Socket will connect to using the parameter given</summary>
        /// <exception cref="NotSupportedException">Throw when Client is not supported on current platform</exception>
        public override IEndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            return new EpicEndPoint(address);
        }
    }

    internal sealed class EpicEndPoint : IEndPoint
    {
        public string Address;

        public EpicEndPoint() { }
        public EpicEndPoint(string address)
        {
            Address = address;
        }

        IEndPoint IEndPoint.CreateCopy()
        {
            return new EpicEndPoint(Address);
        }
    }
    internal sealed class EpicSocket : ISocket
    {
        private readonly EpicOptions _epicOptions;
        private readonly P2PInterface _p2p;

        public EpicSocket(EpicOptions options)
        {
            DebugLogger.RegularDebugLog("[EpicSocket] - Staring up socket.");

            _epicOptions = options;
            _p2p = EOSManager.Instance.GetEOSP2PInterface();
        }

        public void Bind(IEndPoint endPoint)
        {
            // _p2p.
        }

        public void Connect(IEndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public bool Poll()
        {
            throw new NotImplementedException();
        }

        public int Receive(byte[] buffer, out IEndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public void Send(IEndPoint endPoint, byte[] packet, int length)
        {
            throw new NotImplementedException();
        }
    }
}
