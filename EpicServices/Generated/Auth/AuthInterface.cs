// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	public sealed class AuthInterface : Handle
	{
		public AuthInterface(IntPtr innerHandle) : base(innerHandle)
		{
		}

		/// <summary>
		/// The most recent version of the <see cref="DeletePersistentAuth" /> API.
		/// </summary>
		public const int DeletepersistentauthApiLatest = 2;

		/// <summary>
		/// The most recent version of the <see cref="AddNotifyLoginStatusChanged" /> API.
		/// </summary>
		public const int AddnotifyloginstatuschangedApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="CopyUserAuthToken" /> API.
		/// </summary>
		public const int CopyuserauthtokenApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="VerifyUserAuth" /> API.
		/// </summary>
		public const int VerifyuserauthApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="LinkAccount" /> API.
		/// </summary>
		public const int LinkaccountApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="Logout" /> API.
		/// </summary>
		public const int LogoutApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="Login" /> API.
		/// </summary>
		public const int LoginApiLatest = 2;

		/// <summary>
		/// The most recent version of the <see cref="AccountFeatureRestrictedInfo" /> struct.
		/// </summary>
		public const int AccountfeaturerestrictedinfoApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="PinGrantInfo" /> struct.
		/// </summary>
		public const int PingrantinfoApiLatest = 2;

		/// <summary>
		/// The most recent version of the <see cref="Credentials" /> struct.
		/// </summary>
		public const int CredentialsApiLatest = 3;

		/// <summary>
		/// The most recent version of the <see cref="Token" /> struct.
		/// </summary>
		public const int TokenApiLatest = 2;

		/// <summary>
		/// Login/Authenticate with user credentials.
		/// </summary>
		/// <param name="options">structure containing the account credentials to use during the login operation</param>
		/// <param name="clientData">arbitrary data that is passed back to you in the CompletionDelegate</param>
		/// <param name="completionDelegate">a callback that is fired when the login operation completes, either successfully or in error</param>
		public void Login(LoginOptions options, object clientData, OnLoginCallback completionDelegate)
		{
			var optionsInternal = Helper.CopyProperties<LoginOptionsInternal>(options);

			var completionDelegateInternal = new OnLoginCallbackInternal(OnLogin);
			var clientDataAddress = IntPtr.Zero;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, completionDelegateInternal);

			EOS_Auth_Login(InnerHandle, ref optionsInternal, clientDataAddress, completionDelegateInternal);
			Helper.TryMarshalDispose(ref optionsInternal);
		}

		/// <summary>
		/// Signs the player out of the online service.
		/// </summary>
		/// <param name="options">structure containing information about which account to log out.</param>
		/// <param name="clientData">arbitrary data that is passed back to you in the CompletionDelegate</param>
		/// <param name="completionDelegate">a callback that is fired when the logout operation completes, either successfully or in error</param>
		public void Logout(LogoutOptions options, object clientData, OnLogoutCallback completionDelegate)
		{
			var optionsInternal = Helper.CopyProperties<LogoutOptionsInternal>(options);

			var completionDelegateInternal = new OnLogoutCallbackInternal(OnLogout);
			var clientDataAddress = IntPtr.Zero;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, completionDelegateInternal);

			EOS_Auth_Logout(InnerHandle, ref optionsInternal, clientDataAddress, completionDelegateInternal);
			Helper.TryMarshalDispose(ref optionsInternal);
		}

		/// <summary>
		/// Link external account by continuing previous login attempt with a continuance token.
		/// 
		/// On Desktop and Mobile platforms, the user will be presented the Epic Account Portal to resolve their identity.
		/// 
		/// On Console, the user will login to their Epic Account using an external device, e.g. a mobile device or a desktop PC,
		/// by browsing to the presented authentication URL and entering the device code presented by the game on the console.
		/// 
		/// On success, the user will be logged in at the completion of this action.
		/// This will commit this external account to the Epic Account and cannot be undone in the SDK.
		/// </summary>
		/// <param name="options">structure containing the account credentials to use during the link account operation</param>
		/// <param name="clientData">arbitrary data that is passed back to you in the CompletionDelegate</param>
		/// <param name="completionDelegate">a callback that is fired when the link account operation completes, either successfully or in error</param>
		public void LinkAccount(LinkAccountOptions options, object clientData, OnLinkAccountCallback completionDelegate)
		{
			var optionsInternal = Helper.CopyProperties<LinkAccountOptionsInternal>(options);

			var completionDelegateInternal = new OnLinkAccountCallbackInternal(OnLinkAccount);
			var clientDataAddress = IntPtr.Zero;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, completionDelegateInternal);

			EOS_Auth_LinkAccount(InnerHandle, ref optionsInternal, clientDataAddress, completionDelegateInternal);
			Helper.TryMarshalDispose(ref optionsInternal);
		}

		/// <summary>
		/// Deletes a previously received and locally stored persistent auth access token for the currently logged in user of the local device.
		/// 
		/// On Desktop and Mobile platforms, the access token is deleted from the keychain of the local user and a backend request is made to revoke the token on the authentication server.
		/// On Console platforms, even though the caller is responsible for storing and deleting the access token on the local device,
		/// this function should still be called with the access token before its deletion to make the best effort in attempting to also revoke it on the authentication server.
		/// If the function would fail on Console, the caller should still proceed as normal to delete the access token locally as intended.
		/// </summary>
		/// <param name="options">structure containing operation input parameters</param>
		/// <param name="clientData">arbitrary data that is passed back to you in the CompletionDelegate</param>
		/// <param name="completionDelegate">a callback that is fired when the deletion operation completes, either successfully or in error</param>
		public void DeletePersistentAuth(DeletePersistentAuthOptions options, object clientData, OnDeletePersistentAuthCallback completionDelegate)
		{
			var optionsInternal = Helper.CopyProperties<DeletePersistentAuthOptionsInternal>(options);

			var completionDelegateInternal = new OnDeletePersistentAuthCallbackInternal(OnDeletePersistentAuth);
			var clientDataAddress = IntPtr.Zero;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, completionDelegateInternal);

			EOS_Auth_DeletePersistentAuth(InnerHandle, ref optionsInternal, clientDataAddress, completionDelegateInternal);
			Helper.TryMarshalDispose(ref optionsInternal);
		}

		/// <summary>
		/// Contact the backend service to verify validity of an existing user auth token.
		/// This function is intended for server-side use only.
		/// </summary>
		/// <param name="options">structure containing information about the auth token being verified</param>
		/// <param name="clientData">arbitrary data that is passed back to you in the CompletionDelegate</param>
		/// <param name="completionDelegate">a callback that is fired when the logout operation completes, either successfully or in error</param>
		public void VerifyUserAuth(VerifyUserAuthOptions options, object clientData, OnVerifyUserAuthCallback completionDelegate)
		{
			var optionsInternal = Helper.CopyProperties<VerifyUserAuthOptionsInternal>(options);

			var completionDelegateInternal = new OnVerifyUserAuthCallbackInternal(OnVerifyUserAuth);
			var clientDataAddress = IntPtr.Zero;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, completionDelegateInternal);

			EOS_Auth_VerifyUserAuth(InnerHandle, ref optionsInternal, clientDataAddress, completionDelegateInternal);
			Helper.TryMarshalDispose(ref optionsInternal);
		}

		/// <summary>
		/// Fetch the number of accounts that are logged in.
		/// </summary>
		/// <returns>
		/// the number of accounts logged in.
		/// </returns>
		public int GetLoggedInAccountsCount()
		{
			var funcResult = EOS_Auth_GetLoggedInAccountsCount(InnerHandle);
			var funcResultReturn = Helper.GetDefault<int>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Fetch an Epic Online Services Account ID that is logged in.
		/// </summary>
		/// <param name="index">An index into the list of logged in accounts. If the index is out of bounds, the returned Epic Online Services Account ID will be invalid.</param>
		/// <returns>
		/// The Epic Online Services Account ID associated with the index passed
		/// </returns>
		public EpicAccountId GetLoggedInAccountByIndex(int index)
		{
			var funcResult = EOS_Auth_GetLoggedInAccountByIndex(InnerHandle, index);
			var funcResultReturn = Helper.GetDefault<EpicAccountId>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Fetches the login status for an Epic Online Services Account ID.
		/// </summary>
		/// <param name="localUserId">The Epic Online Services Account ID of the user being queried</param>
		/// <returns>
		/// The enum value of a user's login status
		/// </returns>
		public LoginStatus GetLoginStatus(EpicAccountId localUserId)
		{
			var funcResult = EOS_Auth_GetLoginStatus(InnerHandle, localUserId.InnerHandle);
			var funcResultReturn = Helper.GetDefault<LoginStatus>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Fetches a user auth token for an Epic Online Services Account ID.
		/// <seealso cref="Release" />
		/// </summary>
		/// <param name="options">Structure containing the api version of CopyUserAuthToken to use</param>
		/// <param name="localUserId">The Epic Online Services Account ID of the user being queried</param>
		/// <param name="outUserAuthToken">The auth token for the given user, if it exists and is valid; use <see cref="Release" /> when finished</param>
		/// <returns>
		/// <see cref="Result.Success" /> if the information is available and passed out in OutUserAuthToken
		/// <see cref="Result.InvalidParameters" /> if you pass a null pointer for the out parameter
		/// <see cref="Result.NotFound" /> if the auth token is not found or expired.
		/// </returns>
		public Result CopyUserAuthToken(CopyUserAuthTokenOptions options, EpicAccountId localUserId, out Token outUserAuthToken)
		{
			var optionsInternal = Helper.CopyProperties<CopyUserAuthTokenOptionsInternal>(options);

			outUserAuthToken = Helper.GetDefault<Token>();

			var outUserAuthTokenAddress = IntPtr.Zero;

			var funcResult = EOS_Auth_CopyUserAuthToken(InnerHandle, ref optionsInternal, localUserId.InnerHandle, ref outUserAuthTokenAddress);
			Helper.TryMarshalDispose(ref optionsInternal);

			if (Helper.TryMarshalGet<TokenInternal, Token>(outUserAuthTokenAddress, out outUserAuthToken))
			{
				EOS_Auth_Token_Release(outUserAuthTokenAddress);
			}

			var funcResultReturn = Helper.GetDefault<Result>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Register to receive login status updates.
		/// @note must call RemoveNotifyLoginStatusChanged to remove the notification
		/// </summary>
		/// <param name="options">structure containing the api version of AddNotifyLoginStatusChanged to use</param>
		/// <param name="clientData">arbitrary data that is passed back to you in the callback</param>
		/// <param name="notification">a callback that is fired when the login status for a user changes</param>
		/// <returns>
		/// handle representing the registered callback
		/// </returns>
		public ulong AddNotifyLoginStatusChanged(AddNotifyLoginStatusChangedOptions options, object clientData, OnLoginStatusChangedCallback notification)
		{
			var optionsInternal = Helper.CopyProperties<AddNotifyLoginStatusChangedOptionsInternal>(options);

			var notificationInternal = new OnLoginStatusChangedCallbackInternal(OnLoginStatusChanged);
			var clientDataAddress = IntPtr.Zero;
			Helper.AddCallback(ref clientDataAddress, clientData, notification, notificationInternal);

			var funcResult = EOS_Auth_AddNotifyLoginStatusChanged(InnerHandle, ref optionsInternal, clientDataAddress, notificationInternal);
			Helper.TryMarshalDispose(ref optionsInternal);

			Helper.TryAssignNotificationIdToCallback(clientDataAddress, funcResult);

			var funcResultReturn = Helper.GetDefault<ulong>();
			Helper.TryMarshalGet(funcResult, out funcResultReturn);
			return funcResultReturn;
		}

		/// <summary>
		/// Unregister from receiving login status updates.
		/// </summary>
		/// <param name="inId">handle representing the registered callback</param>
		public void RemoveNotifyLoginStatusChanged(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			EOS_Auth_RemoveNotifyLoginStatusChanged(InnerHandle, inId);
		}

		[MonoPInvokeCallback]
		internal static void OnLoginStatusChanged(IntPtr address)
		{
			OnLoginStatusChangedCallback callback = null;
			LoginStatusChangedCallbackInfo callbackInfo = null;
			if (Helper.TryGetAndRemoveCallback<OnLoginStatusChangedCallback, LoginStatusChangedCallbackInfoInternal, LoginStatusChangedCallbackInfo>(address, out callback, out callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback]
		internal static void OnVerifyUserAuth(IntPtr address)
		{
			OnVerifyUserAuthCallback callback = null;
			VerifyUserAuthCallbackInfo callbackInfo = null;
			if (Helper.TryGetAndRemoveCallback<OnVerifyUserAuthCallback, VerifyUserAuthCallbackInfoInternal, VerifyUserAuthCallbackInfo>(address, out callback, out callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback]
		internal static void OnDeletePersistentAuth(IntPtr address)
		{
			OnDeletePersistentAuthCallback callback = null;
			DeletePersistentAuthCallbackInfo callbackInfo = null;
			if (Helper.TryGetAndRemoveCallback<OnDeletePersistentAuthCallback, DeletePersistentAuthCallbackInfoInternal, DeletePersistentAuthCallbackInfo>(address, out callback, out callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback]
		internal static void OnLinkAccount(IntPtr address)
		{
			OnLinkAccountCallback callback = null;
			LinkAccountCallbackInfo callbackInfo = null;
			if (Helper.TryGetAndRemoveCallback<OnLinkAccountCallback, LinkAccountCallbackInfoInternal, LinkAccountCallbackInfo>(address, out callback, out callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback]
		internal static void OnLogout(IntPtr address)
		{
			OnLogoutCallback callback = null;
			LogoutCallbackInfo callbackInfo = null;
			if (Helper.TryGetAndRemoveCallback<OnLogoutCallback, LogoutCallbackInfoInternal, LogoutCallbackInfo>(address, out callback, out callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback]
		internal static void OnLogin(IntPtr address)
		{
			OnLoginCallback callback = null;
			LoginCallbackInfo callbackInfo = null;
			if (Helper.TryGetAndRemoveCallback<OnLoginCallback, LoginCallbackInfoInternal, LoginCallbackInfo>(address, out callback, out callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[DllImport(Config.BinaryName)]
		private static extern void EOS_Auth_Token_Release(IntPtr authToken);

		[DllImport(Config.BinaryName)]
		private static extern void EOS_Auth_RemoveNotifyLoginStatusChanged(IntPtr handle, ulong inId);

		[DllImport(Config.BinaryName)]
		private static extern ulong EOS_Auth_AddNotifyLoginStatusChanged(IntPtr handle, ref AddNotifyLoginStatusChangedOptionsInternal options, IntPtr clientData, OnLoginStatusChangedCallbackInternal notification);

		[DllImport(Config.BinaryName)]
		private static extern Result EOS_Auth_CopyUserAuthToken(IntPtr handle, ref CopyUserAuthTokenOptionsInternal options, IntPtr localUserId, ref IntPtr outUserAuthToken);

		[DllImport(Config.BinaryName)]
		private static extern LoginStatus EOS_Auth_GetLoginStatus(IntPtr handle, IntPtr localUserId);

		[DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Auth_GetLoggedInAccountByIndex(IntPtr handle, int index);

		[DllImport(Config.BinaryName)]
		private static extern int EOS_Auth_GetLoggedInAccountsCount(IntPtr handle);

		[DllImport(Config.BinaryName)]
		private static extern void EOS_Auth_VerifyUserAuth(IntPtr handle, ref VerifyUserAuthOptionsInternal options, IntPtr clientData, OnVerifyUserAuthCallbackInternal completionDelegate);

		[DllImport(Config.BinaryName)]
		private static extern void EOS_Auth_DeletePersistentAuth(IntPtr handle, ref DeletePersistentAuthOptionsInternal options, IntPtr clientData, OnDeletePersistentAuthCallbackInternal completionDelegate);

		[DllImport(Config.BinaryName)]
		private static extern void EOS_Auth_LinkAccount(IntPtr handle, ref LinkAccountOptionsInternal options, IntPtr clientData, OnLinkAccountCallbackInternal completionDelegate);

		[DllImport(Config.BinaryName)]
		private static extern void EOS_Auth_Logout(IntPtr handle, ref LogoutOptionsInternal options, IntPtr clientData, OnLogoutCallbackInternal completionDelegate);

		[DllImport(Config.BinaryName)]
		private static extern void EOS_Auth_Login(IntPtr handle, ref LoginOptionsInternal options, IntPtr clientData, OnLoginCallbackInternal completionDelegate);
	}
}