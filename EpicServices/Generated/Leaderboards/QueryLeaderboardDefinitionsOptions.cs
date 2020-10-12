// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	/// <summary>
	/// Input parameters for the <see cref="LeaderboardsInterface.QueryLeaderboardDefinitions" /> function.
	/// StartTime and EndTime are optional parameters, they can be used to limit the list of definitions
	/// to overlap the time window specified.
	/// </summary>
	public class QueryLeaderboardDefinitionsOptions
	{
		/// <summary>
		/// API Version: Set this to <see cref="LeaderboardsInterface.QueryleaderboarddefinitionsApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return LeaderboardsInterface.QueryleaderboarddefinitionsApiLatest; } }

		/// <summary>
		/// An optional POSIX timestamp for the leaderboard's start time, or <see cref="LeaderboardsInterface.TimeUndefined" />
		/// </summary>
		public DateTimeOffset? StartTime { get; set; }

		/// <summary>
		/// An optional POSIX timestamp for the leaderboard's end time, or <see cref="LeaderboardsInterface.TimeUndefined" />
		/// </summary>
		public DateTimeOffset? EndTime { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryLeaderboardDefinitionsOptionsInternal : IDisposable
	{
		private int m_ApiVersion;
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
		}
	}
}