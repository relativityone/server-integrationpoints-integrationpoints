﻿using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Interfaces.Private.Requests;
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
		/// <returns></returns>
		Task<PercentagePushedToReviewModel> GetPercentagePushedToReview(PercentagePushedToReviewRequest request);
	}
}