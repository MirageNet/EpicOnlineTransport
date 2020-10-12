// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	/// <summary>
	/// Input parameters for the <see cref="LeaderboardsInterface.QueryLeaderboardUserScores" /> function.
	/// </summary>
	public class QueryLeaderboardUserScoresOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="LeaderboardsInterface.QueryleaderboarduserscoresApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return LeaderboardsInterface.QueryleaderboarduserscoresApiLatest; } }

		/// <summary>
		/// An array of Product User IDs indicating the users whose scores you want to retrieve
		/// </summary>
		public ProductUserId[] UserIds { get; set; }

		/// <summary>
		/// The stats to be collected, along with the sorting method to use when determining rank order for each stat
		/// </summary>
		public UserScoresQueryStatInfo[] StatInfo { get; set; }

		/// <summary>
		/// An optional POSIX timestamp, or <see cref="LeaderboardsInterface.TimeUndefined" />; results will only include scores made after this time
		/// </summary>
		public DateTimeOffset? StartTime { get; set; }

		/// <summary>
		/// An optional POSIX timestamp, or <see cref="LeaderboardsInterface.TimeUndefined" />; results will only include scores made before this time
		/// </summary>
		public DateTimeOffset? EndTime { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryLeaderboardUserScoresOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
		private IntPtr m_UserIds;
		private uint m_UserIdsCount;
		private IntPtr m_StatInfo;
		private uint m_StatInfoCount;
		private long m_StartTime;
		private long m_EndTime;

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

		public ProductUserId[] UserIds
		{
			get
			{
				var value = Helper.GetDefault<ProductUserId[]>();
				Helper.TryMarshalGet(m_UserIds, out value, m_UserIdsCount);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_UserIds, value, out m_UserIdsCount); }
		}

		public UserScoresQueryStatInfoInternal[] StatInfo
		{
			get
			{
				var value = Helper.GetDefault<UserScoresQueryStatInfoInternal[]>();
				Helper.TryMarshalGet(m_StatInfo, out value, m_StatInfoCount);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_StatInfo, value, out m_StatInfoCount); }
		}

		public DateTimeOffset? StartTime
		{
			get
			{
				var value = Helper.GetDefault<DateTimeOffset?>();
				Helper.TryMarshalGet(m_StartTime, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_StartTime, value); }
		}

		public DateTimeOffset? EndTime
		{
			get
			{
				var value = Helper.GetDefault<DateTimeOffset?>();
				Helper.TryMarshalGet(m_EndTime, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_EndTime, value); }
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_UserIds);
			Helper.TryMarshalDispose(ref m_StatInfo);
		}
	}
}