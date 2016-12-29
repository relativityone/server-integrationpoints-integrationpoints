using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Installers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using kCura.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
{
	/// <summary>
	///     Get information about the documents in ECA case such as pushed to
	///     review, included, excluded, untagged, etc.
	/// </summary>
	public class DocumentManager : KeplerServiceBase, IDocumentManager
	{
		private Installer _installer;

		/// <summary>
		///     For testing purposes only
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositoryFactory"></param>
		internal DocumentManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory)
			: base(logger, permissionRepositoryFactory)
		{
		}

		public DocumentManager(ILog logger) : base(logger)
		{
		}

		public void Dispose()
		{
		}

		public async Task<PercentagePushedToReviewModel> GetPercentagePushedToReviewAsync(PercentagePushedToReviewRequest request)
		{
			return
				await
					Execute((IDocumentRepository documentRepository) => documentRepository.GetPercentagePushedToReview(request), request.WorkspaceArtifactId).ConfigureAwait(false);
		}

		public async Task<CurrentPromotionStatusModel> GetCurrentPromotionStatusAsync(CurrentPromotionStatusRequest request)
		{
			return
				await
					Execute((IDocumentRepository documentRepository) => documentRepository.GetCurrentPromotionStatus(request), request.WorkspaceArtifactId).ConfigureAwait(false);
		}

		public async Task<HistoricalPromotionStatusSummaryModel> GetHistoricalPromotionStatusAsync(HistoricalPromotionStatusRequest request)
		{
			return
				await
					Execute((IDocumentRepository documentRepository) => documentRepository.GetHistoricalPromotionStatus(request), request.WorkspaceArtifactId).ConfigureAwait(false);
		}

		protected override Installer Installer => _installer ?? (_installer = new DocumentManagerInstaller());
	}
}