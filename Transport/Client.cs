#region Statements

using System;
using Epic.Logging;
using Epic.OnlineServices;
using EpicChill.EpicCore;
using UnityEngine;

#endregion

namespace EpicChill.Transport
{
    public class Client : Common
    {
        #region Fields

        private readonly Action<byte[], int> _onReceivedData;
        private readonly Action _onConnected;
        private readonly Action _onDisconnected;
        private readonly ProductUserId _remoteServerId = null;

        #endregion

        #region Class specific

        /// <summary>
        ///     Create a new client.
        /// </summary>
        /// <param name="transport">The transport the client will be using.</param>
        /// <param name="debug">Whether or not we want to debug info.</param>
        /// <param name="epic">The epic manager to use for the client.</param>
#if UNITY_EDITOR
        public Client(EpicChillTransport transport, bool debug, EpicManager epic) : base(transport, debug, epic)
#else
        public Client(EpicChillTransport transport, EpicManager epic) : base(transport, epic)
#endif
        {
            EpicManager = epic;

            // Setup event listener's to transport.
            _onReceivedData += (data, channel) =>
                transport.OnClientDataReceived.Invoke(new ArraySegment<byte>(data), channel);
            _onConnected += () => transport.OnClientConnected.Invoke();
            _onDisconnected += () => transport.OnClientDisconnected.Invoke();
        }

        /// <summary>
        ///     Process queue up messages.
        /// </summary>
        protected override void ProcessQueuedMessages()
        {
            // Process queue messages.
            while (!CancellationToken.IsCancellationRequested)
            {
                QueuedMessages.TryDequeue(out EpicMessage result);

                if ((InternalMessage)result.Data[0] != InternalMessage.Data)
                {
                    ProcessInternalMessages((InternalMessage)result.Data[0],
                        ProductUserId.FromString(result.ConnectionId.ToString()));
                    continue;
                }

                _onReceivedData.Invoke(result.Data, result.Channel);
            }
        }

        /// <summary>
        ///     Process internal messages that have been queued up from us.
        /// </summary>
        protected override void ProcessInternalMessages(InternalMessage message, ProductUserId userId)
        {
            switch (message)
            {
                case InternalMessage.Accept:
                    DebugLogger.RegularDebugLog("[Client] Connection to server accepted.");
                    _onConnected.Invoke();
                    break;
                case InternalMessage.TooManyUsers:
                    DebugLogger.RegularDebugLog("[Client] Connection to server refused. Too many users connected.", LogType.Warning);
                    break;
                default:
                    DebugLogger.RegularDebugLog($"[Client] Somehow client received an internal message of {message}", LogType.Error);
                    break;
            }
        }

        /// <summary>
        ///     Attempt a connection to epic user.
        /// </summary>
        /// <param name="address">The epic user we want to connect to for p2p.</param>
        public void Connect(string address)
        {
            Result results = SendInternalMessage(InternalMessage.Connect, ProductUserId.FromString(address));

            if (results != Result.Success)
                DebugLogger.RegularDebugLog($"[Client] Could not connect to server. Results: {results}", LogType.Warning);
        }

        /// <summary>
        ///     Send data to server.
        /// </summary>
        /// <param name="channel">The channel to send data on.</param>
        /// <param name="data">The data to send to server.</param>
        /// <returns></returns>
        public bool Send(int channel, ArraySegment<byte> data)
        {
            byte[] packetData = new byte[data.Count];

            Array.Copy(data.Array, data.Offset, packetData, 0, data.Count);

            DebugLogger.RegularDebugLog($"[Client] Sending data: {BitConverter.ToString(packetData)} to server.");

            return EpicManager.P2PInterface.SendPacket(CreatePacket(channel, packetData, _remoteServerId )) == Result.Success;
        }

        #endregion

        #region Overrides of Common

        /// <summary>
        ///     Shutdown and clean up resources of client.
        /// </summary>
        public override void Shutdown()
        {
            // Cancel token out to stop processing messages.
            CancellationToken.Cancel();

            while (!QueuedMessages.IsEmpty)
            {
                // Dump the queue as we are disconnecting.
                QueuedMessages.TryDequeue(out _);
            }

            Result result =
                EpicManager.P2PInterface.CloseConnection(
                    CreateCloseConnectionOptions(EpicManager.AccountId.ProductUserId));

            if (result == Result.Success)
                _onDisconnected.Invoke();
            else
            {
                DebugLogger.RegularDebugLog($"[Client] Disconnection attempt failed due to {result}", LogType.Error);
            }
        }

        /// <summary>
        ///     Disconnect the client from server.
        /// </summary>
        public void Disconnect()
        {
            Shutdown();
        }

        #endregion
    }
}
