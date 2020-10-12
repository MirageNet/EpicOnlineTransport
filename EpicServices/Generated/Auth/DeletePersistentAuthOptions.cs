// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	/// <summary>
	/// Input parameters for the <see cref="AuthInterface.DeletePersistentAuth" /> function.
	/// </summary>
	public class DeletePersistentAuthOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="AuthInterface.DeletepersistentauthApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return AuthInterface.DeletepersistentauthApiLatest; } }

		/// <summary>
		/// A long-lived refresh token that is used with the <see cref="LoginCredentialType.PersistentAuth" /> login type and is to be revoked from the authentication server. Only used on Console platforms.
		/// On Desktop and Mobile platforms, set this parameter to NULL.
		/// </summary>
		public string RefreshToken { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct DeletePersistentAuthOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_RefreshToken;

		public int ApiVersion
		{
			get
			{
				var value = Helper.GetDefault<int>();
				Helper.TryMarshalGet(m_ApiVersion, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_ApiVersion, value); }
		}

		public string RefreshToken
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_RefreshToken, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_RefreshToken, value); }
		}

		public void Dispose()
		{
		}
	}
}