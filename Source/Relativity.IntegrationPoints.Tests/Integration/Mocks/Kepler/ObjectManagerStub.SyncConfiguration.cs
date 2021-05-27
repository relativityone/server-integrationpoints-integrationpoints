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
        private void SetupSyncConfiguration()
        {
			Mock.Setup(x => x.QuerySlimAsync(It.IsAny<int>(), It.Is<QueryRequest>(
					q => IsSyncConfigurationByJobHistoryIdQuery(q)), It.IsAny<int>(), It.IsAny<int>()))
				.Returns((int workspaceId, QueryRequest request, int start, int length) =>
				{
					WorkspaceTest workspace = _relativity.Workspaces.Single(x => x.ArtifactId == workspaceId);

					string jobHistory = Regex.Match(request.Condition, @"'JobHistoryId' == (\d+)").Groups[1].Value;
					int jobHistoryId = int.Parse(jobHistory);

					var configs = workspace.SyncConfigurations.Where(x => x.JobHistoryId == jobHistoryId);

					List<RelativityObjectSlim> result = configs
						.Select(x => x.ToRelativityObject())
						.Select(x => ToSlim(x, request.Fields))
						.ToList();

					return Task.FromResult(new QueryResultSlim
					{
						Objects = result,
						TotalCount = result.Count,
						ResultCount = result.Count
					});

				});
		}

		private bool IsSyncConfigurationByJobHistoryIdQuery(QueryRequest query) =>
			query.ObjectType.Guid == ObjectTypeGuids.SyncConfigurationGuid &&
			Regex.Match(query.Condition, @"'JobHistoryId' == (\d+)").Success;
	}
}
