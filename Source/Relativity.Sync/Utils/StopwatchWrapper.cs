using System;
using System.Diagnostics;

namespace Relativity.Sync.Utils
{
    /// <summary>
    /// Wrapper for Stopwatch from the standard .NET library.
    /// </summary>
    internal sealed class StopwatchWrapper : IStopwatch
    {
        private readonly Stopwatch _stopwatch;

        public StopwatchWrapper()
        {
            _stopwatch = new Stopwatch();
        }

        /// <inheritdoc />
        public void Start()
        {
            _stopwatch.Start();
        }

        public IStopwatch StartNew()
        {
            var stopwatch = new StopwatchWrapper();
            stopwatch.Start();
            return stopwatch;
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
