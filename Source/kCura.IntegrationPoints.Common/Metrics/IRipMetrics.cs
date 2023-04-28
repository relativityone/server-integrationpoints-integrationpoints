using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Common.Metrics
{
    public interface IRipMetrics
    {
        void TimedOperation(string name, TimeSpan duration, Dictionary<string, object> customData, string correlationId);

        void PointInTimeLong(string name, long value, Dictionary<string, object> customData, string correlationId);

        void PointInTimeDouble(string name, double value, Dictionary<string, object> customData, string correlationId);
    }
}
