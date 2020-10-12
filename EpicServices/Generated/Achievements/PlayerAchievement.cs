// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	/// <summary>
	/// Contains information about a single player achievement.
	/// </summary>
	public class PlayerAchievement
	{
		/// <summary>
		/// API Version: Set this to <see cref="AchievementsInterface.PlayerachievementApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return AchievementsInterface.PlayerachievementApiLatest; } }

		/// <summary>
		/// This achievement's unique identifier.
		/// </summary>
		public string AchievementId { get; set; }

		/// <summary>
		/// Progress towards completing this achievement (as a percentage).
		/// </summary>
		public double Progress { get; set; }

		/// <summary>
		/// The POSIX timestamp when the achievement was unlocked. If the achievement has not been unlocked, this value will be <see cref="AchievementsInterface.AchievementUnlocktimeUndefined" />.
		/// </summary>
		public DateTimeOffset? UnlockTime { get; set; }

		/// <summary>
		/// Array of <see cref="PlayerStatInfo" /> structures containing information about stat thresholds used to unlock the achievement and the player's current values for those stats.
		/// </summary>
		public PlayerStatInfo[] StatInfo { get; set; }

		/// <summary>
		/// Localized display name for the achievement based on this specific player's current progress on the achievement.
		/// @note The current progress is updated when <see cref="AchievementsInterface.QueryPlayerAchievements" /> succeeds and when an achievement is unlocked.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Localized description for the achievement based on this specific player's current progress on the achievement.
		/// @note The current progress is updated when <see cref="AchievementsInterface.QueryPlayerAchievements" /> succeeds and when an achievement is unlocked.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// URL of an icon to display for the achievement based on this specific player's current progress on the achievement. This may be null if there is no data configured in the dev portal.
		/// @note The current progress is updated when <see cref="AchievementsInterface.QueryPlayerAchievements" /> succeeds and when an achievement is unlocked.
		/// </summary>
		public string IconURL { get; set; }

		/// <summary>
		/// Localized flavor text that can be used by the game in an arbitrary manner. This may be null if there is no data configured in the dev portal.
		/// </summary>
		public string FlavorText { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PlayerAchievementInternal : IDisposable
	{
		private int m_ApiVersion;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_AchievementId;
		private double m_Progress;
		private long m_UnlockTime;
		private int m_StatInfoCount;
		private IntPtr m_StatInfo;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_DisplayName;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_Description;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_IconURL;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_FlavorText;

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

		public string AchievementId
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_AchievementId, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_AchievementId, value); }
		}

		public double Progress
		{
			get
			{
				var value = Helper.GetDefault<double>();
				Helper.TryMarshalGet(m_Progress, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_Progress, value); }
		}

		public DateTimeOffset? UnlockTime
		{
			get
			{
				var value = Helper.GetDefault<DateTimeOffset?>();
				Helper.TryMarshalGet(m_UnlockTime, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_UnlockTime, value); }
		}

		public PlayerStatInfoInternal[] StatInfo
		{
			get
			{
				var value = Helper.GetDefault<PlayerStatInfoInternal[]>();
				Helper.TryMarshalGet(m_StatInfo, out value, m_StatInfoCount);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_StatInfo, value, out m_StatInfoCount); }
		}

		public string DisplayName
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_DisplayName, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_DisplayName, value); }
		}

		public string Description
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_Description, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_Description, value); }
		}

		public string IconURL
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_IconURL, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_IconURL, value); }
		}

		public string FlavorText
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_FlavorText, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_FlavorText, value); }
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_StatInfo);
		}
	}
}