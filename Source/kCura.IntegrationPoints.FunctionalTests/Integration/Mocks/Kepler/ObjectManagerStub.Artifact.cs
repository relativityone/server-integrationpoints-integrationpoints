using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
	{
		private void SetupArtifact()
		{
			Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), 
					It.Is<QueryRequest>(q => q.ObjectType.ArtifactTypeID == 0 && q.ObjectType.ArtifactID == 0 && q.ObjectType.Guid == null), 
					It.IsAny<int>(), It.IsAny<int>()))
				.Returns((int workspaceId, QueryRequest request, int start, int length) =>
					{
						WorkspaceTest workspace = Relativity.Workspaces.First(x => x.ArtifactId == workspaceId);
						List<RelativityObject> foundObjects = new List<RelativityObject>();
                      
						if (IsArtifactIdCondition(request.Condition, out int artifactId))
						{
							foundObjects.AddRange(workspace.Artifacts.Where(x => x.ArtifactId == artifactId).Select(x => new RelativityObject
							{
								ArtifactID = x.ArtifactId,
								FieldValues = new List<FieldValuePair>()
							}));
						}
						else if (IsArtifactIdListCondition(request.Condition, out int[] artifactIds))
						{
							foundObjects.AddRange(workspace.Artifacts.Where(x => artifactIds.Contains(x.ArtifactId))
								.Select(x => new RelativityObject
							{
								ArtifactID = x.ArtifactId,
								FieldValues = new List<FieldValuePair>()
							}));
						}

						QueryResult result = new QueryResult();
						result.Objects = foundObjects.Take(length).ToList();
						result.TotalCount = result.ResultCount = result.Objects.Count;

						return Task.FromResult(result);
					}
				);
		}
	}
}
