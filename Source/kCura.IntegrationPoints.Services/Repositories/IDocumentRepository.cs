namespace kCura.IntegrationPoints.Services.Repositories
{
	public interface IDocumentRepository
	{
		CurrentPromotionStatusModel GetCurrentPromotionStatus(CurrentPromotionStatusRequest request);
		HistoricalPromotionStatusSummaryModel GetHistoricalPromotionStatus(HistoricalPromotionStatusRequest request);
		PercentagePushedToReviewModel GetPercentagePushedToReview(PercentagePushedToReviewRequest request);
	}
}