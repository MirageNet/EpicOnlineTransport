// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	/// <summary>
	/// Input parameters for the <see cref="LobbySearch.RemoveParameter" /> function.
	/// </summary>
	public class LobbySearchRemoveParameterOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="LobbyInterface.LobbysearchRemoveparameterApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return LobbyInterface.LobbysearchRemoveparameterApiLatest; } }

		/// <summary>
		/// Search parameter key to remove from the search
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		/// Search comparison operation associated with the key to remove
		/// </summary>
		public ComparisonOp ComparisonOp { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbySearchRemoveParameterOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_Key;
		private ComparisonOp m_ComparisonOp;

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

		public string Key
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_Key, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_Key, value); }
		}

		public ComparisonOp ComparisonOp
		{
			get
			{
				var value = Helper.GetDefault<ComparisonOp>();
				Helper.TryMarshalGet(m_ComparisonOp, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_ComparisonOp, value); }
		}

		public void Dispose()
		{
		}
	}
}