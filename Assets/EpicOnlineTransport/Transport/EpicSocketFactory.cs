using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.P2P;
using Mirage.Logging;
using Mirage.SocketLayer;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mirage.Sockets.EpicSocket
{
    public static class EOSManagerFixer
    {
        // enum EOSState
        // {
        //     NotStarted,
        //     Starting,
        //     Running,
        //     ShuttingDown,
        //     Shutdown
        // };


        // todo make state public
        //private static EOSState GetState()
        //{
        //    System.Reflection.FieldInfo stateField = typeof(EOSManager).GetField("s_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        //    var value = (EOSState)stateField.GetValue(null);
        //    return value;
        //}

        //public static bool IsRunning()
        //{
        //    EOSState value = GetState();
        //    return value == EOSState.Running;
        //}

        //public static bool IsShutdown()
        //{
        //    EOSState value = GetState();
        //    return value == EOSState.Shutdown || value == EOSState.ShuttingDown;
        //}

        public static bool IsLoaded()
        {
            return EOSManager.Instance.GetEOSPlatformInterface() != null;
        }

        /// <summary>
        /// Call that can wait for Callbacks async
        /// </summary>
        /// <remarks>
        /// This must be a class not a struct, other wise copies will be made and callback wont set the _result field correctly for Wait
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        public class Waiter<T> where T : class
        {
            T _result;

            public void Callback(T result)
            {
                _result = result;
            }
            public async UniTask<T> Wait()
            {
                while (_result == null)
                {
                    await UniTask.Yield();
                }

                return _result;
            }
        }

    }

    internal static class DeviceIdConnect
    {
        public static async UniTask Connect(EOSManager.EOSSingleton manager, ConnectInterface connectInterface, string displayName)
        {
            CreateDeviceIdCallbackInfo createInfo = await CreateDeviceIdAsync(connectInterface);

            ThrowIfResultInvalid(createInfo);

            LoginCallbackInfo loginInfo = await LoginAsync(manager, displayName);

            EpicLogger.WarnResult("Login Callback", loginInfo.ResultCode);
        }

        private static void ThrowIfResultInvalid(CreateDeviceIdCallbackInfo createInfo)
        {
            if (createInfo.ResultCode == Result.Success)
                return;

            // already exists, this is ok
            if (createInfo.ResultCode == Result.DuplicateNotAllowed)
            {
                EpicLogger.logger.Log($"Device Id already exists");
                return;
            }

            if (createInfo.ResultCode != Result.Success && createInfo.ResultCode != Result.DuplicateNotAllowed)
                throw new EpicSocketException($"Failed to Create DeviceId, Result code: {createInfo.ResultCode}");
        }

        private static UniTask<CreateDeviceIdCallbackInfo> CreateDeviceIdAsync(ConnectInterface connect)
        {
            var createOptions = new CreateDeviceIdOptions()
            {
                // todo get device model
#if UNITY_EDITOR
                DeviceModel = "DemoModel_Editor"
#else
                DeviceModel = "DemoModel"
#endif
            };
            var waiter = new EOSManagerFixer.Waiter<CreateDeviceIdCallbackInfo>();
            connect.CreateDeviceId(createOptions, null, waiter.Callback);
            return waiter.Wait();
        }

        private static UniTask<LoginCallbackInfo> LoginAsync(EOSManager.EOSSingleton manager, string displayName)
        {
            var waiter = new EOSManagerFixer.Waiter<LoginCallbackInfo>();
            manager.StartConnectLoginWithDeviceToken(displayName, waiter.Callback);
            return waiter.Wait();
        }
    }

    internal static class DevAuthLogin
    {
        public static async UniTask LoginAndConnect(DevAuthSettings settings)
        {
            // we must authenticate first,
            // and then connect to relay
            EpicAccountId user = await LogInWithDevAuth(settings);

            await Connect(user);
        }

        private static async UniTask<EpicAccountId> LogInWithDevAuth(DevAuthSettings settings)
        {
            Epic.OnlineServices.Auth.LoginCredentialType type = Epic.OnlineServices.Auth.LoginCredentialType.Developer;
            string id = $"localhost:{settings.Port}";
            string token = settings.CredentialName;

            var waiter = new EOSManagerFixer.Waiter<Epic.OnlineServices.Auth.LoginCallbackInfo>();
            EOSManager.Instance.StartLoginWithLoginTypeAndToken(type, id, token, waiter.Callback);
            Epic.OnlineServices.Auth.LoginCallbackInfo result = await waiter.Wait();

            EpicAccountId epicAccountId = result.LocalUserId;
            if (result.ResultCode == Result.InvalidUser)
                epicAccountId = await CreateNewAccount(result.ContinuanceToken);
            else if (result.ResultCode != Result.Success)
                throw new EpicSocketException($"Failed to login with Dev auth, result code={result.ResultCode}");

            return epicAccountId;
        }

        private static async UniTask<EpicAccountId> CreateNewAccount(ContinuanceToken continuanceToken)
        {
            Debug.Log($"Trying Auth link with external account: {continuanceToken}");

            var waiter = new EOSManagerFixer.Waiter<Epic.OnlineServices.Auth.LinkAccountCallbackInfo>();
            EOSManager.Instance.AuthLinkExternalAccountWithContinuanceToken(continuanceToken, Epic.OnlineServices.Auth.LinkAccountFlags.NoFlags, waiter.Callback);
            Epic.OnlineServices.Auth.LinkAccountCallbackInfo result = await waiter.Wait();
            if (result.ResultCode != Result.Success)
                throw new EpicSocketException($"Failed to login with Dev auth, result code={result.ResultCode}");

            EpicLogger.Verbose($"Create New Account: [User:{result.ResultCode} Selected:{result.SelectedAccountId}]");
            return result.LocalUserId;
        }

        private static async UniTask Connect(EpicAccountId user)
        {
            LoginCallbackInfo firstTry = await _connect(user);

            Result result = firstTry.ResultCode;
            if (result == Result.InvalidUser)
            {
                // ask user if they want to connect; sample assumes they do
                var createWaiter = new EOSManagerFixer.Waiter<CreateUserCallbackInfo>();
                EOSManager.Instance.CreateConnectUserWithContinuanceToken(firstTry.ContinuanceToken, createWaiter.Callback);
                CreateUserCallbackInfo createResult = await createWaiter.Wait();

                Debug.Log("Created new account");

                LoginCallbackInfo secondTry = await _connect(user);
                result = secondTry.ResultCode;
            }

            if (result != Result.Success)
                throw new EpicSocketException($"Failed to login with Dev auth, result code={result}");
        }

        private static async UniTask<LoginCallbackInfo> _connect(EpicAccountId user)
        {
            var waiter = new EOSManagerFixer.Waiter<LoginCallbackInfo>();
            EOSManager.Instance.StartConnectLoginWithEpicAccount(user, waiter.Callback);
            LoginCallbackInfo result = await waiter.Wait();
            return result;
        }
    }

    [System.Serializable]
    public struct DevAuthSettings
    {
        public string CredentialName;
        public int Port;
    }

    public class EpicSocketFactory : SocketFactory, IEOSCoroutineOwner
    {
        enum InitializeStatus
        {
            None,
            Initializing,
            Initialized
        }
        private static InitializeStatus s_isInitialize;

        public struct InitializeResult
        {
            /// <summary>
            /// Exception thrown by Async task.
            /// </summary>
            public Exception Exception;

            public bool Successful => Exception == null;
        }

        RelayHandle relayHandle;

        public void Initialize(Action<InitializeResult> callback, DevAuthSettings? devAuth, string displayName = null)
        {
            UniTask.Void(async () =>
            {
                InitializeResult result = default;
                try
                {
                    await InitializeAsync(devAuth, displayName);
                }
                catch (Exception e)
                {
                    result.Exception = e;
                }

                callback.Invoke(result);
            });
        }

        /// <summary>
        /// Call this before starting Mirage
        /// </summary>
        /// <returns></returns>
        public async UniTask InitializeAsync(DevAuthSettings? devAuth, string displayName = null)
        {
            if (s_isInitialize == InitializeStatus.Initialized)
            {
                Debug.LogWarning("Already Initialize");
                return;
            }
            if (s_isInitialize == InitializeStatus.Initializing)
            {
                while (s_isInitialize == InitializeStatus.Initializing)
                    await UniTask.Yield();

                return;
            }

            s_isInitialize = InitializeStatus.Initializing;

            checkName(ref displayName);

            // wait for sdk to finish
            while (!EOSManagerFixer.IsLoaded())
                await UniTask.Yield();

            if (devAuth.HasValue)
            {
                await DevAuthLogin.LoginAndConnect(devAuth.Value);
            }
            else
            {
                await DeviceIdConnect.Connect(EOSManager.Instance, EOSManager.Instance.GetEOSConnectInterface(), displayName);
            }

            // todo do we need this?
            ChangeRelayStatus();

            ProductUserId productId = EOSManager.Instance.GetProductUserId();
            Debug.Log($"<color=cyan>Relay set up, localUser={productId}, isNull={productId == null}</color>");
            s_isInitialize = InitializeStatus.Initialized;
        }

        private static void checkName(ref string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
                displayName = "Default User " + UnityEngine.Random.Range(0, 1000).ToString().PadLeft(4, '0');
        }

        private void ChangeRelayStatus()
        {
            var setRelayControlOptions = new SetRelayControlOptions();
            setRelayControlOptions.RelayControl = RelayControl.AllowRelays;

            Result result = EOSManager.Instance.GetEOSP2PInterface().SetRelayControl(setRelayControlOptions);
            EpicLogger.WarnResult("Set Relay Controls", result);
        }


        #region SocketFactory overrides
        public override int MaxPacketSize => P2PInterface.MaxPacketSize;

        public override ISocket CreateServerSocket()
        {
            if (relayHandle == null && relayHandle.IsOpen)
                throw new InvalidOperationException("Relay now active, Call Initialize first");

            return new EpicSocket(relayHandle);
        }

        public override ISocket CreateClientSocket()
        {
            if (relayHandle == null && relayHandle.IsOpen)
                throw new InvalidOperationException("Relay now active, Call Initialize first");

            return new EpicSocket(relayHandle);
        }

        public override IEndPoint GetBindEndPoint()
        {
            return new EpicEndPoint();
        }

        public override IEndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            return new EpicEndPoint(address);
        }
        #endregion

        void IEOSCoroutineOwner.StartCoroutine(IEnumerator routine)
        {
            // this should call the non-explicity methods from MonoBehaviour
            _ = StartCoroutine(routine);
        }
    }

    internal sealed class EpicEndPoint : IEndPoint
    {
        private string _hostProductUserId;
        private ProductUserId _userId;
        public ProductUserId UserId
        {
            get
            {
                if (_userId == null)
                {
                    // can only get id when loaded
                    if (EOSManagerFixer.IsLoaded())
                    {
                        // only call this is host Id is set
                        if (string.IsNullOrEmpty(_hostProductUserId))
                            throw new InvalidOperationException("Host Id is empty");

                        _userId = ProductUserId.FromString(_hostProductUserId);
                    }
                }
                return _userId;
            }
            set
            {
                if (!string.IsNullOrEmpty(_hostProductUserId))
                {
                    Assert.AreEqual(_userId, value);
                }

                _userId = value;
            }
        }

        public EpicEndPoint() { }
        public EpicEndPoint(string hostProductUserId)
        {
            _hostProductUserId = hostProductUserId;
            if (string.IsNullOrEmpty(_hostProductUserId))
                throw new ArgumentException("Host Id is empty");
        }
        private EpicEndPoint(ProductUserId userId)
        {
            _userId = userId;
        }

        IEndPoint IEndPoint.CreateCopy()
        {
            Assert.IsNotNull(UserId);
            return new EpicEndPoint(UserId);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EpicEndPoint other))
                return false;

            if (UserId != null)
            {
                // user Equals because of IEquatable
                return UserId.Equals(other.UserId);
            }
            else if (other.UserId != null)
            {
                // userId is only set on other, so not equal
                return false;
            }
            else
            {
                return _hostProductUserId == other._hostProductUserId;
            }
        }

        public override int GetHashCode()
        {
            // user UserId first, if that is null fallback to string
            if (UserId != null)
            {
                return UserId.GetHashCode();
            }
            else
            {
                return _hostProductUserId.GetHashCode();
            }
        }

        internal void CopyFrom(EpicEndPoint endPoint)
        {
            Assert.IsNotNull(endPoint.UserId);
            UserId = endPoint.UserId;
        }
    }

    [Serializable]
    public class EpicSocketException : Exception
    {
        public EpicSocketException(string message) : base(message) { }
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
            if (!EOSManagerFixer.IsLoaded())
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
            return IsOpen && EOSManagerFixer.IsLoaded();
        }



        // todo find way to send byte[] with length
        public void SendGameData(ProductUserId userId, byte[] data)
        {
            _sendOptions.Channel = CHANNEL_GAME;
            _sendOptions.Data = data;
            _sendOptions.RemoteUserId = userId;
            sendUsingOptions();
        }

        public void SendCommand(ProductUserId userId, byte info)
        {
            _sendOptions.Channel = CHANNEL_COMMAND;
            _singleByteCommand[0] = info;
            _sendOptions.Data = _singleByteCommand;
            _sendOptions.RemoteUserId = userId;
            sendUsingOptions();
        }

        public void SendCommand(ProductUserId userId, byte[] info)
        {
            _sendOptions.Channel = CHANNEL_COMMAND;
            _sendOptions.Data = info;
            _sendOptions.RemoteUserId = userId;
            sendUsingOptions();
        }

        private void sendUsingOptions()
        {
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

    struct ReceivedPacket
    {
        public ProductUserId userId;
        public byte[] data;
    }

    internal sealed class EpicSocket : ISocket
    {
        bool _isClosed;
        RelayHandle _relayHandle;

        SendPacketOptions _sendOptions;
        ReceivePacketOptions _receiveOptions;

        int _lastTickedFrame;
        ReceivedPacket _receivedPacket;
        bool _isClient;
        readonly EpicEndPoint _receiveEndPoint = new EpicEndPoint();

        public EpicSocket(RelayHandle relayHandle)
        {
            _relayHandle = relayHandle ?? throw new ArgumentNullException(nameof(relayHandle));
        }

        void ThrowIfRelayNotActive()
        {
            if (_relayHandle.IsOpen)
                throw new InvalidOperationException("Relay not open, can not start socket");
        }

        public void Bind(IEndPoint endPoint)
        {
            ThrowIfRelayNotActive();
        }

        public void Connect(IEndPoint _endPoint)
        {
            ThrowIfRelayNotActive();
            _isClient = true;

            _receiveEndPoint.CopyFrom((EpicEndPoint)_endPoint);
            _relayHandle.ConnectToRemoteUser(_receiveEndPoint.UserId);
        }

        public void Close()
        {
            _relayHandle.CloseRelay();
            _relayHandle = default;
            _isClosed = true;
        }

        bool IsOpenAndLoaded()
        {
            if (_isClosed)
                return false;

            if (_relayHandle.CheckOpen())
            {
                return true;
            }
            else
            {
                EpicLogger.logger.LogError("Calling when when EOS is not loaded, Closing socket");
                Close();
                return false;
            }
        }

        public bool Poll()
        {
            if (!IsOpenAndLoaded()) return false;

            // first time this tick?
            if (_lastTickedFrame != Time.frameCount)
            {
                _relayHandle.Manager.Tick();
                _lastTickedFrame = Time.frameCount;
            }

            // todo do we need to do anything with socketid or channel?
            Result result = _relayHandle.P2P.ReceivePacket(_receiveOptions, out _receivedPacket.userId, out SocketId _, out byte _, out _receivedPacket.data);

            if (result != Result.Success && result != Result.NotFound) // log for results other than Success/NotFound
                EpicLogger.WarnResult("Receive Packet", result);

            return result == Result.Success;
        }

        public int Receive(byte[] buffer, out IEndPoint endPoint)
        {
            Assert.IsNotNull(_receivedPacket.data);

            Buffer.BlockCopy(_receivedPacket.data, 0, buffer, 0, _receivedPacket.data.Length);

            _receiveEndPoint.UserId = _receivedPacket.userId;
            endPoint = _receiveEndPoint;
            int length = _receivedPacket.data.Length;

            // clear refs
            _receivedPacket = default;
            EpicLogger.Verbose($"Receive {length} bytes from {_receivedPacket.userId}");
            return length;
        }

        public void Send(IEndPoint iEndPoint, byte[] packet, int length)
        {
            if (!IsOpenAndLoaded()) return;

            var endPoint = (EpicEndPoint)iEndPoint;

            // send option has no length field, we have to copy to new array
            // todo avoid allocation
            byte[] data = packet.Take(length).ToArray();
            _relayHandle.SendGameData(endPoint.UserId, data);

            EpicLogger.Verbose($"Send {length} bytes to {_sendOptions.RemoteUserId}");
        }

        private void setEndPoint(IEndPoint iEndPoint)
        {
            var endPoint = (EpicEndPoint)iEndPoint;

            // dont set remote user if this is client (it always uses hostId)
            if (_isClient)
            {
                Assert.AreEqual(_receiveEndPoint.UserId, endPoint.UserId);
            }
            _sendOptions.RemoteUserId = endPoint.UserId;
        }
    }

    internal static class EpicLogger
    {
        // change default log level based on if we are in debug or release mode.
        // this is only default, if there are log settings they will be used instead of these
#if DEBUG
        const LogType DEFAULT_LOG = LogType.Warning;
#else
        const LogType DEFAULT_LOG = LogType.Error;
#endif
        internal static readonly ILogger logger = LogFactory.GetLogger(typeof(EpicSocket), DEFAULT_LOG);

        internal static void WarnResult(string tag, Result result)
        {
            if (result == Result.Success) return;
            if (logger.WarnEnabled())
                logger.LogWarning($"{tag} failed with result: {result}");
        }

        internal static void Verbose(string log)
        {
#if UNITY_EDITOR
            Debug.Log(log);
#else
            Console.WriteLine(log);
#endif
        }
    }
}

