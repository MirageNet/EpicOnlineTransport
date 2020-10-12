// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	/// <summary>
	/// Input parameters for the <see cref="SessionsInterface.CopyActiveSessionHandle" /> function.
	/// </summary>
	public class CopyActiveSessionHandleOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="SessionsInterface.CopyactivesessionhandleApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return SessionsInterface.CopyactivesessionhandleApiLatest; } }

		/// <summary>
		/// Name of the session for which to retrieve a session handle
		/// </summary>
		public string SessionName { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyActiveSessionHandleOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_SessionName;

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

		public string SessionName
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_SessionName, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_SessionName, value); }
		}

		public void Dispose()
		{
		}
	}
}