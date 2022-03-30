using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.P2P;
using Mirage.Logging;
using Mirage.SocketLayer;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mirage.Sockets.Epic
{
    public class EpicSocketFactory : SocketFactory, IEOSCoroutineOwner
    {
        private EOSManager.EOSSingleton _eos;

        public override int MaxPacketSize => P2PInterface.MaxPacketSize;

        private void Awake()
        {
            //_eos = EOSManager.Instance;
            //_eos.Init(this);
        }
        private async void Start()
        {
            // give chance for eos to init,
            // todo find callback to check if eos is ready
            await Task.Delay(5000);

            _eos = EOSManager.Instance;
            ConnectInterface connect = _eos.GetEOSConnectInterface();
            CreateDeviceIdCallbackInfo createInfo = await CreateDeviceIdAsync(connect);

            // created or already exists
            if (createInfo.ResultCode == Result.DuplicateNotAllowed)
            {
                Debug.Log($"<color=red>Device Id already exists</color>");
            }
            else
            {
                EpicHelper.WarnResult("Create DeviceId", createInfo.ResultCode);
            }

            if (createInfo.ResultCode != Result.Success && createInfo.ResultCode != Result.DuplicateNotAllowed)
            {
                return;
            }

            LoginCallbackInfo loginInfo = await LoginAsync();

            EpicHelper.WarnResult("Login Callback", loginInfo.ResultCode);

            ChangeRelayStatus();


            Debug.Log($"<color=red>Relay set up, localUser={_eos.GetLocalUserId()}</color>");
            Debug.Log($"<color=red>Relay set up, localUser={loginInfo.LocalUserId}</color>");
        }

        private static async Task<CreateDeviceIdCallbackInfo> CreateDeviceIdAsync(ConnectInterface connect)
        {
            var createOptions = new CreateDeviceIdOptions()
            {
                DeviceModel = "DemoModel"
            };
            CreateDeviceIdCallbackInfo createInfo = null;
            connect.CreateDeviceId(createOptions, null, info => createInfo = info);
            while (createInfo == null)
                await Task.Yield();
            return createInfo;
        }


        private async Task<LoginCallbackInfo> LoginAsync()
        {
            var credentials = new Credentials
            {
                Type = ExternalCredentialType.DeviceidAccessToken,
            };
            var userLoginInfo = new UserLoginInfo { DisplayName = "Mirage User" };
            var options = new LoginOptions() { Credentials = credentials, UserLoginInfo = userLoginInfo, };

            LoginCallbackInfo loginInfo = null;

            _eos.StartConnectLoginWithOptions(options, info => loginInfo = info);
            while (loginInfo == null)
                await Task.Yield();
            return loginInfo;
        }

        private void OnDestroy()
        {
            //_eos = EOSManager.Instance;
            //if (_eos != null)
            //    _eos.OnShutdown();
        }
        private void OnApplicationQuit()
        {
            //if (_eos != null)
            //    _eos.OnShutdown();
        }

        private void ChangeRelayStatus()
        {
            var setRelayControlOptions = new SetRelayControlOptions();
            setRelayControlOptions.RelayControl = RelayControl.AllowRelays;

            Result result = _eos.GetEOSP2PInterface().SetRelayControl(setRelayControlOptions);
            EpicHelper.WarnResult("Set Relay Controls", result);
        }


        public override ISocket CreateServerSocket()
        {
            _eos = EOSManager.Instance;
            return new EpicSocket(_eos);
        }


        public override ISocket CreateClientSocket()
        {
            _eos = EOSManager.Instance;

            return new EpicSocket(_eos);
        }

        public override IEndPoint GetBindEndPoint()
        {
            return new EpicEndPoint(null);
        }

        public override IEndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            return new EpicEndPoint(address);
        }

        void IEOSCoroutineOwner.StartCoroutine(IEnumerator routine)
        {
            // this should call the non-explicity methods from MonoBehaviour
            _ = StartCoroutine(routine);
        }
    }

    internal sealed class EpicEndPoint : IEndPoint
    {
        public string HostProductUserId;
        public ProductUserId UserId;

        public EpicEndPoint(string hostProductUserId)
        {
            HostProductUserId = hostProductUserId;
        }
        private EpicEndPoint(ProductUserId userId)
        {
            UserId = userId;
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
                return HostProductUserId == other.HostProductUserId;
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
                return HostProductUserId.GetHashCode();
            }
        }
    }

    [Serializable]
    public class EpicSocketException : Exception
    {
        public EpicSocketException(string message) : base(message) { }
    }

    internal sealed class EpicSocket : ISocket
    {
        bool isClosed;
        RelayHandle _relayHandle;

        readonly EOSManager.EOSSingleton _eos;
        readonly P2PInterface _p2p;
        readonly ProductUserId _localUser;

        SendPacketOptions _sendOptions;
        ReceivePacketOptions _receiveOptions;

        /// <summary>Used by client</summary>
        ProductUserId _hostId;

        int _lastTickedFrame;
        ReceivedPacket _receivedPacket;
        readonly EpicEndPoint _receiveEndPoint = new EpicEndPoint(null);

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

            var endPoint = (EpicEndPoint)_endPoint;

            if (string.IsNullOrEmpty(endPoint.HostProductUserId))
                throw new ArgumentException("Host Id is empty");

            _hostId = ProductUserId.FromString(endPoint.HostProductUserId);
            Assert.IsNotNull(_hostId);

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
                SocketId = data.SocketId
            };
            Result result = _p2p.AcceptConnection(options);
            EpicHelper.WarnResult("Accept Connection", result);
        }

        bool checkRemoteUser(ProductUserId remoteUser)
        {
            // host is null on server, and server doesn't need to check remote user
            if (_hostId == null)
                return true;

            // check that remote user is host
            return _hostId == remoteUser;
        }

        void closeCallback(OnRemoteConnectionClosedInfo data)
        {
            // isClient
            if (_hostId != null)
            {
                Close();
            }

            if (EpicHelper.logger.WarnEnabled()) EpicHelper.logger.LogWarning($"Connection closed with reason: {data.Reason}");
        }

        public void Close()
        {
            // todo do we need to call close on p2p?

            EpicHelper.DisableRelay(_p2p, _relayHandle);
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
            return _receivedPacket.data.Length;
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
        }

        private void setEndPoint(IEndPoint iEndPoint)
        {
            var endPoint = (EpicEndPoint)iEndPoint;

            // dont set remote user if this is client (it always uses hostId)
            if (_hostId != null)
            {
                Assert.AreEqual(_hostId, endPoint.UserId);
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

        public static SendPacketOptions CreateSendOptions(ProductUserId localUser)
        {
            // random name length 20
            string socketName = Guid.NewGuid().ToString("N").Substring(0, 20);
            var socketId = new SocketId() { SocketName = socketName };

            return new SendPacketOptions()
            {
                AllowDelayedDelivery = true,
                Channel = 0,
                LocalUserId = localUser,
                Reliability = PacketReliability.UnreliableUnordered,
                SocketId = socketId,

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

