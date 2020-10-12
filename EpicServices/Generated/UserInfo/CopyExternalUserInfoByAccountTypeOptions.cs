// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UserInfo
{
	/// <summary>
	/// Input parameters for the <see cref="UserInfoInterface.CopyExternalUserInfoByAccountType" /> function.
	/// </summary>
	public class CopyExternalUserInfoByAccountTypeOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="UserInfoInterface.CopyexternaluserinfobyaccounttypeApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return UserInfoInterface.CopyexternaluserinfobyaccounttypeApiLatest; } }

		/// <summary>
		/// The Epic Online Services Account ID of the local player requesting the information
		/// </summary>
		public EpicAccountId LocalUserId { get; set; }

		/// <summary>
		/// The Epic Online Services Account ID of the player whose information is being retrieved
		/// </summary>
		public EpicAccountId TargetUserId { get; set; }

		/// <summary>
		/// Account type of the external user info to retrieve from the cache
		/// </summary>
		public ExternalAccountType AccountType { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyExternalUserInfoByAccountTypeOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
		private IntPtr m_LocalUserId;
		private IntPtr m_TargetUserId;
		private ExternalAccountType m_AccountType;

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

		public EpicAccountId LocalUserId
		{
			get
			{
				var value = Helper.GetDefault<EpicAccountId>();
				Helper.TryMarshalGet(m_LocalUserId, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_LocalUserId, value); }
		}

		public EpicAccountId TargetUserId
		{
			get
			{
				var value = Helper.GetDefault<EpicAccountId>();
				Helper.TryMarshalGet(m_TargetUserId, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_TargetUserId, value); }
		}

		public ExternalAccountType AccountType
		{
			get
			{
				var value = Helper.GetDefault<ExternalAccountType>();
				Helper.TryMarshalGet(m_AccountType, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_AccountType, value); }
		}

		public void Dispose()
		{
		}
	}
}