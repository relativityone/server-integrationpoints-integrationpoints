using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace Relativity.IntegrationPoints.Services
{
    // TODO: Promote was renamed to designation. Update all corresponding references to that field

    /// <summary>
    /// Enables access to information about documents in the eca and investigations application
    /// </summary>
    [WebService("Document Manager")]
    [ServiceAudience(Audience.Private)]
    public interface IDocumentManager : IKeplerService, IDisposable
    {
        /// <summary>
        /// Gets the number of documents that exist in the current workspace and how many have
        /// been pushed to other workspaces.
        /// </summary>
        /// <param name="request">A <see cref="PercentagePushedToReviewRequest"/></param>
        /// <returns>Returns a <see cref="PercentagePushedToReviewModel"/></returns>
        Task<PercentagePushedToReviewModel> GetPercentagePushedToReviewAsync(PercentagePushedToReviewRequest request);

        /// <summary>
        /// Gets the number of documents for each choice of the designation field as well as the number
        /// that have been pushed to other workspaces.
        /// </summary>
        /// <param name="request">A <see cref="CurrentPromotionStatusRequest"/></param>
        /// <returns>Returns a <see cref="CurrentPromotionStatusModel"/></returns>
        Task<CurrentPromotionStatusModel> GetCurrentPromotionStatusAsync(CurrentPromotionStatusRequest request);

        /// <summary>
        /// Gets the number of documents for each choice of the designation field for the last thirty days
        /// </summary>
        /// <param name="request">A <see cref="HistoricalPromotionStatusRequest"/></param>
        /// <returns>Returns a <see cref="HistoricalPromotionStatusSummaryModel"/></returns>
        Task<HistoricalPromotionStatusSummaryModel> GetHistoricalPromotionStatusAsync(HistoricalPromotionStatusRequest request);
    }
}