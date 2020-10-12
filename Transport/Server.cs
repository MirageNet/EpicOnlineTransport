#region Statements

using System;
using System.Collections.Generic;
using Epic.Logging;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using EpicChill.EpicCore;
using UnityEngine;

#endregion

namespace EpicChill.Transport
{
    public class Server : Common
    {
        #region Fields

        private readonly Dictionary<int, EpicUser> _connectedUsers = new Dictionary<int, EpicUser>();

        private readonly Action<int> _onConnected;
        private Action<int, byte[], int> _onReceivedData;
        private readonly Action<int> _onDisconnected;
        private Action<int, Exception> _onReceivedError;

        #endregion

        #region Class Specific

        /// <summary>
        ///     Create a new server.
        /// </summary>
        /// <param name="transport">The transport the server will be using.</param>
        /// <param name="debug">Whether or not we want to debug info.</param>
        /// <param name="epic">The epic manager to use for the server.</param>
#if UNITY_EDITOR
        public Server(EpicChillTransport transport, bool debug, EpicManager epic) : base(transport, debug, epic)
#else
        public Server(EpicChillTransport transport, EpicManager epic) : base(transport, epic)
#endif
        {
            EpicManager = epic;

            // Setup event listener's to transport.
            _onConnected += (id) => transport.OnServerConnected.Invoke(id);
            _onDisconnected += (id) => transport.OnServerDisconnected.Invoke(id);
            _onReceivedData += (id, data, channel) => transport.OnServerDataReceived.Invoke(id, new ArraySegment<byte>(data), channel);
            _onReceivedError += (id, exception) => transport.OnServerError.Invoke(id, exception);
        }

        /// <summary>
        ///     Start up server and listen for connections.
        /// </summary>
        public void Start()
        {
            DebugLogger.RegularDebugLog("[Server] Started listening for connection requests.");

            AddNotifyPeerConnectionRequestOptions
                connectionRequestOptions = new AddNotifyPeerConnectionRequestOptions
                {
                    LocalUserId = ProductUserId.FromString(EpicManager.AccountId.AccountId.ToString()),
                };

            EpicManager.P2PInterface.AddNotifyPeerConnectionRequest(connectionRequestOptions, null,
                ConnectionRequestHandler);
        }

        /// <summary>
        ///     Process queue up messages.
        /// </summary>
        protected override void ProcessQueuedMessages()
        {
            // Process queue messages.
            while (!CancellationToken.IsCancellationRequested)
            {
                if (QueuedMessages.IsEmpty)
                {
                    continue;
                }

                QueuedMessages.TryDequeue(out EpicMessage result);

                _onReceivedData.Invoke(result.ConnectionId, result.Data, result.Channel);
            }
        }

        /// <summary>
        ///     Process internal messages that have been queued up from us.
        /// </summary>
        protected override void ProcessInternalMessages(InternalMessage message, ProductUserId userId)
        {
            switch (message)
            {
                case InternalMessage.Connect:
                    DebugLogger.RegularDebugLog("[Server] Connection attempt from {}");

                    SendInternalMessage(InternalMessage.Accept, userId);
                    break;
                case InternalMessage.Disconnect:
                    DebugLogger.RegularDebugLog("[Server] Disconnect call from {}");

                    Disconnect(userId.InnerHandle.ToInt32());

                    break;
                default:
                    DebugLogger.RegularDebugLog($"[Server] Somehow client received an internal message of {message}", LogType.Error);
                    break;
            }
        }

        /// <summary>
        ///     Shutdown the server and close all connections out.
        /// </summary>
        public override void Shutdown()
        {
            DebugLogger.RegularDebugLog("[Server] Shutting down server and cleaning up resources.");

            if(SocketListener is null) return;

            // Create a close connection struct and close all connections down.
            CloseConnectionsOptions closeConnectionOptions = new CloseConnectionsOptions
            {
                LocalUserId = ProductUserId.FromString(EpicManager.AccountId.ToString()),
                SocketId = SocketListener
            };

            EpicManager.P2PInterface.CloseConnections(closeConnectionOptions);

            SocketListener = null;
            CancellationToken.Cancel();
        }

        /// <summary>
        ///     Send data to everyone connected to server.
        /// </summary>
        /// <param name="connectionIds">The list of connection ids to send data to.</param>
        /// <param name="channel">The channel to send data on.</param>
        /// <param name="data">The data to send to users.</param>
        public bool Send(List<int> connectionIds, int channel, ArraySegment<byte> data)
        {
            byte[] packetData = new byte[data.Count];

            Array.Copy(data.Array, data.Offset, packetData, 0, data.Count);

            SendPacketOptions packet = new SendPacketOptions
            {
                Channel = 0,
                Data = packetData,
                Reliability = PacketReliability.ReliableOrdered,
                LocalUserId = ProductUserId.FromString(EpicManager.AccountId.AccountId.InnerHandle.ToString()),
                SocketId = SocketListener
            };

            int sent = 0;

            for (int client = 0; client < connectionIds.Count; client++)
            {
                packet.RemoteUserId =
                    ProductUserId.FromString(_connectedUsers[connectionIds[client]].AccountId.InnerHandle.ToString());

                sent += (int)EpicManager.P2PInterface.SendPacket(packet);

                DebugLogger.RegularDebugLog(
                    $"[Server] Sending data {BitConverter.ToString(packetData)} to user {packet.RemoteUserId.InnerHandle}");
            }

            DebugLogger.RegularDebugLog(
                $"[Server] sent data to {(sent == connectionIds.Count ? "all" : sent + "/" + connectionIds.Count)} users.");

            return sent == connectionIds.Count;
        }

        /// <summary>
        ///     Disconnect client from server.
        /// </summary>
        public bool Disconnect(int connectionId)
        {
            if (!_connectedUsers.TryGetValue(connectionId, out EpicUser id))
            {
                return false;
            }

            CloseConnectionOptions closeConnectionOptions = new CloseConnectionOptions
            {
                LocalUserId = ProductUserId.FromString(id.AccountId.ToString()),
                SocketId = SocketListener
            };

            DebugLogger.RegularDebugLog($"[Server] Disconnecting user {id.AccountId.InnerHandle}");

            _onDisconnected?.Invoke(connectionId);

            return EpicManager.P2PInterface.CloseConnection(closeConnectionOptions) == Result.Success;
        }

        /// <summary>
        ///     Get a specific client address.
        /// </summary>
        /// <param name="connectionId">The connection id of client to get address from.</param>
        /// <returns>Returns back empty if no client exists or returns back address.</returns>
        public string ClientAddress(int connectionId)
        {
            return _connectedUsers.TryGetValue(connectionId, out EpicUser accountId)
                ? accountId.AccountId.ToString()
                : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void ConnectionRequestHandler(OnIncomingConnectionRequestInfo data)
        {
            DebugLogger.RegularDebugLog($"[Server] Received connection request from {data.RemoteUserId.InnerHandle.ToInt32()}");

            // Create a new accept connections options to accept incoming connection.
            AcceptConnectionOptions acceptConnection = new AcceptConnectionOptions
            {
                RemoteUserId = data.RemoteUserId, LocalUserId = data.LocalUserId, SocketId = data.SocketId
            };

            _onConnected?.Invoke(data.RemoteUserId.InnerHandle.ToInt32());

            EpicManager.P2PInterface.AcceptConnection(acceptConnection);

            SocketListener = data.SocketId;
        }

        #endregion
    }
}
