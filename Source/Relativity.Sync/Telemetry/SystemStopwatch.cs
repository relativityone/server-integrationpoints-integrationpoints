using System;
using System.Diagnostics;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	/// Wrapper for Stopwatch from the standard .NET library.
	/// </summary>
	internal sealed class SystemStopwatch : IStopwatch
	{
		private readonly Stopwatch _stopwatch;

		/// <inheritdoc />
		public SystemStopwatch()
		{
			_stopwatch = new Stopwatch();
		}

		/// <inheritdoc />
		public void Start()
		{
			_stopwatch.Start();
		}

		/// <inheritdoc />
		public void Stop()
		{
			_stopwatch.Stop();
		}

		/// <inheritdoc />
		public TimeSpan Elapsed => _stopwatch.Elapsed;
	}
}