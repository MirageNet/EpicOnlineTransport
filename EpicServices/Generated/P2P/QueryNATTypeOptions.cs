// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P
{
	/// <summary>
	/// Structure containing information needed to query NAT-types
	/// </summary>
	public class QueryNATTypeOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="P2PInterface.QuerynattypeApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return P2PInterface.QuerynattypeApiLatest; } }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryNATTypeOptionsInternal : IDisposable
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