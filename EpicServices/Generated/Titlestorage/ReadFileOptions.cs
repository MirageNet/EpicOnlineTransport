// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.TitleStorage
{
	/// <summary>
	/// Input data for the <see cref="TitleStorageInterface.ReadFile" /> function
	/// </summary>
	public class ReadFileOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="TitleStorageInterface.ReadfileoptionsApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return TitleStorageInterface.ReadfileoptionsApiLatest; } }

		/// <summary>
		/// Product User ID of the local user who is reading the requested file (optional)
		/// </summary>
		public ProductUserId LocalUserId { get; set; }

		/// <summary>
		/// The file name to read; this file must already exist
		/// </summary>
		public string Filename { get; set; }

		/// <summary>
		/// The maximum amount of data in bytes should be available to read in a single <see cref="OnReadFileDataCallback" /> call
		/// </summary>
		public uint ReadChunkLengthBytes { get; set; }

		/// <summary>
		/// Callback function to handle copying read data
		/// </summary>
		public OnReadFileDataCallback ReadFileDataCallback { get; set; }

		/// <summary>
		/// Optional callback function to be informed of download progress, if the file is not already locally cached. If set, this will be called at least once before completion if the request is successfully started
		/// </summary>
		public OnFileTransferProgressCallback FileTransferProgressCallback { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ReadFileOptionsInternal : IDisposable, IInitializable
	{
		private int m_ApiVersion;
		private IntPtr m_LocalUserId;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_Filename;
		private uint m_ReadChunkLengthBytes;
		private OnReadFileDataCallbackInternal m_ReadFileDataCallback;
		private OnFileTransferProgressCallbackInternal m_FileTransferProgressCallback;

		public void Initialize()
		{
			 m_ReadFileDataCallback = new OnReadFileDataCallbackInternal(TitleStorageInterface.OnReadFileData);
			 m_FileTransferProgressCallback = new OnFileTransferProgressCallbackInternal(TitleStorageInterface.OnFileTransferProgress);
		}

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

		public uint ReadChunkLengthBytes
		{
			get
			{
				var value = Helper.GetDefault<uint>();
				Helper.TryMarshalGet(m_ReadChunkLengthBytes, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_ReadChunkLengthBytes, value); }
		}

		public OnReadFileDataCallbackInternal ReadFileDataCallback
		{
			get { return m_ReadFileDataCallback; }
		}

		public OnFileTransferProgressCallbackInternal FileTransferProgressCallback
		{
			get { return m_FileTransferProgressCallback; }
		}

		public void Dispose()
		{
		}
	}
}