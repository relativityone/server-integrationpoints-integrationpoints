#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
using System.Linq;
using kCura.EDDS.WebAPI.ProductionManagerBase;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.Client;
using kCura.WinEDDS.Service.Export;
using Relativity.API;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
	public class ExportedArtifactNameRepository : IExportedArtifactNameRepository
	{
		private readonly IRSAPIClient _rsapiClient;
		private readonly IServicesMgr _servicesMgr;
		private readonly IServiceManagerProvider _serviceManagerProvider;

		public ExportedArtifactNameRepository(IServicesMgr servicesMgr, IRSAPIClient rsapiClient, IServiceManagerProvider serviceManagerProvider)
		{
			_servicesMgr = servicesMgr;
			_rsapiClient = rsapiClient;
			_serviceManagerProvider = serviceManagerProvider;
		}

		public string GetViewName(int workspaceId, int viewId)
		{
			_rsapiClient.APIOptions.WorkspaceID = workspaceId;
			return _rsapiClient.Repositories.View.ReadSingle(viewId).Name;
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
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
