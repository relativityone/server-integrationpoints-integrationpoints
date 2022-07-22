namespace Relativity.Sync.Telemetry
{
    /// <summary>
    ///     Represents the types of metrics logged.
    /// </summary>
    internal enum MetricType
    {
        PointInTimeString = 0,
        PointInTimeLong,
        PointInTimeDouble,
        TimedOperation,
        Counter,
        GaugeOperation
    }
}
