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

            EpicHelper.WarnResult("Login Callback", loginInfo.ResultCode);
        }

        private static void ThrowIfResultInvalid(CreateDeviceIdCallbackInfo createInfo)
        {
            if (createInfo.ResultCode == Result.Success)
                return;

            // already exists, this is ok
            if (createInfo.ResultCode == Result.DuplicateNotAllowed)
            {
                EpicHelper.logger.Log($"Device Id already exists");
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

            if (result.ResultCode != Result.Success)
                throw new EpicSocketException($"Failed to login with Dev auth, result code={result.ResultCode}");

            return result.LocalUserId;
        }


        private static async UniTask Connect(EpicAccountId user)
        {
            var waiter = new EOSManagerFixer.Waiter<LoginCallbackInfo>();
            EOSManager.Instance.StartConnectLoginWithEpicAccount(user, waiter.Callback);
            LoginCallbackInfo result = await waiter.Wait();
            if (result.ResultCode != Result.Success)
                throw new EpicSocketException($"Failed to login with Dev auth, result code={result.ResultCode}");
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
        public struct InitializeResult
        {
            /// <summary>
            /// Exception thrown by Async task.
            /// </summary>
            public Exception Exception;

            public bool Successful => Exception == null;
        }
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
            EpicHelper.WarnResult("Set Relay Controls", result);
        }


        #region SocketFactory overrides
        public override int MaxPacketSize => P2PInterface.MaxPacketSize;

        public override ISocket CreateServerSocket()
        {
            return new EpicSocket(EOSManager.Instance);
        }

        public override ISocket CreateClientSocket()
        {

            return new EpicSocket(EOSManager.Instance);
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

    internal sealed class EpicSocket : ISocket
    {
        static void Verbose(string log)
        {
#if UNITY_EDITOR
            Debug.Log(log);
#else
            Console.WriteLine(log);
#endif
        }

        bool isClosed;
        RelayHandle _relayHandle;

        readonly EOSManager.EOSSingleton _eos;
        readonly P2PInterface _p2p;
        readonly ProductUserId _localUser;

        SendPacketOptions _sendOptions;
        ReceivePacketOptions _receiveOptions;

        int _lastTickedFrame;
        ReceivedPacket _receivedPacket;
        bool _isClient;
        readonly EpicEndPoint _receiveEndPoint = new EpicEndPoint();

        public EpicSocket(EOSManager.EOSSingleton eos)
        {
            _eos = eos;
            _p2p = _eos.GetEOSP2PInterface();
            _localUser = EOSManager.Instance.GetProductUserId();
        }

        void ThrowIfActive()
        {
            if (_relayHandle.openId != 0 || _relayHandle.closeId != 0)
                throw new InvalidOperationException("Socket already bound");
        }

        public void Bind(IEndPoint endPoint)
        {
            ThrowIfActive();

            setupRelay();
        }

        public void Connect(IEndPoint _endPoint)
        {
            ThrowIfActive();
            _isClient = true;

            _receiveEndPoint.CopyFrom((EpicEndPoint)_endPoint);

            setupRelay();
        }

        void setupRelay()
        {
            _relayHandle = EpicHelper.EnableRelay(_p2p, _localUser, openCallback, closeCallback);
            _sendOptions = EpicHelper.CreateSendOptions(_localUser);
            _receiveOptions = EpicHelper.CreateReceiveOptions(_localUser);
            if (EpicHelper.logger.LogEnabled()) EpicHelper.logger.Log($"Relay set up, localUser={_localUser}");
        }

        void openCallback(OnIncomingConnectionRequestInfo data)
        {
            bool validHost = checkRemoteUser(data.RemoteUserId);
            if (!validHost)
            {
                EpicHelper.logger.LogError($"User ({data.RemoteUserId}) tried to connect to client");
                return;
            }

            var options = new AcceptConnectionOptions()
            {
                LocalUserId = _localUser,
                RemoteUserId = data.RemoteUserId,
                // todo do we need to need to create new here
                SocketId = EpicHelper.CreateSocketId()
            };
            Result result = _p2p.AcceptConnection(options);
            EpicHelper.WarnResult("Accept Connection", result);
        }

        bool checkRemoteUser(ProductUserId remoteUser)
        {
            // and server doesn't need to check remote user
            if (!_isClient)
                return true;

            // check that remote user is host
            return _receiveEndPoint.UserId == remoteUser;
        }

        void closeCallback(OnRemoteConnectionClosedInfo data)
        {
            // isClient
            if (_isClient)
            {
                Close();
            }

            if (EpicHelper.logger.WarnEnabled()) EpicHelper.logger.LogWarning($"Connection closed with reason: {data.Reason}");
        }

        public void Close()
        {
            // todo do we need to call close on p2p?
            // only disable if sdk is loaded
            if (EOSManagerFixer.IsLoaded())
            {
                EpicHelper.DisableRelay(_p2p, _relayHandle);
            }
            _relayHandle = default;

            isClosed = true;
        }


        public bool Poll()
        {
            if (isClosed) return false;

            // first time this tick?
            if (_lastTickedFrame != Time.frameCount)
            {
                _eos.Tick();
                _lastTickedFrame = Time.frameCount;
            }

            // todo do we need to do anything with socketid or channel?
            Result result = _p2p.ReceivePacket(_receiveOptions, out _receivedPacket.userId, out SocketId _, out byte _, out _receivedPacket.data);

            if (result != Result.Success && result != Result.NotFound) // log for results other than Success/NotFound
                EpicHelper.WarnResult("Receive Packet", result);

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
            Verbose($"Receive {length} bytes from {_receivedPacket.userId}");
            return length;
        }

        public void Send(IEndPoint endPoint, byte[] packet, int length)
        {
            if (isClosed) return;

            setEndPoint(endPoint);

            // send option has no length field, we have to copy to new array
            // todo avoid allocation
            _sendOptions.Data = packet.Take(length).ToArray();

            Result result = _p2p.SendPacket(_sendOptions);

            EpicHelper.WarnResult("Send Packet", result);

            Verbose($"Send {length} bytes to {_receivedPacket.userId}");
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

        struct ReceivedPacket
        {
            public ProductUserId userId;
            public byte[] data;
        }
    }

    internal struct RelayHandle
    {
        public ulong openId;
        public ulong closeId;
    }

    internal class EpicHelper
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

        protected EpicHelper() { }

        /// <summary>
        /// Starts relay as server, allows new connections
        /// </summary>
        /// <param name="notifyId"></param>
        /// <param name="socketName"></param>
        public static RelayHandle EnableRelay(P2PInterface p2p, ProductUserId localUser, OnIncomingConnectionRequestCallback openCallback, OnRemoteConnectionClosedCallback closedCallback)
        {
            var openOptions = new AddNotifyPeerConnectionRequestOptions { LocalUserId = localUser, };
            var closeOptions = new AddNotifyPeerConnectionClosedOptions { LocalUserId = localUser, };

            RelayHandle handle = default;
            handle.openId = p2p.AddNotifyPeerConnectionRequest(openOptions, null, openCallback);
            handle.closeId = p2p.AddNotifyPeerConnectionClosed(closeOptions, null, closedCallback);

            if (handle.openId == Common.InvalidNotificationid)
                throw new EpicSocketException("Failed add ConnectionRequest");

            if (handle.closeId == Common.InvalidNotificationid)
                throw new EpicSocketException("Failed add ConnectionClosed");

            return handle;
        }

        public static void DisableRelay(P2PInterface p2p, RelayHandle handle)
        {
            if (handle.openId != Common.InvalidNotificationid)
                p2p.RemoveNotifyPeerConnectionRequest(handle.openId);

            if (handle.closeId != Common.InvalidNotificationid)
                p2p.RemoveNotifyPeerConnectionRequest(handle.closeId);
        }

        public static SocketId CreateSocketId()
        {
            return new SocketId() { SocketName = "Game" };
        }

        public static SendPacketOptions CreateSendOptions(ProductUserId localUser)
        {
            // random name length 20
            //string socketName = Guid.NewGuid().ToString("N").Substring(0, 20);


            return new SendPacketOptions()
            {
                AllowDelayedDelivery = true,
                Channel = 0,
                LocalUserId = localUser,
                Reliability = PacketReliability.UnreliableUnordered,
                SocketId = CreateSocketId(),

                RemoteUserId = null,
                Data = null,
            };
        }

        public static ReceivePacketOptions CreateReceiveOptions(ProductUserId localUser)
        {
            return new ReceivePacketOptions
            {
                LocalUserId = localUser,
                MaxDataSizeBytes = P2PInterface.MaxPacketSize,
                RequestedChannel = 0,
            };
        }
    }
}

