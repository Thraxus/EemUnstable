using System;
using System.Diagnostics;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;

namespace Eem.Thraxus.Common.Utilities.Tools
{
	/// <inheritdoc cref="Profiler" />
	/// <summary>
	/// This code was provided by Digi as a simple profiler
	/// Usage:
	///		Wrap code you want to profile in:
	///			using(new Profiler("somename"))
	///			{
	///				// code to profile
	///			}
	/// </summary>
	public struct Profiler : IDisposable
	{
		private readonly string _name;
		private readonly long _start;

		public Profiler(string name = "unnamed")
		{
			_name = name;
			_start = Stopwatch.GetTimestamp();
		}

		public void Dispose()
		{
			long end = Stopwatch.GetTimestamp();
			TimeSpan timespan = new TimeSpan(end - _start);
			if(Settings.GeneralSettings.ProfilingEnabled)
				StaticLog.WriteToLog(_name, $"{timespan.TotalMilliseconds:0.##########}ms", LogType.Profiling);
		}
	}
}
