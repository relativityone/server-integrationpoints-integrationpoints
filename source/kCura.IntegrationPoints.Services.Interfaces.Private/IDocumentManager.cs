﻿using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace kCura.IntegrationPoints.Services
{
	/// <summary>
	/// Enables access to information about documents in the eca and investigations application
	/// </summary>
	[WebService("Document Manager")]
	[ServiceAudience(Audience.Private)]
	public interface IDocumentManager: IDisposable
	{
		/// <summary>
		/// Pings the service to ensure it is up and running.
		/// </summary>
		Task<bool> PingAsync();

		/// <summary>
		/// Gets the number of documents that exist in the current workspace and how many have
		/// been pushed to other workspaces.
		/// </summary>
		/// <param name="request">A <see cref="PercentagePushedToReviewRequest"/></param>
		/// <returns>Returns a <see cref="PercentagePushedToReviewModel"/></returns>
		Task<PercentagePushedToReviewModel> GetPercentagePushedToReviewAsync(PercentagePushedToReviewRequest request);

		/// <summary>
		/// Gets the number of documents for each choice of the promote field as well as the number
		/// that have been pushed to other workspaces.
		/// </summary>
		/// <param name="request">A <see cref="CurrentPromotionStatusRequest"/></param>
		/// <returns>Returns a <see cref="CurrentPromotionStatusModel"/></returns>
		Task<CurrentPromotionStatusModel> GetCurrentPromotionStatusAsync(CurrentPromotionStatusRequest request);

		/// <summary>
		/// Gets the number of documents for each choice of the promote field for each day
		/// </summary>
		/// <param name="request">A <see cref="HistoricalPromotionStatusRequest"/></param>
		/// <returns>Returns a <see cref="HistoricalPromotionStatusSummaryModel"/></returns>
		Task<HistoricalPromotionStatusSummaryModel> GetHistoricalPromotionStatusAsync(HistoricalPromotionStatusRequest request);
	}
}