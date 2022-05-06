using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Sockets.EpicSocket
{
    public class LobbyHelper
    {
        internal static readonly ILogger logger = LogFactory.GetLogger(typeof(LobbyHelper));

        readonly ProductUserId _localUser;
        readonly LobbyInterface _lobby;

        public LobbyHelper(ProductUserId localUser, LobbyInterface lobby)
        {
            _localUser = localUser;
            _lobby = lobby;
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
            CreateLobbyCallbackInfo result = await awaiter.Wait();
            logger.WarnResult("Create Lobby", result.ResultCode);
            Debug.Assert(result.LobbyId == id);
        }

        public async UniTask<List<LobbyDetails>> GetAllLobbies(uint maxResults = 10)
        {
            logger.WarnResult("Create Search", _lobby.CreateLobbySearch(new CreateLobbySearchOptions { MaxResults = maxResults, }, out LobbySearch searchHandle));

            var options = new LobbySearchFindOptions
            {
                LocalUserId = _localUser
            };
            var awaiter = new AsyncWaiter<LobbySearchFindCallbackInfo>();
            searchHandle.Find(options, null, awaiter.Callback);
            LobbySearchFindCallbackInfo result = await awaiter.Wait();
            logger.WarnResult("Search Find", result.ResultCode);

            var getOption = new LobbySearchCopySearchResultByIndexOptions();
            var lobbyDetails = new List<LobbyDetails>();
            for (int i = 0; i < maxResults; i++)
            {
                getOption.LobbyIndex = (uint)i;
                Result getResult = searchHandle.CopySearchResultByIndex(getOption, out LobbyDetails lobbyDetail);
                if (getResult == Result.Success)
                {
                    lobbyDetails.Add(lobbyDetail);
                }
                else
                {
                    logger.WarnResult("Search Get", result.ResultCode);
                }
            }

            return lobbyDetails;
        }
    }
}

