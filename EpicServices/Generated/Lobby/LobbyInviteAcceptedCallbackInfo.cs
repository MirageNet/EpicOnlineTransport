// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	/// <summary>
	/// Output parameters for the <see cref="OnLobbyInviteAcceptedCallback" /> Function.
	/// </summary>
	public class LobbyInviteAcceptedCallbackInfo
	{
		/// <summary>
		/// Context that was passed into <see cref="LobbyInterface.AddNotifyLobbyInviteAccepted" />
		/// </summary>
		public object ClientData { get; set; }

		/// <summary>
		/// The invite ID
		/// </summary>
		public string InviteId { get; set; }

		/// <summary>
		/// The Product User ID of the local user who received the invitation
		/// </summary>
		public ProductUserId LocalUserId { get; set; }

		/// <summary>
		/// The Product User ID of the user who sent the invitation
		/// </summary>
		public ProductUserId TargetUserId { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyInviteAcceptedCallbackInfoInternal : ICallbackInfo
	{
		private IntPtr m_ClientData;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_InviteId;
		private IntPtr m_LocalUserId;
		private IntPtr m_TargetUserId;

		public object ClientData
		{
			get
			{
				var value = Helper.GetDefault<object>();
				Helper.TryMarshalGet(m_ClientData, out value);
				return value;
			}
		}

		public IntPtr ClientDataAddress { get { return m_ClientData; } }

		public string InviteId
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_InviteId, out value);
				return value;
			}
		}

		public ProductUserId LocalUserId
		{
			get
			{
				var value = Helper.GetDefault<ProductUserId>();
				Helper.TryMarshalGet(m_LocalUserId, out value);
				return value;
			}
		}

		public ProductUserId TargetUserId
		{
			get
			{
				var value = Helper.GetDefault<ProductUserId>();
				Helper.TryMarshalGet(m_TargetUserId, out value);
				return value;
			}
		}
	}
}