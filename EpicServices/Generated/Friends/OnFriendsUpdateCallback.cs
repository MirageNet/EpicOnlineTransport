// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Friends
{
	/// <summary>
	/// Callback for information related to a friend status update.
	/// </summary>
	public delegate void OnFriendsUpdateCallback(OnFriendsUpdateInfo data);

	internal delegate void OnFriendsUpdateCallbackInternal(IntPtr messagePtr);
}