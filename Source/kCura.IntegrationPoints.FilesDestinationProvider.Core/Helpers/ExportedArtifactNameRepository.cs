using System.Linq;
using kCura.EDDS.WebAPI.ProductionManagerBase;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.WinEDDS.Service.Export;
using Relativity.API;
using Relativity.Services.Search;
using Relativity.Services.View;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
	public class ExportedArtifactNameRepository : IExportedArtifactNameRepository
	{
		private readonly IServicesMgr _servicesMgr;
		private readonly IServiceManagerProvider _serviceManagerProvider;

		public ExportedArtifactNameRepository(IServicesMgr servicesMgr, IServiceManagerProvider serviceManagerProvider)
		{
			_servicesMgr = servicesMgr;
			_serviceManagerProvider = serviceManagerProvider;
		}

		public string GetViewName(int workspaceId, int viewId)
		{
			using (IViewManager viewManager = _servicesMgr.CreateProxy<IViewManager>(ExecutionIdentity.System))
			{
				View view = viewManager.ReadSingleAsync(workspaceId, viewId).GetAwaiter().GetResult();
				return view.Name;
			}
		}

		public string GetProductionName(int workspaceId, int productionId)
		{
			IProductionManager productionManager = _serviceManagerProvider.Create<IProductionManager, ProductionManagerFactory>();
			ProductionInfo production = productionManager.Read(workspaceId, productionId);
			return production.Name;
		}

		public string GetSavedSearchName(int workspaceId, int savedSearchId)
		{
			var query = new GetSavedSearchQuery(_servicesMgr, workspaceId, savedSearchId);
			KeywordSearchQueryResultSet queryResult = query.ExecuteQuery();
			
			if (!queryResult.Success)
			{
				throw new IntegrationPointsException($"Error occured when querying for saved search Artifact ID: {savedSearchId}. Message: {queryResult.Message}");
			}

			if (!queryResult.Results.Any())
			{
				throw new IntegrationPointsException($"Cannot find saved search artifact ID: {savedSearchId}");
			}

			return queryResult.Results.Single().Artifact.Name;
		}
	}
}
