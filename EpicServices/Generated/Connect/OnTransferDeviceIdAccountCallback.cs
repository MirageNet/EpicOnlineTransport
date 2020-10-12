// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	/// <summary>
	/// Function prototype definition for callbacks passed to <see cref="ConnectInterface.TransferDeviceIdAccount" />
	/// </summary>
	/// <param name="data">A <see cref="TransferDeviceIdAccountCallbackInfo" /> containing the output information and result</param>
	public delegate void OnTransferDeviceIdAccountCallback(TransferDeviceIdAccountCallbackInfo data);

	internal delegate void OnTransferDeviceIdAccountCallbackInternal(IntPtr messagePtr);
}