using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Common.Metrics
{
    public interface IRipMetrics
    {
        string GetWorkflowId();

        void TimedOperation(string name, TimeSpan duration, Dictionary<string, object> customData);

        void PointInTimeLong(string name, long value, Dictionary<string, object> customData);

        void PointInTimeDouble(string name, double value, Dictionary<string, object> customData);
    }
}
