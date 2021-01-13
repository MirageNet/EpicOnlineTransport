#region Statements

using System;
using System.Collections.Concurrent;
using System.Threading;
using Cysharp.Threading.Tasks;
using Epic.Core;
using Epic.Logging;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using EpicChill.Transport;

#endregion

namespace EpicTransport
{
    public abstract class Common
    {
        #region Fields

        protected const string SocketName = "SOCKETID";

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly OnIncomingConnectionRequestCallback _onIncomingConnectionRequest;
        private readonly OnRemoteConnectionClosedCallback _onRemoteConnectionClosed;
        protected EpicOptions Options;
        protected EpicTransport Transport;
        protected readonly EpicManager EpicManager;
        internal readonly ConcurrentQueue<EpicMessage> QueuedData = new ConcurrentQueue<EpicMessage>();

        public Action<Result, string> Error;

        public bool Connected => _cancellationToken.IsCancellationRequested != true;

        #endregion

        #region Class Specific

        protected Common(EpicTransport transport, EpicOptions options)
        {
            Transport = transport;
            EpicManager = Transport.EpicManager;
            Options = options;

            AddNotifyPeerConnectionRequestOptions addNotifyPeerConnectionRequestOptions =
                new AddNotifyPeerConnectionRequestOptions {LocalUserId = EpicManager.AccountId.ProductUserId};

            SocketId socketId = new SocketId {SocketName = SocketName};

            addNotifyPeerConnectionRequestOptions.SocketId = socketId;

            _onIncomingConnectionRequest += OnNewConnection;
            _onRemoteConnectionClosed += OnConnectionFailed;

            EpicManager.P2PInterface.AddNotifyPeerConnectionRequest(addNotifyPeerConnectionRequestOptions,
                null, _onIncomingConnectionRequest);

            AddNotifyPeerConnectionClosedOptions addNotifyPeerConnectionClosedOptions =
                new AddNotifyPeerConnectionClosedOptions
                {
                    LocalUserId = EpicManager.AccountId.ProductUserId, SocketId = socketId
                };

            EpicManager.P2PInterface.AddNotifyPeerConnectionClosed(addNotifyPeerConnectionClosedOptions,
                null, _onRemoteConnectionClosed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        protected abstract void OnNewConnection(OnIncomingConnectionRequestInfo result);

        /// <summary>
        ///     Connection request has failed to connect to user.
        /// </summary>
        /// <param name="result"></param>
        protected virtual void OnConnectionFailed(OnRemoteConnectionClosedInfo result)
        {
            CloseP2PSessionWithUser(result.RemoteUserId);

            _cancellationToken.Cancel();

            switch (result.Reason)
            {
                case ConnectionClosedReason.ClosedByLocalUser:
                    throw new Exception("Connection closed: The Connection was gracefully closed by the local user.");
                case ConnectionClosedReason.ClosedByPeer:
                    throw new Exception("Connection closed: The connection was gracefully closed by remote user.");
                case ConnectionClosedReason.ConnectionClosed:
                    throw new Exception("Connection closed: The connection was unexpectedly closed.");
                case ConnectionClosedReason.ConnectionFailed:
                    throw new Exception("Connection failed: Failed to establish connection.");
                case ConnectionClosedReason.InvalidData:
                    throw new Exception("Connection failed: The remote user sent us invalid data..");
                case ConnectionClosedReason.InvalidMessage:
                    throw new Exception("Connection failed: The remote user sent us an invalid message.");
                case ConnectionClosedReason.NegotiationFailed:
                    throw new Exception("Connection failed: Negotiation failed.");
                case ConnectionClosedReason.TimedOut:
                    throw new Exception("Connection failed: Timeout.");
                case ConnectionClosedReason.TooManyConnections:
                    throw new Exception("Connection failed: Too many connections.");
                case ConnectionClosedReason.UnexpectedError:
                    throw new Exception("Unexpected Error, connection will be closed");
                case ConnectionClosedReason.Unknown:
                default:
                    throw new Exception("Unknown Error, connection has been closed.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientUserID"></param>
        protected void CloseP2PSessionWithUser(ProductUserId clientUserID)
        {
            EpicManager.P2PInterface.CloseConnection(
                new CloseConnectionOptions
                {
                    LocalUserId = EpicManager.AccountId.ProductUserId,
                    RemoteUserId = clientUserID,
                    SocketId = new SocketId
                    {
                        SocketName = SocketName
                    }
                });
        }

        /// <summary>
        ///     Send an internal message through system.
        /// </summary>
        /// <param name="target">The steam person we are sending internal message to.</param>
        /// <param name="type">The type of <see cref="InternalMessage"/> we want to send.</param>
        internal bool SendInternal(ProductUserId target, InternalMessage type)
        {
            bool sent = EpicManager.P2PInterface.SendPacket(new SendPacketOptions
            {
                AllowDelayedDelivery = true,
                Channel = (byte)Options.Channels.Length,
                Data = new[] {(byte)type},
                LocalUserId = EpicManager.AccountId.ProductUserId,
                Reliability = PacketReliability.ReliableOrdered,
                RemoteUserId = target,
                SocketId = new SocketId
                {
                    SocketName = SocketName
                }
            }) == Result.Success;

            if (sent)
            {
                if (Transport.transportDebug)
                    DebugLogger.RegularDebugLog("[Client] - Packet sent successfully.");
            }
            else
            {
                if (Transport.transportDebug)
                    DebugLogger.RegularDebugLog("[Client] - Packet failed to send.");
            }

            return sent;
        }

        /// <summary>
        ///     Check to see if we have received any data from steam users.
        /// </summary>
        /// <param name="clientProductUserId">Returns back the steam id of users who sent message.</param>
        /// <param name="receiveBuffer">The data that was sent to use.</param>
        /// <param name="channel">The channel the data was sent on.</param>
        /// <returns></returns>
        protected bool DataAvailable(out ProductUserId clientProductUserId, out byte[] receiveBuffer, byte channel)
        {
            Result result = EpicManager.P2PInterface.ReceivePacket(new ReceivePacketOptions
            {
                LocalUserId = EpicManager.AccountId.ProductUserId,
                MaxDataSizeBytes = P2PInterface.MaxPacketSize,
                RequestedChannel = channel
            }, out clientProductUserId, out _, out _, out receiveBuffer);

            if (result == Result.Success)
            {
                return true;
            }

            receiveBuffer = null;
            clientProductUserId = null;

            return false;
        }

        public virtual void Disconnect()
        {
            _cancellationToken?.Cancel();
        }

        /// <summary>
        ///     Update method to be called by the transport.
        /// </summary>
        protected async void ProcessIncomingMessages()
        {
            while (Connected)
            {
                while (DataAvailable(out ProductUserId clientUserID, out byte[] internalMessage,
                    (byte)Options.Channels.Length))
                {
                    if (internalMessage.Length == 1)
                    {
                        OnReceiveInternalData((InternalMessage)internalMessage[0], clientUserID);
                    }

                    if (Transport.transportDebug)
                        DebugLogger.RegularDebugLog("[Client] - Incorrect package length on internal channel.");
                }

                for (int chNum = 0; chNum < Options.Channels.Length; chNum++)
                {
                    while (DataAvailable(out ProductUserId clientUserID, out byte[] receiveBuffer, (byte)chNum))
                    {
                        OnReceiveData(receiveBuffer, clientUserID, chNum);
                    }
                }

                await UniTask.Delay(1);
            }
        }

        /// <summary>
        ///     Process our internal messages away from mirror.
        /// </summary>
        /// <param name="type">The <see cref="InternalMessage"/> type message we received.</param>
        /// <param name="clientEpicId">The client id which the internal message came from.</param>
        protected abstract void OnReceiveInternalData(InternalMessage type, ProductUserId clientEpicId);

        /// <summary>
        ///     Process data incoming from steam backend.
        /// </summary>
        /// <param name="data">The data that has come in.</param>
        /// <param name="clientEpicId">The client the data came from.</param>
        /// <param name="channel">The channel the data was received on.</param>
        internal abstract void OnReceiveData(byte[] data, ProductUserId clientEpicId, int channel);

        #endregion
    }
}
