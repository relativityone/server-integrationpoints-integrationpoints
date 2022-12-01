namespace kCura.IntegrationPoints.Data.Statistics
{
    public class CalculationState
    {
        public CalculationStatus Status { get; set; }

        public DocumentsStatistics DocumentStatistics { get; set; }
    }

    // TODO: MOVE IT TO SEPARATE FILE (if finally used)
    public enum CalculationStatus
    {
        New = 0,
        InProgress = 1,
        Completed = 2,
        Canceled = 3,
        Error = 4
    }
}
