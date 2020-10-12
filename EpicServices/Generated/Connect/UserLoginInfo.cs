// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	/// <summary>
	/// Additional information about the local user.
	/// 
	/// As the information passed here is client-controlled and not part of the user authentication tokens, it is only treated as non-authoritative informational data to be used by some of the feature services. For example displaying player names in Leaderboards rankings.
	/// </summary>
	public class UserLoginInfo
	{
		/// <summary>
		/// API Version: Set this to <see cref="ConnectInterface.UserlogininfoApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return ConnectInterface.UserlogininfoApiLatest; } }

		/// <summary>
		/// The userâ€™s display name on the identity provider systems as UTF-8 encoded null-terminated string. The length of the name can be at maximum up to <see cref="ConnectInterface.UserlogininfoDisplaynameMaxLength" /> bytes.
		/// </summary>
		public string DisplayName { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UserLoginInfoInternal : IDisposable
	{
		private int m_ApiVersion;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_DisplayName;

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

		public string DisplayName
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_DisplayName, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_DisplayName, value); }
		}

		public void Dispose()
		{
		}
	}
}