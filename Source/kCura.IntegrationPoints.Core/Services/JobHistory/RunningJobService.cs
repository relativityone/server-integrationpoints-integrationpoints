using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class RunningJobService : IRunningJobService
	{
		private readonly IHelper _helper;

		public RunningJobService(IHelper helper)
		{
			_helper = helper;
		}

		public List<RDO> GetRunningJobs(int workspaceArtifactId)
		{
			var unfinishedJobsCondition = new SingleChoiceCondition(new Guid(JobHistoryFieldGuids.JobStatus), SingleChoiceConditionEnum.AnyOfThese, new List<Guid>
			{
				JobStatusChoices.JobHistoryProcessing.Guids.FirstOrDefault()
			});
			Query<RDO> query = new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.JobHistory),
				Fields = FieldValue.AllFields,
				Condition = unfinishedJobsCondition
			};
			QueryResultSet<RDO> result;
			using (var proxy = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				proxy.APIOptions.WorkspaceID = workspaceArtifactId;
				result = proxy.Repositories.RDO.Query(query);
			}
			if (!result.Success)
			{
				throw new AggregateException(result.Message, result.Results.Select(x => new Exception(x.Message)));
			}
			return result.Results.Select(x => x.Artifact).ToList();
		}
	}
}