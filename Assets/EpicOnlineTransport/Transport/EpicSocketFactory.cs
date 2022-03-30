using System;
using System.Net;
using Epic.Logging;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using Mirage.Logging;
using Mirage.SocketLayer;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;

namespace Mirage.Sockets.Epic
{
    [Serializable]
    public class EpicOptions
    {
        public int ConnectionTimeOut;

        // todo do we need this in inspector?
        public string SocketName = "MirageGame";

        // todo need to get this from mirage
        public int MaxConnections = 10;
    }

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

        private void OnValidate()
        {
            string socketName = Guid.NewGuid().ToString("XXXXXXXXXXXXXXXXXXXX");
            Debug.Log(socketName);
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

    [Serializable]
    public class EpicSocketException : Exception
    {
        public EpicSocketException(string message) : base(message) { }
    }

    internal sealed class EpicSocket : ISocket
    {
        private readonly EpicOptions _epicOptions;
        ulong _notificationID;

        public EpicSocket(EpicOptions options)
        {
            DebugLogger.RegularDebugLog("[EpicSocket] - Staring up socket.");

            _epicOptions = options;
        }

        void ThrowIfActive()
        {
            if (_notificationID != 0)
                throw new InvalidOperationException("Socket already bound");

        }

        public void Bind(IEndPoint endPoint)
        {
            ThrowIfActive();
            _notificationID = EpicHelper_Server.Bind(_epicOptions.SocketName);
        }

        public void Connect(IEndPoint endPoint)
        {
            ThrowIfActive();
            throw new NotImplementedException();
        }

        public void Close()
        {
            EpicHelper_Server.Unbind(ref _notificationID);
            // todo close client
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

    public static class EpicHelper_Client
    {
        // change default log level based on if we are in debug or release mode.
        // this is only default, if there are log settings they will be used instead of these
#if DEBUG
        const LogType DEFAULT_LOG = LogType.Warning;
#else
        const LogType DEFAULT_LOG = LogType.Error;
#endif
        static readonly ILogger logger = LogFactory.GetLogger(typeof(EpicHelper_Client), DEFAULT_LOG);

        public static void Connect(string address)
        {
            // random name length 20
            string socketName = Guid.NewGuid().ToString("XXXXXXXXXXXXXXXXXXXX");
            var socketId = new SocketId() { SocketName = socketName };
        }
    }

    public static class EpicHelper_Server
    {
        // change default log level based on if we are in debug or release mode.
        // this is only default, if there are log settings they will be used instead of these
#if DEBUG
        const LogType DEFAULT_LOG = LogType.Warning;
#else
        const LogType DEFAULT_LOG = LogType.Error;
#endif
        static readonly ILogger logger = LogFactory.GetLogger(typeof(EpicHelper_Server), DEFAULT_LOG);

        /// <summary>
        /// Starts relay as server, allows new connections
        /// </summary>
        /// <param name="notifyId"></param>
        /// <param name="socketName"></param>
        public static ulong Bind(string socketName)
        {
            var socketId = new SocketId()
            {
                SocketName = socketName
            };

            var options = new AddNotifyPeerConnectionRequestOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                SocketId = socketId
            };

            P2PInterface p2p = EOSManager.Instance.GetEOSP2PInterface();
            ulong notifyId = p2p.AddNotifyPeerConnectionRequest(options, null, data => TryAcceptNewConnection(options, data));

            if (notifyId == Common.InvalidNotificationid)
                throw new EpicSocketException("Failed to bind: Invalid notification id");

            return notifyId;
        }

        public static void Unbind(ref ulong notifyId)
        {
            if (notifyId == Common.InvalidNotificationid)
                return;

            P2PInterface p2p = EOSManager.Instance.GetEOSP2PInterface();
            p2p.RemoveNotifyPeerConnectionRequest(notifyId);
            // clear value after removing
            notifyId = 0;
        }

        private static void TryAcceptNewConnection(AddNotifyPeerConnectionRequestOptions localOptions, OnIncomingConnectionRequestInfo data)
        {
            // just log warnings for fail before this is on server, some connections are allowed to fail

            // use Equals because of IEquatable
            if (localOptions.LocalUserId.Equals(data.LocalUserId))
            {
                if (logger.WarnEnabled()) logger.LogWarning("LocalUserId not equal");
                return;
            }

            if (localOptions.SocketId.SocketName != data.SocketId.SocketName)
            {
                if (logger.WarnEnabled()) logger.LogWarning("SocketId not equal");
                return;
            }

            AcceptNewConnection(data.SocketId, data.RemoteUserId);
        }

        /// <summary>
        /// Accepts a new connect from anther peer
        /// </summary>
        /// <param name="socketId"></param>
        /// <param name="remoteUser"></param>
        static void AcceptNewConnection(SocketId socketId, ProductUserId remoteUser)
        {
            var options = new AcceptConnectionOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = remoteUser,
                SocketId = socketId
            };

            P2PInterface p2p = EOSManager.Instance.GetEOSP2PInterface();
            Result result = p2p.AcceptConnection(options);
            if (result != Result.Success)
                if (logger.WarnEnabled()) logger.LogWarning($"Failed to AcceptConnection with result:{result}");

            // todo do something with new user here?
        }
    }
}

