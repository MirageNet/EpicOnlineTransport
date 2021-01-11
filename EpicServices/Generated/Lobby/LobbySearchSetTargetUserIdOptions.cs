// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Lobby
{
	/// <summary>
	/// Input parameters for the <see cref="LobbySearch.SetTargetUserId" /> function.
	/// </summary>
	public class LobbySearchSetTargetUserIdOptions
	{
		/// <summary>
		/// Search lobbies for given user by Product User ID, returning any lobbies where this user is currently registered
		/// </summary>
		public ProductUserId TargetUserId { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct LobbySearchSetTargetUserIdOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_TargetUserId;

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public void Set(LobbySearchSetTargetUserIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = LobbySearch.LobbysearchSettargetuseridApiLatest;
				TargetUserId = other.TargetUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbySearchSetTargetUserIdOptions);
		}

		public void Dispose()
		{
		}
	}
}