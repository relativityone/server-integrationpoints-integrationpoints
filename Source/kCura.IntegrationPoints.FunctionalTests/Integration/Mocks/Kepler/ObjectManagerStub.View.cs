using System.Threading.Tasks;
using Moq;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
	{
		private void SetupView()
		{
			Mock.Setup(x => x.QueryAsync(It.IsAny<int>(),
					It.Is<QueryRequest>(r => IsViewQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
				.Returns((int workspaceId, QueryRequest request, int start, int length) =>
				{
					QueryResult result = GetRelativityObjectsForRequest(x => x.Views, null, workspaceId, request, length);
					return Task.FromResult(result);
				}
				);
		}

		private bool IsViewQuery(QueryRequest query)
		{
			return query.ObjectType.ArtifactTypeID == (int)ArtifactType.View;
		}
	}
}
