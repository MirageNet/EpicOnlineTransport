using Boo.Lang;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using UnityEngine;

namespace Mirage.Sockets.EpicSocket
{
    public class LobbyHelper
    {
        readonly ProductUserId _localUser;
        readonly LobbyInterface _lobby;

        public LobbyHelper(ProductUserId localUser, LobbyInterface lobby)
        {
            this._localUser = localUser;
            this._lobby = lobby;
        }

        public async UniTask StartLobby(string id)
        {
            var options = new CreateLobbyOptions
            {
                LocalUserId = _localUser,
                MaxLobbyMembers = 4,
                PermissionLevel = LobbyPermissionLevel.Publicadvertised,
                PresenceEnabled = false,
                AllowInvites = false,
                DisableHostMigration = true,
                EnableRTCRoom = false,
                LobbyId = id,
            };

            var awaiter = new AsyncWaiter<CreateLobbyCallbackInfo>();
            _lobby.CreateLobby(options, null, awaiter.Callback);
            var result = await awaiter.Wait();
            EpicLogger.WarnResult("Create Lobby", result.ResultCode);
            Debug.Assert(result.LobbyId == id);
        }

        public async UniTask<List<LobbyDetails>> GetAllLobbies()
        {
            const int MaxResults = 10;
            EpicLogger.WarnResult("Create Search", _lobby.CreateLobbySearch(new CreateLobbySearchOptions { MaxResults = MaxResults, }, out var searchHandle));

            var options = new LobbySearchFindOptions
            {
                LocalUserId = _localUser
            };
            var awaiter = new AsyncWaiter<LobbySearchFindCallbackInfo>();
            searchHandle.Find(options, null, awaiter.Callback);
            var result = await awaiter.Wait();
            EpicLogger.WarnResult("Search Find", result.ResultCode);

            var getOption = new LobbySearchCopySearchResultByIndexOptions();
            var lobbyDetails = new List<LobbyDetails>();
            for (var i = 0; i < MaxResults; i++)
            {
                getOption.LobbyIndex = (uint)i;
                var getResult = searchHandle.CopySearchResultByIndex(getOption, out var lobbyDetail);
                if (getResult == Result.Success)
                {
                    lobbyDetails.Add(lobbyDetail);
                }
                else
                {
                    EpicLogger.WarnResult("Search Get", result.ResultCode);
                }
            }

            return lobbyDetails;
        }
    }
}

