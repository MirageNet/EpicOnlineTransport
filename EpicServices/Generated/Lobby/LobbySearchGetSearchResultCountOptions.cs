// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	/// <summary>
	/// Input parameters for the <see cref="LobbySearch.GetSearchResultCount" /> function.
	/// </summary>
	public class LobbySearchGetSearchResultCountOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="LobbyInterface.LobbysearchGetsearchresultcountApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return LobbyInterface.LobbysearchGetsearchresultcountApiLatest; } }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbySearchGetSearchResultCountOptionsInternal : IDisposable
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