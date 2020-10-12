// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UI
{
	/// <summary>
	/// Input parameters for the <see cref="UIInterface.AddNotifyDisplaySettingsUpdated" /> function.
	/// </summary>
	public class AddNotifyDisplaySettingsUpdatedOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="UIInterface.AddnotifydisplaysettingsupdatedApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return UIInterface.AddnotifydisplaysettingsupdatedApiLatest; } }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct AddNotifyDisplaySettingsUpdatedOptionsInternal : IDisposable
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