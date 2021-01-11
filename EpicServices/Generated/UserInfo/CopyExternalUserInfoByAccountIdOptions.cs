// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.UserInfo
{
	/// <summary>
	/// Input parameters for the <see cref="UserInfoInterface.CopyExternalUserInfoByAccountId" /> function.
	/// </summary>
	public class CopyExternalUserInfoByAccountIdOptions
	{
		/// <summary>
		/// The Epic Online Services Account ID of the local player requesting the information
		/// </summary>
		public EpicAccountId LocalUserId { get; set; }

		/// <summary>
		/// The Epic Online Services Account ID of the player whose information is being retrieved
		/// </summary>
		public EpicAccountId TargetUserId { get; set; }

		/// <summary>
		/// The external account ID associated with the (external) user info to retrieve from the cache; cannot be null
		/// </summary>
		public string AccountId { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct CopyExternalUserInfoByAccountIdOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_LocalUserId;
		private System.IntPtr m_TargetUserId;
		private System.IntPtr m_AccountId;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public EpicAccountId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public string AccountId
		{
			set
			{
				Helper.TryMarshalSet(ref m_AccountId, value);
			}
		}

		public void Set(CopyExternalUserInfoByAccountIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = UserInfoInterface.CopyexternaluserinfobyaccountidApiLatest;
				LocalUserId = other.LocalUserId;
				TargetUserId = other.TargetUserId;
				AccountId = other.AccountId;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyExternalUserInfoByAccountIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AccountId);
		}
	}
}