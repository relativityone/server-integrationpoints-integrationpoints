using System;

namespace Relativity.Sync.Telemetry
{
    internal sealed class LongTextStreamStatistics
    {
        public long TotalBytesRead { get; set; }
        public TimeSpan TotalReadTime { get; set; }

        public static LongTextStreamStatistics Empty => new LongTextStreamStatistics();
    }
}