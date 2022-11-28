using System;

namespace kCura.IntegrationPoints.Data.Statistics
{
    public class CalculationState
    {
        public bool IsCalculating { get; set; }

        public DateTime CalculationFinishTime { get; set; }

        public DocumentsStatistics DocumentStatistics { get; set; }
    }
}
