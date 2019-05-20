using System;
using System.Diagnostics;

namespace Relativity.Sync.Telemetry
{
	internal sealed class DisposableStopwatch : IDisposable
	{
		private readonly Action<TimeSpan> _disposeAction;
		private readonly Stopwatch _stopwatch;

		public DisposableStopwatch(Action<TimeSpan> disposeAction)
		{
			_disposeAction = disposeAction;
			_stopwatch = Stopwatch.StartNew();
		}

		public void Dispose()
		{
			_stopwatch.Stop();
			_disposeAction.Invoke(_stopwatch.Elapsed);
		}
	}
}