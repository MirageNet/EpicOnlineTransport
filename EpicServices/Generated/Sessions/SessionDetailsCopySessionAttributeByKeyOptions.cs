// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	/// <summary>
	/// Input parameters for the <see cref="SessionDetails.CopySessionAttributeByKey" /> function.
	/// </summary>
	public class SessionDetailsCopySessionAttributeByKeyOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="SessionsInterface.SessiondetailsCopysessionattributebykeyApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return SessionsInterface.SessiondetailsCopysessionattributebykeyApiLatest; } }

		/// <summary>
		/// <seealso cref="SessionModification.AddAttribute" />
		/// </summary>
		public string AttrKey { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionDetailsCopySessionAttributeByKeyOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_AttrKey;

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

		public string AttrKey
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_AttrKey, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_AttrKey, value); }
		}

		public void Dispose()
		{
		}
	}
}