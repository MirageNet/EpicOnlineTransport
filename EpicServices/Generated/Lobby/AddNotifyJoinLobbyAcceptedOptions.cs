// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	public class AddNotifyJoinLobbyAcceptedOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="LobbyInterface.AddnotifyjoinlobbyacceptedApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return LobbyInterface.AddnotifyjoinlobbyacceptedApiLatest; } }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct AddNotifyJoinLobbyAcceptedOptionsInternal : IDisposable
	{
		private int m_ApiVersion;

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

		public void Dispose()
		{
		}
	}
}