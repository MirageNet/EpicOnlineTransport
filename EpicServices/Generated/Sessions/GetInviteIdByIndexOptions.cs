// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	/// <summary>
	/// Input parameters for the <see cref="SessionsInterface.GetInviteIdByIndex" /> function.
	/// </summary>
	public class GetInviteIdByIndexOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="SessionsInterface.GetinviteidbyindexApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return SessionsInterface.GetinviteidbyindexApiLatest; } }

		/// <summary>
		/// The Product User ID of the local user who has an invitation in the cache
		/// </summary>
		public ProductUserId LocalUserId { get; set; }

		/// <summary>
		/// Index of the invite ID to retrieve
		/// </summary>
		public uint Index { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct GetInviteIdByIndexOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
		private IntPtr m_LocalUserId;
		private uint m_Index;

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

		public uint Index
		{
			get
			{
				var value = Helper.GetDefault<uint>();
				Helper.TryMarshalGet(m_Index, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_Index, value); }
		}

		public void Dispose()
		{
		}
	}
}