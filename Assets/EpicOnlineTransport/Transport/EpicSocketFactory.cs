using System;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using Mirage.SocketLayer;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;

namespace Mirage.Sockets.EpicSocket
{
    public class EpicSocketFactory : SocketFactory
    {
        private static InitializeStatus s_isInitialize;

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
            while (!EpicHelper.IsLoaded())
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
            Debug.Log($"<color=cyan>Connected to EOS, localUser={productId}, isNull={productId == null}</color>");

            relayHandle = new RelayHandle(EOSManager.Instance);
            relayHandle.OpenRelay();
            s_isInitialize = InitializeStatus.Initialized;
        }

        public void StartAsClient(ProductUserId remoteHost)
        {
            throw new System.NotImplementedException();
        }
        public void StartAsHost()
        {
            throw new System.NotImplementedException();
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

        enum InitializeStatus
        {
            None,
            Initializing,
            Initialized,
        }

        public struct InitializeResult
        {
            /// <summary>
            /// Exception thrown by Async task.
            /// </summary>
            public Exception Exception;

            public bool Successful => Exception == null;
        }
    }
}

