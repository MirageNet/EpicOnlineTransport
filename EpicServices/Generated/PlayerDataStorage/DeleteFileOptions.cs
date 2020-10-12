// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage
{
	/// <summary>
	/// Input data for the <see cref="PlayerDataStorageInterface.DeleteFile" /> function
	/// </summary>
	public class DeleteFileOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="PlayerDataStorageInterface.DeletefileoptionsApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return PlayerDataStorageInterface.DeletefileoptionsApiLatest; } }

		/// <summary>
		/// The Product User ID of the local user who authorizes deletion of the file; must be the file's owner
		/// </summary>
		public ProductUserId LocalUserId { get; set; }

		/// <summary>
		/// The name of the file to delete
		/// </summary>
		public string Filename { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct DeleteFileOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
		private IntPtr m_LocalUserId;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_Filename;

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

		public string Filename
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_Filename, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_Filename, value); }
		}

		public void Dispose()
		{
		}
	}
}