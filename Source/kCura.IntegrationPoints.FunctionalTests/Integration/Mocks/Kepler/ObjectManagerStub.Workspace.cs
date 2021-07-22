using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
	{
		private void SetupWorkspace()
		{
			Mock.Setup(x => x.QueryAsync(-1, 
					It.Is<QueryRequest>(q => q.ObjectType.ArtifactTypeID == (int) ArtifactType.Case), 
					It.IsAny<int>(), It.IsAny<int>()))
				.Returns((int workspaceId, QueryRequest request, int start, int length) =>
					{
						List<RelativityObject> foundObjects = GetWorkspaces(request);

						QueryResult result = new QueryResult();
						result.Objects = foundObjects.Take(length).ToList();
						result.TotalCount = result.ResultCount = result.Objects.Count;

						return Task.FromResult(result);
					}
				);
			
			Mock.Setup(x => x.QuerySlimAsync(-1, 
					It.Is<QueryRequest>(q => q.ObjectType.ArtifactTypeID == (int) ArtifactType.Case), 
					It.IsAny<int>(), It.IsAny<int>()))
				.Returns((int workspaceId, QueryRequest request, int start, int length) =>
					{
						List<RelativityObject> foundObjects = GetWorkspaces(request);

						QueryResultSlim result = new QueryResultSlim();
						result.Objects = foundObjects.Take(length).Select(x => ToSlim(x, request.Fields)).ToList();
						result.TotalCount = result.ResultCount = result.Objects.Count;

						return Task.FromResult(result);
					}
				);
		}

		private List<RelativityObject> GetWorkspaces(QueryRequest request)
		{
			List<RelativityObject> foundObjects = new List<RelativityObject>();

			if (IsArtifactIdCondition(request.Condition, out int artifactId))
			{
				AddRelativityObjectsToResult(
					Relativity.Workspaces.Where(
						x => x.ArtifactId == artifactId)
					, foundObjects);
			}
			else if (IsArtifactIdListCondition(request.Condition, out int[] artifactIds))
			{
				AddRelativityObjectsToResult(
					Relativity.Workspaces.Where(
						x => artifactIds.Contains(x.ArtifactId))
					, foundObjects);
			}

			return foundObjects;
		}
	}
}
