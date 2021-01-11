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

        private byte[] _clientSendPoolData;
        private EpicMessage _clientReceivePoolData;
        private EpicMessage _clientQueuePoolData;
        private AutoResetUniTaskCompletionSource _connectedComplete;

        #endregion

        #region Class Specific

        /// <summary>
        ///     Connect to server.
        /// </summary>
        /// <returns></returns>
        public async UniTask ConnectAsync()
        {
#if UNITY_EDITOR
            if (Logger.logEnabled)
#endif
                if (Transport.transportDebug)
                    DebugLogger.RegularDebugLog($"[Client] - attempting connection to {Options.ConnectionAddress}");

            try
            {
                // Send a message to server to initiate handshake connection
                SendInternal(Options.ConnectionAddress, InternalMessage.Connect);

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
                                $"[Client] - Connection to {Options.ConnectionAddress} timed out.", LogType.Error);

                    Error.Invoke(Result.LobbyInviteFailed,
                        $"[Client] - Connection to {Options.ConnectionAddress} timed out.");

                    Disconnect();
                }

                // Everything went good let's just return.
                // We need to switch to main thread for some reason.
                await UniTask.SwitchToMainThread();
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
                            "[Client] - Connection string was not in the right format. Did you enter a SteamId?",
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
            return EpicManager.P2PInterface.SendPacket(new SendPacketOptions
            {
                AllowDelayedDelivery = true,
                Channel = (byte)channel,
                Data = msgBuffer,
                LocalUserId = EpicManager.AccountId.ProductUserId,
                Reliability = Options.Channels[channel],
                RemoteUserId = host,
                SocketId = new SocketId {SocketName = SocketName}
            }) == Result.Success;
        }

        #endregion

        #region Common Overrides

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="options"></param>
        public Client(EpicTransport transport, EpicOptions options) : base(transport, options)
        {
            Options = options;
            Transport = transport;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        protected override void OnNewConnection(OnIncomingConnectionRequestInfo result)
        {
            if (Options.ConnectionAddress == result.RemoteUserId)
            {
                EpicManager.P2PInterface.AcceptConnection(
                    new AcceptConnectionOptions
                    {
                        LocalUserId = EpicManager.AccountId.ProductUserId,
                        RemoteUserId = result.RemoteUserId,
                        SocketId = new SocketId {SocketName = SocketName}
                    });
            }
            else
            {
#if UNITY_EDITOR
                if (Logger.logEnabled)
#endif
                    if (Transport.transportDebug)
                        DebugLogger.RegularDebugLog("[Client] - P2P Acceptance Request from unknown host ID.",
                            LogType.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessIncomingMessages()
        {
            while (Connected)
            {
                while (DataAvailable(out ProductUserId clientSteamId, out byte[] internalMessage,
                    (byte)Options.Channels.Length))
                {
                    if (internalMessage.Length != 1) continue;

                    OnReceiveInternalData((InternalMessage)internalMessage[0], clientSteamId);

                    break;
                }

                for (int chNum = 0; chNum < Options.Channels.Length; chNum++)
                {
                    while (DataAvailable(out ProductUserId clientSteamId, out byte[] receiveBuffer, (byte)chNum))
                    {
                        OnReceiveData(receiveBuffer, clientSteamId, chNum);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="clientEpicId"></param>
        protected override void OnReceiveInternalData(InternalMessage type, ProductUserId clientEpicId)
        {
            if (!Connected) return;

            switch (type)
            {
                case InternalMessage.Accept:
#if UNITY_EDITOR
                    if (Logger.logEnabled)
#endif
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                "[Client] - Received internal message of server accepted our request to connect.");

                    _connectedComplete.TrySetResult();

                    break;

                case InternalMessage.Disconnect:
                    Disconnect();

#if UNITY_EDITOR
                    if (Logger.logEnabled)
#endif
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                "[Client] - Received internal message to disconnect steam user.");

                    break;

                case InternalMessage.TooManyUsers:
#if UNITY_EDITOR
                    if (Logger.logEnabled)
#endif
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                "[Client] - Received internal message that there are too many users connected to server.",
                                LogType.Warning);

                    Error.Invoke(Result.LobbyTooManyPlayers, "Too many users currently connected.");

                    break;

                default:
#if UNITY_EDITOR
                    if (Logger.logEnabled)
#endif
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                $"[Client] - Cannot process internal message {type}. If this is anything other then {InternalMessage.Data} something has gone wrong.",
                                LogType.Warning);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="clientEpicId"></param>
        /// <param name="channel"></param>
        protected override void OnReceiveData(byte[] data, ProductUserId clientEpicId, int channel)
        {
            if (!Connected) return;

            _clientQueuePoolData = new EpicMessage(clientEpicId, channel, InternalMessage.Data, data);

#if UNITY_EDITOR
            if (Logger.logEnabled)
#endif
                if (Transport.transportDebug)
                    DebugLogger.RegularDebugLog(
                        $"[Client] - Queue up message Event Type: {_clientQueuePoolData.EventType} data: {BitConverter.ToString(_clientQueuePoolData.Data)}");

            QueuedData.Enqueue(_clientQueuePoolData);
        }

        #endregion

        #region IConnection Overrides

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="channel"></param>
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
        /// 
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

                QueuedData.TryDequeue(out _clientReceivePoolData);

                buffer.SetLength(0);

#if UNITY_EDITOR
                if (Logger.logEnabled)
#endif
                    if (Transport.transportDebug)
                        DebugLogger.RegularDebugLog(
                            $"[Client] Processing message: {BitConverter.ToString(_clientReceivePoolData.Data)}");

                await buffer.WriteAsync(_clientReceivePoolData.Data, 0, _clientReceivePoolData.Data.Length);

                return _clientReceivePoolData.Channel;
            }
            catch (EndOfStreamException)
            {
                throw new EndOfStreamException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override async void Disconnect()
        {
            if (!Connected) return;

#if UNITY_EDITOR
            if (Logger.logEnabled)
#endif
                if (Transport.transportDebug)
                    DebugLogger.RegularDebugLog("[Client] - Shutting down.");

            _clientSendPoolData = null;

            SendInternal(Options.ConnectionAddress, InternalMessage.Disconnect);

            // Wait 1 seconds to make sure the disconnect message gets fired.
            await UniTask.Delay(1000);

            base.Disconnect();

            CloseP2PSessionWithUser(Options.ConnectionAddress);

            _connectedComplete?.TrySetCanceled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EndPoint GetEndPointAddress()
        {
            return new DnsEndPoint(Options.ConnectionAddress.ToString(), 0);
        }

        #endregion
    }
}