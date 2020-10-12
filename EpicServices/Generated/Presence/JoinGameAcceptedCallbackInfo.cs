// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	/// <summary>
	/// Output parameters for the <see cref="OnJoinGameAcceptedCallback" /> Function.
	/// </summary>
	public class JoinGameAcceptedCallbackInfo
	{
		/// <summary>
		/// Context that was passed into <see cref="PresenceInterface.AddNotifyJoinGameAccepted" />
		/// </summary>
		public object ClientData { get; set; }

		/// <summary>
		/// The Join Info custom game-data string to use to join the target user.
		/// Set to a null pointer to delete the value.
		/// </summary>
		public string JoinInfo { get; set; }

		/// <summary>
		/// The Epic Online Services Account ID of the user who accepted the invitation
		/// </summary>
		public EpicAccountId LocalUserId { get; set; }

		/// <summary>
		/// The Epic Online Services Account ID of the user who sent the invitation
		/// </summary>
		public EpicAccountId TargetUserId { get; set; }

		/// <summary>
		/// If the value is not <see cref="UI.UIInterface.EventidInvalid" /> then it must be passed back to the SDK using <see cref="UI.UIInterface.AcknowledgeEventId" />.
		/// This should be done after attempting to join the game and either succeeding or failing to connect.
		/// This is necessary to allow the Social Overlay UI to manage the `Join` button.
		/// </summary>
		public ulong UiEventId { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct JoinGameAcceptedCallbackInfoInternal : ICallbackInfo
	{
		private IntPtr m_ClientData;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_JoinInfo;
		private IntPtr m_LocalUserId;
		private IntPtr m_TargetUserId;
		private ulong m_UiEventId;

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

		public string JoinInfo
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_JoinInfo, out value);
				return value;
			}
		}

		public EpicAccountId LocalUserId
		{
			get
			{
				var value = Helper.GetDefault<EpicAccountId>();
				Helper.TryMarshalGet(m_LocalUserId, out value);
				return value;
			}
		}

		public EpicAccountId TargetUserId
		{
			get
			{
				var value = Helper.GetDefault<EpicAccountId>();
				Helper.TryMarshalGet(m_TargetUserId, out value);
				return value;
			}
		}

		public ulong UiEventId
		{
			get
			{
				var value = Helper.GetDefault<ulong>();
				Helper.TryMarshalGet(m_UiEventId, out value);
				return value;
			}
		}
	}
}