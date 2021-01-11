#region Statements

using System;
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
#if UNITY_EDITOR
        [SerializeField, Tooltip("Set this to the port you are using the dev auth tool on.")] private int _devAuthToolPort = 7777;
        [SerializeField, Tooltip("The name you set for dev auth tool after login.")]
        private string _devAuthToolName = "";
#endif

        [Header("Epic Product Settings")]
        [SerializeField] private Options _options = new Options();

        [Header("Epic Manager Settings.")]
        [SerializeField, Range(.1f, .5f)] private float _tickTime = .1f;

        [SerializeField, Tooltip("If epic account login fails create new device specific account.")] private bool _useDeviceAccountCreation = true;

        public Action UserLoggedIn;
        public Action UserLoggedOut;

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

                LoginEpicAccountUser();

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
        ///     Allows usage of connecting to epic services using an epic account.
        /// </summary>
        private void LoginEpicAccountUser()
        {
            LoginOptions EpicLoginOptions = new LoginOptions
            {
                Credentials = new OnlineServices.Auth.Credentials
                {
#if UNITY_EDITOR
                    Type = LoginCredentialType.Developer, Id = $"localhost:{_devAuthToolPort}", Token = _devAuthToolName
#else
                    Type = LoginCredentialType.AccountPortal,
#endif
                },
                ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence
            };

            AuthInterface.Login(EpicLoginOptions, null, AuthLoginCallback);
        }

        /// <summary>
        ///     The authentication process has return back information
        ///     of successful or non successful authentication of epic account.
        /// </summary>
        /// <param name="data">The data we received back from our callback.</param>
        private void AuthLoginCallback(OnlineServices.Auth.LoginCallbackInfo data)
        {
            switch (data.ResultCode)
            {
                case Result.Success:
                    CopyUserAuthTokenOptions copyAuth = new CopyUserAuthTokenOptions { };

                    Result copyResults = Platform.GetAuthInterface()
                        .CopyUserAuthToken(copyAuth, data.LocalUserId, out Token authToken);

                    if (copyResults == Result.Success)
                    {
                        OnlineServices.Connect.LoginOptions loginOptions =
                            new OnlineServices.Connect.LoginOptions
                            {
                                Credentials = new OnlineServices.Connect.Credentials
                                {
                                    Token = authToken.AccessToken, Type = ExternalCredentialType.Epic
                                },
                                UserLoginInfo = null
                            };

                        Platform.GetConnectInterface().Login(loginOptions, null, info =>
                        {
                            if (info.ResultCode == Result.Success)
                            {
                                AccountId = new EpicUser(data.LocalUserId, info.LocalUserId);

                                // Let's query for account information and cache info.
                                QueryUserInfoOptions queryUserInfoOptions = new QueryUserInfoOptions
                                {
                                    LocalUserId = data.LocalUserId, TargetUserId = data.LocalUserId
                                };

                                Platform.GetUserInfoInterface().QueryUserInfo(queryUserInfoOptions, null,
                                    callbackInfo =>
                                    {
                                        if (callbackInfo.ResultCode == Result.Success)
                                        {
                                            CopyUserInfoOptions copyUserInfoOptions = new CopyUserInfoOptions
                                            {
                                                LocalUserId = callbackInfo.LocalUserId,
                                                TargetUserId = callbackInfo.TargetUserId
                                            };

                                            Result result = Platform.GetUserInfoInterface()
                                                .CopyUserInfo(copyUserInfoOptions, out UserInfoData userInfoData);

                                            if (result == Result.Success)
                                            {
                                                if (_enableDebugLogs)
                                                    DebugLogger.RegularDebugLog(
                                                        $"<color=green>Connect Successful. Welcome {AccountId.Name}</color>");

                                                if (data.ResultCode == Result.Success)
                                                    UserLoggedIn?.Invoke();
                                            }
                                        }
                                    });
                            }
                            else
                            {
                                if (_useDeviceAccountCreation)
                                {
                                    AuthenticateDeviceUser();
                                }
                                else
                                {
                                    if (_enableDebugLogs)
                                        DebugLogger.RegularDebugLog(
                                            $"<color=red>Connect Not Successful.</color>");
                                }
                            }
                        });
                    }
                    else
                    {
                        if (_enableDebugLogs)
                            DebugLogger.RegularDebugLog(
                                $"<color=red>Auth copy Not Successful./color>");
                    }

                    break;
                default:

                    if (_enableDebugLogs)
                        DebugLogger.RegularDebugLog(
                            $"<color=red>Authentication Login Not Successful. Status: {data.ResultCode}</color>");

                    if (data.ResultCode == Result.Success)
                        UserLoggedIn?.Invoke();

                    break;
            }
        }

        /// <summary>
        ///     Authenticate epic user to the manager.
        /// </summary>
        /// <returns></returns>
        public void AuthenticateDeviceUser()
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
                OnlineServices.Connect.LoginOptions loginOptions = new OnlineServices.Connect.LoginOptions
                {
                    UserLoginInfo = new UserLoginInfo {DisplayName = "One More Night Player"},

                    Credentials = new OnlineServices.Connect.Credentials
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
        private void ConnectLoginCallback(OnlineServices.Connect.LoginCallbackInfo data)
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

                    AccountId = new EpicUser(null, data.LocalUserId);
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
