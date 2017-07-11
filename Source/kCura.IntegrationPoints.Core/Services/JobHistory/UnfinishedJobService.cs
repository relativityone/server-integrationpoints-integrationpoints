using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

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
			var unfinishedChoicesNames = new List<Guid>
			{
				JobStatusChoices.JobHistoryPending.Guids.FirstOrDefault(),
				JobStatusChoices.JobHistoryProcessing.Guids.FirstOrDefault(),
				JobStatusChoices.JobHistoryStopping.Guids.FirstOrDefault()
			};
			Condition unfinishedJobsCondition = new SingleChoiceCondition(new Guid(JobHistoryFieldGuids.JobStatus), SingleChoiceConditionEnum.AnyOfThese, unfinishedChoicesNames);
			Query<RDO> query = new Query<RDO>
			{
				Fields = FieldValue.AllFields,
				Condition = unfinishedJobsCondition
			};

			return _rsapiServiceFactory.Create(workspaceArtifactId).JobHistoryLibrary.Query(query);
		}
	}
}