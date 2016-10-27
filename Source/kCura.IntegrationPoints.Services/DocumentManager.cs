using System.Threading.Tasks;
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
		private readonly IDocumentRepository _documentRepository;

		/// <summary>
		///     For testing purposes only
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositoryFactory"></param>
		/// <param name="documentRepository"></param>
		internal DocumentManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IDocumentRepository documentRepository)
			: base(logger, permissionRepositoryFactory)
		{
			_documentRepository = documentRepository;
		}

		public DocumentManager(ILog logger) : base(logger)
		{
			_documentRepository = new DocumentRepository(logger);
		}

		public void Dispose()
		{
		}

		public async Task<PercentagePushedToReviewModel> GetPercentagePushedToReviewAsync(PercentagePushedToReviewRequest request)
		{
			return await Execute(() => _documentRepository.GetPercentagePushedToReviewAsync(request), request.WorkspaceArtifactId).ConfigureAwait(false);
		}

		public async Task<CurrentPromotionStatusModel> GetCurrentPromotionStatusAsync(CurrentPromotionStatusRequest request)
		{
			return await Execute(() => _documentRepository.GetCurrentPromotionStatusAsync(request), request.WorkspaceArtifactId).ConfigureAwait(false);
		}

		public async Task<HistoricalPromotionStatusSummaryModel> GetHistoricalPromotionStatusAsync(HistoricalPromotionStatusRequest request)
		{
			return await Execute(() => _documentRepository.GetHistoricalPromotionStatusAsync(request), request.WorkspaceArtifactId).ConfigureAwait(false);
		}
	}
}