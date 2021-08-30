using System;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    class FakeDocumentRepository : IDocumentRepository
    {
        public Task<CurrentPromotionStatusModel> GetCurrentPromotionStatusAsync(CurrentPromotionStatusRequest request)
        {
            CurrentPromotionStatusModel currentPromotionStatusModel = new CurrentPromotionStatusModel
            {
                TotalDocumentsPushedToReview = 10,
                TotalDocumentsExcluded = 10,
                TotalDocumentsIncluded = 10,
                TotalDocumentsUntagged = 10
            };
            return Task.FromResult(currentPromotionStatusModel);
        }

        public Task<HistoricalPromotionStatusSummaryModel> GetHistoricalPromotionStatusAsync(HistoricalPromotionStatusRequest request)
        {
            HistoricalPromotionStatusSummaryModel historicalPromotionStatusSummaryModel =
                new HistoricalPromotionStatusSummaryModel()
                {
                    HistoricalPromotionStatus = new[]
                    {
                        new HistoricalPromotionStatusModel
                        {
                            TotalDocumentsExcluded = 10,
                            TotalDocumentsIncluded = 10,
                            TotalDocumentsUntagged = 10,
                            Date = new DateTime(10)
                        }
                    }
                };

            return Task.FromResult(historicalPromotionStatusSummaryModel);
        }

        public Task<PercentagePushedToReviewModel> GetPercentagePushedToReviewAsync(PercentagePushedToReviewRequest request)
        {
            PercentagePushedToReviewModel percentagePushedToReviewModel = new PercentagePushedToReviewModel
            {
                TotalDocumentsPushedToReview = 10,
                TotalDocuments = 10
            };

            return Task.FromResult(percentagePushedToReviewModel);
        }
    }
}
