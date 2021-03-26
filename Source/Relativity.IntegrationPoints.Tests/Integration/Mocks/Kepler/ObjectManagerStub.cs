using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Exceptions;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub : KeplerStubBase<IObjectManager>
	{
		public void SetupArtifact(InMemoryDatabase database, RdoTestBase testRdo)
		{
			Mock.Setup(x => x.QueryAsync(testRdo.WorkspaceId, It.Is<QueryRequest>(q =>
					q.ObjectType.Name == testRdo.Artifact.ArtifactType &&
					q.Condition == $"'ArtifactID' == {testRdo.ArtifactId}"), It.IsAny<int>(), It.IsAny<int>()))
				.Returns((int workspaceId, QueryRequest request, int start, int length) =>
				{
					var artifacts = database.Artifacts
						.Where(x => x.ArtifactType == request.ObjectType.Name && x.ArtifactId == testRdo.ArtifactId)
						.ToList();

					return Task.FromResult(new QueryResult
					{
						Objects = artifacts.Select(x => new RelativityObject
						{
							ArtifactID = x.ArtifactId,
							FieldValues = new List<FieldValuePair>()
						}).ToList()
					});
				});
		}
	}
}
