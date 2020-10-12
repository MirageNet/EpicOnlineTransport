// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	/// <summary>
	/// Input parameters for the <see cref="ConnectInterface.CopyProductUserExternalAccountByAccountType" /> function.
	/// </summary>
	public class CopyProductUserExternalAccountByAccountTypeOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="ConnectInterface.CopyproductuserexternalaccountbyaccounttypeApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return ConnectInterface.CopyproductuserexternalaccountbyaccounttypeApiLatest; } }

		/// <summary>
		/// The Product User ID to look for when copying external account info from the cache.
		/// </summary>
		public ProductUserId TargetUserId { get; set; }

		/// <summary>
		/// External auth service account type to look for when copying external account info from the cache.
		/// </summary>
		public ExternalAccountType AccountIdType { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyProductUserExternalAccountByAccountTypeOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
		private IntPtr m_TargetUserId;
		private ExternalAccountType m_AccountIdType;

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

		public ProductUserId TargetUserId
		{
			get
			{
				var value = Helper.GetDefault<ProductUserId>();
				Helper.TryMarshalGet(m_TargetUserId, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_TargetUserId, value); }
		}

		public ExternalAccountType AccountIdType
		{
			get
			{
				var value = Helper.GetDefault<ExternalAccountType>();
				Helper.TryMarshalGet(m_AccountIdType, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_AccountIdType, value); }
		}

		public void Dispose()
		{
		}
	}
}