// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	/// <summary>
	/// Contains information about lobby and lobby member data
	/// </summary>
	public class AttributeData
	{
		/// <summary>
		/// API Version: Set this to <see cref="LobbyInterface.AttributedataApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return LobbyInterface.AttributedataApiLatest; } }

		public string Key { get; set; }

		public AttributeDataValue Value { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct AttributeDataInternal : IDisposable
	{
		private int m_ApiVersion;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_Key;
		private AttributeDataValueInternal m_Value;

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

		public AttributeDataValueInternal Value
		{
			get
			{
				var value = Helper.GetDefault<AttributeDataValueInternal>();
				Helper.TryMarshalGet(m_Value, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_Value, value); }
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Value);
		}
	}
}