// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	/// <summary>
	/// Data for the <see cref="PresenceModification.SetStatus" /> function.
	/// </summary>
	public class PresenceModificationSetStatusOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="PresenceInterface.PresencemodificationSetstatusApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return PresenceInterface.PresencemodificationSetstatusApiLatest; } }

		/// <summary>
		/// The status of the user
		/// </summary>
		public Status Status { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PresenceModificationSetStatusOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
		private Status m_Status;

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

		public Status Status
		{
			get
			{
				var value = Helper.GetDefault<Status>();
				Helper.TryMarshalGet(m_Status, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_Status, value); }
		}

		public void Dispose()
		{
		}
	}
}