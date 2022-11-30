namespace kCura.IntegrationPoints.Data.Statistics
{
    public class CalculationState
    {
        public bool IsCalculating { get; set; }

        public bool HasErrors { get; set; }

        public DocumentsStatistics DocumentStatistics { get; set; }
    }
}
