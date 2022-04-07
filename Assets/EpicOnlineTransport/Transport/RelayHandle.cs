using System;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using Mirage.Logging;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine.Assertions;

namespace Mirage.Sockets.EpicSocket
{
    struct ReceivedPacket
    {
        public ProductUserId userId;
        public byte[] data;
    }

    internal class RelayHandle
    {
        const int CHANNEL_GAME = 0;
        const int CHANNEL_COMMAND = 1;

        public readonly EOSManager.EOSSingleton Manager;
        public readonly P2PInterface P2P;
        public readonly ProductUserId LocalUser;

        ulong _openId;
        ulong _closeId;
        ulong _establishedId;
        ulong _queueFullId;
        ProductUserId _remoteUser;
        SendPacketOptions _sendOptions;
        ReceivePacketOptions _receiveOptions;
        byte[] _singleByteCommand = new byte[1];

        public bool IsOpen { get; private set; }

        static RelayHandle s_instance;

        public static RelayHandle GetOrCreate(EOSManager.EOSSingleton manager)
        {
            if (s_instance == null)
            {
                s_instance = new RelayHandle(manager);
            }
            Assert.AreEqual(manager, s_instance.Manager);

            return s_instance;
        }

        public RelayHandle(EOSManager.EOSSingleton manager)
        {
            Manager = manager;
            P2P = manager.GetEOSP2PInterface();
            LocalUser = manager.GetProductUserId();
        }

        public void OpenRelay()
        {
            if (IsOpen)
                throw new InvalidOperationException("Already open");

            IsOpen = true;
            enableRelay(connectionRequestCallback, connectionClosedCallback);
            _sendOptions = createSendOptions();
            _receiveOptions = createReceiveOptions();
        }

        public void CloseRelay()
        {
            if (!IsOpen)
                return;
            IsOpen = false;

            DisableRelay();
        }

        /// <summary>
        /// Limit incoming message from a single user
        /// <para>Useful for clients to only receive messager from Host</para>
        /// </summary>
        /// <param name="remoteUser"></param>
        public void ConnectToRemoteUser(ProductUserId remoteUser)
        {
            if (_remoteUser != null && _remoteUser != remoteUser)
                throw new InvalidOperationException("Already connected to another host");

            _remoteUser = remoteUser ?? throw new ArgumentNullException(nameof(remoteUser));
        }

        void connectionRequestCallback(OnIncomingConnectionRequestInfo data)
        {
            bool validHost = checkRemoteUser(data.RemoteUserId);
            if (!validHost)
            {
                EpicLogger.logger.LogError($"User ({data.RemoteUserId}) tried to connect to client");
                return;
            }

            var options = new AcceptConnectionOptions()
            {
                LocalUserId = LocalUser,
                RemoteUserId = data.RemoteUserId,
                // todo do we need to need to create new here
                SocketId = createSocketId()
            };
            Result result = P2P.AcceptConnection(options);
            EpicLogger.WarnResult("Accept Connection", result);
        }

        bool checkRemoteUser(ProductUserId remoteUser)
        {
            // host remote user, no need to check it
            if (_remoteUser == null)
                return true;

            // check incoming message is from expected user
            return _remoteUser == remoteUser;
        }

        void connectionClosedCallback(OnRemoteConnectionClosedInfo data)
        {
            // if we have set remoteUser, then close is probably from them, so we want to close the socket
            if (_remoteUser != null)
            {
                CloseRelay();
            }

            if (EpicLogger.logger.WarnEnabled()) EpicLogger.logger.LogWarning($"Connection closed with reason: {data.Reason}");
        }


        /// <summary>
        /// Starts relay as server, allows new connections
        /// </summary>
        /// <param name="notifyId"></param>
        /// <param name="socketName"></param>
        void enableRelay(OnIncomingConnectionRequestCallback openCallback, OnRemoteConnectionClosedCallback closedCallback)
        {
            //EpicHelper.WarnResult("SetPacketQueueSize", p2p.SetPacketQueueSize(new SetPacketQueueSizeOptions { IncomingPacketQueueMaxSizeBytes = 64000, OutgoingPacketQueueMaxSizeBytes = 64000 }));
            //EpicHelper.WarnResult("SetRelayControl", p2p.SetRelayControl(new SetRelayControlOptions { RelayControl = RelayControl.ForceRelays }));

            AddHandle(ref _openId, P2P.AddNotifyPeerConnectionRequest(new AddNotifyPeerConnectionRequestOptions { LocalUserId = LocalUser, }, null, (info) =>
            {
                EpicLogger.Verbose($"Connection Request [User:{info.RemoteUserId} Socket:{info.SocketId}]");
                openCallback.Invoke(info);
            }));
            AddHandle(ref _openId, P2P.AddNotifyPeerConnectionEstablished(new AddNotifyPeerConnectionEstablishedOptions { LocalUserId = LocalUser, }, null, (info) =>
            {
                EpicLogger.Verbose($"Connection Established: [User:{info.RemoteUserId} Socket:{info.SocketId} Type:{info.ConnectionType}]");
            }));
            AddHandle(ref _openId, P2P.AddNotifyPeerConnectionClosed(new AddNotifyPeerConnectionClosedOptions { LocalUserId = LocalUser, }, null, (info) =>
             {
                 EpicLogger.Verbose($"Connection Closed [User:{info.RemoteUserId} Socket:{info.SocketId} Reason:{info.Reason}]");
                 closedCallback.Invoke(info);
             }));
            AddHandle(ref _openId, P2P.AddNotifyIncomingPacketQueueFull(new AddNotifyIncomingPacketQueueFullOptions { }, null, (info) =>
            {
                EpicLogger.Verbose($"Incoming Packet Queue Full");
            }));
        }

        void DisableRelay()
        {
            // todo do we need to call close on p2p?
            // only disable if sdk is loaded
            if (!EpicHelper.IsLoaded())
                return;

            RemoveHandle(ref _openId, P2P.RemoveNotifyPeerConnectionRequest);
            RemoveHandle(ref _closeId, P2P.RemoveNotifyPeerConnectionClosed);
            RemoveHandle(ref _establishedId, P2P.RemoveNotifyPeerConnectionEstablished);
            RemoveHandle(ref _queueFullId, P2P.RemoveNotifyIncomingPacketQueueFull);
        }

        static void AddHandle(ref ulong handle, ulong value)
        {
            if (value == Common.InvalidNotificationid)
                throw new EpicSocketException("Handle was invalid");

            handle = value;
        }

        static void RemoveHandle(ref ulong handle, Action<ulong> removeAction)
        {
            if (handle != Common.InvalidNotificationid)
            {
                removeAction.Invoke(handle);
                handle = Common.InvalidNotificationid;
            }
        }

        SocketId createSocketId()
        {
            return new SocketId() { SocketName = "Game" };
        }

        SendPacketOptions createSendOptions()
        {
            return new SendPacketOptions()
            {
                AllowDelayedDelivery = true,
                Channel = 0,
                LocalUserId = LocalUser,
                Reliability = PacketReliability.UnreliableUnordered,
                SocketId = createSocketId(),

                RemoteUserId = null,
                Data = null,
            };
        }

        ReceivePacketOptions createReceiveOptions()
        {
            return new ReceivePacketOptions
            {
                LocalUserId = LocalUser,
                MaxDataSizeBytes = P2PInterface.MaxPacketSize,
                RequestedChannel = 0,
            };
        }


        public bool CheckOpen()
        {
            // if handle is open and Eos is loaded
            return IsOpen && EpicHelper.IsLoaded();
        }


        // todo find way to send byte[] with length
        public void SendGameData(ProductUserId userId, byte[] data)
        {
            _sendOptions.Channel = CHANNEL_GAME;
            _sendOptions.Data = data;
            _sendOptions.RemoteUserId = userId;
            _sendOptions.Reliability = PacketReliability.UnreliableUnordered;
            sendUsingOptions();
        }

        public void SendCommand(ProductUserId userId, byte opcode)
        {
            _sendOptions.Channel = CHANNEL_COMMAND;
            _singleByteCommand[0] = opcode;
            _sendOptions.Data = _singleByteCommand;
            _sendOptions.RemoteUserId = userId;
            _sendOptions.Reliability = PacketReliability.ReliableUnordered;
            sendUsingOptions();
        }

        private void sendUsingOptions()
        {
            // check client is only sending to Host
            if (_remoteUser != null)
            {
                Assert.AreEqual(_remoteUser, _sendOptions.RemoteUserId);
            }

            Result result = P2P.SendPacket(_sendOptions);
            EpicLogger.WarnResult("Send Packet", result);
        }

        public bool ReceiveGameData(out ReceivedPacket receivedPacket)
        {
            _receiveOptions.RequestedChannel = CHANNEL_GAME;
            return receiveUsingOptions(out receivedPacket);
        }

        public bool ReceiveCommand(out ReceivedPacket receivedPacket)
        {
            _receiveOptions.RequestedChannel = CHANNEL_COMMAND;
            return receiveUsingOptions(out receivedPacket);
        }

        private bool receiveUsingOptions(out ReceivedPacket receivedPacket)
        {
            Result result = P2P.ReceivePacket(_receiveOptions, out receivedPacket.userId, out SocketId _, out byte _, out receivedPacket.data);

            if (result != Result.Success && result != Result.NotFound) // log for results other than Success/NotFound
                EpicLogger.WarnResult("Receive Packet", result);

            return result == Result.Success;
        }
    }
}

