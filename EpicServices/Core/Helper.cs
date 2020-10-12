// Copyright Epic Games, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices
{
	internal class AllocationException : Exception
	{
		public AllocationException(string message)
			: base(message)
		{
		}
	}

	internal class ExternalAllocationException : AllocationException
	{
		public ExternalAllocationException(IntPtr address, Type type)
			: base(string.Format("Attempting to allocate {0} over externally allocated memory at {1:X}", type, address.ToString("X")))
		{
		}
	}

	internal class TypeAllocationException : AllocationException
	{
		public TypeAllocationException(IntPtr address, Type foundType, Type expectedType)
			: base(string.Format("Found allocation of {0} but expected {1} at {2}", foundType, expectedType, address.ToString("X")))
		{
		}
	}

	internal class ArrayAllocationException : AllocationException
	{
		public ArrayAllocationException(IntPtr address, int foundLength, int expectedLength)
			: base(string.Format("Found array allocation with length {0} but expected {1} at {2:X}", foundLength, expectedLength, address.ToString("X")))
		{
		}
	}

	public static class Helper
	{
		private class Allocation
		{
			public object Data { get; private set; }

			public Allocation(object data)
			{
				Data = data;
			}
		}

		private class ArrayAllocation : Allocation
		{
			public bool IsElementAllocated { get; private set; }

			public ArrayAllocation(object data, bool isElementAllocated) : base(data)
			{
				IsElementAllocated = isElementAllocated;
			}
		}

		private class DelegateHolder
		{
			public Delegate Public { get; private set; }
			public Delegate Private { get; private set; }
			public Delegate[] Additional { get; private set; }
			public ulong? NotificationId { get; set; }

			public DelegateHolder(Delegate publicDelegate, Delegate privateDelegate, params Delegate[] additionalDelegates)
			{
				Public = publicDelegate;
				Private = privateDelegate;
				Additional = additionalDelegates;
			}
		}

		private static Dictionary<IntPtr, Allocation> s_Allocations = new Dictionary<IntPtr, Allocation>();
		private static Dictionary<IntPtr, DelegateHolder> s_Callbacks = new Dictionary<IntPtr, DelegateHolder>();

		/// <summary>
		/// Gets the number of unmanaged allocations currently active within the wrapper. Use this to find leaks related to the usage of wrapper code.
		/// </summary>
		/// <returns>The number of unmanaged allocations currently active within the wrapper.</returns>
		public static int GetAllocationCount()
		{
			return s_Allocations.Count;
		}

		/// <summary>
		/// Checks whether the given result indicates that the operation has completed. Some operations may callback with a result indicating that they will callback again.
		/// </summary>
		/// <param name="result">The result to check.</param>
		/// <returns>Whether the operation has completed or not.</returns>
		public static bool IsOperationComplete(Result result)
		{
			int isOperationCompleteInt = EOS_EResult_IsOperationComplete(result);

			bool isOperationComplete = false;
			TryMarshalGet(isOperationCompleteInt, out isOperationComplete);
			return isOperationComplete;
		}

		[DllImport(Config.BinaryName)]
		private static extern int EOS_EResult_IsOperationComplete(Result result);

		/// <summary>
		/// Converts a byte array into a hex string, e.g. "A56904FF".
		/// </summary>
		/// <param name="byteArray">The byte array to convert.</param>
		/// <returns>A hex string, e.g. "A56904FF".</returns>
		public static string ToHexString(byte[] byteArray)
		{
			return string.Join("", byteArray.Select(b => string.Format("{0:X2}", b)).ToArray());
		}

		// These functions are the front end when changing SDK values into wrapper values.
		// They will either fetch or convert; whichever is most appropriate for the source and target types.
		#region Marshal Getters
		internal static bool TryMarshalGet<T>(T source, out T target)
		{
			target = source;

			return true;
		}

		internal static bool TryMarshalGet(IntPtr source, out IntPtr target)
		{
			target = source;

			return true;
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T target)
			where T : Handle
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet(int source, out bool target)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet(long source, out DateTimeOffset? target)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T[] target, int arrayLength, bool isElementAllocated)
		{
			return TryFetch(source, out target, arrayLength, isElementAllocated);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T[] target, uint arrayLength, bool isElementAllocated)
		{
			return TryFetch(source, out target, (int)arrayLength, isElementAllocated);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T[] target, int arrayLength)
		{
			return TryMarshalGet(source, out target, arrayLength, !typeof(T).IsValueType);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T[] target, uint arrayLength)
		{
			return TryMarshalGet(source, out target, arrayLength, !typeof(T).IsValueType);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T? target)
			where T : struct
		{
			return TryFetch(source, out target);
		}

		internal static bool TryMarshalGet(byte[] source, out string target)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet(IntPtr source, out object target)
		{
			target = null;

			BoxedData boxedData;
			if (TryFetch(source, out boxedData))
			{
				target = boxedData.Data;
				return true;
			}

			return false;
		}

		internal static bool TryMarshalGet(IntPtr source, out string target)
		{
			return TryFetch(source, out target);
		}

		internal static bool TryMarshalGet<T, TEnum>(T source, out T target, TEnum currentEnum, TEnum comparisonEnum)
		{
			target = GetDefault<T>();

			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				target = source;
				return true;
			}

			return false;
		}

		internal static bool TryMarshalGet<T, TEnum>(T source, out T? target, TEnum currentEnum, TEnum comparisonEnum)
			where T : struct
		{
			target = GetDefault<T?>();

			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				target = source;
				return true;
			}

			return false;
		}

		internal static bool TryMarshalGet<T, TEnum>(IntPtr source, out T target, TEnum currentEnum, TEnum comparisonEnum)
			where T : Handle
		{
			target = GetDefault<T>();

			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				return TryMarshalGet(source, out target);
			}

			return false;
		}

		internal static bool TryMarshalGet<TEnum>(IntPtr source, out string target, TEnum currentEnum, TEnum comparisonEnum)
		{
			target = GetDefault<string>();

			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				return TryMarshalGet(source, out target);
			}

			return false;
		}

		internal static bool TryMarshalGet<TEnum>(int source, out bool? target, TEnum currentEnum, TEnum comparisonEnum)
		{
			target = GetDefault<bool?>();

			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				bool targetConvert;
				if (TryConvert(source, out targetConvert))
				{
					target = targetConvert;
					return true;
				}
			}

			return false;
		}

		internal static bool TryMarshalGet<TInternal, TPublic>(IntPtr source, out TPublic target)
			where TInternal : struct
			where TPublic : class, new()
		{
			target = null;

			TInternal sourceObject;
			if (TryFetch(source, out sourceObject))
			{
				target = CopyProperties<TPublic>(sourceObject);

				return true;
			}

			return false;
		}

		internal static bool TryMarshalGet<TCallbackInfoInternal, TCallbackInfo>(IntPtr callbackInfoAddress, out TCallbackInfo callbackInfo, out IntPtr clientDataAddress)
			where TCallbackInfoInternal : struct, ICallbackInfo
			where TCallbackInfo : class, new()
		{
			callbackInfo = null;
			clientDataAddress = IntPtr.Zero;

			TCallbackInfoInternal callbackInfoInternal;
			if (TryFetch(callbackInfoAddress, out callbackInfoInternal))
			{
				callbackInfo = CopyProperties<TCallbackInfo>(callbackInfoInternal);
				clientDataAddress = callbackInfoInternal.ClientDataAddress;

				return true;
			}

			return false;
		}
		#endregion

		// These functions are the front end for changing wrapper values into SDK values.
		// They will either allocate or convert; whichever is most appropriate for the source and target types.
		#region Marshal Setters
		internal static bool TryMarshalSet<T>(ref T target, T source)
		{
			target = source;

			return true;
		}

		internal static bool TryMarshalSet(ref IntPtr target, Handle source)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T? source)
			where T : struct
		{
			return TryAllocate(ref target, source);
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source, bool isElementAllocated)
		{
			return TryAllocate(ref target, source, isElementAllocated);
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source)
		{
			return TryMarshalSet(ref target, source, !typeof(T).IsValueType);
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source, out int arrayLength, bool isElementAllocated)
		{
			arrayLength = 0;

			if (TryMarshalSet(ref target, source, isElementAllocated))
			{
				arrayLength = source.Length;
				return true;
			}

			return false;
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source, out uint arrayLength, bool isElementAllocated)
		{
			arrayLength = 0;

			int arrayLengthInternal = 0;
			if (TryMarshalSet(ref target, source, out arrayLengthInternal, isElementAllocated))
			{
				arrayLength = (uint)arrayLengthInternal;
				return true;
			}

			return false;
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source, out int arrayLength)
		{
			return TryMarshalSet(ref target, source, out arrayLength, !typeof(T).IsValueType);
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source, out uint arrayLength)
		{
			return TryMarshalSet(ref target, source, out arrayLength, !typeof(T).IsValueType);
		}

		internal static bool TryMarshalSet(ref long target, DateTimeOffset? source)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalSet(ref int target, bool source)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalSet(ref byte[] target, string source)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalSet(ref byte[] target, string source, int length)
		{
			return TryConvert(source, out target, length);
		}

		internal static bool TryMarshalSet(ref IntPtr target, string source)
		{
			return TryAllocate(ref target, source);
		}

		internal static bool TryMarshalSet<T, TEnum>(ref T target, T source, ref TEnum currentEnum, TEnum comparisonEnum, object disposable)
		{
			if (source != null)
			{
				TryMarshalDispose(ref disposable);

				if (TryMarshalSet(ref target, source))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}

			return false;
		}

		internal static bool TryMarshalSet<T, TEnum>(ref T target, T? source, ref TEnum currentEnum, TEnum comparisonEnum, object disposable)
			where T : struct
		{
			if (source != null)
			{
				TryMarshalDispose(ref disposable);

				if (TryMarshalSet(ref target, source.Value))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}

			return true;
		}

		internal static bool TryMarshalSet<T, TEnum>(ref IntPtr target, T source, ref TEnum currentEnum, TEnum comparisonEnum, object disposable)
			where T : Handle
		{
			if (source != null)
			{
				TryMarshalDispose(ref disposable);

				if (TryMarshalSet(ref target, source))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}

			return true;
		}

		internal static bool TryMarshalSet<TEnum>(ref IntPtr target, string source, ref TEnum currentEnum, TEnum comparisonEnum, object disposable)
		{
			if (source != null)
			{
				TryMarshalDispose(ref disposable);

				if (TryMarshalSet(ref target, source))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}

			return true;
		}

		internal static bool TryMarshalSet<TEnum>(ref int target, bool? source, ref TEnum currentEnum, TEnum comparisonEnum, object disposable)
		{
			if (source != null)
			{
				TryMarshalDispose(ref disposable);

				if (TryMarshalSet(ref target, source.Value))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}

			return true;
		}

		#endregion

		// These functions are the front end for disposing of unmanaged memory that this wrapper has allocated.
		#region Marshal Disposers
		internal static bool TryMarshalDispose(ref object value)
		{
			var disposable = value as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
				return true;
			}

			return false;
		}

		internal static bool TryMarshalDispose<T>(ref T value)
			where T : IDisposable
		{
			value.Dispose();
			return true;
		}

		internal static bool TryMarshalDispose(ref IntPtr value)
		{
			return TryRelease(ref value);
		}

		internal static bool TryMarshalDispose<TEnum>(ref IntPtr member, TEnum currentEnum, TEnum comparisonEnum)
		{
			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				return TryRelease(ref member);
			}

			return false;
		}
		#endregion

		// These functions are exposed to the wrapper to generally streamline blocks of generated code.
		#region Helpers
		internal static T GetDefault<T>()
		{
			return default(T);
		}

		internal static T CopyProperties<T>(object value) where T : new()
		{
			object valueBoxed = new T();

			var initiailizable = valueBoxed as IInitializable;
			if (initiailizable != null)
			{
				initiailizable.Initialize();
			}

			CopyProperties(value, valueBoxed);

			return (T)valueBoxed;
		}

		internal static void AddCallback(ref IntPtr clientDataAddress, object clientData, Delegate publicDelegate, Delegate privateDelegate, params Delegate[] additionalDelegates)
		{
			TryAllocate(ref clientDataAddress, new BoxedData(clientData));
			s_Callbacks.Add(clientDataAddress, new DelegateHolder(publicDelegate, privateDelegate, additionalDelegates));
		}

		internal static bool TryAssignNotificationIdToCallback(IntPtr clientDataAddress, ulong notificationId)
		{
			if (notificationId != 0)
			{
				DelegateHolder delegateHolder = null;
				if (s_Callbacks.TryGetValue(clientDataAddress, out delegateHolder))
				{
					delegateHolder.NotificationId = notificationId;
					return true;
				}
			}
			// We can safely release if the notification id came back invalid
			else
			{
				s_Callbacks.Remove(clientDataAddress);
				TryRelease(ref clientDataAddress);
			}

			return false;
		}

		internal static bool TryRemoveCallbackByNotificationId(ulong notificationId)
		{
			var delegateHolderPairs = s_Callbacks.Where(pair => pair.Value.NotificationId.HasValue && pair.Value.NotificationId == notificationId);
			if (delegateHolderPairs.Any())
			{
				IntPtr clientDataAddress = delegateHolderPairs.First().Key;

				s_Callbacks.Remove(clientDataAddress);
				TryRelease(ref clientDataAddress);

				return true;
			}

			return false;
		}

		internal static bool TryGetAndRemoveCallback<TCallback, TCallbackInfoInternal, TCallbackInfo>(IntPtr callbackInfoAddress, out TCallback callback, out TCallbackInfo callbackInfo)
			where TCallback : class
			where TCallbackInfoInternal : struct, ICallbackInfo
			where TCallbackInfo : class, new()
		{
			callback = null;
			callbackInfo = null;

			IntPtr clientDataAddress = IntPtr.Zero;
			if (TryMarshalGet<TCallbackInfoInternal, TCallbackInfo>(callbackInfoAddress, out callbackInfo, out clientDataAddress)
				&& TryGetAndRemoveCallback(clientDataAddress, callbackInfo, out callback))
			{
				return true;
			}

			return false;
		}

		internal static bool TryGetAdditionalCallback<TDelegate, TCallbackInfoInternal, TCallbackInfo>(IntPtr callbackInfoAddress, out TDelegate callback, out TCallbackInfo callbackInfo)
			where TDelegate : class
			where TCallbackInfoInternal : struct, ICallbackInfo
			where TCallbackInfo : class, new()
		{
			callback = null;
			callbackInfo = null;

			IntPtr clientDataAddress = IntPtr.Zero;
			if (TryMarshalGet<TCallbackInfoInternal, TCallbackInfo>(callbackInfoAddress, out callbackInfo, out clientDataAddress)
				&& TryGetAdditionalCallback(clientDataAddress, out callback))
			{
				return true;
			}

			return false;
		}
		#endregion

		// These functions are used for allocating unmanaged memory.
		// They should not be exposed outside of this helper.
		#region Private Allocators
		private static bool TryAllocate<T>(ref IntPtr target, T source)
		{
			TryRelease(ref target);

			if (target != IntPtr.Zero)
			{
				throw new ExternalAllocationException(target, source.GetType());
			}

			if (source == null)
			{
				return false;
			}

			target = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
			Marshal.StructureToPtr(source, target, false);
			s_Allocations.Add(target, new Allocation(source));

			return true;
		}

		private static bool TryAllocate<T>(ref IntPtr target, T? source)
			where T : struct
		{
			TryRelease(ref target);

			if (target != IntPtr.Zero)
			{
				throw new ExternalAllocationException(target, source.GetType());
			}

			if (source == null)
			{
				return false;
			}

			return TryAllocate(ref target, source.Value);
		}

		private static bool TryAllocate(ref IntPtr target, string source)
		{
			TryRelease(ref target);

			if (target != IntPtr.Zero)
			{
				throw new ExternalAllocationException(target, source.GetType());
			}

			if (source == null)
			{
				return false;
			}

			byte[] bytes;
			if (TryConvert(source, out bytes))
			{
				return TryAllocate(ref target, bytes, false);
			}

			return false;
		}

		private static bool TryAllocate<T>(ref IntPtr target, T[] source, bool isElementAllocated)
		{
			TryRelease(ref target);

			if (target != IntPtr.Zero)
			{
				throw new ExternalAllocationException(target, source.GetType());
			}

			if (source == null)
			{
				return false;
			}

			var itemSize = 0;
			if (isElementAllocated)
			{
				itemSize = Marshal.SizeOf(typeof(IntPtr));
			}
			else
			{
				itemSize = Marshal.SizeOf(typeof(T));
			}

			// Allocate the array
			target = Marshal.AllocHGlobal(source.Length * itemSize);
			s_Allocations.Add(target, new ArrayAllocation(source, isElementAllocated));

			for (int itemIndex = 0; itemIndex < source.Length; ++itemIndex)
			{
				var item = (T)source.GetValue(itemIndex);

				if (isElementAllocated)
				{
					// Allocate the item
					IntPtr newItemAddress = IntPtr.Zero;

					if (typeof(T) == typeof(string))
					{
						TryAllocate(ref newItemAddress, (string)(object)item);
					}
					else if (typeof(T).BaseType == typeof(Handle))
					{
						TryConvert((Handle)(object)item, out newItemAddress);
					}
					else
					{
						TryAllocate(ref newItemAddress, item);
					}

					// Copy the item's address into the array
					IntPtr itemAddress = new IntPtr(target.ToInt64() + itemIndex * itemSize);
					Marshal.StructureToPtr(newItemAddress, itemAddress, false);
				}
				else
				{
					// Copy the data straight into memory
					IntPtr itemAddress = new IntPtr(target.ToInt64() + itemIndex * itemSize);
					Marshal.StructureToPtr(item, itemAddress, false);
				}
			}

			return true;
		}
		#endregion

		// These functions are used for releasing unmanaged memory.
		// They should not be exposed outside of this helper.
		#region Private Releasers
		private static bool TryRelease(ref IntPtr target)
		{
			if (target == IntPtr.Zero)
			{
				return false;
			}

			Allocation allocation = null;
			if (!s_Allocations.TryGetValue(target, out allocation))
			{
				return false;
			}

			if (allocation is ArrayAllocation)
			{
				ArrayAllocation arrayAllocation = allocation as ArrayAllocation;

				var itemSize = 0;
				if (arrayAllocation.IsElementAllocated)
				{
					itemSize = Marshal.SizeOf(typeof(IntPtr));
				}
				else
				{
					itemSize = Marshal.SizeOf(arrayAllocation.Data.GetType().GetElementType());
				}

				var array = arrayAllocation.Data as Array;

				for (int itemIndex = 0; itemIndex < array.Length; ++itemIndex)
				{
					if (arrayAllocation.IsElementAllocated)
					{
						var itemAddress = new IntPtr(target.ToInt64() + itemIndex * itemSize);
						itemAddress = Marshal.ReadIntPtr(itemAddress);
						TryRelease(ref itemAddress);
					}
					else
					{
						var item = array.GetValue(itemIndex);
						if (item is IDisposable)
						{
							var disposable = item as IDisposable;
							if (disposable != null)
							{
								disposable.Dispose();
							}
						}
					}
				}
			}

			if (allocation.Data is IDisposable)
			{
				var disposable = allocation.Data as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}

			Marshal.FreeHGlobal(target);
			s_Allocations.Remove(target);
			target = IntPtr.Zero;

			return true;
		}
		#endregion

		// These functions are used for fetching unmanaged memory.
		// They should not be exposed outside of this helper.
		#region Private Fetchers
		private static bool TryFetch<T>(IntPtr source, out T target)
		{
			target = GetDefault<T>();

			if (source == IntPtr.Zero)
			{
				return false;
			}

			if (s_Allocations.ContainsKey(source))
			{
				Allocation allocation = s_Allocations[source];
				if (allocation.Data.GetType() == typeof(T))
				{
					target = (T)allocation.Data;
					return true;
				}
				else
				{
					throw new TypeAllocationException(source, allocation.Data.GetType(), typeof(T));
				}
			}

			target = (T)Marshal.PtrToStructure(source, typeof(T));
			return true;
		}

		private static bool TryFetch<T>(IntPtr source, out T? target)
			where T : struct
		{
			target = GetDefault<T?>();

			if (source == IntPtr.Zero)
			{
				return false;
			}

			if (s_Allocations.ContainsKey(source))
			{
				Allocation allocation = s_Allocations[source];
				if (allocation.Data.GetType() == typeof(T))
				{
					target = (T?)allocation.Data;
					return true;
				}
				else
				{
					throw new TypeAllocationException(source, allocation.Data.GetType(), typeof(T));
				}
			}

			target = (T?)Marshal.PtrToStructure(source, typeof(T));
			return true;
		}

		private static bool TryFetch<T>(IntPtr source, out T[] target, int arrayLength, bool isElementAllocated)
		{
			target = null;

			if (source == IntPtr.Zero)
			{
				return false;
			}

			// Try to retrieve the array from our allocation cache
			if (s_Allocations.ContainsKey(source))
			{
				Allocation allocation = s_Allocations[source];
				if (allocation.Data.GetType() == typeof(T[]))
				{
					var cachedArray = (Array)allocation.Data;
					if (cachedArray.Length == arrayLength)
					{
						target = cachedArray as T[];
						return true;
					}
					else
					{
						throw new ArrayAllocationException(source, cachedArray.Length, arrayLength);
					}
				}
				else
				{
					throw new TypeAllocationException(source, allocation.Data.GetType(), typeof(T[]));
				}
			}

			var itemSize = 0;
			if (isElementAllocated)
			{
				itemSize = Marshal.SizeOf(typeof(IntPtr));
			}
			else
			{
				itemSize = Marshal.SizeOf(typeof(T));
			}

			List<T> items = new List<T>();
			for (int itemIndex = 0; itemIndex < arrayLength; ++itemIndex)
			{
				IntPtr itemAddress = new IntPtr(source.ToInt64() + itemIndex * itemSize);

				if (isElementAllocated)
				{
					itemAddress = Marshal.ReadIntPtr(itemAddress);
				}

				T item;
				TryFetch(itemAddress, out item);
				items.Add(item);
			}

			target = items.ToArray();
			return true;
		}

		private static bool TryFetch(IntPtr source, out string target)
		{
			target = null;

			if (source == IntPtr.Zero)
			{
				return false;
			}

			// Find the null terminator
			int length = 0;
			while (Marshal.ReadByte(source, length) != 0)
			{
				++length;
			}

			byte[] bytes = new byte[length];
			Marshal.Copy(source, bytes, 0, length);

			target = Encoding.UTF8.GetString(bytes);

			return true;
		}
		#endregion

		// These functions are used for converting managed memory.
		// They should not be exposed outside of this helper.
		#region Private Converters
		private static bool TryConvert<THandle>(IntPtr source, out THandle target)
			where THandle : Handle
		{
			target = null;

			if (source != IntPtr.Zero)
			{
				target = Activator.CreateInstance(typeof(THandle), source) as THandle;
			}

			return true;
		}

		private static bool TryConvert(Handle source, out IntPtr target)
		{
			target = IntPtr.Zero;

			if (source != null)
			{
				target = source.InnerHandle;
			}

			return true;
		}

		private static bool TryConvert(byte[] source, out string target)
		{
			target = null;

			if (source == null)
			{
				return false;
			}

			int length = 0;
			foreach (byte currentByte in source)
			{
				if (currentByte == 0)
				{
					break;
				}

				++length;
			}

			target = Encoding.UTF8.GetString(source.Take(length).ToArray());

			return true;
		}

		private static bool TryConvert(string source, out byte[] target, int length)
		{
			if (source == null)
			{
				source = "";
			}

			target = Encoding.UTF8.GetBytes(new string(source.Take(length).ToArray()).PadRight(length, '\0'));

			return true;
		}

		private static bool TryConvert(string source, out byte[] target)
		{
			return TryConvert(source, out target, source.Length + 1);
		}

		private static bool TryConvert(int source, out bool target)
		{
			target = source != 0;

			return true;
		}

		private static bool TryConvert(bool source, out int target)
		{
			target = source ? 1 : 0;

			return true;
		}

		private static bool TryConvert(DateTimeOffset? source, out long target)
		{
			target = -1;

			if (source.HasValue)
			{
				DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				long unixTimestampTicks = (source.Value.UtcDateTime - unixStart).Ticks;
				long unixTimestampSeconds = unixTimestampTicks / TimeSpan.TicksPerSecond;
				target = unixTimestampSeconds;
			}

			return true;
		}

		private static bool TryConvert(long source, out DateTimeOffset? target)
		{
			target = null;

			if (source >= 0)
			{
				DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				long unixTimeStampTicks = source * TimeSpan.TicksPerSecond;
				target = new DateTimeOffset(unixStart.Ticks + unixTimeStampTicks, TimeSpan.Zero);
			}

			return true;
		}
		#endregion

		// These functions exist to further streamline blocks of generated code.
		#region Private Helpers
		private static void CopyProperties(object source, object target)
		{
			if (source == null || target == null)
			{
				return;
			}

			var sourceProperties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);
			var targetProperties = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);

			foreach (var sourceProperty in sourceProperties)
			{
				var targetProperty = targetProperties.SingleOrDefault(property => property.Name == sourceProperty.Name);
				if (targetProperty == null || targetProperty.GetSetMethod(false) == null)
				{
					continue;
				}

				// If the property types are the same, we can simply copy
				if (sourceProperty.PropertyType == targetProperty.PropertyType)
				{
					targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
				}
				// If it's an array, we need to copy the properties of each item
				else if (targetProperty.PropertyType.IsArray)
				{
					var sourceArray = sourceProperty.GetValue(source, null) as Array;

					if (sourceArray != null)
					{
						var targetArray = Array.CreateInstance(targetProperty.PropertyType.GetElementType(), sourceArray.Length);

						for (int index = 0; index < sourceArray.Length; ++index)
						{
							var sourceItem = sourceArray.GetValue(index);
							var targetItem = Activator.CreateInstance(targetProperty.PropertyType.GetElementType());
							CopyProperties(sourceItem, targetItem);
							targetArray.SetValue(targetItem, index);
						}

						targetProperty.SetValue(target, targetArray, null);
					}
					else
					{
						targetProperty.SetValue(target, null, null);
					}
				}
				// Otherwise, we have to instantiate the target type and copy the properties
				else
				{
					object targetInstance = null;

					Type typeToInstantiate = targetProperty.PropertyType;
					Type nullableType = Nullable.GetUnderlyingType(typeToInstantiate);
					if (nullableType != null)
					{
						typeToInstantiate = nullableType;
					}
					else
					{
						targetInstance = Activator.CreateInstance(typeToInstantiate);
					}

					object sourcePropertyValue = sourceProperty.GetValue(source, null);
					if (sourcePropertyValue != null)
					{
						targetInstance = Activator.CreateInstance(typeToInstantiate);
						CopyProperties(sourcePropertyValue, targetInstance);
					}

					targetProperty.SetValue(target, targetInstance, null);
				}
			}
		}

		private static bool CanRemoveCallback(IntPtr clientDataAddress, object callbackInfo)
		{
			DelegateHolder delegateHolder = null;
			if (s_Callbacks.TryGetValue(clientDataAddress, out delegateHolder))
			{
				if (delegateHolder.NotificationId.HasValue)
				{
					return false;
				}
			}

			var resultProperty = callbackInfo.GetType().GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance).Where(property => property.PropertyType == typeof(Result)).FirstOrDefault();
			if (resultProperty != null)
			{
				var result = (Result)resultProperty.GetValue(callbackInfo, null);
				return IsOperationComplete(result);
			}

			return true;
		}

		private static bool TryGetAndRemoveCallback<TCallback>(IntPtr clientDataAddress, object callbackInfo, out TCallback callback)
			where TCallback : class
		{
			callback = null;

			if (clientDataAddress != IntPtr.Zero && s_Callbacks.ContainsKey(clientDataAddress))
			{
				callback = s_Callbacks[clientDataAddress].Public as TCallback;

				if (CanRemoveCallback(clientDataAddress, callbackInfo))
				{
					s_Callbacks.Remove(clientDataAddress);
					TryRelease(ref clientDataAddress);
				}

				return true;
			}

			return false;
		}

		private static bool TryGetAdditionalCallback<TCallback>(IntPtr clientDataAddress, out TCallback additionalCallback)
			where TCallback : class
		{
			additionalCallback = null;

			if (clientDataAddress != IntPtr.Zero && s_Callbacks.ContainsKey(clientDataAddress))
			{
				additionalCallback = s_Callbacks[clientDataAddress].Additional.FirstOrDefault(delegat => delegat.GetType() == typeof(TCallback)) as TCallback;
				if (additionalCallback != null)
				{
					return true;
				}
			}

			return false;
		}
		#endregion
	}
}