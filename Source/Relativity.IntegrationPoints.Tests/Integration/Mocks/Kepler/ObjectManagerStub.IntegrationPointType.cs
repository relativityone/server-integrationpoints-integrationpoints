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
		public void SetupIntegrationPointTypes(InMemoryDatabase database, IEnumerable<IntegrationPointTypeTest> integrationPointTypes)
		{
			foreach (IntegrationPointTypeTest integrationPointType in integrationPointTypes)
			{
				Mock.Setup(x => x.ReadAsync(integrationPointType.WorkspaceId, It.Is<ReadRequest>(r =>
						r.Object.ArtifactID == integrationPointType.ArtifactId)))
					.Returns((int workspaceId, ReadRequest request) =>
						{
							ReadResult result = database.IntegrationPointTypes.FirstOrDefault(
								x => x.ArtifactId == request.Object.ArtifactID) != null
								? new ReadResult { Object = integrationPointType.ToRelativityObject() }
								: new ReadResult { Object = null };

							return Task.FromResult(result);
						}
					);
			}
		}
	}
}
