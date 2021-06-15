using kCura.IntegrationPoints.Data;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
	{
		private void SetupJobHistoryError()
		{
			Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(
					q => q.ObjectType.Guid == ObjectTypeGuids.JobHistoryErrorGuid), It.IsAny<int>(), It.IsAny<int>()))
				.Returns((int workspaceId, QueryRequest request, int start, int length) =>
				{
					QueryResult result = GetRelativityObjectsForRequest(x => x.JobHistoryErrors,
						JobHistoryErrorsFilter, workspaceId, request, length);
					return Task.FromResult(result);
				});
		}

		private bool IsSingleJobHistoryErrorJobLevelCondition(string condition, out int jobHistoryId, out Guid[] errorTypes)
		{
			System.Text.RegularExpressions.Match match = Regex.Match(condition,
				@"'Job History' == OBJECT (.*) AND 'Error Type' IN CHOICE \[(.*)\]");

			if (match.Success)
			{
				jobHistoryId = int.Parse(match.Groups[1].Value);
				errorTypes = match.Groups[2].Value.Split(',').Select(x => Guid.Parse(x)).ToArray();
				return true;
			}

			jobHistoryId = 0;
			errorTypes = Array.Empty<Guid>();
			return false;
		}

		private bool IsMultiJobHistoryErrorItemLevelCondition(string condition, out List<int> jobHistoryIds)
		{
			System.Text.RegularExpressions.Match match = Regex.Match(condition,
				@"\('Job History' IN OBJECT \[(.*)\]\) AND \('Error Type' == CHOICE 9ddc4914-fef3-401f-89b7-2967cd76714b\)");

			if (match.Success)
			{
				jobHistoryIds = match.Groups[1].Value.Split(',').Select(x => int.Parse(x)).ToList();
				return true;
			}

			jobHistoryIds = new List<int>();
			return false;
		}

		private IList<JobHistoryErrorTest> JobHistoryErrorsFilter(QueryRequest request, IList<JobHistoryErrorTest> list)
		{
			if(IsMultiJobHistoryErrorItemLevelCondition(request.Condition, out List<int> jobHistoryIds))
			{
				return list.Where(x => jobHistoryIds.Contains(x.JobHistory.Value)).ToList();
			}
			else if (IsSingleJobHistoryErrorJobLevelCondition(request.Condition, out int jobHistory, out Guid[] errorTypes))
			{
				return list.Where(x => x.JobHistory == jobHistory && errorTypes.Contains(x.ErrorType.Guids.Single())).ToList();
			}

			return new List<JobHistoryErrorTest>();
		}
	}
}
