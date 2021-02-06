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

        protected readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();
        private readonly OnIncomingConnectionRequestCallback _onIncomingConnectionRequest;
        private readonly OnRemoteConnectionClosedCallback _onRemoteConnectionClosed;
        private  readonly ulong _incomingNotificationId, _outgoingNotificationId;
        protected EpicOptions Options;
        protected EpicTransport Transport;
        protected readonly EpicManager EpicManager;

        public Action<Result, string> Error;

        public bool Connected => CancellationToken.IsCancellationRequested != true;

        #endregion

        #region Class Specific

        protected Common(EpicTransport transport, EpicOptions options)
        {
            Transport = transport;
            EpicManager = Transport.EpicManager;
            Options = options;
            
            AddNotifyPeerConnectionRequestOptions addNotifyPeerConnectionRequestOptions =
                new AddNotifyPeerConnectionRequestOptions {LocalUserId = EpicManager.AccountId.ProductUserId};

            _onIncomingConnectionRequest += OnNewConnection;
            _onRemoteConnectionClosed += OnConnectionFailed;

            _incomingNotificationId = EpicManager.P2PInterface.AddNotifyPeerConnectionRequest(addNotifyPeerConnectionRequestOptions,
                null, _onIncomingConnectionRequest);

            AddNotifyPeerConnectionClosedOptions addNotifyPeerConnectionClosedOptions =
                new AddNotifyPeerConnectionClosedOptions
                {
                    LocalUserId = EpicManager.AccountId.ProductUserId
                };

            _outgoingNotificationId = EpicManager.P2PInterface.AddNotifyPeerConnectionClosed(addNotifyPeerConnectionClosedOptions,
                null, _onRemoteConnectionClosed);
        }

        /// <summary>
        ///     Override this to do something when epic sends us a new connection request.
        /// </summary>
        /// <param name="result">The data coming in with new connection request.</param>
        protected abstract void OnNewConnection(OnIncomingConnectionRequestInfo result);

        /// <summary>
        ///     Connection request has failed to connect user.
        /// </summary>
        /// <param name="result">The information coming back from failed connection.</param>
        protected virtual void OnConnectionFailed(OnRemoteConnectionClosedInfo result)
        {
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
        /// <param name="socket"></param>
        protected void CloseP2PSessionWithUser(ProductUserId clientUserID, SocketId socket)
        {
            EpicManager.P2PInterface.CloseConnection(
                new CloseConnectionOptions
                {
                    LocalUserId = EpicManager.AccountId.ProductUserId,
                    RemoteUserId = clientUserID,
                    SocketId = socket
                });
        }

        /// <summary>
        ///     Send an internal message through system.
        /// </summary>
        /// <param name="target">The steam person we are sending internal message to.</param>
        /// <param name="type">The type of <see cref="InternalMessage"/> we want to send.</param>
        internal bool SendInternal(ProductUserId target, InternalMessage type, SocketId socket)
        {
            Result sent = EpicManager.P2PInterface.SendPacket(new SendPacketOptions
            {
                AllowDelayedDelivery = true,
                Channel = (byte)Options.Channels.Length,
                Data = new[] {(byte)type},
                LocalUserId = EpicManager.AccountId.ProductUserId,
                Reliability = PacketReliability.ReliableOrdered,
                RemoteUserId = target,
                SocketId = socket
            });

            if (sent == Result.Success)
            {
                if (Transport.transportDebug)
                    DebugLogger.RegularDebugLog("[Client] - Packet sent successfully.");
            }
            else
            {
                if (Transport.transportDebug)
                    DebugLogger.RegularDebugLog($"[Client] - Packet failed to send. Results: {sent}");
            }

            return sent == Result.Success;
        }

        /// <summary>
        ///     Check to see if we have received any data from epic users.
        /// </summary>
        /// <param name="clientProductUserId">Returns back the epic id of users who sent message.</param>
        /// <param name="receiveBuffer">The data that was sent to use.</param>
        /// <param name="channel">The channel the data was sent on.</param>
        /// <returns></returns>
        protected bool DataAvailable(out ProductUserId clientProductUserId, out byte[] receiveBuffer, byte channel, out SocketId socket)
        {
            Result result = EpicManager.P2PInterface.ReceivePacket(new ReceivePacketOptions
            {
                LocalUserId = EpicManager.AccountId.ProductUserId,
                MaxDataSizeBytes = P2PInterface.MaxPacketSize,
                RequestedChannel = channel
            }, out clientProductUserId, out socket, out _, out receiveBuffer);

            if (result == Result.Success)
            {
                return true;
            }

            receiveBuffer = null;
            clientProductUserId = null;

            return false;
        }

        protected void Dispose()
        {
            EpicManager.P2PInterface.RemoveNotifyPeerConnectionRequest(_incomingNotificationId);
            EpicManager.P2PInterface.RemoveNotifyPeerConnectionClosed(_outgoingNotificationId);
        }

        /// <summary>
        ///     Cleanup before we finalize disconnection.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        ///     Update method to be called by the transport.
        /// </summary>
        protected async void ProcessIncomingMessages()
        {
            while (Connected)
            {
                while (DataAvailable(out ProductUserId clientUserID, out byte[] internalMessage,
                    (byte)Options.Channels.Length, out SocketId socket))
                {
                    if (internalMessage.Length == 1)
                    {
                        OnReceiveInternalData((InternalMessage)internalMessage[0], clientUserID, socket);
                    }

                    if (Transport.transportDebug)
                        DebugLogger.RegularDebugLog("[Client] - Incorrect package length on internal channel.");
                }

                for (int chNum = 0; chNum < Options.Channels.Length; chNum++)
                {
                    while (DataAvailable(out ProductUserId clientUserID, out byte[] receiveBuffer, (byte)chNum, out SocketId _))
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
        /// <param name="socket">The socket we want to process internal data on.</param>
        protected abstract void OnReceiveInternalData(InternalMessage type, ProductUserId clientEpicId, SocketId socket);

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
