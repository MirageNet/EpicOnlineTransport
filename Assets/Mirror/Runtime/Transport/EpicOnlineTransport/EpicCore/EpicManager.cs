#region Statements

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Epic.Logging;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
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
using LoginOptions = Epic.OnlineServices.Auth.LoginOptions;

#endregion

namespace Epic.Core
{
    [DisallowMultipleComponent]
    public class EpicManager : MonoBehaviour
    {
        #region Fields

        [Header("Debug Information")]
        [SerializeField] private bool _enableDebugLogs;
        [SerializeField] private LogLevel _epicLoggingLevel = LogLevel.Error;

        [Header("Epic Device Auth Tool Settings")]
        [SerializeField, Tooltip("Set this to the port you are using the dev auth tool on.")] private int _devAuthToolPort = 7878;
        [SerializeField, Tooltip("The name you set for dev auth tool after login.")]
        private string _devAuthToolName = "";

        [Header("Epic Account Login Settings")]
        [SerializeField] private bool _authInterfaceLogin = false;
        [SerializeField] private ExternalCredentialType _connectInterfaceCredentialType = ExternalCredentialType.DeviceidAccessToken;
        [SerializeField] private LoginCredentialType _authInterfaceCredentialType = LoginCredentialType.AccountPortal;
        [SerializeField, Tooltip("The name you want the fake creation of new users on the fly to be.")] private string displayName = "User";

        [Header("Epic Product Settings")]
        [SerializeField] private Options _options;

        [Header("Epic Manager Settings.")]
        [SerializeField, Range(.00f, .1f)] private float _tickTime;

        private string _authInterfaceLoginCredentialId = string.Empty;
        private string _authInterfaceCredentialToken = string.Empty;
        private string _connectInterfaceCredentialToken = string.Empty;

        public Action OnInitialized;
        public Action OnLoggedIn;

        #endregion

        #region Properties

        /// <summary>
        ///     Has the epic platform been started yet.
        /// </summary>
        public bool Initialized =>
            Platform != null && AccountId != null;

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
        ///     Quick access to authenticate interface.
        /// </summary>
        private AuthInterface AuthInterface => Platform.GetAuthInterface();

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
        private void Awake()
        {
            if (!Initialized)
                Initialize();
        }

        public void OnDisable()
        {

        }

        /// <summary>
        ///     Shutdown epic services.
        /// </summary>
        public void OnDestroy()
        {
            if (_enableDebugLogs)
                DebugLogger.RegularDebugLog("[EpicManager] - Releasing epic resources and shutting down epic services.");

            if (Application.isEditor)
            {
                LogoutOptions logoutOptions =
                    new LogoutOptions { LocalUserId = AccountId.EpicAccountId };

                // Callback might not be called since we call Logout in OnDestroy()
                AuthInterface.Logout(logoutOptions, null, OnAuthInterfaceLogout);
            }
            else
            {
                Platform.Release();
                Platform = null;
                PlatformInterface.Shutdown();
            }
        }

        #endregion

        #region Class Specific

        /// <summary>
        ///     Hack to logout for editor only.
        /// </summary>
        private void OnAuthInterfaceLogout(LogoutCallbackInfo logoutCallbackInfo){}

        /// <summary>
        ///     Initialize epic sdk.
        /// </summary>
        /// <returns>Returns back whether or not the engine initialized correctly.</returns>
        private void Initialize()
        {
            if (_enableDebugLogs)
                DebugLogger.RegularDebugLog("[EpicManager] - Initializing epic services.");

            InitializeOptions initializeOptions =
                new InitializeOptions {ProductName = _options.ProductName, ProductVersion = _options.ProductVersion};

            Result initializeResult = PlatformInterface.Initialize(initializeOptions);

            // This code is called each time the game is run in the editor, so we catch the case where the SDK has already been initialized in the editor.
            bool isAlreadyConfiguredInEditor = Application.isEditor && initializeResult == Result.AlreadyConfigured;

            if (initializeResult != Result.Success && !isAlreadyConfiguredInEditor)
            {
                throw new Exception("[EpicManager] - Failed to initialize platform: " + initializeResult);
            }

            if (_enableDebugLogs)
            {
                LoggingInterface.SetLogLevel(LogCategory.AllCategories, _epicLoggingLevel);
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
                    DeploymentId = _options.DeploymentId,
                    TickBudgetInMilliseconds = (uint) _tickTime * 1000
                };

            Platform = PlatformInterface.Create(options);

            if (Platform != null)
            {
                if (_enableDebugLogs)
                    DebugLogger.RegularDebugLog("[EpicManager] - Initialization of epic services complete.");

                // Process epic services in a separate task.
                _ = UniTask.Run(Tick);

                // If we use the Auth interface then only login into the Connect interface after finishing the auth interface login
                // If we don't use the Auth interface we can directly login to the Connect interface
                if (_authInterfaceLogin)
                {
                    if (_authInterfaceCredentialType == LoginCredentialType.Developer)
                    {
                        _authInterfaceLoginCredentialId = $"localhost:{_devAuthToolPort}";
                        _authInterfaceCredentialToken = _devAuthToolName;
                    }

                    // Login to Auth Interface
                    LoginOptions loginOptions = new LoginOptions
                    {
                        Credentials = new OnlineServices.Auth.Credentials
                        {
                            Type = _authInterfaceCredentialType,
                            Id = _authInterfaceLoginCredentialId,
                            Token = _authInterfaceCredentialToken
                        },
                        ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence
                    };

                    AuthInterface.Login(loginOptions, null, OnAuthInterfaceLogin);
                }
                else
                {
                    // Login to Connect Interface
                    if (_connectInterfaceCredentialType == ExternalCredentialType.DeviceidAccessToken)
                    {
                        CreateDeviceIdOptions createDeviceIdOptions =
                            new CreateDeviceIdOptions
                            {
                                DeviceModel = Application.platform.ToString()
                            };
                        ConnectInterface.CreateDeviceId(createDeviceIdOptions, null, OnCreateDeviceId);
                    }
                    else
                    {
                        ConnectInterfaceLogin();
                    }
                }

                OnInitialized?.Invoke();

                return;
            }

            DebugLogger.RegularDebugLog(
                $"[EpicManager] - Failed to create platform. Ensure the relevant {typeof(Options)} are set or passed into the application as arguments.");
        }

        /// <summary>
        ///     Need to process updates from epic services every xx ms based on user variable. Default 0ms.
        /// </summary>
        private async Task Tick()
        {
            while (!(Platform is null))
            {
                Platform.Tick();

                await UniTask.Delay((int) (_tickTime * 1000));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginCallbackInfo"></param>
        private void OnAuthInterfaceLogin(OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo)
        {
            if (loginCallbackInfo.ResultCode == Result.Success)
            {
                if (_enableDebugLogs)
                    DebugLogger.RegularDebugLog("[EpicManager] - Auth Interface Login succeeded");

                Result result = loginCallbackInfo.LocalUserId.ToString(out string accountIdString);

                if (Result.Success == result)
                {
                    if (_enableDebugLogs)
                        DebugLogger.RegularDebugLog("[EpicManager] - User ID:" + accountIdString);

                    AccountId = new EpicUser(loginCallbackInfo.LocalUserId, null);
                }

                ConnectInterfaceLogin();
            }
            else
            {
                if (_enableDebugLogs)
                    DebugLogger.RegularDebugLog("[EpicManager] - Login returned " + loginCallbackInfo.ResultCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createDeviceIdCallbackInfo"></param>
        private void OnCreateDeviceId(CreateDeviceIdCallbackInfo createDeviceIdCallbackInfo)
        {
            if (createDeviceIdCallbackInfo.ResultCode == Result.Success || createDeviceIdCallbackInfo.ResultCode == Result.DuplicateNotAllowed)
            {
                ConnectInterfaceLogin();
            }
            else
            {
                if (_enableDebugLogs)
                    DebugLogger.RegularDebugLog("[EpicManager] - Device ID creation returned " +
                                                createDeviceIdCallbackInfo.ResultCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ConnectInterfaceLogin()
        {
            var loginOptions = new OnlineServices.Connect.LoginOptions();

            switch (_connectInterfaceCredentialType)
            {
                case ExternalCredentialType.Epic:
                {
                    Result result = AuthInterface.CopyUserAuthToken(new CopyUserAuthTokenOptions(), AccountId.EpicAccountId,
                        out Token token);

                    if (result == Result.Success)
                    {
                        _connectInterfaceCredentialToken = token.AccessToken;
                    }
                    else
                    {
                        if (_enableDebugLogs)
                            DebugLogger.RegularDebugLog("[EpicManager] - Failed to retrieve User Auth Token");
                    }

                    break;
                }
                case ExternalCredentialType.DeviceidAccessToken:
                    loginOptions.UserLoginInfo = new UserLoginInfo {DisplayName = displayName};
                    break;
            }

            loginOptions.Credentials =
                new OnlineServices.Connect.Credentials
                {
                    Type = _connectInterfaceCredentialType, Token = _connectInterfaceCredentialToken
                };

            ConnectInterface.Login(loginOptions, null, OnConnectInterfaceLogin);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginCallbackInfo"></param>
        private void OnConnectInterfaceLogin(OnlineServices.Connect.LoginCallbackInfo loginCallbackInfo)
        {
            if (loginCallbackInfo.ResultCode == Result.Success)
            {
                if (_enableDebugLogs)
                    DebugLogger.RegularDebugLog("[EpicManager] - Connect Interface Login succeeded");

                Result result = loginCallbackInfo.LocalUserId.ToString(out string productIdString);

                if (Result.Success == result)
                {
                    if (_enableDebugLogs)
                        DebugLogger.RegularDebugLog("[EpicManager] - User Product ID:" + productIdString);

                    AccountId = new EpicUser(AccountId.EpicAccountId, loginCallbackInfo.LocalUserId);

                    OnLoggedIn?.Invoke();
                }
            }
            else
            {
                if (_enableDebugLogs)
                    DebugLogger.RegularDebugLog("[EpicManager] - Login returned " + loginCallbackInfo.ResultCode);

                ConnectInterface.CreateUser(
                    new CreateUserOptions {ContinuanceToken = loginCallbackInfo.ContinuanceToken}, null, cb =>
                    {
                        if (cb.ResultCode != Result.Success)
                        {
                            if (_enableDebugLogs)
                                DebugLogger.RegularDebugLog(
                                    $"[EpicManager] - User creation failed. Result: {cb.ResultCode}");
                            return;
                        }

                        AccountId = new EpicUser(AccountId.EpicAccountId, loginCallbackInfo.LocalUserId);

                        ConnectInterfaceLogin();
                    });
            }
        }
        #endregion
    }
}
