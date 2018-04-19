using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class UnfinishedJobService : IUnfinishedJobService
	{
		private readonly IRSAPIServiceFactory _rsapiServiceFactory;

		public UnfinishedJobService(IRSAPIServiceFactory rsapiServiceFactory)
		{
			_rsapiServiceFactory = rsapiServiceFactory;
		}

		public IList<Data.JobHistory> GetUnfinishedJobs(int workspaceArtifactId)
		{
			Guid[] unfinishedChoicesNames = {
				JobStatusChoices.JobHistoryPending.Guids.FirstOrDefault(),
				JobStatusChoices.JobHistoryValidating.Guids.FirstOrDefault(),
				JobStatusChoices.JobHistoryProcessing.Guids.FirstOrDefault(),
				JobStatusChoices.JobHistoryStopping.Guids.FirstOrDefault()
			};

			var request = new QueryRequest
			{
				Fields = new Data.JobHistory().ToFieldList(),
				Condition = $"'{JobHistoryFields.JobStatus}' IN CHOICE [{string.Join(",", unfinishedChoicesNames)}]"
			};

			return _rsapiServiceFactory.Create(workspaceArtifactId).RelativityObjectManager.Query<Data.JobHistory>(request);
		}
	}
}