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

        [SerializeField] private EpicOptions _epicOptions = new EpicOptions();

        [Header("Debug Information")]
        [SerializeField] protected internal bool transportDebug = false;

        protected internal EpicManager EpicManager;
        private Server _server;
        private Client _client;
        private AutoResetUniTaskCompletionSource _listenCompletionSource;

        public Action<Result, string> Error;

        #endregion

        #region Unity Methods

        private void Start()
        {
            _epicOptions.Channels = new PacketReliability[2];
            _epicOptions.Channels[0] = PacketReliability.ReliableOrdered;
            _epicOptions.Channels[1] = PacketReliability.UnreliableUnordered;

            EpicManager = GetComponent<EpicManager>();
        }

        private void OnApplicationQuit()
        {
            Shutdown();
        }

        #endregion

        /// <summary>
        ///     Shut down the transport, both as client and server
        /// </summary>
        private void Shutdown()
        {
            if (Logger.logEnabled)
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
            if (!EpicManager.Initialized)
            {
                if (Logger.logEnabled)
                    if (transportDebug)
                        DebugLogger.RegularDebugLog("Epic not initialized. Server could not be started.");

                return UniTask.CompletedTask;
            }

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
                return EpicManager.Initialized;
            }
        }

        /// <summary>
        ///     Connect clients async to mirror backend.
        /// </summary>
        /// <param name="uri">The address we want to connect to using steam ids.</param>
        /// <returns></returns>
        public override async UniTask<IConnection> ConnectAsync(Uri uri)
        {
            if (!EpicManager.Initialized)
            {
                if (Logger.logEnabled)
                    if (transportDebug)
                        DebugLogger.RegularDebugLog("Epic not initialized. Client could not be started.");

                return null;
            }

            _epicOptions.ConnectionAddress = ProductUserId.FromString(uri.Host);

            _client = new Client(this, _epicOptions, false);

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
            EpicManager.AccountId.ProductUserId.ToString(out string host);

            var steamBuilder = new UriBuilder
            {
                Scheme = "epic",
                Host = host
            };

            return new[] { steamBuilder.Uri };
        }

        #endregion
    }
}
