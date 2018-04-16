using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class UnlinkedJobHistoryService : IUnlinkedJobHistoryService
	{
		private readonly IRSAPIServiceFactory _rsapiServiceFactory;

		public UnlinkedJobHistoryService(IRSAPIServiceFactory rsapiServiceFactory)
		{
			_rsapiServiceFactory = rsapiServiceFactory;
		}

		public List<int> FindUnlinkedJobHistories(int workspaceArtifactId)
		{
			IRSAPIService rsapiService = _rsapiServiceFactory.Create(workspaceArtifactId);

			var request = new QueryRequest
			{
				Condition = $"NOT '{JobHistoryFields.IntegrationPoint}' ISSET",
				Fields = JobHistoryFields.SlimFieldList
			};
			return rsapiService.RelativityObjectManager.Query<Data.JobHistory>(request).Select(x => x.ArtifactId).ToList();
		}
	}
}