#region Statements

using System;
using System.Collections.Generic;
using Epic.Logging;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using EpicChill.Transport;
using Mirror;
using UnityEngine;

#endregion

namespace EpicTransport
{
    public class Server : Common
    {
        #region Fields

        private static readonly ILogger Logger = LogFactory.GetLogger(typeof(Server));

        private readonly IDictionary<ProductUserId, Client> _connectedSteamUsers;

        #endregion

        #region Class Specific

        /// <summary>
        ///     Start listening for connections on server.
        /// </summary>
        public void StartListening()
        {
#if UNITY_EDITOR
            if (Logger.logEnabled)
#endif
                if (Transport.transportDebug)
                    DebugLogger.RegularDebugLog("[Server] - Listening for incoming connections....");

            Transport.Started.Invoke();
        }

        #endregion

        #region Common Overrides

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="options"></param>
        public Server(EpicTransport transport, EpicOptions options) : base(transport, options)
        {
            Options = options;
            Transport = transport;
            _connectedSteamUsers = new Dictionary<ProductUserId, Client>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        protected override void OnNewConnection(OnIncomingConnectionRequestInfo result)
        {
            EpicManager.P2PInterface.AcceptConnection(new AcceptConnectionOptions
            {
                LocalUserId = EpicManager.AccountId.ProductUserId,
                RemoteUserId = result.RemoteUserId,
                SocketId = new SocketId
                {
                    SocketName = SocketName
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessIncomingMessages()
        {
            while (Connected)
            {
                while (DataAvailable(out ProductUserId clientProductId, out byte[] internalMessage, (byte) Options.Channels.Length))
                {
                    if (internalMessage.Length != 1) continue;

                    OnReceiveInternalData((InternalMessage)internalMessage[0], clientProductId);

                    break;
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
            switch (type)
            {
                case InternalMessage.Disconnect:
#if UNITY_EDITOR
                    if (Logger.logEnabled)
#endif
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                "[Server] - Received internal message to disconnect epic user.");

                    if (_connectedSteamUsers.TryGetValue(clientEpicId, out Client connection))
                    {
                        connection.Disconnect();

                        CloseP2PSessionWithUser(clientEpicId);

                        _connectedSteamUsers.Remove(clientEpicId);

#if UNITY_EDITOR
                        if (Logger.logEnabled)
#endif
                            if (Transport.transportDebug)
                                DebugLogger.RegularDebugLog(
                                    $"[Server] - Client with ProductId {clientEpicId} disconnected.");
                    }

                    break;

                case InternalMessage.Connect:
                    if (_connectedSteamUsers.Count >= Options.MaxConnections)
                    {
                        SendInternal(clientEpicId, InternalMessage.TooManyUsers);

                        return;
                    }

                    if (_connectedSteamUsers.ContainsKey(clientEpicId)) return;

                    Options.ConnectionAddress = clientEpicId;

                    var client = new Client(null, Options);

                    Transport.Connected.Invoke(client);

#if UNITY_EDITOR
                    if (Logger.logEnabled)
#endif
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                $"[Server] - Connecting with {clientEpicId} and accepting handshake.");

                    _connectedSteamUsers.Add(clientEpicId, client);

                    SendInternal(clientEpicId, InternalMessage.Accept);
                    break;

                default:
#if UNITY_EDITOR
                    if (Logger.logEnabled)
#endif
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                $"[Server] - Client connection cannot process internal message {type}. If this is anything other then {InternalMessage.Data} something has gone wrong.",
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
            var dataMsg = new EpicMessage(clientEpicId, channel, InternalMessage.Data, data);

#if UNITY_EDITOR
            if (Logger.logEnabled)
#endif
                if (Transport.transportDebug)
                    DebugLogger.RegularDebugLog(
                        $"[Server] - Queue up message Event Type: {dataMsg.EventType} data: {BitConverter.ToString(dataMsg.Data)}");
        }

        #endregion
    }
}
