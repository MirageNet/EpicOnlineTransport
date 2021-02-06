#region Statements

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

        private readonly IDictionary<ProductUserId, Client> _connectedEpicUsers;

        #endregion

        #region Class Specific

        /// <summary>
        ///     Start listening for connections on server.
        /// </summary>
        public void StartListening()
        {
            if (Logger.logEnabled)
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
            _connectedEpicUsers = new Dictionary<ProductUserId, Client>();

            UniTask.Run(ProcessIncomingMessages).Forget();
        }

        protected override void OnConnectionFailed(OnRemoteConnectionClosedInfo result)
        {
            if (_connectedEpicUsers.ContainsKey(result.RemoteUserId))
                _connectedEpicUsers.Remove(result.RemoteUserId);

            base.OnConnectionFailed(result);
        }

        public override void Disconnect()
        {
            foreach (KeyValuePair<ProductUserId, Client> client in _connectedEpicUsers)
            {
                client.Value.Disconnect();
                _connectedEpicUsers.Remove(client);
            }

            ProcessIncomingMessages();

            CancellationToken.Cancel();

            Dispose();
        }

        /// <summary>
        ///     We accept all incoming connection request on server.
        /// </summary>
        /// <param name="result">The data we need to process to make a accept the new connection request.</param>
        protected override void OnNewConnection(OnIncomingConnectionRequestInfo result)
        {
            Result accepted = EpicManager.P2PInterface.AcceptConnection(new AcceptConnectionOptions
            {
                LocalUserId = EpicManager.AccountId.ProductUserId,
                RemoteUserId = result.RemoteUserId,
                SocketId = result.SocketId
            });

            if (accepted == Result.Success)
            {
                return;
            }

            if (Logger.logEnabled)
                if (Transport.transportDebug)
                    DebugLogger.RegularDebugLog(
                        $"[Server] - Received internal connection message but failed to accepted connection. Result: {accepted}");
        }

        /// <summary>
        ///     Process our internal messages away from mirror or epic.
        /// </summary>
        /// <param name="type">The type of internal messages we received. <see cref="InternalMessage"/></param>
        /// <param name="clientEpicId">The epic user who sent the internal message.</param>
        /// <param name="socket"></param>
        protected override void OnReceiveInternalData(InternalMessage type, ProductUserId clientEpicId, SocketId socket)
        {
            switch (type)
            {
                case InternalMessage.Disconnect:
                    if (Logger.logEnabled)
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                "[Server] - Received internal message to disconnect epic user.");

                    if (_connectedEpicUsers.TryGetValue(clientEpicId, out Client connection))
                    {
                        connection.Disconnect();

                        _connectedEpicUsers.Remove(clientEpicId);

                        if (Logger.logEnabled)
                            if (Transport.transportDebug)
                                DebugLogger.RegularDebugLog(
                                    $"[Server] - Client with ProductId {clientEpicId} disconnected.");
                    }

                    break;

                case InternalMessage.Connect:
                    if (_connectedEpicUsers.Count >= Options.MaxConnections)
                    {
                        SendInternal(clientEpicId, InternalMessage.TooManyUsers, socket);

                        return;
                    }

                    if (_connectedEpicUsers.ContainsKey(clientEpicId))
                    {
                        if (Logger.logEnabled)
                            if (Transport.transportDebug)
                                DebugLogger.RegularDebugLog(
                                    $"[Server] - Client with ProductId {clientEpicId} already connected.");
                        return;
                    }

                    Options.ConnectionAddress = clientEpicId;

                    var client = new Client(Transport, Options, true);
                    client.SocketName = socket;

                    _connectedEpicUsers.Add(clientEpicId, client);

                    Transport.Connected.Invoke(client);

                    if (Logger.logEnabled)
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                $"[Server] - Connecting with {clientEpicId} and accepting handshake.");

                    SendInternal(clientEpicId, InternalMessage.Accept, socket);
                    break;

                default:
                    if (Logger.logEnabled)
                        if (Transport.transportDebug)
                            DebugLogger.RegularDebugLog(
                                $"[Server] - Client connection cannot process internal message {type}. If this is anything other then {InternalMessage.Data} something has gone wrong.",
                                LogType.Warning);
                    break;
            }
        }

        /// <summary>
        ///     Process data that has come in to the correct clients.
        /// </summary>
        /// <param name="data">The data that we received.</param>
        /// <param name="clientEpicId">The client in which the data came from.</param>
        /// <param name="channel">The channel the data came on.</param>
        internal override void OnReceiveData(byte[] data, ProductUserId clientEpicId, int channel)
        {
            if (_connectedEpicUsers.TryGetValue(clientEpicId, out Client client))
            {
                client.OnReceiveData(data, clientEpicId, channel);
            }

            if (Logger.logEnabled)
                if (Transport.transportDebug)
                    DebugLogger.RegularDebugLog(
                        $"[Server] - Queue up message Event Type: {InternalMessage.Data} data: {BitConverter.ToString(data)}");
        }

        #endregion
    }
}
