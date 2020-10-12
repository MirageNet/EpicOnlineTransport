// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	/// <summary>
	/// Input parameters for the <see cref="AchievementsInterface.UnlockAchievements" /> function.
	/// </summary>
	public class UnlockAchievementsOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="AchievementsInterface.UnlockachievementsApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return AchievementsInterface.UnlockachievementsApiLatest; } }

		/// <summary>
		/// The Product User ID for the user whose achievements we want to unlock.
		/// </summary>
		public ProductUserId UserId { get; set; }

		/// <summary>
		/// An array of Achievement IDs to unlock.
		/// </summary>
		public string[] AchievementIds { get; set; }

	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UnlockAchievementsOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
		private IntPtr m_UserId;
		private IntPtr m_AchievementIds;
		private uint m_AchievementsCount;

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

		public string[] AchievementIds
		{
			get
			{
				var value = Helper.GetDefault<string[]>();
				Helper.TryMarshalGet(m_AchievementIds, out value, m_AchievementsCount);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_AchievementIds, value, out m_AchievementsCount); }
		}


		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AchievementIds);
		}
	}
}