// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.TitleStorage
{
	/// <summary>
	/// Structure containing the result of a delete cache operation
	/// </summary>
	public class DeleteCacheCallbackInfo
	{
		/// <summary>
		/// Result code for the operation. <see cref="Result.Success" /> is returned for a successful request, other codes indicate an error
		/// </summary>
		public Result ResultCode { get; set; }

		/// <summary>
		/// Client-specified data passed into the delete cache request
		/// </summary>
		public object ClientData { get; set; }

		/// <summary>
		/// Product User ID of the local user who initiated this request (optional, will only be present in case it was provided during operation start)
		/// </summary>
		public ProductUserId LocalUserId { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct DeleteCacheCallbackInfoInternal : ICallbackInfo
	{
		private Result m_ResultCode;
		private IntPtr m_ClientData;
		private IntPtr m_LocalUserId;

		public Result ResultCode
		{
			get
			{
				var value = Helper.GetDefault<Result>();
				Helper.TryMarshalGet(m_ResultCode, out value);
				return value;
			}
		}

		public object ClientData
		{
			get
			{
				var value = Helper.GetDefault<object>();
				Helper.TryMarshalGet(m_ClientData, out value);
				return value;
			}
		}

		public IntPtr ClientDataAddress { get { return m_ClientData; } }

		public ProductUserId LocalUserId
		{
			get
			{
				var value = Helper.GetDefault<ProductUserId>();
				Helper.TryMarshalGet(m_LocalUserId, out value);
				return value;
			}
		}
	}
}