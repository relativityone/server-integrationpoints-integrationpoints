using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Windsor;
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
		/// <param name="container"></param>
		internal DocumentManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
			: base(logger, permissionRepositoryFactory, container)
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
			CheckDocumentManagerPermissions(request.WorkspaceArtifactId, nameof(GetPercentagePushedToReviewAsync));
			try
			{
				using (var container = GetDependenciesContainer(request.WorkspaceArtifactId))
				{
					var documentRepository = container.Resolve<IDocumentRepository>();
					return await Task.Run(() => documentRepository.GetPercentagePushedToReview(request)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetPercentagePushedToReviewAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<CurrentPromotionStatusModel> GetCurrentPromotionStatusAsync(CurrentPromotionStatusRequest request)
		{
			CheckDocumentManagerPermissions(request.WorkspaceArtifactId, nameof(GetCurrentPromotionStatusAsync));
			try
			{
				using (var container = GetDependenciesContainer(request.WorkspaceArtifactId))
				{
					var documentRepository = container.Resolve<IDocumentRepository>();
					return await Task.Run(() => documentRepository.GetCurrentPromotionStatus(request)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetCurrentPromotionStatusAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<HistoricalPromotionStatusSummaryModel> GetHistoricalPromotionStatusAsync(HistoricalPromotionStatusRequest request)
		{
			CheckDocumentManagerPermissions(request.WorkspaceArtifactId, nameof(GetHistoricalPromotionStatusAsync));
			try
			{
				using (var container = GetDependenciesContainer(request.WorkspaceArtifactId))
				{
					var documentRepository = container.Resolve<IDocumentRepository>();
					return await Task.Run(() => documentRepository.GetHistoricalPromotionStatus(request)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetHistoricalPromotionStatusAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		private void CheckDocumentManagerPermissions(int workspaceId, string endpointName)
		{
			SafePermissionCheck(() =>
			{
				var permissionRepository = GetPermissionRepository(workspaceId);
				bool hasWorkspaceAccess = permissionRepository.UserHasPermissionToAccessWorkspace();
				if (hasWorkspaceAccess)
				{
					return;
				}
				LogAndThrowInsufficientPermissionException(endpointName, new List<string> {"Workspace"});
			});
		}

		protected override Installer Installer => _installer ?? (_installer = new DocumentManagerInstaller());
	}
}