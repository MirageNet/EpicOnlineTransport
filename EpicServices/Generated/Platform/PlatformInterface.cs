// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Platform
{
	public sealed class PlatformInterface : Handle
	{
		public PlatformInterface(IntPtr innerHandle) : base(innerHandle)
		{
		}

		public const int OptionsApiLatest = 8;

		public const int LocalecodeMaxBufferLen = (LocalecodeMaxLength + 1);

		public const int LocalecodeMaxLength = 9;

		public const int CountrycodeMaxBufferLen = (CountrycodeMaxLength + 1);

		public const int CountrycodeMaxLength = 4;

		/// <summary>
		/// The most recent version of the <see cref="Initialize" /> API.
		/// </summary>
		public const int InitializeApiLatest = 3;

		/// <summary>
		/// Initialize the Epic Online Services SDK.
		/// 
		/// Before calling any other function in the SDK, clients must call this function.
		/// 
		/// This function must only be called one time and must have a corresponding <see cref="Shutdown" /> call.
		/// </summary>
		/// <param name="options">- The initialization options to use for the SDK.</param>
		/// <returns>
		/// An <see cref="Result" /> is returned to indicate success or an error.
		/// <see cref="Result.Success" /> is returned if the SDK successfully initializes.
		/// <see cref="Result.AlreadyConfigured" /> is returned if the function has already been called.
		/// <see cref="Result.InvalidParameters" /> is returned if the provided options are invalid.
		/// </returns>
		public static Result Initialize(InitializeOptions options)
		{
			var optionsInternal = Helper.CopyProperties<InitializeOptionsInternal>(options);

			int[] reservedData = new int[] { 1, 1 };
			IntPtr reservedDataAddress = IntPtr.Zero;
			Helper.TryMarshalSet(ref reservedDataAddress, reservedData);
			optionsInternal.Reserved = reservedDataAddress;

			var funcResult = EOS_Initialize(ref optionsInternal);
			Helper.TryMarshalDispose(ref optionsInternal);

			Helper.TryMarshalDispose(ref reservedDataAddress);

			var funcResultReturn = Helper.GetDefault<Result>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Tear down the Epic Online Services SDK.
		/// 
		/// Once this function has been called, no more SDK calls are permitted; calling anything after <see cref="Shutdown" /> will result in undefined behavior.
		/// </summary>
		/// <returns>
		/// An <see cref="Result" /> is returned to indicate success or an error.
		/// <see cref="Result.Success" /> is returned if the SDK is successfully torn down.
		/// <see cref="Result.NotConfigured" /> is returned if a successful call to <see cref="Initialize" /> has not been made.
		/// <see cref="Result.UnexpectedError" /> is returned if <see cref="Shutdown" /> has already been called.
		/// </returns>
		public static Result Shutdown()
		{
			var funcResult = EOS_Shutdown();
			var funcResultReturn = Helper.GetDefault<Result>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Create a single Epic Online Services Platform Instance.
		/// 
		/// The platform instance is used to gain access to the various Epic Online Services.
		/// 
		/// This function returns an opaque handle to the platform instance, and that handle must be passed to <see cref="Release" /> to release the instance.
		/// </summary>
		/// <returns>
		/// An opaque handle to the platform instance.
		/// </returns>
		public static PlatformInterface Create(Options options)
		{
			var optionsInternal = Helper.CopyProperties<OptionsInternal>(options);

			var funcResult = EOS_Platform_Create(ref optionsInternal);
			Helper.TryMarshalDispose(ref optionsInternal);

			var funcResultReturn = Helper.GetDefault<PlatformInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Release an Epic Online Services platform instance previously returned from <see cref="Create" />.
		/// 
		/// This function should only be called once per instance returned by <see cref="Create" />. Undefined behavior will result in calling it with a single instance more than once.
		/// Typically only a single platform instance needs to be created during the lifetime of a game.
		/// You should release each platform instance before calling the <see cref="Shutdown" /> function.
		/// </summary>
		public void Release()
		{
			EOS_Platform_Release(InnerHandle);
		}

		/// <summary>
		/// Notify the platform instance to do work. This function must be called frequently in order for the services provided by the SDK to properly
		/// function. For tick-based applications, it is usually desireable to call this once per-tick.
		/// </summary>
		public void Tick()
		{
			EOS_Platform_Tick(InnerHandle);
		}

		/// <summary>
		/// Get a handle to the Metrics Interface.
		/// <seealso cref="Metrics" />
		/// <seealso cref="Metrics" />
		/// </summary>
		/// <returns>
		/// <see cref="Metrics.MetricsInterface" /> handle
		/// </returns>
		public Metrics.MetricsInterface GetMetricsInterface()
		{
			var funcResult = EOS_Platform_GetMetricsInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<Metrics.MetricsInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the Auth Interface.
		/// <seealso cref="Auth" />
		/// <seealso cref="Auth" />
		/// </summary>
		/// <returns>
		/// <see cref="Auth.AuthInterface" /> handle
		/// </returns>
		public Auth.AuthInterface GetAuthInterface()
		{
			var funcResult = EOS_Platform_GetAuthInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<Auth.AuthInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the Connect Interface.
		/// <seealso cref="Connect" />
		/// <seealso cref="Connect" />
		/// </summary>
		/// <returns>
		/// <see cref="Connect.ConnectInterface" /> handle
		/// </returns>
		public Connect.ConnectInterface GetConnectInterface()
		{
			var funcResult = EOS_Platform_GetConnectInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<Connect.ConnectInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the Ecom Interface.
		/// <seealso cref="Ecom" />
		/// <seealso cref="Ecom" />
		/// </summary>
		/// <returns>
		/// <see cref="Ecom.EcomInterface" /> handle
		/// </returns>
		public Ecom.EcomInterface GetEcomInterface()
		{
			var funcResult = EOS_Platform_GetEcomInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<Ecom.EcomInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the UI Interface.
		/// <seealso cref="UI" />
		/// <seealso cref="UI" />
		/// </summary>
		/// <returns>
		/// <see cref="UI.UIInterface" /> handle
		/// </returns>
		public UI.UIInterface GetUIInterface()
		{
			var funcResult = EOS_Platform_GetUIInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<UI.UIInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the Friends Interface.
		/// <seealso cref="Friends" />
		/// <seealso cref="Friends" />
		/// </summary>
		/// <returns>
		/// <see cref="Friends.FriendsInterface" /> handle
		/// </returns>
		public Friends.FriendsInterface GetFriendsInterface()
		{
			var funcResult = EOS_Platform_GetFriendsInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<Friends.FriendsInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the Presence Interface.
		/// <seealso cref="Presence" />
		/// <seealso cref="Presence" />
		/// </summary>
		/// <returns>
		/// <see cref="Presence.PresenceInterface" /> handle
		/// </returns>
		public Presence.PresenceInterface GetPresenceInterface()
		{
			var funcResult = EOS_Platform_GetPresenceInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<Presence.PresenceInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the Sessions Interface.
		/// <seealso cref="Sessions" />
		/// <seealso cref="Sessions" />
		/// </summary>
		/// <returns>
		/// <see cref="Sessions.SessionsInterface" /> handle
		/// </returns>
		public Sessions.SessionsInterface GetSessionsInterface()
		{
			var funcResult = EOS_Platform_GetSessionsInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<Sessions.SessionsInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the Lobby Interface.
		/// <seealso cref="Lobby" />
		/// <seealso cref="Lobby" />
		/// </summary>
		/// <returns>
		/// <see cref="Lobby.LobbyInterface" /> handle
		/// </returns>
		public Lobby.LobbyInterface GetLobbyInterface()
		{
			var funcResult = EOS_Platform_GetLobbyInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<Lobby.LobbyInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the UserInfo Interface.
		/// <seealso cref="UserInfo" />
		/// <seealso cref="UserInfo" />
		/// </summary>
		/// <returns>
		/// <see cref="UserInfo.UserInfoInterface" /> handle
		/// </returns>
		public UserInfo.UserInfoInterface GetUserInfoInterface()
		{
			var funcResult = EOS_Platform_GetUserInfoInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<UserInfo.UserInfoInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the Peer-to-Peer Networking Interface.
		/// <seealso cref="P2P" />
		/// <seealso cref="P2P" />
		/// </summary>
		/// <returns>
		/// <see cref="P2P.P2PInterface" /> handle
		/// </returns>
		public P2P.P2PInterface GetP2PInterface()
		{
			var funcResult = EOS_Platform_GetP2PInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<P2P.P2PInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the PlayerDataStorage Interface.
		/// <seealso cref="PlayerDataStorage" />
		/// <seealso cref="PlayerDataStorage" />
		/// </summary>
		/// <returns>
		/// <see cref="PlayerDataStorage.PlayerDataStorageInterface" /> handle
		/// </returns>
		public PlayerDataStorage.PlayerDataStorageInterface GetPlayerDataStorageInterface()
		{
			var funcResult = EOS_Platform_GetPlayerDataStorageInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<PlayerDataStorage.PlayerDataStorageInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the TitleStorage Interface.
		/// <seealso cref="TitleStorage" />
		/// <seealso cref="TitleStorage" />
		/// </summary>
		/// <returns>
		/// <see cref="TitleStorage.TitleStorageInterface" /> handle
		/// </returns>
		public TitleStorage.TitleStorageInterface GetTitleStorageInterface()
		{
			var funcResult = EOS_Platform_GetTitleStorageInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<TitleStorage.TitleStorageInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the Achievements Interface.
		/// <seealso cref="Achievements" />
		/// <seealso cref="Achievements" />
		/// </summary>
		/// <returns>
		/// <see cref="Achievements.AchievementsInterface" /> handle
		/// </returns>
		public Achievements.AchievementsInterface GetAchievementsInterface()
		{
			var funcResult = EOS_Platform_GetAchievementsInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<Achievements.AchievementsInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the Stats Interface.
		/// <seealso cref="Stats" />
		/// <seealso cref="Stats" />
		/// </summary>
		/// <returns>
		/// <see cref="Stats.StatsInterface" /> handle
		/// </returns>
		public Stats.StatsInterface GetStatsInterface()
		{
			var funcResult = EOS_Platform_GetStatsInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<Stats.StatsInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get a handle to the Leaderboards Interface.
		/// <seealso cref="Leaderboards" />
		/// <seealso cref="Leaderboards" />
		/// </summary>
		/// <returns>
		/// <see cref="Leaderboards.LeaderboardsInterface" /> handle
		/// </returns>
		public Leaderboards.LeaderboardsInterface GetLeaderboardsInterface()
		{
			var funcResult = EOS_Platform_GetLeaderboardsInterface(InnerHandle);
			var funcResultReturn = Helper.GetDefault<Leaderboards.LeaderboardsInterface>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// This only will return the value set as the override otherwise <see cref="Result.NotFound" /> is returned.
		/// This is not currently used for anything internally.
		/// <seealso cref="Ecom" />
		/// <seealso cref="CountrycodeMaxLength" />
		/// </summary>
		/// <param name="localUserId">The account to use for lookup if no override exists.</param>
		/// <param name="outBuffer">The buffer into which the character data should be written. The buffer must be long enough to hold a string of <see cref="CountrycodeMaxLength" />.</param>
		/// <param name="inOutBufferLength">
		/// The size of the OutBuffer in characters.
		/// The input buffer should include enough space to be null-terminated.
		/// When the function returns, this parameter will be filled with the length of the string copied into OutBuffer.
		/// </param>
		/// <returns>
		/// An <see cref="Result" /> that indicates whether the active country code string was copied into the OutBuffer.
		/// <see cref="Result.Success" /> if the information is available and passed out in OutBuffer
		/// <see cref="Result.InvalidParameters" /> if you pass a null pointer for the out parameter
		/// <see cref="Result.NotFound" /> if there is not an override country code for the user.
		/// <see cref="Result.LimitExceeded" /> - The OutBuffer is not large enough to receive the country code string. InOutBufferLength contains the required minimum length to perform the operation successfully.
		/// </returns>
		public Result GetActiveCountryCode(EpicAccountId localUserId, System.Text.StringBuilder outBuffer, ref int inOutBufferLength)
		{
			var funcResult = EOS_Platform_GetActiveCountryCode(InnerHandle, localUserId.InnerHandle, outBuffer, ref inOutBufferLength);
			var funcResultReturn = Helper.GetDefault<Result>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get the active locale code that the SDK will send to services which require it.
		/// This returns the override value otherwise it will use the locale code of the given user.
		/// This is used for localization. This follows ISO 639.
		/// <seealso cref="Ecom" />
		/// <seealso cref="LocalecodeMaxLength" />
		/// </summary>
		/// <param name="localUserId">The account to use for lookup if no override exists.</param>
		/// <param name="outBuffer">The buffer into which the character data should be written. The buffer must be long enough to hold a string of <see cref="LocalecodeMaxLength" />.</param>
		/// <param name="inOutBufferLength">
		/// The size of the OutBuffer in characters.
		/// The input buffer should include enough space to be null-terminated.
		/// When the function returns, this parameter will be filled with the length of the string copied into OutBuffer.
		/// </param>
		/// <returns>
		/// An <see cref="Result" /> that indicates whether the active locale code string was copied into the OutBuffer.
		/// <see cref="Result.Success" /> if the information is available and passed out in OutBuffer
		/// <see cref="Result.InvalidParameters" /> if you pass a null pointer for the out parameter
		/// <see cref="Result.NotFound" /> if there is neither an override nor an available locale code for the user.
		/// <see cref="Result.LimitExceeded" /> - The OutBuffer is not large enough to receive the locale code string. InOutBufferLength contains the required minimum length to perform the operation successfully.
		/// </returns>
		public Result GetActiveLocaleCode(EpicAccountId localUserId, System.Text.StringBuilder outBuffer, ref int inOutBufferLength)
		{
			var funcResult = EOS_Platform_GetActiveLocaleCode(InnerHandle, localUserId.InnerHandle, outBuffer, ref inOutBufferLength);
			var funcResultReturn = Helper.GetDefault<Result>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get the override country code that the SDK will send to services which require it.
		/// This is not currently used for anything internally.
		/// <seealso cref="Ecom" />
		/// <seealso cref="CountrycodeMaxLength" />
		/// </summary>
		/// <param name="outBuffer">The buffer into which the character data should be written. The buffer must be long enough to hold a string of <see cref="CountrycodeMaxLength" />.</param>
		/// <param name="inOutBufferLength">
		/// The size of the OutBuffer in characters.
		/// The input buffer should include enough space to be null-terminated.
		/// When the function returns, this parameter will be filled with the length of the string copied into OutBuffer.
		/// </param>
		/// <returns>
		/// An <see cref="Result" /> that indicates whether the override country code string was copied into the OutBuffer.
		/// <see cref="Result.Success" /> if the information is available and passed out in OutBuffer
		/// <see cref="Result.InvalidParameters" /> if you pass a null pointer for the out parameter
		/// <see cref="Result.LimitExceeded" /> - The OutBuffer is not large enough to receive the country code string. InOutBufferLength contains the required minimum length to perform the operation successfully.
		/// </returns>
		public Result GetOverrideCountryCode(System.Text.StringBuilder outBuffer, ref int inOutBufferLength)
		{
			var funcResult = EOS_Platform_GetOverrideCountryCode(InnerHandle, outBuffer, ref inOutBufferLength);
			var funcResultReturn = Helper.GetDefault<Result>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Get the override locale code that the SDK will send to services which require it.
		/// This is used for localization. This follows ISO 639.
		/// <seealso cref="Ecom" />
		/// <seealso cref="LocalecodeMaxLength" />
		/// </summary>
		/// <param name="outBuffer">The buffer into which the character data should be written. The buffer must be long enough to hold a string of <see cref="LocalecodeMaxLength" />.</param>
		/// <param name="inOutBufferLength">
		/// The size of the OutBuffer in characters.
		/// The input buffer should include enough space to be null-terminated.
		/// When the function returns, this parameter will be filled with the length of the string copied into OutBuffer.
		/// </param>
		/// <returns>
		/// An <see cref="Result" /> that indicates whether the override locale code string was copied into the OutBuffer.
		/// <see cref="Result.Success" /> if the information is available and passed out in OutBuffer
		/// <see cref="Result.InvalidParameters" /> if you pass a null pointer for the out parameter
		/// <see cref="Result.LimitExceeded" /> - The OutBuffer is not large enough to receive the locale code string. InOutBufferLength contains the required minimum length to perform the operation successfully.
		/// </returns>
		public Result GetOverrideLocaleCode(System.Text.StringBuilder outBuffer, ref int inOutBufferLength)
		{
			var funcResult = EOS_Platform_GetOverrideLocaleCode(InnerHandle, outBuffer, ref inOutBufferLength);
			var funcResultReturn = Helper.GetDefault<Result>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Set the override country code that the SDK will send to services which require it.
		/// This is not currently used for anything internally.
		/// <seealso cref="Ecom" />
		/// <seealso cref="CountrycodeMaxLength" />
		/// </summary>
		/// <returns>
		/// An <see cref="Result" /> that indicates whether the override country code string was saved.
		/// <see cref="Result.Success" /> if the country code was overridden
		/// <see cref="Result.InvalidParameters" /> if you pass an invalid country code
		/// </returns>
		public Result SetOverrideCountryCode(string newCountryCode)
		{
			var funcResult = EOS_Platform_SetOverrideCountryCode(InnerHandle, newCountryCode);
			var funcResultReturn = Helper.GetDefault<Result>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Set the override locale code that the SDK will send to services which require it.
		/// This is used for localization. This follows ISO 639.
		/// <seealso cref="Ecom" />
		/// <seealso cref="LocalecodeMaxLength" />
		/// </summary>
		/// <returns>
		/// An <see cref="Result" /> that indicates whether the override locale code string was saved.
		/// <see cref="Result.Success" /> if the locale code was overridden
		/// <see cref="Result.InvalidParameters" /> if you pass an invalid locale code
		/// </returns>
		public Result SetOverrideLocaleCode(string newLocaleCode)
		{
			var funcResult = EOS_Platform_SetOverrideLocaleCode(InnerHandle, newLocaleCode);
			var funcResultReturn = Helper.GetDefault<Result>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Checks if the app was launched through the Epic Launcher, and relaunches it through the Epic Launcher if it wasn't.
		/// </summary>
		/// <returns>
		/// An <see cref="Result" /> is returned to indicate success or an error.
		/// <see cref="Result.Success" /> is returned if the app is being restarted. You should quit your process as soon as possible.
		/// <see cref="Result.NoChange" /> is returned if the app was already launched through the Epic Launcher, and no action needs to be taken.
		/// <see cref="Result.UnexpectedError" /> is returned if the LauncherCheck module failed to initialize, or the module tried and failed to restart the app.
		/// </returns>
		public Result CheckForLauncherAndRestart()
		{
			var funcResult = EOS_Platform_CheckForLauncherAndRestart(InnerHandle);
			var funcResultReturn = Helper.GetDefault<Result>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		[DllImport(Config.BinaryName)]
		private static extern Result EOS_Platform_CheckForLauncherAndRestart(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern Result EOS_Platform_SetOverrideLocaleCode(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string newLocaleCode);

		[DllImport(Config.BinaryName)]
		private static extern Result EOS_Platform_SetOverrideCountryCode(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string newCountryCode);

		[DllImport(Config.BinaryName)]
		private static extern Result EOS_Platform_GetOverrideLocaleCode(IntPtr handle, System.Text.StringBuilder outBuffer, ref int inOutBufferLength);

		[DllImport(Config.BinaryName)]
		private static extern Result EOS_Platform_GetOverrideCountryCode(IntPtr handle, System.Text.StringBuilder outBuffer, ref int inOutBufferLength);

		[DllImport(Config.BinaryName)]
		private static extern Result EOS_Platform_GetActiveLocaleCode(IntPtr handle, IntPtr localUserId, System.Text.StringBuilder outBuffer, ref int inOutBufferLength);

		[DllImport(Config.BinaryName)]
		private static extern Result EOS_Platform_GetActiveCountryCode(IntPtr handle, IntPtr localUserId, System.Text.StringBuilder outBuffer, ref int inOutBufferLength);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetLeaderboardsInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetStatsInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetAchievementsInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetTitleStorageInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetPlayerDataStorageInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetP2PInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetUserInfoInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetLobbyInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetSessionsInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetPresenceInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetFriendsInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetUIInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetEcomInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetConnectInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetAuthInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_GetMetricsInterface(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern void EOS_Platform_Tick(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern void EOS_Platform_Release(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_Create(ref OptionsInternal options);

		[DllImport(Config.BinaryName)]
		private static extern Result EOS_Shutdown();

		[DllImport(Config.BinaryName)]
		private static extern Result EOS_Initialize(ref InitializeOptionsInternal options);
	}
}