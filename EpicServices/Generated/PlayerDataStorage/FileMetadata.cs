// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage
{
	/// <summary>
	/// Metadata information for a specific file
	/// </summary>
	public class FileMetadata
	{
		/// <summary>
		/// API Version: Set this to <see cref="PlayerDataStorageInterface.FilemetadataApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return PlayerDataStorageInterface.FilemetadataApiLatest; } }

		/// <summary>
		/// The total size of the file in bytes (Includes file header in addition to file contents)
		/// </summary>
		public uint FileSizeBytes { get; set; }

		/// <summary>
		/// The MD5 Hash of the entire file (including additional file header), in hex digits
		/// </summary>
		public string MD5Hash { get; set; }

		/// <summary>
		/// The file's name
		/// </summary>
		public string Filename { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct FileMetadataInternal : IDisposable
	{
		private int m_ApiVersion;
		private uint m_FileSizeBytes;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_MD5Hash;
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

		public uint FileSizeBytes
		{
			get
			{
				var value = Helper.GetDefault<uint>();
				Helper.TryMarshalGet(m_FileSizeBytes, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_FileSizeBytes, value); }
		}

		public string MD5Hash
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_MD5Hash, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_MD5Hash, value); }
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