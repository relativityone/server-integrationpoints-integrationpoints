using System;
using System.Threading.Tasks;
using Castle.Windsor;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Installers;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace Relativity.IntegrationPoints.Services
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
            CheckPermissions(nameof(GetPercentagePushedToReviewAsync), request.WorkspaceArtifactId);
            try
            {
                using (var container = GetDependenciesContainer(request.WorkspaceArtifactId))
                {
                    var documentRepository = container.Resolve<IDocumentAccessor>();
                    return await documentRepository.GetPercentagePushedToReviewAsync(request).ConfigureAwait(false);
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
            CheckPermissions(nameof(GetCurrentPromotionStatusAsync), request.WorkspaceArtifactId);
            try
            {
                using (var container = GetDependenciesContainer(request.WorkspaceArtifactId))
                {
                    var documentRepository = container.Resolve<IDocumentAccessor>();
                    return await documentRepository.GetCurrentPromotionStatusAsync(request).ConfigureAwait(false);
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
            CheckPermissions(nameof(GetHistoricalPromotionStatusAsync), request.WorkspaceArtifactId);
            try
            {
                using (var container = GetDependenciesContainer(request.WorkspaceArtifactId))
                {
                    var documentRepository = container.Resolve<IDocumentAccessor>();
                    return await documentRepository.GetHistoricalPromotionStatusAsync(request).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                LogException(nameof(GetHistoricalPromotionStatusAsync), e);
                throw CreateInternalServerErrorException();
            }
        }

        protected override Installer Installer => _installer ?? (_installer = new DocumentManagerInstaller());
    }
}
