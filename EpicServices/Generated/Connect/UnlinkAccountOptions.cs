// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	/// <summary>
	/// Input parameters for the <see cref="ConnectInterface.UnlinkAccount" /> Function.
	/// </summary>
	public class UnlinkAccountOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="ConnectInterface.UnlinkaccountApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return ConnectInterface.UnlinkaccountApiLatest; } }

		/// <summary>
		/// Existing logged in product user that is subject for the unlinking operation.
		/// The external account that was used to login to the product user will be unlinked from the owning keychain.
		/// 
		/// On a successful operation, the product user will be logged out as the external account used to authenticate the user was unlinked from the owning keychain.
		/// </summary>
		public ProductUserId LocalUserId { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UnlinkAccountOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
		private IntPtr m_LocalUserId;

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

		public ProductUserId LocalUserId
		{
			get
			{
				var value = Helper.GetDefault<ProductUserId>();
				Helper.TryMarshalGet(m_LocalUserId, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_LocalUserId, value); }
		}

		public void Dispose()
		{
		}
	}
}