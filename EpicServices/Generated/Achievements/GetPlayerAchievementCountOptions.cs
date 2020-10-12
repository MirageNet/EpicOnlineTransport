// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	/// <summary>
	/// Input parameters for the <see cref="AchievementsInterface.GetPlayerAchievementCount" /> function.
	/// </summary>
	public class GetPlayerAchievementCountOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="AchievementsInterface.GetplayerachievementcountApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return AchievementsInterface.GetplayerachievementcountApiLatest; } }

		/// <summary>
		/// The Product User ID for the user whose achievement count is being retrieved.
		/// </summary>
		public ProductUserId UserId { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct GetPlayerAchievementCountOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
		private IntPtr m_UserId;

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

		public ProductUserId UserId
		{
			get
			{
				var value = Helper.GetDefault<ProductUserId>();
				Helper.TryMarshalGet(m_UserId, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_UserId, value); }
		}

		public void Dispose()
		{
		}
	}
}