using System;
using System.Collections.Generic;

namespace Relativity.IntegrationPoints.FieldsMapping.Metrics
{
    public interface IMetricsSender
    {
        void CountOperation(string name);
        void GaugeOperation(string name, long value, string unitOfMeasure, Dictionary<string, object> customData = null);
    }
}