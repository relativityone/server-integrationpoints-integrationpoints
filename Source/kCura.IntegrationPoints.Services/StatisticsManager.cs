using System;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Services.Installers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
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

		public Task<int> GetDocumentsTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId)
		{
			return ForSavedSearch<IDocumentTotalStatistics>(workspaceArtifactId, savedSearchId);
		}

		public Task<int> GetNativesTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId)
		{
			return ForSavedSearch<INativeTotalStatistics>(workspaceArtifactId, savedSearchId);
		}

		public Task<int> GetImagesTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId)
		{
			return ForSavedSearch<IImageTotalStatistics>(workspaceArtifactId, savedSearchId);
		}

		public Task<int> GetImagesFileSizeForSavedSearchAsync(int workspaceArtifactId, int savedSearchId)
		{
			return ForSavedSearch<IImageFileSizeStatistics>(workspaceArtifactId, savedSearchId);
		}

		public Task<int> GetNativesFileSizeForSavedSearchAsync(int workspaceArtifactId, int savedSearchId)
		{
			return ForSavedSearch<INativeFileSizeStatistics>(workspaceArtifactId, savedSearchId);
		}

		public Task<int> GetDocumentsTotalForProductionAsync(int workspaceArtifactId, int productionSetId)
		{
			return ForProduction<IDocumentTotalStatistics>(workspaceArtifactId, productionSetId);
		}

		public Task<int> GetNativesTotalForProductionAsync(int workspaceArtifactId, int productionSetId)
		{
			return ForProduction<INativeTotalStatistics>(workspaceArtifactId, productionSetId);
		}

		public Task<int> GetImagesTotalForProductionAsync(int workspaceArtifactId, int productionSetId)
		{
			return ForProduction<IImageTotalStatistics>(workspaceArtifactId, productionSetId);
		}

		public Task<int> GetImagesFileSizeForProductionAsync(int workspaceArtifactId, int productionSetId)
		{
			return ForProduction<IImageFileSizeStatistics>(workspaceArtifactId, productionSetId);
		}

		public Task<int> GetNativesFileSizeForProductionAsync(int workspaceArtifactId, int productionSetId)
		{
			return ForProduction<INativeFileSizeStatistics>(workspaceArtifactId, productionSetId);
		}

		public Task<int> GetDocumentsTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
		{
			return ForFolder<IDocumentTotalStatistics>(workspaceArtifactId, folderId, viewId, includeSubFoldersTotals);
		}

		public Task<int> GetNativesTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
		{
			return ForFolder<INativeTotalStatistics>(workspaceArtifactId, folderId, viewId, includeSubFoldersTotals);
		}

		public Task<int> GetImagesTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
		{
			return ForFolder<IImageTotalStatistics>(workspaceArtifactId, folderId, viewId, includeSubFoldersTotals);
		}

		public Task<int> GetImagesFileSizeForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
		{
			return ForFolder<IImageFileSizeStatistics>(workspaceArtifactId, folderId, viewId, includeSubFoldersTotals);
		}

		public Task<int> GetNativesFileSizeForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
		{
			return ForFolder<INativeFileSizeStatistics>(workspaceArtifactId, folderId, viewId, includeSubFoldersTotals);
		}

		private Task<int> ForSavedSearch<T>(int workspaceArtifactId, int savedSearchId) where T : IDocumentStatistics
		{
			return Execute<T>(statistics => statistics.ForSavedSearch(workspaceArtifactId, savedSearchId), workspaceArtifactId);
		}

		private Task<int> ForProduction<T>(int workspaceArtifactId, int productionId) where T : IDocumentStatistics
		{
			return Execute<T>(statistics => statistics.ForProduction(workspaceArtifactId, productionId), workspaceArtifactId);
		}

		private Task<int> ForFolder<T>(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals) where T : IDocumentStatistics
		{
			return Execute<T>(statistics => statistics.ForFolder(workspaceArtifactId, folderId, viewId, includeSubFoldersTotals), workspaceArtifactId);
		}

		private Task<int> Execute<T>(Func<IDocumentStatistics, int> action, int workspaceArtifactId) where T : IDocumentStatistics
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