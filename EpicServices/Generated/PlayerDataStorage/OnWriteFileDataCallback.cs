// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage
{
	/// <summary>
	/// Callback for when we are ready to get more data to be written into the requested file. It is undefined how often this will be called during a single tick.
	/// </summary>
	/// <param name="data">Struct containing metadata for the file being written to, as well as the max length in bytes that can be safely written to DataBuffer</param>
	/// <param name="outDataBuffer">A buffer to write data into, to be appended to the end of the file that is being written to. The maximum length of this value is provided in the Info parameter. The number of bytes written to this buffer should be set in OutDataWritten.</param>
	/// <param name="outDataWritten">The length of the data written to OutDataBuffer. This must be less than or equal than the DataBufferLengthBytes provided in the Info parameter</param>
	/// <returns>
	/// The result of the write operation. If this value is not <see cref="WriteResult.ContinueWriting" />, this callback will not be called again for the same request. If this is set to <see cref="WriteResult.FailRequest" /> or <see cref="WriteResult.CancelRequest" />, all data written during the request will not be saved
	/// </returns>
	public delegate WriteResult OnWriteFileDataCallback(WriteFileDataCallbackInfo data, out byte[] outDataBuffer, out uint outDataWritten);

	internal delegate WriteResult OnWriteFileDataCallbackInternal(IntPtr messagePtr, IntPtr outDataBuffer, ref uint outDataWritten);
}