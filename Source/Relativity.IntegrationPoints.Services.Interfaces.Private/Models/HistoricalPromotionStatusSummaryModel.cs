namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Model representing a summary of historical promotion status.
    /// </summary>
    public class HistoricalPromotionStatusSummaryModel
    {
        /// <summary>
        /// An array of <see cref="HistoricalPromotionStatusModel"/>
        /// </summary>
        public HistoricalPromotionStatusModel[] HistoricalPromotionStatus { get; set; }
    }
}
