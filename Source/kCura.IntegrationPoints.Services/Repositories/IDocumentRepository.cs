namespace kCura.IntegrationPoints.Services.Repositories
{
	public interface IDocumentRepository
	{
		CurrentPromotionStatusModel GetCurrentPromotionStatusAsync(CurrentPromotionStatusRequest request);
		HistoricalPromotionStatusSummaryModel GetHistoricalPromotionStatusAsync(HistoricalPromotionStatusRequest request);
		PercentagePushedToReviewModel GetPercentagePushedToReviewAsync(PercentagePushedToReviewRequest request);
	}
}