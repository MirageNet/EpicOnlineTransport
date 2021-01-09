#region Statements

using System;
using Cysharp.Threading.Tasks;
using Epic.Logging;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Ecom;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.Leaderboards;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Metrics;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.PlayerDataStorage;
using Epic.OnlineServices.Presence;
using Epic.OnlineServices.Sessions;
using Epic.OnlineServices.Stats;
using Epic.OnlineServices.TitleStorage;
using Epic.OnlineServices.UserInfo;
using UnityEngine;

#endregion

namespace Epic.Core
{
    [DisallowMultipleComponent]
    public class EpicManager : MonoBehaviour
    {
        #region Fields

        [Header("Debug Information")]
        [SerializeField] private string _loggedInAccountId;
        [SerializeField] private bool _enableDebugLogs;

        [Header("Epic Product Settings")]
        [SerializeField] private Options _options = new Options();

        [Header("Epic Manager Settings.")]
        [SerializeField, Range(.1f, .5f)] private float _tickTime = .1f;

        #endregion

        #region Properties

        /// <summary>
        ///     Has the epic platform been started yet.
        /// </summary>
        public bool Initialized => Platform != null;

        #region Epic Platforms

        /// <summary>
        ///     Quick access to the epic p2p interface.
        /// </summary>
        public P2PInterface P2PInterface => Platform.GetP2PInterface();

        /// <summary>
        ///     Quick access to epic friends interface.
        /// </summary>
        public FriendsInterface FriendsInterface => Platform.GetFriendsInterface();

        /// <summary>
        ///     Quick access to epic user interface.
        /// </summary>
        public UserInfoInterface UserInterface => Platform.GetUserInfoInterface();

        /// <summary>
        ///     Quick access to epic lobby interface.
        /// </summary>
        public LobbyInterface LobbyInterface => Platform.GetLobbyInterface();

        /// <summary>
        ///     Quick access to epic ecommerce interface.
        /// </summary>
        public EcomInterface EcomInterface => Platform.GetEcomInterface();

        /// <summary>
        ///     Quick access to epic Leader board interface.
        /// </summary>
        public LeaderboardsInterface LeaderBoardInterface => Platform.GetLeaderboardsInterface();

        /// <summary>
        ///     Quick access to epic player storage interface.
        /// </summary>
        public PlayerDataStorageInterface PlayerStorageInterface => Platform.GetPlayerDataStorageInterface();

        /// <summary>
        ///     Quick access to presence interface.
        /// </summary>
        public PresenceInterface PresenceInterface => Platform.GetPresenceInterface();

        /// <summary>
        ///     Quick access to stats interface.
        /// </summary>
        public StatsInterface StatsInterface => Platform.GetStatsInterface();

        /// <summary>
        ///     Quick access to metrics interface.
        /// </summary>
        public MetricsInterface MetricsInterface => Platform.GetMetricsInterface();

        /// <summary>
        ///     Quick access to title storage interface.
        /// </summary>
        public TitleStorageInterface TitleStorageInterface => Platform.GetTitleStorageInterface();

        /// <summary>
        ///     Quick access to session storage interface.
        /// </summary>
        public SessionsInterface SessionInterface => Platform.GetSessionsInterface();

        /// <summary>
        ///     Quick access to connect interface.
        /// </summary>
        private ConnectInterface ConnectInterface => Platform.GetConnectInterface();

        /// <summary>
        ///     Epic services main interface to all platforms.
        /// </summary>
        private PlatformInterface Platform { get; set; }

        /// <summary>
        /// </summary>
        public EpicUser AccountId { get; private set; }

        #endregion

        #endregion

        #region Unity Methods

        /// <summary>
        ///     Create and initialize a new epic service manager.
        /// </summary>
        private void Start()
        {
            if (!Initialized)
                Initialize();
        }

        /// <summary>
        ///     Shutdown epic services.
        /// </summary>
        public void OnDestroy()
        {
            if (_enableDebugLogs)
                DebugLogger.RegularDebugLog("Releasing epic resources and shutting down epic services.");

            if (!Application.isEditor && Platform != null)
            {
                Platform.Release();
                Platform = null;
                PlatformInterface.Shutdown();
            }
        }

        #endregion

        #region Class Specific

        /// <summary>
        ///     Initialize epic sdk.
        /// </summary>
        /// <returns>Returns back whether or not the engine initialized correctly.</returns>
        private void Initialize()
        {
            if (_enableDebugLogs)
                DebugLogger.RegularDebugLog("Initializing epic services.");

            InitializeOptions initializeOptions =
                new InitializeOptions {ProductName = _options.ProductName, ProductVersion = _options.ProductVersion};

            Result initializeResult = PlatformInterface.Initialize(initializeOptions);

            // This code is called each time the game is run in the editor, so we catch the case where the SDK has already been initialized in the editor.
            var isAlreadyConfiguredInEditor = Application.isEditor && initializeResult == Result.AlreadyConfigured;

            if (initializeResult != Result.Success && !isAlreadyConfiguredInEditor)
            {
                throw new System.Exception("Failed to initialize platform: " + initializeResult);
            }

            if (_enableDebugLogs)
            {
                LoggingInterface.SetLogLevel(LogCategory.AllCategories, LogLevel.Verbose);
                LoggingInterface.SetCallback(message => DebugLogger.EpicDebugLog(message));
            }

            ClientCredentials clientCredentials =
                new ClientCredentials {ClientId = _options.ClientId, ClientSecret = _options.ClientSecret};

            OnlineServices.Platform.Options options =
                new OnlineServices.Platform.Options
                {
                    ProductId = _options.ProductId,
                    SandboxId = _options.SandboxId,
                    ClientCredentials = clientCredentials,
                    IsServer = false,
                    DeploymentId = _options.DeploymentId
                };

            Platform = PlatformInterface.Create(options);

            if (Platform != null)
            {
                if (_enableDebugLogs)
                    DebugLogger.RegularDebugLog("Initialization of epic services complete.");

                // Process epic services in a separate task.
                _ = UniTask.Run(Tick);

                AuthenticateUser();

                return;
            }

            DebugLogger.RegularDebugLog(
                $"Failed to create platform. Ensure the relevant {typeof(Options)} are set or passed into the application as arguments.");
        }

        /// <summary>
        ///     Need to process updates from epic services every 100ms.
        /// </summary>
        private void Tick()
        {
            while (!(Platform is null))
            {
                Platform.Tick();

                UniTask.Delay((int) (_tickTime * 1000));
            }
        }

        /// <summary>
        ///     Authenticate epic user to the manager.
        /// </summary>
        /// <returns></returns>
        private void AuthenticateUser()
        {
            CreateDeviceIdOptions createDeviceIdOptions =
                new CreateDeviceIdOptions {DeviceModel = $"{Application.platform}"};

            ConnectInterface.CreateDeviceId(createDeviceIdOptions, null, CreateDeviceCallback);
        }

        /// <summary>
        ///     Received callback upon successful or unsuccessful device creation attempt.
        /// </summary>
        /// <param name="data">The data return back to us from epic.</param>
        private void CreateDeviceCallback(CreateDeviceIdCallbackInfo data)
        {
            if (data.ResultCode == Result.Success || data.ResultCode == Result.DuplicateNotAllowed)
            {
                var loginOptions = new LoginOptions
                {
                    UserLoginInfo = new UserLoginInfo {DisplayName = "One More Night Player"},

                    Credentials = new Credentials
                    {
                        Type = ExternalCredentialType.DeviceidAccessToken,
                        Token = null
                    }
                };

                ConnectInterface.Login(loginOptions, null, ConnectLoginCallback);
            }
            else
            {
                DebugLogger.RegularDebugLog("Device ID creation returned " + data.ResultCode, LogType.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void ConnectLoginCallback(LoginCallbackInfo data)
        {
            if (data.ResultCode == Result.Success)
            {
                if (_enableDebugLogs)
                    DebugLogger.RegularDebugLog("Login successful.");

                Result result = data.LocalUserId.ToString(out string productIdString);

                if (Result.Success == result)
                {
                    if (_enableDebugLogs)
                        DebugLogger.RegularDebugLog("User Product ID:" + productIdString);

                    AccountId = new EpicUser(data.LocalUserId);

                    _loggedInAccountId = AccountId.ProductUserId.ToString();
                }
                else
                {
                    if (_enableDebugLogs)
                        DebugLogger.RegularDebugLog("Login returned " + data.ResultCode, LogType.Error);
                }
            }
            else
            {
                if (_enableDebugLogs)
                    DebugLogger.RegularDebugLog($"Login failed. Results: {data.ResultCode}");
            }
        }

        #endregion
    }
}
