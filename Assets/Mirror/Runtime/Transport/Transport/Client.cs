#region Statements

using System;
using System.IO;
using System.Net;
using Cysharp.Threading.Tasks;
using Epic.Logging;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using EpicChill.Transport;
using Mirror;
using UnityEngine;
using Channel = Mirror.Channel;

#endregion

namespace EpicTransport
{
    public class Client : Common, IConnection
    {
        #region Fields

        private static readonly ILogger Logger = LogFactory.GetLogger(typeof(Client));
        internal SocketId SocketName;
        private byte[] _clientSendPoolData;
        private AutoResetUniTaskCompletionSource _connectedComplete;
        private bool _serverControlled = false;

        #endregion

        #region Class Specific

        /// <summary>
        ///     Connect to server.
        /// </summary>
        /// <returns></returns>
        public async UniTask ConnectAsync()
        {
            Options.ConnectionAddress.ToString(out string productId);

#if UNITY_EDITOR
            if (Logger.logEnabled)
#endif
                if (Transport.transportDebug)
                    DebugLogger.RegularDebugLog($"[Client] - attempting connection to {productId}");

            try
            {
                // Send a message to server to initiate handshake connection
                SendInternal(Options.ConnectionAddress, InternalMessage.Connect, SocketName);

                _connectedComplete = AutoResetUniTaskCompletionSource.Create();

                while (
                    await UniTask.WhenAny(_connectedComplete.Task,
                        UniTask.Delay(TimeSpan.FromSeconds(Math.Max(1, Options.ConnectionTimeOut)))) != 0)
                {
#if UNITY_EDITOR
                    if (Logger.logEnabled)
#endif
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                $"[Client] - Connection to {productId} timed out.", LogType.Error);

                    Error.Invoke(Result.LobbyInviteFailed,
                        $"[Client] - Connection to {productId} timed out.");

                    Disconnect();

                    return;
                }

                // Everything went good let's just return.
                // We need to switch to main thread for some reason.
                await UniTask.SwitchToMainThread();

                if (_delayConnection)
                {
                    await UniTask.Delay((int) (_delayConnectionTime * 1000));
                    _delayConnection = false;
                }
            }
            catch (FormatException)
            {
                Error?.Invoke(Result.InvalidProductUserID,
                    $"[Client] - Connection string was not in the correct format.");

#if UNITY_EDITOR
                if (Logger.logEnabled)
#endif
                    if (Transport.transportDebug)
                        DebugLogger.RegularDebugLog(
                            "[Client] - Connection string was not in the right format. Did you enter a ProductId?",
                            LogType.Error);
            }
            catch (Exception ex)
            {
                Error?.Invoke(Result.NotFound, $"Error: {ex.Message}");

#if UNITY_EDITOR
                if (Logger.logEnabled)
#endif
                    if (Transport.transportDebug)
                        DebugLogger.RegularDebugLog($"[Client] - Error: {ex.Message}", LogType.Error);

                Disconnect();
            }
        }

        /// <summary>
        ///     Send data through epic network.
        /// </summary>
        /// <param name="host">The person we want to send data to.</param>
        /// <param name="msgBuffer">The data we are sending.</param>
        /// <param name="channel">The channel we are going to send data on.</param>
        /// <returns></returns>
        private bool Send(ProductUserId host, byte[] msgBuffer, int channel)
        {
            bool sent = EpicManager.P2PInterface.SendPacket(new SendPacketOptions
            {
                AllowDelayedDelivery = true,
                Channel = (byte)channel,
                Data = msgBuffer,
                LocalUserId = EpicManager.AccountId.ProductUserId,
                Reliability = Options.Channels[channel],
                RemoteUserId = host,
                SocketId = SocketName
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

        #endregion

        #region Common Overrides

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="options"></param>
        /// <param name="serverControlled">Is the data being processed by server or client.</param>
        public Client(EpicTransport transport, EpicOptions options, bool serverControlled) : base(transport, options)
        {
            Options = options;
            Transport = transport;
            _serverControlled = serverControlled;

            if (serverControlled) return;

            SocketName = new SocketId {SocketName = Guid.NewGuid().GetHashCode().ToString().Replace("\u2013", "")};

            UniTask.Run(ProcessIncomingMessages).Forget();
        }

        /// <summary>
        ///     New incoming connection from someone. We check to see if its same as in address atm for security.
        /// </summary>
        /// <param name="result">The data we need to process to make a accept the new connection request.</param>
        protected override void OnNewConnection(OnIncomingConnectionRequestInfo result)
        {
            if (Options.ConnectionAddress == result.RemoteUserId)
            {
                EpicManager.P2PInterface.AcceptConnection(
                    new AcceptConnectionOptions
                    {
                        LocalUserId = EpicManager.AccountId.ProductUserId,
                        RemoteUserId = result.RemoteUserId,
                        SocketId = result.SocketId
                    });

                _connectedComplete.TrySetResult();

                SocketName = result.SocketId;
            }
            else
            {
                if (Logger.logEnabled)
                    if (Transport.transportDebug)
                        DebugLogger.RegularDebugLog("[Client] - P2P Acceptance Request from unknown host ID.",
                            LogType.Error);
            }
        }

        /// <summary>
        ///     Overriding this to place cancellation token on connection failed here.
        /// </summary>
        /// <param name="result"></param>
        protected override void OnConnectionFailed(OnRemoteConnectionClosedInfo result)
        {
            base.OnConnectionFailed(result);

            Disconnect();
        }

        /// <summary>
        ///     Internal data received from server.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="clientEpicId"></param>
        /// <param name="socket"></param>
        protected override void OnReceiveInternalData(InternalMessage type, ProductUserId clientEpicId, SocketId socket)
        {
            if (!Connected) return;

            switch (type)
            {
                case InternalMessage.Accept:

                    if (Logger.logEnabled)
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                "[Client] - Received internal message of server accepted our request to connect.");

                    _connectedComplete.TrySetResult();

                    break;

                case InternalMessage.Disconnect:
                    Disconnect();

                    if (Logger.logEnabled)
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                "[Client] - Received internal message to disconnect epic user.");

                    break;

                case InternalMessage.TooManyUsers:

                    if (Logger.logEnabled)
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                "[Client] - Received internal message that there are too many users connected to server.",
                                LogType.Warning);

                    Error.Invoke(Result.LobbyTooManyPlayers, "Too many users currently connected.");

                    break;

                default:

                    if (Logger.logEnabled)
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                $"[Client] - Cannot process internal message {type}. If this is anything other then {InternalMessage.Data} something has gone wrong.",
                                LogType.Warning);
                    break;
            }
        }

        /// <summary>
        ///     Process the data only on this specific client.
        /// </summary>
        /// <param name="data">The data to process.</param>
        /// <param name="clientEpicId">The epic user who sent the data.</param>
        /// <param name="channel">The channel that the data was sent on.</param>
        internal override void OnReceiveData(byte[] data, ProductUserId clientEpicId, int channel)
        {
            if (!Connected) return;

            var clientQueuePoolData = new EpicMessage(clientEpicId, channel, InternalMessage.Data, data);

            if (Logger.logEnabled)
                if (Transport.transportDebug)
                    DebugLogger.RegularDebugLog(
                        $"[Client] - Queue up message Event Type: {clientQueuePoolData.EventType} data: {BitConverter.ToString(clientQueuePoolData.Data)}");

            QueuedData.Enqueue(clientQueuePoolData);
        }

        #endregion

        #region IConnection Overrides

        /// <summary>
        ///     Process data to be sent to server.
        /// </summary>
        /// <param name="data">The data we want to send.</param>
        /// <param name="channel">The channel we want to send data on.</param>
        /// <returns></returns>
        public UniTask SendAsync(ArraySegment<byte> data, int channel = Channel.Reliable)
        {
            if (!Connected) return UniTask.CompletedTask;

            _clientSendPoolData = new byte[data.Count];

            Array.Copy(data.Array, data.Offset, _clientSendPoolData, 0, data.Count);

            Send(Options.ConnectionAddress, _clientSendPoolData, channel);

            return UniTask.CompletedTask;
        }

        /// <summary>
        ///     Received data from server and process it to mirror.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public async UniTask<int> ReceiveAsync(MemoryStream buffer)
        {
            try
            {
                if (!Connected) throw new EndOfStreamException();

                while (QueuedData.Count <= 0)
                {
                    // Due to how steam works we have no connection state to be able to
                    // know when server disconnects us truly. So when steam sends a internal disconnect
                    // message we disconnect as normal but the _cancellation Token will trigger and we can exit cleanly
                    // using mirror.
                    if (!Connected) throw new EndOfStreamException();

                    await UniTask.Delay(1);
                }

                QueuedData.TryDequeue(out EpicMessage clientReceivePoolData);

                buffer.SetLength(0);

                if (Logger.logEnabled)
                    if (Transport.transportDebug)
                        DebugLogger.RegularDebugLog(
                            $"[Client] Processing message: {BitConverter.ToString(clientReceivePoolData.Data)}");

                await buffer.WriteAsync(clientReceivePoolData.Data, 0, clientReceivePoolData.Data.Length);

                return clientReceivePoolData.Channel;
            }
            catch (EndOfStreamException)
            {
                throw new EndOfStreamException();
            }
        }

        /// <summary>
        ///     Disconnect client from server.
        /// </summary>
        public override async void Disconnect()
        {
            if (!Connected) return;

            if (Logger.logEnabled)
                if (Transport.transportDebug)
                    DebugLogger.RegularDebugLog("[Client] - Shutting down.");

            _clientSendPoolData = null;

            if(!_serverControlled)
            {
                SendInternal(Options.ConnectionAddress, InternalMessage.Disconnect, SocketName);

                // Wait 1 seconds to make sure the disconnect message gets fired.
                await UniTask.Delay(1000);

                CloseP2PSessionWithUser(Options.ConnectionAddress, SocketName);
            }

            base.Disconnect();

            _connectedComplete?.TrySetCanceled();
        }

        /// <summary>
        ///     Just returns back the clients <see cref="ProductUserId"/>
        /// </summary>
        /// <returns></returns>
        public EndPoint GetEndPointAddress()
        {
            return new DnsEndPoint(Options.ConnectionAddress.ToString(), 0);
        }

        #endregion
    }
}
