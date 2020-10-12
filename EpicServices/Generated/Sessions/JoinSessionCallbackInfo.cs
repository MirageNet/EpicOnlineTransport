// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	/// <summary>
	/// Output parameters for the <see cref="SessionsInterface.JoinSession" /> function.
	/// </summary>
	public class JoinSessionCallbackInfo
	{
		/// <summary>
		/// The <see cref="Result" /> code for the operation. <see cref="Result.Success" /> indicates that the operation succeeded; other codes indicate errors.
		/// </summary>
		public Result ResultCode { get; set; }

		/// <summary>
		/// Context that was passed into <see cref="SessionsInterface.JoinSession" />
		/// </summary>
		public object ClientData { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct JoinSessionCallbackInfoInternal : ICallbackInfo
	{
		private Result m_ResultCode;
		private IntPtr m_ClientData;

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
	}
}