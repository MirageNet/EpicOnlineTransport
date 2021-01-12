#region Statements

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Epic.Core;
using Epic.Logging;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using Mirror;
using UnityEngine;

#endregion

namespace EpicTransport
{
    [DisallowMultipleComponent, RequireComponent(typeof(EpicManager))]
    public class EpicTransport : Transport
    {
        #region Fields

        static readonly ILogger Logger = LogFactory.GetLogger(typeof(EpicTransport));

        [Header("Transport Options")]

        [SerializeField] private EpicOptions _epicOptions;

        [Header("Debug Information")]
        [SerializeField] protected internal bool transportDebug = false;

        protected internal EpicManager EpicManager;
        private Server _server;
        private Client _client;
        private AutoResetUniTaskCompletionSource _listenCompletionSource;

        public Action<Result, string> Error;

        #endregion

        #region Unity Methods

        private void OnValidate()
        {
            EpicManager = GetComponent<EpicManager>();

            _epicOptions.Channels = new PacketReliability[2];
            _epicOptions.Channels[0] = PacketReliability.ReliableOrdered;
            _epicOptions.Channels[1] = PacketReliability.UnreliableUnordered;
        }

        #endregion

        /// <summary>
        ///     Shut down the transport, both as client and server
        /// </summary>
        public void Shutdown()
        {
#if UNITY_EDITOR
            if (Logger.logEnabled)
#endif
                if (transportDebug)
                    DebugLogger.RegularDebugLog("[EpicTransport] - Shutting down.");

            _server?.Disconnect();
            _server = null;

            _client?.Disconnect();
            _client = null;
        }

        #region Transport Overrides

        /// <summary>
        ///     Type of connection scheme transport supports.
        /// </summary>

        public override IEnumerable<string> Scheme => new[] { "epic" };

        public override UniTask ListenAsync()
        {
            _server = new Server(this, _epicOptions);

            _server.StartListening();

            _listenCompletionSource = AutoResetUniTaskCompletionSource.Create();

            return _listenCompletionSource.Task;
        }

        /// <summary>
        ///     Disconnect the server and client and shutdown.
        /// </summary>
        public override void Disconnect()
        {
            Shutdown();
        }

        /// <summary>
        ///     Does this transport support this specific platform.
        /// </summary>
        public override bool Supported
        {
            get
            {
                return Application.platform == RuntimePlatform.WindowsEditor ||
                       Application.platform == RuntimePlatform.WindowsPlayer ||
                       Application.platform == RuntimePlatform.LinuxEditor ||
                       Application.platform == RuntimePlatform.LinuxPlayer ||
                       Application.platform == RuntimePlatform.OSXEditor ||
                       Application.platform == RuntimePlatform.OSXPlayer;
            }
        }

        /// <summary>
        ///     Connect clients async to mirror backend.
        /// </summary>
        /// <param name="uri">The address we want to connect to using steam ids.</param>
        /// <returns></returns>
        public override async UniTask<IConnection> ConnectAsync(Uri uri)
        {
            _epicOptions.ConnectionAddress = ProductUserId.FromString(uri.Host);

            _client = new Client(this, _epicOptions);

            _client.Error += (errorCode, message) => Error?.Invoke(errorCode, message);

            await _client.ConnectAsync();

            return _client;
        }

        /// <summary>
        ///     Server's different connection scheme's
        /// </summary>
        /// <returns>Returns back a array of supported scheme's</returns>
        public override IEnumerable<Uri> ServerUri()
        {
            var steamBuilder = new UriBuilder
            {
                Scheme = "epic",
                Host = EpicManager.AccountId.ProductUserId.ToString()
            };

            return new[] { steamBuilder.Uri };
        }

        #endregion
    }
}
