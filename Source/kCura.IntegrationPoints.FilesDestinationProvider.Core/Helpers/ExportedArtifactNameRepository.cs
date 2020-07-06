#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Relativity.Client;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
	public class ExportedArtifactNameRepository : IExportedArtifactNameRepository
	{
		private readonly IRSAPIClient _rsapiClient;
		private readonly IServiceManagerProvider _serviceManagerProvider;

		public ExportedArtifactNameRepository(IRSAPIClient rsapiClient, IServiceManagerProvider serviceManagerProvider)
		{
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
			var productionManager = _serviceManagerProvider.Create<IProductionManager, ProductionManagerFactory>();
			var production = productionManager.Read(workspaceId, productionId);
			return production.Name;
		}

		public string GetSavedSearchName(int workspaceId, int savedSearchId)
		{
			_rsapiClient.APIOptions.WorkspaceID = workspaceId;
			var query = new GetSavedSearchQuery(_rsapiClient, savedSearchId);
			var queryResult = query.ExecuteQuery();
			return queryResult.QueryArtifacts[0].getFieldByName("Text Identifier").ToString();
		}
	}
}
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
