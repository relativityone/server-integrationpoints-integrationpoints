using System;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	internal sealed class StopwatchStub : IStopwatch
	{
		/// <inheritdoc />
		public void Start()
		{
			// Intentionally left empty
		}

		/// <inheritdoc />
		public void Stop()
		{
			// Intentionally left empty
		}

		/// <inheritdoc />
		public TimeSpan Elapsed => TimeSpan.MaxValue;
	}
}