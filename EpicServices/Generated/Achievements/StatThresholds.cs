// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	/// <summary>
	/// Contains information about a collection of stat threshold data.
	/// 
	/// The threshold will depend on the stat aggregate type:
	/// LATEST (Tracks the latest value)
	/// MAX (Tracks the maximum value)
	/// MIN (Tracks the minimum value)
	/// SUM (Generates a rolling sum of the value)
	/// <seealso cref="Definition" />
	/// </summary>
	public class StatThresholds
	{
		/// <summary>
		/// API Version: Set this to <see cref="AchievementsInterface.StatthresholdsApiLatest" />.
		/// </summary>
		public int ApiVersion { get { return AchievementsInterface.StatthresholdsApiLatest; } }

		/// <summary>
		/// The name of the stat.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The value that the stat must surpass to satisfy the requirement for unlocking an achievement.
		/// </summary>
		public int Threshold { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct StatThresholdsInternal : IDisposable
	{
		private int m_ApiVersion;
		[MarshalAs(UnmanagedType.LPStr)]
		private string m_Name;
		private int m_Threshold;

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

		public string Name
		{
			get
			{
				var value = Helper.GetDefault<string>();
				Helper.TryMarshalGet(m_Name, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_Name, value); }
		}

		public int Threshold
		{
			get
			{
				var value = Helper.GetDefault<int>();
				Helper.TryMarshalGet(m_Threshold, out value);
				return value;
			}
			set { Helper.TryMarshalSet(ref m_Threshold, value); }
		}

		public void Dispose()
		{
		}
	}
}