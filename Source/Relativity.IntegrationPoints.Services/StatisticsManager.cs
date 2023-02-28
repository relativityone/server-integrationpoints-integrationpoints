using System;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Statistics;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Installers;
using Relativity.Logging;

namespace Relativity.IntegrationPoints.Services
{
    public class StatisticsManager : KeplerServiceBase, IStatisticsManager
    {
        private Installer _installer;

        protected override Installer Installer => _installer ?? (_installer = new StatisticsManagerInstaller());

        internal StatisticsManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
            : base(logger, permissionRepositoryFactory, container)
        {
        }

        public StatisticsManager(ILog logger) : base(logger)
        {
        }

        public Task<long> GetDocumentsTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId)
        {
            return ForSavedSearch<IDocumentTotalStatistics>(workspaceArtifactId, savedSearchId);
        }

        public Task<long> GetNativesTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId)
        {
            return ForSavedSearch<INativeTotalStatistics>(workspaceArtifactId, savedSearchId);
        }

        public Task<long> GetImagesTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId)
        {
            return ForSavedSearch<IImageTotalStatistics>(workspaceArtifactId, savedSearchId);
        }

        public Task<long> GetImagesFileSizeForSavedSearchAsync(int workspaceArtifactId, int savedSearchId)
        {
            return ForSavedSearch<IImageFileSizeStatistics>(workspaceArtifactId, savedSearchId);
        }

        public Task<long> GetNativesFileSizeForSavedSearchAsync(int workspaceArtifactId, int savedSearchId)
        {
            return ForSavedSearch<INativeFileSizeStatistics>(workspaceArtifactId, savedSearchId);
        }

        public Task<long> GetDocumentsTotalForProductionAsync(int workspaceArtifactId, int productionSetId)
        {
            return ForProduction<IDocumentTotalStatistics>(workspaceArtifactId, productionSetId);
        }

        public Task<long> GetNativesTotalForProductionAsync(int workspaceArtifactId, int productionSetId)
        {
            return ForProduction<INativeTotalStatistics>(workspaceArtifactId, productionSetId);
        }

        public Task<long> GetImagesTotalForProductionAsync(int workspaceArtifactId, int productionSetId)
        {
            return ForProduction<IImageTotalStatistics>(workspaceArtifactId, productionSetId);
        }

        public Task<long> GetImagesFileSizeForProductionAsync(int workspaceArtifactId, int productionSetId)
        {
            return ForProduction<IImageFileSizeStatistics>(workspaceArtifactId, productionSetId);
        }

        public Task<long> GetNativesFileSizeForProductionAsync(int workspaceArtifactId, int productionSetId)
        {
            return ForProduction<INativeFileSizeStatistics>(workspaceArtifactId, productionSetId);
        }

        public Task<long> GetDocumentsTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
        {
            return ForFolder<IDocumentTotalStatistics>(workspaceArtifactId, folderId, viewId, includeSubFoldersTotals);
        }

        public Task<long> GetNativesTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
        {
            return ForFolder<INativeTotalStatistics>(workspaceArtifactId, folderId, viewId, includeSubFoldersTotals);
        }

        public Task<long> GetImagesTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
        {
            return ForFolder<IImageTotalStatistics>(workspaceArtifactId, folderId, viewId, includeSubFoldersTotals);
        }

        public Task<long> GetImagesFileSizeForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
        {
            return ForFolder<IImageFileSizeStatistics>(workspaceArtifactId, folderId, viewId, includeSubFoldersTotals);
        }

        public Task<long> GetNativesFileSizeForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
        {
            return ForFolder<INativeFileSizeStatistics>(workspaceArtifactId, folderId, viewId, includeSubFoldersTotals);
        }

        private Task<long> ForSavedSearch<T>(int workspaceArtifactId, int savedSearchId) where T : IDocumentStatistics
        {
            return Execute<T>(statistics => statistics.ForSavedSearch(workspaceArtifactId, savedSearchId), workspaceArtifactId);
        }

        private Task<long> ForProduction<T>(int workspaceArtifactId, int productionId) where T : IDocumentStatistics
        {
            return Execute<T>(statistics => statistics.ForProduction(workspaceArtifactId, productionId), workspaceArtifactId);
        }

        private Task<long> ForFolder<T>(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals) where T : IDocumentStatistics
        {
            return Execute<T>(statistics => statistics.ForFolder(workspaceArtifactId, folderId, viewId, includeSubFoldersTotals), workspaceArtifactId);
        }

        private Task<long> Execute<T>(Func<IDocumentStatistics, long> action, int workspaceArtifactId) where T : IDocumentStatistics
        {
            CheckPermissions($"{nameof(StatisticsManager)} - {nameof(T)}", workspaceArtifactId);
            try
            {
                using (IWindsorContainer container = GetDependenciesContainer(workspaceArtifactId))
                {
                    var statistics = container.Resolve<T>();
                    return Task.Run(() => action(statistics));
                }
            }
            catch (Exception e)
            {
                LogException($"{nameof(ForSavedSearch)} - {nameof(T)}", e);
                throw CreateInternalServerErrorException();
            }
        }

        public void Dispose()
        {
        }
    }
}
