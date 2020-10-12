// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	/// <summary>
	/// Input parameters for the <see cref="LobbyInterface.RejectInvite" /> function.
	/// </summary>
	public class RejectInviteOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="LobbyInterface.RejectinviteApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return LobbyInterface.RejectinviteApiLatest; } }

		/// <summary>
		/// The ID of the lobby associated with the invitation
		/// </summary>
		public string InviteId { get; set; }

		/// <summary>
		/// The Product User ID of the local user who is rejecting the invitation
		/// </summary>
		public ProductUserId LocalUserId { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct RejectInviteOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_InviteId;
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

		public string InviteId
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_InviteId, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_InviteId, value); }
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